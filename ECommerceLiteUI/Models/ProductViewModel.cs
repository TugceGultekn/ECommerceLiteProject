﻿using ECommerceLiteBLL.Repostory;
using ECommerceLiteEntity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ECommerceLiteUI.Models
{
    public class ProductViewModel
    {
        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductPictureRepo myProductPictureRepo = new ProductPictureRepo();

        public int Id { get; set; }
        public DateTime RegisterDate { get; set; }


        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 2, ErrorMessage = "Ürün 2 ile 100 karakter arasında olmalı!")]
        [Display(Name = "Ürün Adı")]
        public string ProductName { get; set; }
        [Required]
        [StringLength(maximumLength: 500, ErrorMessage = "Ürün açıklaması en fazla 500 karakter arasında olmalı!")]
        [Display(Name = "Ürün Açıklaması")]
        public string Description { get; set; }
        [Required]
        [StringLength(maximumLength: 8, MinimumLength = 8, ErrorMessage = "Ürün kodu en fazla 8 karakter arasında olmalı!")]
        [Display(Name = "Ürün Kodu")]
        [Index(IsUnique = true)] // Ürün kodunun benzersiz olmasını sağlar.
        public string ProductCode { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public string Price { get; set; }
        public string Quantity { get; set; }
        public double Discount { get; set; }

        // her ürünün bir kategorisi olur. ilişki kurduk
        public int CategoryId { get; set; }
        
        public Category Category { get; set; }
        public List<ProductPicture> ProductPictureList { get; set; }
        //ürün eklenirken ürüne ait resimler seçilebilir. Seçilen resimleri hafızada tutacak proporty
        public List<HttpPostedFileBase> Files { get; set; } =new List<HttpPostedFileBase>();

        public void GetProductPictures()
        {
            if (Id > 0)
            {
                ProductPictureList = myProductPictureRepo.AsQueryable().Where(x => x.ProductId == Id).ToList();

            }
          
        }
    }
}