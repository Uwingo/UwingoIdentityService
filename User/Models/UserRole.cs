using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entity.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        [JsonIgnore]
        public User User { get; set; }
        [JsonIgnore]
        public Role Role { get; set; }
        public DateTime AssignedDate { get; set; }
    }

}
