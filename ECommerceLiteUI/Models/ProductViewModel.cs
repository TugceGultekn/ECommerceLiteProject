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
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public double Discount { get; set; }

        // her ürünün bir kategorisi olur. ilişki kurduk
        public int CategoryId { get; set; }
        private decimal _SalePrice;
        public decimal SalePrice // read only
        {
            get
            {
                //matematik: ürünün fiyatı - ürün fiyatı*indirim/100
                decimal _salePrice= Price -
                    (
                    Price * ((decimal) Discount/100)
                    );
                return _salePrice;
            }
        }

       
           

        public Category CategoryOfProduct { get; set; }
        public List<ProductPicture> PicturesOfProduct { get; set; }
        //ürün eklenirken ürüne ait resimler seçilebilir. Seçilen resimleri hafızada tutacak proporty
        public List<HttpPostedFileBase> Files { get; set; } = new List<HttpPostedFileBase>();

        public void GetProductPictures()
        {
            if (Id > 0)
            {
                PicturesOfProduct = myProductPictureRepo.AsQueryable().Where(x => x.ProductId == Id).ToList();

            }

        }
        public void GetCategory()
        {
            if (CategoryId > 0)
            {
                //ÖRN: Elektronik kat.--> Akıllı Telefon kat. --> ürün(iphone 13 pro max)
                CategoryOfProduct = myCategoryRepo.GetById(CategoryId);
                CategoryOfProduct.CategoryList = new List<Category>();
                // Akıllı telefon kat artık elimde!
                // Akıllı telefon kat. bir üst kategorisi var mı?
                // ÖRN: Elek--> Akkıl tel --> applegiller
                if (CategoryOfProduct.BaseCategoryId != null
                    && CategoryOfProduct.BaseCategoryId > 0)
                {
                    CategoryOfProduct.BaseCategory = myCategoryRepo.GetById
                        (CategoryOfProduct.BaseCategoryId.Value);
                    CategoryOfProduct.CategoryList.Add(CategoryOfProduct.BaseCategory);

                    bool isOver = false;
                    Category currentBaseCategory = CategoryOfProduct.BaseCategory;
                    while (!isOver)
                    {
                        if (currentBaseCategory.BaseCategoryId != null
                            && currentBaseCategory.BaseCategoryId > 0)
                        {
                            // mevcuttaki ana kategorinin üst kategorisi varmış
                            // onu alalım
                            currentBaseCategory = myCategoryRepo.GetById(currentBaseCategory.BaseCategoryId.Value);
                            CategoryOfProduct.CategoryList.Add(currentBaseCategory);
                        }
                        else
                        {
                            isOver = true;
                        }
                    }
                }
            }
        }
    }
}