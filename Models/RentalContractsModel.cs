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
        public string? Status { get; set; }              
        public DateTime CreatedAt { get; set; }
    }
}
