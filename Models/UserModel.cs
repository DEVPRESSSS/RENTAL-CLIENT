using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rental.Models
{
    public class UserModel
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? RoleId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Contact { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
