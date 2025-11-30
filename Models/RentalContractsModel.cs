using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rental.Models
{
    public class RentalContractsModel
    {

        public string? ContractID { get; set; }         
        public string? PropertyID { get; set; }           
        public string? PropName { get; set; }           
        public string? TenantID { get; set; }             
        public string? FullName { get; set; }             
        public DateTime? StartDate { get; set; }          
        public DateTime? EndDate { get; set; }          
        public decimal? MonthlyRent { get; set; }         
        public decimal? DepositAmount { get; set; }     
        public decimal? CashAdvance { get; set; }     
        public decimal? Balance { get; set; }     
        public string? Status { get; set; }
        public int? CountOfDays
        {
            get
            {
                if (StartDate.HasValue && EndDate.HasValue)
                    return (EndDate.Value - StartDate.Value).Days;
                else
                    return null;
            }
        }
        public DateTime CreatedAt { get; set; }

        

        public string StockStatus
        {
            get
            {
                if (Status == "Inactive")
                    return "NoContract";
                else
                    return "ActiveContract";
            }
        }

        public bool isDue
        {
            get
            {
                if (CountOfDays == 0)
                    return true;
                else
                    return false;
            }
        }

        public bool hasBalance
        {
            get
            {
                if (Balance > 0)
                    return true;
                else
                    return false;
            }
        }
    }
}
