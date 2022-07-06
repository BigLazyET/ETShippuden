using ETLab_MauiPlainPureMode.ViewModels;

namespace ETLab_MauiPlainPureMode.Models
{
    public class NATCheck5780Outcome : ObservableObject
    {
        public string BindingTest { get; set; }

        public string MappingBehavior { get; set; }

        public string FilteringBehavior { get; set; }

        public string LocalIPEndPoint { get; set; }

        public string PublicIPEndPoint { get; set; }

        private string _testX = "TestXXXX";
        public string TestX
        {
            get
            {
                return _testX;
            }
            set
            {
                SetProperty(ref _testX, value);
            }
        }
    }
}
