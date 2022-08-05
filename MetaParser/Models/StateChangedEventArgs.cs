using System;

namespace MetaParser.Models
{
    public class StateChangedEventArgs : EventArgs
    {
        public string OldState { get; set; }
        public string NewState { get; set; }
    }
}