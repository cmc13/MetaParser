using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class NavNodeOpenVendorViewModel : NavNodeViewModel
    {
        public NavNodeOpenVendorViewModel(NavNodeOpenVendor node) : base(node)
        { }

        public string VendorName
        {
            get => ((NavNodeOpenVendor)Node).Data.vendorName;
            set
            {
                if (VendorName != value)
                {
                    ((NavNodeOpenVendor)Node).Data = (((NavNodeOpenVendor)Node).Data.vendorId, value);
                    OnPropertyChanged(nameof(VendorName));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public int VendorId
        {
            get => ((NavNodeOpenVendor)Node).Data.vendorId;
            set
            {
                if (VendorId != value)
                {
                    ((NavNodeOpenVendor)Node).Data = (value, ((NavNodeOpenVendor)Node).Data.vendorName);
                    OnPropertyChanged(nameof(VendorId));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
