using ETLab_MauiPlainPureMode.ViewModels;

namespace ETLab_MauiPlainPureMode.Models
{
    public class NATCheck5389Outcome : ObservableObject
    {
        public string BindingTest { get; set; }

        public string MappingBehavior { get; set; }

        public string FilteringBehavior { get; set; }

        public string LocalIPEndPoint { get; set; }

        public string PublicIPEndPoint { get; set; }
    }
}
