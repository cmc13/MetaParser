using MetaParser.Models;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Windows.Input;

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

    public class LandBlockConditionViewModel : ConditionViewModel<int>
    {
        private const int LANDBLOCK_DIVISION = 256;
        private bool showMap;
        private (double, double) mousePosition;

        public LandBlockConditionViewModel(Condition<int> condition)
            : base(condition)
        {
            if (condition.Type != ConditionType.LandBlockE)
                throw new ArgumentException("Invalid condition");

            MapClickCommand = new(p =>
            {
                Data = CoordinatesToLandBlock(p.x, p.y);
                ShowMap = false;
            });

            ToggleMapCommand = new(() => ShowMap = !ShowMap);
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

        public RelayCommand<(double x, double y)> MapClickCommand { get; }

        public RelayCommand ToggleMapCommand { get; }

        private static int CoordinatesToLandBlock(double x, double y) => (int)(x * LANDBLOCK_DIVISION) * 0x1000000 + (int)(y * LANDBLOCK_DIVISION) * 0x10000;
    }
}
