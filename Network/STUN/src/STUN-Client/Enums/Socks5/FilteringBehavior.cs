using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STUN.Enums
{
    public enum FilteringBehavior
    {
        Unknown,
        UnSupportedServer,
        EndPointIndependent,
        AddressDependent,
        AddressAndPortDependent,
        Fail
    }
}
