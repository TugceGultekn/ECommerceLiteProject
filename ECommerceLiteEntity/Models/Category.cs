using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.Models
{
    [Table("Categories")]
  public  class Category : Base<int>
    {
        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı 2 ile 100 karakter arasında olmalı!")]
        [Display(Name = "Kategori Adı")]
        public string CategoryName { get; set; }
        [StringLength(500, MinimumLength =2, ErrorMessage ="Kategori Açıklaması 2 ile 500 karakter arasında olmalı")]
        [Display(Name = "Kategori Açıklaması")]
        public string CategoryDescription { get; set; }

        public int? BaseCategoryId { get; set; } //int normalde null deger almaz yanına ? koyarsan alır.
        //[ForeignKey("BaseCategoryId")]
        //public virtual Category BaseCategory { get; set; }
        //public virtual List<Category> CategoryList { get; set; }

        // Her Ürünün bir kategorisi olur cumlesinden yola çıkarak Productta tanımlanan ilişkiyi burada karşılayalım.
        // 1e sonsuz ilişki nedeniyle bir kategorinin birden çok ürünü olabilir mantıgını karşılamak amacıyla burada
        //virtual prop list tipindedir.
        public virtual List<Product> ProductList { get; set; }
    }
}
