using CommunityToolkit.Mvvm.Input;
using MetaParser.Models;
using MetaParser.WPF.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MetaParser.WPF.ViewModels
{
    public class ConditionViewModel : BaseViewModel
    {
        public ConditionViewModel(Condition condition)
        {
            Condition = condition;
        }

        public Condition Condition { get; }

        public string Display => Condition.ToString();

        public ConditionType Type => Condition.Type;
    }

    public class ConditionViewModel<T> : ConditionViewModel
    {
        public ConditionViewModel(Condition<T> condition) : base(condition)
        { }

        public T Data
        {
            get => ((Condition<T>)Condition).Data;
            set
            {
                if (Data == null || !Data.Equals(value))
                {
                    ((Condition<T>)Condition).Data = value;
                    OnPropertyChanged(nameof(Data));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }

    public partial class LandCellConditionViewModel : ConditionViewModel<int>
    {
        private bool showSearchPanel = false;
        private string searchText = null;
        private WeenieService.Weenie selectedPortal = null;
        private bool searching = false;

        public bool ShowSearchPanel
        {
            get => showSearchPanel;
            set
            {
                if (showSearchPanel != value)
                {
                    showSearchPanel = value;
                    OnPropertyChanged(nameof(ShowSearchPanel));
                }
            }
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        public bool Searching
        {
            get => searching;
            set
            {
                if (searching != value)
                {
                    searching = value;
                    OnPropertyChanged(nameof(Searching));
                }
            }
        }

        public ObservableCollection<WeenieService.Weenie> Weenies { get; } = new();

        public WeenieService.Weenie SelectedPortal
        {
            get => selectedPortal;
            set
            {
                if (selectedPortal != value)
                {
                    selectedPortal = value;
                    OnPropertyChanged(nameof(SelectedPortal));

                    Task.Run(async () =>
                    {
                        var service = new WeenieService();
                        var details = await service.GetWeenieDetailsAsync(selectedPortal.WeenieClassId).ConfigureAwait(false);

                        var destination = details?.Positions?.FirstOrDefault(p => p.Key == 2);
                        if (destination != null)
                        {
                            base.Data = (int)destination.Value.Value.LandCellId;
                        }
                    });
                }
            }
        }

        public LandCellConditionViewModel(Condition<int> condition)
            : base(condition)
        {
            if (condition.Type != ConditionType.LandCellE)
                throw new ArgumentException("Invalid condition");

            ShowSearchPanelCommand = new(() => ShowSearchPanel = !ShowSearchPanel);
        }

        public RelayCommand ShowSearchPanelCommand { get; }

        [RelayCommand]
        async Task SearchPortals()
        {
            Searching = true;
            var service = new WeenieService();

            try
            {
                Weenies.Clear();
                await foreach (var weenie in service.GetWeeniesAsync(7, SearchText))
                {
                    Weenies.Add(weenie);
                }
            }
            finally
            {
                Searching = false;
            }
        }
    }

    public partial class LandBlockConditionViewModel : ConditionViewModel<int>
    {
        private const int LANDBLOCK_DIVISION = 256;
        private bool showMap;
        private (double, double) mousePosition;

        public LandBlockConditionViewModel(Condition<int> condition)
            : base(condition)
        {
            if (condition.Type != ConditionType.LandBlockE)
                throw new ArgumentException("Invalid condition");
        }

        public bool ShowMap
        {
            get => showMap;
            set
            {
                if (ShowMap != value)
                {
                    showMap = value;
                    OnPropertyChanged(nameof(ShowMap));
                }
            }
        }

        public (double x, double y) MousePosition
        {
            get => mousePosition;
            set
            {
                if (mousePosition != value)
                {
                    mousePosition = value;
                    OnPropertyChanged(nameof(MousePosition));
                    OnPropertyChanged(nameof(HoverLandblock));
                }
            }
        }

        public int HoverLandblock => CoordinatesToLandBlock(MousePosition.x, MousePosition.y);

        [RelayCommand]
        void MapClick((double x, double y) p)
        {
            Data = CoordinatesToLandBlock(p.x, p.y);
            ShowMap = false;
        }

        [RelayCommand]
        void ToggleMap() => ShowMap = !ShowMap;

        private static int CoordinatesToLandBlock(double x, double y) => (int)(x * LANDBLOCK_DIVISION) * 0x1000000 + (int)(y * LANDBLOCK_DIVISION) * 0x10000;
    }
}
