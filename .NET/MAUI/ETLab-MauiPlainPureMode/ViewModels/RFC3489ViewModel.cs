using System.Windows.Input;

namespace ETLab_MauiPlainPureMode.ViewModels
{
    public class RFC3489ViewModel : BaseViewModel
    {
        public IEnumerable<string> STUNServers => new List<string>
        {
            @"stun.syncthing.net",
            @"stun.qq.com",
            @"stun.miwifi.com",
            @"stun.bige0.com",
            @"stun.stunprotocol.org"
        };

        public string ChoosenSTUNServer { get; set; }

        public ICommand CheckNATTypeCommand { get; private set; }

        public RFC3489ViewModel()
        {
            CheckNATTypeCommand = new Command(CheckNATType);
        }

        private void CheckNATType()
        {

        }
    }
}
