using ECommerceLiteBLL.Repostory;
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
        public void GetCategory()
        {
            if (CategoryId>0)
            {
                //orn elektronik categori > akıllı telefon > ürün(ıphone12)
                Category = myCategoryRepo.GetById(CategoryId);
                //akıllı telefon kategorisi elimde
                //akıllı telefon kategorisinin bir üst kategorisi var mı
                if (Category.BaseCategoryId != null && Category.BaseCategoryId > 0)
                {
                    Category.CategoryList = new List<Category>();
                    Category.BaseCategory = myCategoryRepo.GetById(Category.BaseCategoryId.Value);
                    Category.CategoryList.Add(Category.BaseCategory);
                    bool isOver = false;
                    Category baseCategory = Category.BaseCategory;
                    while (isOver)
                    {

                        if (baseCategory.BaseCategoryId>0)
                        {
                            Category.CategoryList.Add(myCategoryRepo.GetById(baseCategory.BaseCategoryId.Value));
                            baseCategory = myCategoryRepo.GetById(baseCategory.BaseCategoryId.Value);
                        }
                        else
                        {
                            isOver = true;
                        }
                    }
                       Category.CategoryList= Category.CategoryList.OrderBy(x => x.Id).ToList();
                }
            }
        }

    }
}