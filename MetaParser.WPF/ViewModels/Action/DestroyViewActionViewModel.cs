using MetaParser.Models;
using System.Collections.Generic;
using System.Linq;

namespace MetaParser.WPF.ViewModels
{
    public class DestroyViewActionViewModel : ActionViewModel
    {
        public DestroyViewActionViewModel(DestroyViewMetaAction action, MetaViewModel meta) : base(action, meta)
        { }

        public IEnumerable<string> Views
        {
            get
            {
                return Meta.Rules.SelectMany(r => GetViews(r.Action.Action)).Distinct();

                IEnumerable<string> GetViews(MetaAction action)
                {
                    if (action is AllMetaAction am)
                    {
                        foreach (var aa in am.Data)
                        {
                            foreach (var view in GetViews(aa))
                                yield return view;
                        }
                    }
                    else if (action is CreateViewMetaAction cm)
                        yield return cm.ViewName;
                }
            }
        }

        public string ViewName
        {
            get => ((DestroyViewMetaAction)Action).ViewName;
            set
            {
                if (((DestroyViewMetaAction)Action).ViewName != value)
                {
                    ((DestroyViewMetaAction)Action).ViewName = value;
                    OnPropertyChanged(nameof(ViewName));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
