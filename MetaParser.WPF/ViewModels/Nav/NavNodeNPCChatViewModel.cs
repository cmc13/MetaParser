using MetaParser.Models;
using System;

namespace MetaParser.WPF.ViewModels
{
    public class NavNodeNPCChatViewModel : NavNodeViewModel
    {
        public NavNodeNPCChatViewModel(NavNodeNPCChat node) : base(node)
        { }

        public string ObjectName
        {
            get => ((NavNodeNPCChat)Node).Data.objectName;
            set
            {
                if (ObjectName != value)
                {
                    ((NavNodeNPCChat)Node).Data = (
                        value,
                        ((NavNodeNPCChat)Node).Data.objectClass,
                        ((NavNodeNPCChat)Node).Data.x,
                        ((NavNodeNPCChat)Node).Data.y,
                        ((NavNodeNPCChat)Node).Data.z);
                    OnPropertyChanged(nameof(ObjectName));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        //public ObjectClass ObjectClass
        //{
        //    get => ((NavNodeNPCChat)Node).Data.objectClass;
        //    set
        //    {
        //        if (ObjectClass != value)
        //        {
        //            ((NavNodeNPCChat)Node).Data = (
        //                ((NavNodeNPCChat)Node).Data.objectName,
        //                value,
        //                ((NavNodeNPCChat)Node).Data.x,
        //                ((NavNodeNPCChat)Node).Data.y,
        //                ((NavNodeNPCChat)Node).Data.z);
        //            OnPropertyChanged(nameof(ObjectName));
        //            IsDirty = true;
        //        }
        //    }
        //}

        public double TargetX
        {
            get => ((NavNodeNPCChat)Node).Data.x;
            set
            {
                if (TargetX != value)
                {
                    ((NavNodeNPCChat)Node).Data = (
                        ((NavNodeNPCChat)Node).Data.objectName,
                        ((NavNodeNPCChat)Node).Data.objectClass,
                        value,
                        ((NavNodeNPCChat)Node).Data.y,
                        ((NavNodeNPCChat)Node).Data.z);
                    OnPropertyChanged(nameof(TargetX));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public double TargetY
        {
            get => ((NavNodeNPCChat)Node).Data.y;
            set
            {
                if (TargetY != value)
                {
                    ((NavNodeNPCChat)Node).Data = (
                        ((NavNodeNPCChat)Node).Data.objectName,
                        ((NavNodeNPCChat)Node).Data.objectClass,
                        ((NavNodeNPCChat)Node).Data.x,
                        value,
                        ((NavNodeNPCChat)Node).Data.z);
                    OnPropertyChanged(nameof(TargetY));
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
            get => ((NavNodeNPCChat)Node).Data.z;
            set
            {
                if (TargetZ != value)
                {
                    ((NavNodeNPCChat)Node).Data = (
                        ((NavNodeNPCChat)Node).Data.objectName,
                        ((NavNodeNPCChat)Node).Data.objectClass,
                        ((NavNodeNPCChat)Node).Data.x,
                        ((NavNodeNPCChat)Node).Data.y,
                        value);
                    OnPropertyChanged(nameof(TargetZ));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
