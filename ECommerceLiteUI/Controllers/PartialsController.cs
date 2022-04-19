using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECommerceLiteBLL.Repostory;

namespace ECommerceLiteUI.Controllers
{
    public class PartialsController : Controller
    {
        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductRepo myProductRepo = new ProductRepo();
        public PartialViewResult AdminSideBarResult()
        {
            return PartialView("_PartialAdminSideBar");
        }
        public PartialViewResult AdminSideBarMenuResult()
        {
            TempData["CategoryCount"] = myCategoryRepo.GetAll().Count();
            return PartialView("_PartialAdminSideBarMenu");
        }
        public PartialViewResult AdminSideBarProduct()
        {
            TempData["ProductCount"] = myCategoryRepo.GetAll().Count();
            return PartialView("_PartialAdminSideBarProduct");
        }
    }
}