using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Models
{
    public class RoleDatabaseMatch
    {
        [Key]
        public Guid Id { get; set; }
        public string RoleId { get; set; } // Rol ID'si
        [ForeignKey("CompanyApplicationId")]
        public Guid CompanyApplicationId { get; set; } // İlgili uygulama ID'si
        public CompanyApplication CompanyApplication { get; set; } // İlgili uygulama
    }
}