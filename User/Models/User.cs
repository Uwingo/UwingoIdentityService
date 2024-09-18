using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Entity.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        
        [ForeignKey("ApplicationId")]
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; }
        [JsonIgnore]
        public ICollection<UserRole> UserRoles { get; set; }
    }

}