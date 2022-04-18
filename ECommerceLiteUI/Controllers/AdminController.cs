using ECommerceLiteBLL.Repostory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceLiteUI.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        OrderRepo myOrderRepo = new OrderRepo();
     
        public ActionResult Dashboard()
        {
            //bu ayda oluşturulan siparişlerin sayısını ekranda görelim.
            var orderlist = myOrderRepo.GetAll();
            var currentMontlyOrderCount = orderlist.Where(x => x.RegisterDate >= DateTime.Now.AddMonths(-1)).ToList().Count;
            ViewBag.NewOrderCount = currentMontlyOrderCount;
            return View();
        }
        public ActionResult Orders()
        {
            return View();
        }
    }
}