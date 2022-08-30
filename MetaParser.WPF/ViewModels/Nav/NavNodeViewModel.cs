using MetaParser.Models;
using System;
using System.Collections.Generic;

namespace MetaParser.WPF.ViewModels
{
    public class NavNodeViewModel : BaseViewModel
    {
        public NavNodeViewModel(NavNode node)
        {
            Node = node;
        }

        public NavNodeType Type => Node.Type;

        public double X
        {
            get => Node.Point.x;
            set
            {
                if (Node.Point.x != value)
                {
                    Node.Point = (value, Node.Point.y, Node.Point.z);
                    OnPropertyChanged(nameof(X));
                    OnPropertyChanged(nameof(XDirection));
                    OnPropertyChanged(nameof(AbsX));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public double AbsX
        {
            get => Math.Abs(X);
            set
            {
                if (AbsX != value)
                {
                    var sign = Math.Sign(X);
                    sign = sign == 0 ? 1 : sign;
                    X = sign * value;
                }
            }
        }

        public string XDirection
        {
            get => X > 0 ? "E" : "W";
            set
            {
                if (XDirection != value)
                {
                    X *= -1;
                }
            }
        }

        public double Y
        {
            get => Node.Point.y;
            set
            {
                if (Node.Point.y != value)
                {
                    Node.Point = (Node.Point.x, value, Node.Point.z);
                    OnPropertyChanged(nameof(Y));
                    OnPropertyChanged(nameof(YDirection));
                    OnPropertyChanged(nameof(AbsY));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public double AbsY
        {
            get => Math.Abs(Y);
            set
            {
                if (AbsY != value)
                {
                    var sign = Math.Sign(Y);
                    sign = sign == 0 ? 1 : sign;
                    Y = sign * value;
                }
            }
        }

        public string YDirection
        {
            get => Y > 0 ? "N" : "S";
            set
            {
                if (YDirection != value)
                {
                    Y *= -1;
                }
            }
        }

        public double Z
        {
            get => Node.Point.z;
            set
            {
                if (Z != value)
                {
                    Node.Point = (Node.Point.x, Node.Point.y, value);
                    OnPropertyChanged(nameof(Z));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public string Display => Node.ToString();

        public NavNode Node { get; }
    }
}
