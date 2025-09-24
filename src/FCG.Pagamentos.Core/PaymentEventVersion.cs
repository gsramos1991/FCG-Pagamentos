using FCG.Pagamentos.Business.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Core
{
    public class PaymentEventVersion
    {
        public int GetLastVersion (PaymentEvent paymentEvent)
        {
            if (paymentEvent == null)
                return 1;

            return paymentEvent.Version + 1;
        }
    }
}
