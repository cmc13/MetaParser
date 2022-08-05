using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaParser.WPF.ViewModels
{
    public class ActionViewModel : BaseViewModel
    {
        public event EventHandler StateChanged;

        public ActionViewModel(MetaAction action, MetaViewModel meta)
        {
            Action = action;
            Meta = meta;
        }

        public MetaViewModel Meta {get;}

        public MetaAction Action { get; }

        public IEnumerable<string> States => Meta.StateList;

        public ActionType Type => Action.Type;

        public virtual string Display => Action.ToString();

        public virtual bool IsValid => true;

        protected void OnStateChanged() => StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public class ActionViewModel<T> : ActionViewModel
    {
        public ActionViewModel(MetaAction<T> action, MetaViewModel meta) : base(action, meta)
        { }

        public T Data
        {
            get => ((MetaAction<T>)Action).Data;
            set
            {
                if ((Data == null && value != null) || !Data.Equals(value))
                {
                    ((MetaAction<T>)Action).Data = value;
                    OnPropertyChanged(nameof(Data));
                    OnPropertyChanged(nameof(Display));
                    if (Type == ActionType.SetState)
                    {
                        OnStateChanged();
                        OnPropertyChanged(nameof(IsValid));
                    }
                    IsDirty = true;
                }
            }
        }

        public override bool IsValid => Type != ActionType.SetState || Meta.Rules.Any(r => r.State.Equals(Data));
    }
}
