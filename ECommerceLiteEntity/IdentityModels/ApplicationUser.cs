using ECommerceLiteEntity.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.IdentityModels
{
   public class ApplicationUser : IdentityUser
    {
        //IDentityUser'dan kalıtım alındı. IdentityUser Microsoftun ıdentity şemasına ait bir classtır.
        // Identityuser classı ile bize sunulan AspnetUsers tablosundaki kolonları  genişletmek için kalıtım aldık.
        // Aşağıya ihtiyacımız olan kolonları ekledk.
        [Required]
        [Display(Name = "Ad")]
        [StringLength(maximumLength: 30, MinimumLength = 2, ErrorMessage = "İsminizin" +
                " uzunluğu 2 ile 30 karakter aralığında olmalıdır!")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        [StringLength(maximumLength: 30, MinimumLength = 2, ErrorMessage = "Soyisminizin" +
           " uzunluğu 2 ile 30 karakter aralığında olmalıdır!")]
        public string Surname { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        public string ActivationCode { get; set; }
        public bool IsDeleted { get; set; } = false;

        public virtual List<Admin> AdminList { get; set; }
        public virtual List<Customer> CustomerList { get; set; }
        public virtual List<PassiveUser> PassiveUserList { get; set; }


    }
}
