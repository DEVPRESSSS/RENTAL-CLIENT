using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rental.Models
{
   public class TenantModel
    {
        public string? TenantID { get; set; }   
        public string? Name { get; set; }       
        public string? Contact { get; set; }    
        public string? Address { get; set; }   
        public string? Email { get; set; }     
        public string? PropertyName { get; set; }     
        public decimal? MonthlyRent { get; set; }     
        public decimal? DepositAmount { get; set; }     
        public string? Status { get; set; }     
        public DateTime DateJoined { get; set; }

        public string StockStatus
        {
            get
            {
                if (Status == "Inactive")
                    return "NoContract";
                else if (string.IsNullOrEmpty(Status))
                    return "NoValue";
                else
                    return "ActiveContract";
            }
        }
    }
}
