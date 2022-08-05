using System;
using System.Runtime.Serialization;

namespace MetaParser.Models
{
    [Serializable]
    public class Rule : ISerializable
    {
        private string state;
        public event EventHandler<StateChangedEventArgs> StateChanged;

        public string State
        {
            get => state;
            set
            {
                if (state != value)
                {
                    var args = new StateChangedEventArgs
                    {
                        OldState = state,
                        NewState = value
                    };

                    state = value;
                    StateChanged?.Invoke(this, args);
                }
            }
        }

        public Condition Condition { get; set; }

        public MetaAction Action { get; set; }

        public Rule() { }

        private Rule(SerializationInfo info, StreamingContext context)
        {
            info.GetString(nameof(State));
            info.GetValue(nameof(Condition), typeof(Condition));
            info.GetValue(nameof(Action), typeof(Action));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(State), State, typeof(string));
            info.AddValue(nameof(Condition), Condition, typeof(Condition));
            info.AddValue(nameof(Action), Action, typeof(MetaAction));
        }
    }
}
