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

    public class UserDatabaseMatchDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid CompanyApplicationId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
