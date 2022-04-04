using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.Models
{
    [Table("Products")]
   public class Product :Base<int>
    {
        [Required]
        [StringLength(maximumLength:100, MinimumLength =2 , ErrorMessage ="Ürün 2 ile 100 karakter arasında olmalı!")]
        [Display(Name ="Ürün Adı")]
        public  string ProductName { get; set; }
        [Required]
        [StringLength(maximumLength: 500, ErrorMessage = "Ürün açıklaması en fazla 500 karakter arasında olmalı!")]
        [Display(Name = "Ürün Açıklaması")]
        public string Description { get; set; }
        [Required]
        [StringLength(maximumLength: 8, MinimumLength =8, ErrorMessage = "Ürün kodu en fazla 8 karakter arasında olmalı!")]
        [Display(Name = "Ürün Kodu")]
        [Index(IsUnique =true)] // Ürün kodunun benzersiz olmasını sağlar.
        public string ProductCode { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public string Price { get; set; }
        public string Quantity { get; set; }
        public double Discount { get; set; }

        // her ürünün bir kategorisi olur. ikişki kurduk
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual List<OrderDetails> OrderDetails { get; set; }
        public virtual List<ProductPicture> ProductPictures { get; set; }
    }
}
