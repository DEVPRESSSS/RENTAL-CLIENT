using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rental.Models
{
    class PropertyModel
    {
        public string? PropertyID { get; set; }           
        public string? PropertyName { get; set; }         
        public string? Type { get; set; }                 
        public decimal? MonthlyRent { get; set; }        
        public string? Status { get; set; }               
        public string? Description { get; set; }
    }
}
