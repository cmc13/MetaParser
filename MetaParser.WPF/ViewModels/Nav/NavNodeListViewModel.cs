using CommunityToolkit.Mvvm.Input;
using MetaParser.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace MetaParser.WPF.ViewModels
{
    public class NavNodeListViewModel : BaseViewModel
    {
        private NavNodeViewModel selectedNode;
        private NavRoute nav;

        public NavNodeListViewModel(NavRoute nav)
        {
            this.nav = nav;

            if (nav.Data is List<NavNode> navNodes && navNodes != null)
            {
                foreach (var node in navNodes)
                {
                    var vm = NavNodeViewModelFactory.CreateViewModel(node);
                    NavNodes.Add(vm);
                    vm.PropertyChanged += Node_PropertyChanged;
                }
            }

            NavNodes.CollectionChanged += NavNodes_CollectionChanged;

            AddCommand = new(() =>
            {
                var node = NavNode.Create(NavNodeType.Point);
                var vm = NavNodeViewModelFactory.CreateViewModel(node);
                NavNodes.Add(vm);
                SelectedNode = vm;
            });

            RemoveCommand = new(() =>
            {
                NavNodes.Remove(SelectedNode);
                SelectedNode = null;
            }, () => SelectedNode != null);

            CutCommand = new(async () =>
            {
                await CopyCommand.ExecuteAsync(null);
                RemoveCommand.Execute(null);
            }, () => SelectedNode != null);

            CopyCommand = new(async () =>
            {
                using var sw = new StringWriter();
                await sw.WriteLineAsync(((int)SelectedNode.Type).ToString()).ConfigureAwait(false);
                await Formatters.NavWriter.WriteNavNodeAsync(sw, SelectedNode.Node).ConfigureAwait(false);
                var nodeText = sw.ToString();
                Clipboard.SetData(typeof(NavNode).Name, nodeText);
                PasteCommand.NotifyCanExecuteChanged();
            }, () => SelectedNode != null);

            PasteCommand = new(async () =>
            {
                var conditionText = (string)Clipboard.GetData(typeof(Models.NavNode).Name);
                using var sr = new StringReader(conditionText);
                var nodeType = (NavNodeType)int.Parse(await sr.ReadLineAsync().ConfigureAwait(false));
                var node = NavNode.Create(nodeType);
                await Formatters.NavReader.ReadNavNodeAsync(sr, node).ConfigureAwait(false);
                var vm = NavNodeViewModelFactory.CreateViewModel(node);
                NavNodes.Add(vm);
                SelectedNode = vm;
            }, () => Clipboard.ContainsData(typeof(NavNode).Name));
        }

        public RelayCommand AddCommand { get; }

        public RelayCommand RemoveCommand { get; }

        public AsyncRelayCommand CutCommand { get; }

        public AsyncRelayCommand CopyCommand { get; }

        public AsyncRelayCommand PasteCommand { get; }

        private void NavNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (nav.Data == null)
                    nav.Data = new List<NavNode>();
                var navNodes = nav.Data as List<NavNode>;

                foreach (NavNodeViewModel node in e.NewItems)
                {
                    navNodes.Add(node.Node);
                    node.PropertyChanged += Node_PropertyChanged;
                }
                IsDirty = true;
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                var navNodes = nav.Data as List<NavNode>;
                foreach (NavNodeViewModel node in e.OldItems)
                {
                    node.PropertyChanged -= Node_PropertyChanged;
                    navNodes.Remove(node.Node);
                }
                IsDirty = true;
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                var navNodes = nav.Data as List<NavNode>;
                var oldNode = e.OldItems[0] as NavNodeViewModel;
                var newNode = e.NewItems[0] as NavNodeViewModel;

                var idx = navNodes.IndexOf(oldNode.Node);
                navNodes[idx] = newNode.Node;
                IsDirty = true;
            }
        }

        private void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NavNodeViewModel.IsDirty))
            {
                OnPropertyChanged(nameof(IsDirty));
            }
        }

        public ObservableCollection<NavNodeViewModel> NavNodes { get; } = new();

        public NavNodeViewModel SelectedNode
        {
            get => selectedNode;
            set
            {
                if (selectedNode != value)
                {
                    selectedNode = value;
                    OnPropertyChanged(nameof(SelectedNode));
                    OnPropertyChanged(nameof(SelectedNodeType));
                    RemoveCommand.NotifyCanExecuteChanged();
                    CutCommand.NotifyCanExecuteChanged();
                    CopyCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public NavNodeType? SelectedNodeType
        {
            get => SelectedNode?.Type;
            set
            {
                if (SelectedNode?.Type != value)
                {
                    var navNodes = nav.Data as List<NavNode>;
                    var idx = navNodes.IndexOf(SelectedNode.Node);
                    var node = NavNode.Create(value.Value);
                    NavNodes[idx] = NavNodeViewModelFactory.CreateViewModel(node);
                    navNodes[idx] = node;

                    SelectedNode = NavNodes[idx];
                    IsDirty = true;
                }
            }
        }

        public override void Clean()
        {
            base.Clean();
            foreach (var dirtyNode in NavNodes.Where(n => n.IsDirty))
                dirtyNode.Clean();
        }

        public override bool IsDirty
        {
            get => base.IsDirty || NavNodes.Any(v => v.IsDirty);
            set => base.IsDirty = value;
        }
    }
}
