using MetaParser.Models;
using MetaParser.WPF.Services;
using System.Collections.ObjectModel;

namespace MetaParser.WPF.ViewModels;

public class ItemCountConditionViewModel : ConditionViewModel
{
    private static readonly WeenieService weenieService = new();

    public ItemCountConditionViewModel(ItemCountCondition condition) : base(condition)
    { }

    public string ItemName
    {
        get => ((ItemCountCondition)Condition).ItemName;
        set
        {
            if (((ItemCountCondition)Condition).ItemName != value)
            {
                ((ItemCountCondition)Condition).ItemName = value;
                OnPropertyChanged(nameof(ItemName));
                OnPropertyChanged(nameof(Display));
                IsDirty = true;

                //if (!string.IsNullOrWhiteSpace(value))
                //{
                //    Task.Run(async () =>
                //    {
                //        ItemNameList.Clear();
                //        await foreach (var weenie in weenieService.GetWeeniesAsync(value).ConfigureAwait(false))
                //        {
                //            if (!ItemNameList.Contains(weenie.Name))
                //                ItemNameList.Add(weenie.Name);
                //        }
                //    });
                //}
            }
        }
    }

    public int Count
    {
        get => ((ItemCountCondition)Condition).Count;
        set
        {
            if (((ItemCountCondition)Condition).Count != value)
            {
                ((ItemCountCondition)Condition).Count = value;
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(Display));
                IsDirty = true;
            }
        }
    }

    public ObservableCollection<string> ItemNameList { get; } = new();
}
