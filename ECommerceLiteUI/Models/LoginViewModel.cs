using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECommerceLiteUI.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        //Authorize istenen bir action'a gitmek isterse sayfa login atacak kullanıcıyı.
        //kullanıcı bilgilerini girerse onu istediği Authorizeli sayfaya direk göndermek için gitmek istediği url bilgisini
        // bu proepertyde tutuyoruz.
        public string ReturnUrl { get; set; }
    }
}