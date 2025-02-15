using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LocationTrackerAPI.Models.DTOS
{
    public class RegisterDTO
    {   [Required(ErrorMessage = "Username Required")]        
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password Required")]
        public string Password { get; set; } = string.Empty;
    }
}