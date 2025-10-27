using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutePayment.Domain.Interfaces
{
    public interface ISecureHashGenerator
    {
        string Generate(Dictionary<string, string> parameters, string authenticationToken);
        bool Validate(Dictionary<string, string> parameters, string receivedHash, string authenticationToken);
    }
}
