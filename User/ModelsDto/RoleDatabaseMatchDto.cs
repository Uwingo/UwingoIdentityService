using Entity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.ModelsDto
{
    public class RoleDatabaseMatchDto
    {
        public Guid Id { get; set; }
        public string RoleId { get; set; } // Rol ID'si
        public Guid CompanyApplicationId { get; set; } // İlgili uygulama ID'si
    }
}
