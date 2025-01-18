using System;

namespace MetaParser.Models;

public sealed class Rule
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
}
