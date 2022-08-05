using MetaParser.Models;
using System;

namespace MetaParser.WPF.ViewModels
{
    public class NavNodePortalViewModel : NavNodeViewModel
    {
        public NavNodePortalViewModel(NavNodePortal node) : base(node)
        { }

        public string ObjectName
        {
            get => ((NavNodePortal)Node).Data.objectName;
            set
            {
                if (ObjectName != value)
                {
                    ((NavNodePortal)Node).Data = (
                        value,
                        ((NavNodePortal)Node).Data.objectClass,
                        ((NavNodePortal)Node).Data.x,
                        ((NavNodePortal)Node).Data.y,
                        ((NavNodePortal)Node).Data.z);
                    OnPropertyChanged(nameof(ObjectName));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public ObjectClass ObjectClass
        {
            get => ((NavNodePortal)Node).Data.objectClass;
            set
            {
                if (ObjectClass != value)
                {
                    ((NavNodePortal)Node).Data = (
                        ((NavNodePortal)Node).Data.objectName,
                        value,
                        ((NavNodePortal)Node).Data.x,
                        ((NavNodePortal)Node).Data.y,
                        ((NavNodePortal)Node).Data.z);
                    OnPropertyChanged(nameof(ObjectName));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public double TargetX
        {
            get => ((NavNodePortal)Node).Data.x;
            set
            {
                if (TargetX != value)
                {
                    ((NavNodePortal)Node).Data = (
                        ((NavNodePortal)Node).Data.objectName,
                        ((NavNodePortal)Node).Data.objectClass,
                        value,
                        ((NavNodePortal)Node).Data.y,
                        ((NavNodePortal)Node).Data.z);
                    OnPropertyChanged(nameof(TargetX));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public double AbsTargetX
        {
            get => Math.Abs(TargetX);
            set
            {
                if (AbsTargetX != value)
                {
                    TargetX = Math.CopySign(value, TargetX);
                }
            }
        }

        public string TargetXDirection
        {
            get => TargetX > 0 ? "E" : "W";
            set
            {
                if (TargetXDirection != value)
                {
                    TargetX *= -1;
                }
            }
        }

        public double TargetY
        {
            get => ((NavNodePortal)Node).Data.y;
            set
            {
                if (TargetY != value)
                {
                    ((NavNodePortal)Node).Data = (
                        ((NavNodePortal)Node).Data.objectName,
                        ((NavNodePortal)Node).Data.objectClass,
                        ((NavNodePortal)Node).Data.x,
                        value,
                        ((NavNodePortal)Node).Data.z);
                    OnPropertyChanged(nameof(TargetY));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public double AbsTargetY
        {
            get => Math.Abs(TargetY);
            set
            {
                if (AbsTargetY != value)
                {
                    TargetY = Math.CopySign(value, TargetY);
                }
            }
        }

        public string TargetYDirection
        {
            get => TargetY > 0 ? "N" : "S";
            set
            {
                if (TargetYDirection != value)
                {
                    TargetY *= -1;
                }
            }
        }

        public double TargetZ
        {
            get => ((NavNodePortal)Node).Data.z;
            set
            {
                if (TargetZ != value)
                {
                    ((NavNodePortal)Node).Data = (
                        ((NavNodePortal)Node).Data.objectName,
                        ((NavNodePortal)Node).Data.objectClass,
                        ((NavNodePortal)Node).Data.x,
                        ((NavNodePortal)Node).Data.y,
                        value);
                    OnPropertyChanged(nameof(TargetZ));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
