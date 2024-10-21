using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Models
{
    public class UserDatabaseMatch
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }

        [ForeignKey("CompanyApplicationId")]
        public Guid CompanyApplicationId { get; set; }
        public CompanyApplication CompanyApplication { get; set; }
        public string UserName { get; set; }
    }
}
