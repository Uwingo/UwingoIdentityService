using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entity.Models
{
    public class Role : IdentityRole
    {
        public string Description { get; set; }
        //[JsonIgnore]
        //public ICollection<UserRole> UserRoles { get; set; }
    }
}
