using System.ComponentModel.DataAnnotations;

namespace Project19.AuthContactApp
{
    public class UserLogin
    {
        [Required, MaxLength(20)]
        public string? LoginProp { get; set; } 

        [Required, DataType(DataType.Password)]
        public string? Password { get; set; } 

        public string? ReturnUrl { get; set; } 
    }
}
