using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ECommerceLiteBLL.Repostory;
using ECommerceLiteUI.Models;
using Mapster;
using ECommerceLiteBLL.Account;
using ECommerceLiteEntity.Models;
using QRCoder;
using System.Drawing;
using ECommerceLiteEntity.ViewModels;
using ECommerceLiteBLL.Setting;

namespace ECommerceLiteUI.Controllers
{
    public class HomeController : BaseController
    {

        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductRepo myProductRepo = new ProductRepo();
        AdminRepo myAdminRepo = new AdminRepo();
        CustomerRepo myCustomerRepo = new CustomerRepo();
        OrderRepo myOrderRepo = new OrderRepo();
        OrderDetailRepo myOrderDetailRepo = new OrderDetailRepo();
        public ActionResult Index()
        {
            //Ana kategorilerden dördünü viewbag ile sayfaya gönderelim.
            var categoryList = myCategoryRepo.AsQueryable().Where(x => x.BaseCategoryId == null).Take(4).ToList();

            ViewBag.CategoryList = categoryList.OrderByDescending(x => x.Id).ToList();

            //ürünler

            var productList = myProductRepo.AsQueryable().Where(x => x.IsDeleted == false && x.Quantity >= 1).Take(10).ToList();
            List<ProductViewModel> model = new List<ProductViewModel>();
            //Mapster ile mapledik
            productList.ForEach(x =>
            {
                var item = x.Adapt<ProductViewModel>();
                item.GetCategory();
                item.GetProductPictures();
                model.Add(item);
            });

            //    foreach (var item in productList)
            //    {
            //        var product = new ProductViewModel()
            //        {
            //            Id = item.Id,
            //            CategoryId = item.CategoryId,
            //            ProductName = item.ProductName,
            //            Description = item.Description,
            //            Quantity = item.Quantity,
            //            Discount = item.Discount,
            //            RegisterDate = item.RegisterDate,
            //            Price = item.Price,
            //            ProductCode = item.ProductCode
            //        };
            //        product.GetCategory();
            //        product.GetProductPictures();
            //        model.Add(product);
            //    }

            return View(model);
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult AddToCard(int id)
        {
            try
            {
                //Session'a eklenecek Session oturum demektir.

                var shoppingCart = Session["ShoppingCart"] as List<ProductViewModel>;

                if (shoppingCart == null)
                {
                    shoppingCart = new List<ProductViewModel>();

                }
                if (id > 0)
                {
                    var product = myProductRepo.GetById(id);

                    if (product == null)
                    {
                        TempData["AddToCartFailed"] = "Ürün eklemesi başarısızdır. Lütfen tekrar deneyiniz.";
                        //product null geldi logla
                        return RedirectToAction("Index", "Home");

                    }
                    //Tamam ekleme yapılacak.
                    //ProductViewModel productAddToCart = product.Adapt<ProductViewModel>();
                    var productAddToCart = new ProductViewModel()
                    {
                        Id = product.Id,
                        ProductName = product.ProductName,
                        Description = product.Description,
                        CategoryId = product.CategoryId,
                        Discount = product.Discount,
                        Price = product.Price,
                        Quantity = product.Quantity,
                        RegisterDate = product.RegisterDate,
                        ProductCode = product.ProductCode
                    };
                    if (shoppingCart.Count(x => x.Id == productAddToCart.Id) > 0)
                    {
                        shoppingCart.FirstOrDefault(x => x.Id == productAddToCart.Id).Quantity++;
                    }
                    else
                    {
                        productAddToCart.Quantity = 1;
                        shoppingCart.Add(productAddToCart);
                    }
                    //Önemli--> Session'a bu lşsteyi atamak lazım.
                    Session["ShoppingCart"] = shoppingCart;

                    TempData["AddToCartSuccess"] = "Ürün sepete eklendi";
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    TempData["AddToCartFailed"] = "Ürün eklemesi başarısızdır. Lütfen tekrar deneyiniz.";
                    //loglama yap id düzgün gelmeli
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                //ex loglanacak
                TempData["AddToCartFailed"] = "Ürün eklemesi başşarısızdır. Lütfen tekrar deneyiniz.";
                return RedirectToAction("Index", "Home");
            }
        }


        [Authorize]
        public async Task<ActionResult> Buy()
        {
            try
            {
                //1) Eğer adminsen alışveriş yapamazsın.
                var user = MembershipTools.GetUser();
                var customer = myCustomerRepo.AsQueryable().FirstOrDefault(x => x.UserId == user.Id);
                if (customer == null)
                {
                    TempData["BuyFailed"] = "Alışveriş yapabilmeniz için müşteri bilgileriniz ile giriş yapmanız gerekmektedir";
                    return RedirectToAction("Index", "Home");
                }
                //2) Shoppingcart null mı değil mi?
                var shoppingcart = Session["ShoppingCart"] as List<ProductViewModel>;
                if (shoppingcart == null)
                {
                    TempData["BuyFailed"] = "Alışveriş yapabilmeniz sepetinize ürün ekleyin.";
                    return RedirectToAction("Index", "Home");
                }
                //3) Shoppingcart içinde ürün var mı?
                //if (shoppingcart.Count==0)
                //{
                //    TempData["BuyFailed"] = "Alışveriş yapabilmeniz sepetinize ürün ekleyin.";
                //    return RedirectToAction("Index", "Home");
                //}
                // Artık alışveriş tamamlansın.
                Order customerOrder = new Order()
                {
                    CustomerTCNumber = customer.TCNumber,
                    IsDeleted = false,
                    OrderNumber = customer.TCNumber // burayı düzelteceğiz.
                };
                //insert yapılsın.
                int orderInsertResult = myOrderRepo.Insert(customerOrder);
                if (orderInsertResult > 0)
                {
                    //siparişin detayları orderdetailse eklenmeli.
                    int orderDetailsInsertResult = 0;
                    foreach (var item in shoppingcart)
                    {
                        OrderDetails customerOrderDetail = new OrderDetails()
                        {
                            OrderId = customerOrder.Id,
                            IsDeleted = false,
                            ProductId = item.Id,
                            ProductPrice = item.Price,
                            Quantity = item.Quantity,
                            Discount = item.Discount

                        };
                        //TotalCount hesabı
                        if (item.Discount > 0)
                        {
                            customerOrderDetail.TotalPrice = customerOrderDetail.Quantity *
                                  (customerOrderDetail.ProductPrice -
                                  (customerOrderDetail.ProductPrice * (decimal)customerOrderDetail.Discount / 100));
                        }
                        else
                        {
                            //3* telefonun fiyatı
                            customerOrderDetail.TotalPrice = customerOrderDetail.Quantity * customerOrderDetail.ProductPrice;
                        }
                        // orderdetail tabloya insert edilsin
                        orderDetailsInsertResult += myOrderDetailRepo.Insert(customerOrderDetail);
                    }
                    //orderDetailsInsertResult sıfırdan büyükse
                    if (orderDetailsInsertResult > 0 && orderDetailsInsertResult == shoppingcart.Count)
                    {
                        //QR kodu eklenmiş email gönderilecek
                        #region SendOrderEmailWithQR
                        QRCodeGenerator myQRCodeGenerator = new QRCodeGenerator();
                        QRCodeData myQRCodeData = myQRCodeGenerator.CreateQrCode(customerOrder.OrderNumber, QRCodeGenerator.ECCLevel.Q);
                        QRCode myQRCode = new QRCode(myQRCodeData);
                        Bitmap QRBitmap = myQRCode.GetGraphic(60);

                        byte[] bitmapArray = BitmapToByteArray(QRBitmap);
                        string qrUri = string.Format("data:image/pgn;base64,{0}", Convert.ToBase64String(bitmapArray));
                        //emailde gidecek olan ürünleri listeye alalım.
                        List<OrderDetails> orderList = new List<OrderDetails>();
                        orderList = myOrderDetailRepo.AsQueryable().Where(x => x.OrderId == customerOrder.Id).ToList();
                        string message = $"Merhaba {user.Name} {user.Surname} </br>  </br>"
                            + $"{orderList.Count} adet ürünlerinizin siparişini aldık. </br>"
                            + $"Toplam tutar: {orderList.Sum(x => x.TotalPrice)} ₺ </br> </br>"
                            + $"Sipariş Numarası: {customerOrder.OrderNumber} </br> </br>"
                            + $"<table><tr><th>Ürün Adı</th><th>Adet</th><th>Birim Fiyat</th>"
                            + $"<th>İndirim</th><th>Toplam</th></tr>";
                        foreach (var item in orderList)
                        {
                            var product = myProductRepo.GetById(item.ProductId);
                            message += $"<tr><td>{product.ProductName}</td>"
                                + $"<td>{item.Quantity}</td>"
                                + $"<td>{item.ProductPrice} ₺</td>"
                                + $"<td>{item.Discount} %</td>"
                                + $"<td>{item.TotalPrice} ₺</td></tr>";

                        }

                        string siteUrl =
                     Request.Url.Scheme + Uri.SchemeDelimiter
                     + Request.Url.Host
                     + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);

                        message += $"<table></br> Siparişinize ait QR kodunuz:</br> </br>" +
                                     $"<a href='{siteUrl}/Home/Order/{customerOrder.Id}'>" +
                                    $"<img src=\"{qrUri}\" height=250px;  width=250px; class='img-thumbnail' /></a>";
                        await SiteSettings.SendMail(new MailModel()
                        {
                            To = user.Email,
                            Subject = "ECommerceLite - Siparişiniz alındı",
                            Message = message

                        });

                        TempData["BuySuccess"] = "Siparişiniz oluşturuldu. Sipariş numaranız:" + customerOrder.OrderNumber;
                        return RedirectToAction("Index", "Home");
                        #endregion
                    }
                    else
                    {
                        //sistem yöneticisine orderID detayı verilerek email gönderilsin eklenmeyen ürünleri
                        // acilen eklesinler
                        var message = $"Merhaba Admin, <br/>" +
                            $"Aşağıdaki bilgileri verilen siparişin kendisi oluşturulmasına rağmen detaylarından bazıları oluşturulamadı. Acilen müdahale edelim.</br> </br>" +
                            $"OrderId:{customerOrder.Id}</br>" +
                            $"Sipariş detayları </br>";
                        for (int i = 0; i < shoppingcart.Count; i++)
                        {
                            message += $"{i}-Id:{shoppingcart[i].Id}-" +
                                $"Birim Fiyat:{shoppingcart[i].Price}-" +
                                 $"Sipariş Adedi:{shoppingcart[i].Quantity}-" +
                                  $"İndirimi:{shoppingcart[i].Discount}-" +
                                  $"</br> </br>";

                        }

                        await SiteSettings.SendMail(new MailModel()
                        {
                            To = "nayazilim303@gmail.com",
                            Subject = "ACİL - ECommerceLite 303 Sipariş Detay Sorunu",
                            Message = message

                        });

                    }
                }
                return RedirectToAction("Index", "Home");


            }
            catch (Exception ex)
            {
                //ex loglanacak.
                TempData["BuyFailed"] = "Beklenmedik hata sebebiyle sipariş oluşturulamadı.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}