using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class ChatMessageCaptureConditionViewModel : ConditionViewModel
    {
        public ChatMessageCaptureConditionViewModel(ChatMessageCaptureCondition condition) : base(condition)
        { }

        public string Pattern
        {
            get => ((ChatMessageCaptureCondition)Condition).Pattern;
            set
            {
                if (((ChatMessageCaptureCondition)Condition).Pattern != value)
                {
                    ((ChatMessageCaptureCondition)Condition).Pattern = value;
                    OnPropertyChanged(nameof(Pattern));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public string Color
        {
            get => ((ChatMessageCaptureCondition)Condition).Color;
            set
            {
                if (((ChatMessageCaptureCondition)Condition).Color != value)
                {
                    ((ChatMessageCaptureCondition)Condition).Color = value;
                    OnPropertyChanged(nameof(Color));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
