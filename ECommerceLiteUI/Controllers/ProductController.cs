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
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        CategoryRepo mycategoryRepo = new CategoryRepo();
        ProductRepo myproductRepo = new ProductRepo();
        ProductPictureRepo myproductPictureRepo = new ProductPictureRepo();
        private const int pageSize = 5;

        //bu controllera admin gibi yetkili kişiler erişecek. burada ürünlerin listelenmesi. ekleme silme güncelleme işlemleri
        //yapılacaktır.
        public ActionResult ProductList(int? page=1,string search="")
        {
           //Alt kategorileri repo aracılıgıyla dbden çektik.
            ViewBag.SubCategories = mycategoryRepo.AsQueryable().Where(x => x.BaseCategoryId != null).ToList();
            // sayfaya bazı bilgiler göndereceğiz.
            var totalProduct = myproductRepo.GetAll().Count; //toplam ürün sayısı
            ViewBag.TotalPages = (int)Math.Ceiling(totalProduct / (double)pageSize); // toplam urun sayısını sayfaya göndereceğiz
            ViewBag.TotalProduct = totalProduct;//toplam urun sayfada gosterilecek üründen kaç   sayfa oldugu bilgisi

            ViewBag.PageSize = pageSize; //her sayfada kaç ürün gözükecek bilgisini html sayfasına gönderelim

            //frenleme alttaki sayfalar sonsuza gidiyor çünkü
            //1.yontem
            if (page<1)
            {
                page = 1;
            }
            if (page>ViewBag.TotalPages)
            {
                page = ViewBag.TotalPages;
            }



            //2.yontem

            page = page < 1 ? //eğer birden küçükse
                1 : // page in değerini 1 yap
                page > ViewBag.TotalPages ? // değilse bak bakalım page toplam sayfadan büyük mü
                ViewBag.TotalPages : //page değeri = toplam sayfa
                page; // page e dokunma page aynı değerinden devam etsin.




            ViewBag.CurrentPage = page; //viewde kaçıncı sayfada oldugum bilgisini tutsn.
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
            // paging 1.yontem en klasik yontem.
            allproduct = allproduct.Skip(
                (page.Value < 1 ? 1 : page.Value - 1)
                 * pageSize
                )
                .Take(pageSize)  //5 tane al neden 5? çünkü yukarıdaki pagesize 5e eşitlenmiş.
                .ToList();
           

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
                if (model.CategoryId<=0 )
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

                Product product = new Product()
                {
                    ProductName = model.ProductName,
                    Description = model.Description,
                    ProductCode = model.ProductCode,
                    CategoryId = model.CategoryId,
                    Discount = model.Discount,
                    Quantity = model.Quantity,
                    RegisterDate = DateTime.Now,
                    Price = model.Price
                };
                //mapleme yapıldı.
                //mapster paketi indirildi. Mapster bir objeyi diğer objeye zahmetsizce aktarır. aktarım yapabilmesi için 
                //a objesi ile b objesinin içindeki propertylerin isimleri ve tipleri birebir aynı olmalıdır.bu projede 
                //mapster kullandık. Core projesinde daha profesyonel olan automapper ı kullanacağız.
                //Bir dto objesinin içindeki verileri alır asıl objenin içine aktarır.
                //asıl objenın verilerini dto objesinin içindeki propertylerine aktarır.
                //Product product = model.Adapt<Product>();
                //Product product2 = model.Adapt<ProductViewModel, Product>(); //ikinci versiyon
                int insertResult = myproductRepo.Insert(product);
                if (insertResult>0)
                {
                    //sıfırdan buyukse tabloya eklendi.
                    //Acaba bu producta resim seçmiş mi ? Resim seçtiyse o resimlerin yollarını kayıt et.
                    if (model.Files.Any() && model.Files[0] != null)
                    {
                        int pictureinsertResult = 0;
                        foreach (var item in model.Files)
                        {
                            if (item != null && item.ContentType.Contains("image") && item.ContentLength>0)
                            {
                                string productName = SiteSettings.StringCharacterConverter(model.ProductName).ToLower().Replace("-", "");
                                string extensionName = Path.GetExtension(item.FileName);
                                //klasor adresi: ProductPicturs/iphone13/202020
                                string directoryPath = Server.MapPath($"~/ProductPictures/{productName}/{model.ProductCode}");
                                string guid = Guid.NewGuid().ToString().Replace("-", "");
                                //productpicturs/iphone13/202020/iphone13-guid.jpg
                                string filePath = Server.MapPath($"~/ProductPictures/{productName}/{model.ProductCode}" +
                                    $"{productName}-{guid}{extensionName}");
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }
                                //resmi o klasore kayıt edelim
                                item.SaveAs(filePath);
                                //işlem bitti db ye kayıt olacak
                                ProductPicture picture = new ProductPicture()
                                {
                                    ProductId = product.Id,
                                    RegisterDate = DateTime.Now,
                                    Picture = $"ProductPictures/{productName}/{model.ProductCode}/" +
                                    $"{productName}-{guid}{extensionName}",

                                };
                                 pictureinsertResult += myproductPictureRepo.Insert(picture);
                            }
                        }
                        // pictureinsertResult kontrol edilecektir.
                        if (pictureinsertResult > 0 && model.Files.Count== pictureinsertResult)
                        {
                            // bütün resimler eklenmiş.
                            TempData["ProductInsertSuccess"] = "Yeni ürün eklenmiştir.";
                           return RedirectToAction("ProductList", "Product");
                        }
                        else if(pictureinsertResult > 0 && model.Files.Count!= pictureinsertResult)
                        {
                            // eksik eklemiş
                            TempData["ProductInsertWarning"] = "Yeni ürün eklendi ama resimlerden bazıları eklenemedi.";
                            return RedirectToAction("ProductList", "Product");
                        }
                        else
                        {
                            //ürünü ekledi ama resimi eklememiş.
                            TempData["ProductInsertWarning"] = "Yeni ürün eklendi ama resimler eklenemedi.";
                            return RedirectToAction("ProductList", "Product");
                        }
                    }
                    else
                    {
                        TempData["ProductInsertSuccess"] = "Yeni ürün eklenmiştir";
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

        public JsonResult GetProductDetails(int id)
        {
            try
            {
                var product = myproductRepo.GetById(id);
                if (product!=null)
                {
                    //var data = product.Adapt<ProductViewModel>();
                    var data = new ProductViewModel()
                    {
                        Id=product.Id,
                        ProductName = product.ProductName,
                        Description = product.Description,
                        ProductCode = product.ProductCode,
                        CategoryId = product.CategoryId,
                        Discount = product.Discount,
                        Quantity = product.Quantity,
                        RegisterDate = product.RegisterDate,
                        Price = product.Price
                    };
                    return Json(new { isSuccess=true,data},JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isSuccess = false });
                }
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false });

            }
        }

        public ActionResult Edit(ProductViewModel model)
        {
            try
            {
                var product = myproductRepo.GetById(model.Id);
                if (product!=null)
                {
                    product.ProductName = model.ProductName;
                    product.Description = model.Description;
                    product.Discount = model.Discount;
                    product.Quantity = model.Quantity;
                    product.ProductCode = model.ProductCode;
                    product.Price = model.Price;
                    product.CategoryId = model.CategoryId;
                    int updateResult = myproductRepo.Update(product);
                    if (updateResult>0)
                    {
                        TempData["EditSuccess"] = "Ürünler Güncellendi.";
                        return RedirectToAction("ProductList", "Product");
                    }
                    else
                    {
                        TempData["EditFailed"] = "Beklenmedik bir hata olduğu için, ürün bilgileri sisteme aktarılamadı";
                        return RedirectToAction("ProductList", "Product");
                    }
                }
                else
                {
                    TempData["EditFailed"] = "Ürün bulunamadıgı için, ürün bilgileri güncellenemedi";
                    return RedirectToAction("ProductList", "Product");
                }
            }
            catch (Exception ex)
            {

                TempData["EditFailed"] = "Beklenmedik hata nedediyle ürün bilgileri güncellenemedi";
                return RedirectToAction("ProductList", "Product");
            }
        }
    }
}