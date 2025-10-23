using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rental.Models
{
    public class PaymentModel
    {

        public string? PaymentId { get; set; }
        public string? TenantId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? DateOfPayment { get; set; }
    }
}
