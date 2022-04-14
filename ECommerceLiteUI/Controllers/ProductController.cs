using ECommerceLiteBLL.Repostory;
using ECommerceLiteBLL.Setting;
using ECommerceLiteEntity.Models;
using ECommerceLiteUI.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace ECommerceLiteUI.Controllers
{
    public class ProductController : Controller
    {
        CategoryRepo mycategoryRepo = new CategoryRepo();
        ProductRepo myproductRepo = new ProductRepo();
        ProductPictureRepo myproductPictureRepo = new ProductPictureRepo();

        //bu controllera admin gibi yetkili kişiler erişecek. burada ürünlerin listelenmesi. ekleme silme güncelleme işlemleri
        //yapılacaktır.
        public ActionResult ProductList(string search="")
        {
            List<Product> allproduct = new List<Product>();
           
            //return View(allproduct);
            if (string.IsNullOrEmpty(search))
            {
                allproduct = myproductRepo.GetAll();
            }
            else
            {
                allproduct = myproductRepo.GetAll().Where(x => x.ProductName.ToLower().
               Contains(search.ToLower()) || x.Description.ToLower().Contains(search.ToLower())).ToList();
            }
            return View(allproduct);
        }
        [HttpGet]
        
        public ActionResult Create()
        {
           
            //sayfayı çağırırken ürünün kategorisinin ne oldugunu seçmesi lazım bu yüzden sayfaya kategoriler gitmeli.
            List<SelectListItem> subCategories = new List<SelectListItem>();
            //linq
            mycategoryRepo.AsQueryable().Where(x => x.BaseCategoryId != null)
                .ToList().ForEach(x => subCategories.Add(

                    new SelectListItem()
                    {
                        Text = x.CategoryName,
                        Value = x.Id.ToString()
                    }));
            ViewBag.SubCategories = subCategories;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductViewModel model)
        {
            try
            {
                List<SelectListItem> subCategories = new List<SelectListItem>();
                //linq
                mycategoryRepo.AsQueryable().Where(x => x.BaseCategoryId != null)
                    .ToList().ForEach(x => subCategories.Add(

                        new SelectListItem()
                        {
                            Text = x.CategoryName,
                            Value = x.Id.ToString()
                        }));
                ViewBag.SubCategories = subCategories;

                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "Veri girişleri düzgün olmalıdır.");
                    return View();
                }
                if (model.CategoryId<=0 || model.CategoryId >mycategoryRepo.GetAll().Count())
                {
                    ModelState.AddModelError("", "Ürüne ait kategori seçilmelidir.");
                    return View(model);
                }
                //burada kontrol lazım
                //acaba girdiği ürün kodu bizim db de zaten var mı?
                //metotsuz
                if (myproductRepo.IsSameProductCode(model.ProductCode))
                {
                    ModelState.AddModelError("", "Dikkat! Girdiğiniz ürün kodu sistemdeki bir başka ürüne aittir. Ürün kodları aynı olamaz");
                    return View(model);
                }





                //ürün tabloya kayıt olacak.
                //to do mapleme yapılacak

                //Product product = new Product()
                //{
                //    ProductName = model.ProductName,
                //    Description = model.Description,
                //    ProductCode = model.ProductCode,
                //    CategoryId = model.CategoryId,
                //    Discount = model.Discount,
                //    Quantity = model.Quantity,
                //    RegisterDate = DateTime.Now,
                //    Price = model.Price
                //};
                //mapleme yapıldı.
                //mapster paketi indirildi. Mapster bir objeyi diğer objeye zahmetsizce aktarır. aktarım yapabilmesi için 
                //a objesi ile b objesinin içindeki propertylerin isimleri ve tipleri birebir aynı olmalıdır.bu projede 
                //mapster kullandık. Core projesinde daha profesyonel olan automapper ı kullanacağız.
                //Bir dto objesinin içindeki verileri alır asıl objenin içine aktarır.
                //asıl objenın verilerini dto objesinin içindeki propertylerine aktarır.
                Product product = model.Adapt<Product>();
                //Product product2 = model.Adapt<ProductViewModel, Product>(); //ikinci versiyon
                int insertResult = myproductRepo.Insert(product);
                if (insertResult>0)
                {
                    //sıfırdan buyukse tabloya eklendi.
                    //Acaba bu producta resim seçmiş mi ? Resim seçtiyse o resimlerin yollarını kayıt et.
                    if (model.Files.Any())
                    {
                        ProductPicture productPicture = new ProductPicture();
                        productPicture.ProductId = product.Id;
                        productPicture.RegisterDate = DateTime.Now;
                        int counter = 1; // bizim sistemde resim adedi 5 olarak belirlendği için
                        foreach (var item in model.Files)
                        {
                            if (counter == 5) break;
                            if (item != null && item.ContentType.Contains("image") && item.ContentLength>0)
                            {
                                string filename = SiteSettings.StringCharacterConverter(model.ProductName).ToLower().Replace("-", "");
                                string exstensionName = Path.GetExtension(item.FileName);
                                string directoryPath = Server.MapPath($"~/ProductPictures/{filename}/{model.ProductCode}");
                                string guid = Guid.NewGuid().ToString().Replace("-", "");
                                string filePath = Server.MapPath($"~/ProductPictures/{filename}/{model.ProductCode}/") 
                                    +"-"+ filename +"-"+ counter +"-"+ guid + exstensionName;
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }
                                item.SaveAs(filePath);
                                //todo: Fazla if kullanıldı başka türlüde olabilirdi düşünülmedi.
                                if (counter==1)
                                {
                                    productPicture.ProductPicture1=$"/ProductPictures/{filename}/{model.ProductCode}/" 
                                          + filename + "-" + counter + "-" + guid + exstensionName;

                                }
                                if (counter == 2)
                                {
                                    productPicture.ProductPicture2 = $"/ProductPictures/{filename}/{model.ProductCode}/" 
                                          + filename + "-" + counter + "-" + guid + exstensionName;

                                }
                                if (counter == 3)
                                {
                                    productPicture.ProductPicture3 = $"/ProductPictures/{filename}/{model.ProductCode}/" 
                                         + filename + "-" + counter + "-" + guid + exstensionName;

                                }
                            }
                            counter++;
                        }
                        //to do: yukarıyı fora dönüştürebilir miyiz?
                        //for (int i = 0; i < model.Files.Count; i++)
                        //{

                        //}
                        int productPictureInsertResult = myproductPictureRepo.Insert(productPicture);
                        if (productPictureInsertResult>0)
                        {
                            return RedirectToAction("ProductList", "Product");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Ürün eklendi ama ürüne ait fotoğraflar eklenirlen beklenmedik bir hata oluştu.");
                            return View(model);
                        }
                    }
                    else
                    {
                        return RedirectToAction("ProductList", "Product");
                            
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Hata: ürün ekleme işleminde bir hata oluştu.Tekrar deneyiniz.");
                    return View(model);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu.");
                //ex loglanacak
                return View(model);
            }
        }
    }
}