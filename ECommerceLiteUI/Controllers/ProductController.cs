using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceLiteUI.Controllers
{
    public class ProductController : Controller
    {

        //bu controllera admin gibi yetkili kişiler erişecek. burada ürünlerin listelenmesi. ekleme silme güncelleme işlemleri
        //yapılacaktır.
        public ActionResult ProductList()
        {
            return View();
        }
    }
}