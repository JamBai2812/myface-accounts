using System.ComponentModel.DataAnnotations;

namespace MyFace.Authorization
{
    public class AuthenticateModel
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}