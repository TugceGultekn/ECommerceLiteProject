using ECommerceLiteBLL.Account;
using ECommerceLiteEntity.Enums;
using ECommerceLiteEntity.IdentityModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ECommerceLiteUI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //NOT: Application_Start :
            //Uygulama ilk kez çalıştırıldığında bir defaya mahsus olmak üzere çalışır.
            //Bu nedenle ben uyg. ilk kez çalıştığında DB'de Roller ekli mi diye bakmak istiyorum.
            //Ekli değilse rolleri Enum'dan çağırıp ekleyelim
            //Ekli ise bişey yapmaya gerek kalmıyor.

            //adım 1: Rollere bakacağım şey --> Role Manager
            var myRoleManager = MembershipTools.NewRoleManager();
            //adım 2: Rollerin isimlerini almak (ipucu --> Enum)
            var allRoles = Enum.GetNames(typeof(Roles));
            //adım 3: Bize gelen diziyi tek tek tek döneceğiz (döngü)

            foreach (var item in allRoles)
            {
                //adım 4: Acaba bu rol DB'de ekli mi? 
                if (!myRoleManager.RoleExists(item)) // Eğer bu role ekli değilse? myRoleManager.RoleExists(item)==false
                {
                    //Adım 5: Rolü ekle!
                    //1.yol
                    ApplicationRole role = new ApplicationRole()
                    {
                        Name = item,
                        IsDeleted=false
                    };
                    myRoleManager.Create(role);
                    //2.yol
                    //myRoleManager.Create(new ApplicationRole()
                    //{
                    //    Name = item
                    //});

                }
            }
        }
        protected void Application_Error()
        {
            //NOT: ihtiyacım olursa internetten Global.asax'ın metotlarına bakıp kullanabilirim
            //ÖRN: Application_Error : Uygulama içinde istenmeyen bir hata meydana geldiğinde çalışır. Bu metodu yazarsak o hatayı loglayıp sorunu çözebiliriz.

            Exception ex = Server.GetLastError();
            //ex loglanacak
        }
      
    }
}
