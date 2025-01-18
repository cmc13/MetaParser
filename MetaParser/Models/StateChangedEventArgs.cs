using System;

namespace MetaParser.Models;

public sealed class StateChangedEventArgs : EventArgs
{
    public string OldState { get; set; }
    public string NewState { get; set; }
}