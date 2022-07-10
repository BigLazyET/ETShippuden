using ETLab_MauiPlainPureMode.ViewModels;
using System.Net;

namespace ETLab_MauiPlainPureMode.Models
{
    public class NATCheck5389Outcome : ObservableObject
    {
        public string BindingTest { get; set; }

        public string MappingBehavior { get; set; }

        public string FilteringBehavior { get; set; }

        public IPEndPoint LocalIPEndPoint { get; set; }

        public IPEndPoint PublicIPEndPoint { get; set; }
    }
}
