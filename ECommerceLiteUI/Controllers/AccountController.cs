using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECommerceLiteBLL.Account;
using ECommerceLiteBLL.Repostory;
using ECommerceLiteBLL.Setting;
using ECommerceLiteEntity.IdentityModels;
using ECommerceLiteEntity.Models;
using ECommerceLiteUI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ECommerceLiteEntity.Enums;
using System.Threading.Tasks;
using ECommerceLiteEntity.ViewModels;

namespace ECommerceLiteUI.Controllers
{
    public class AccountController : BaseController
    {
        //global alan
        //not: bir sonraki projede repoları UI ın içinde newlemeyeceğiz.
        //çünkü bu bağımlılık oluşturur. bir sonraki projede bağımlılıkları tersine çevirme işlemi olarak bilinen
        //Dependency Injection işlemleri yapacağız.

        CustomerRepo myCustomerRepo = new CustomerRepo();
        PassiveUserRepo myPassiveUserRepo = new PassiveUserRepo();
        UserManager<ApplicationUser> myUserManager = MembershipTools.NewuserManager();
        UserStore<ApplicationUser> myUserStore = MembershipTools.NewuserStore();
        RoleManager<ApplicationRole> myRoleManager = MembershipTools.NewRoleManager();

        [HttpGet]
        public ActionResult Register()
        {
            //Kayıt ol sayfası
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid) // model validasyonları sağladı mı?
                {
                    return View(model);
                }
                var checkUserTC = myUserStore.Context.Set<Customer>()
                    .FirstOrDefault(x => x.TCNumber == model.TCNumber)?.TCNumber; 
                if(checkUserTC!= null)
                {
                    ModelState.AddModelError("", "Bu TC ile daha önceden kayıt yapılmıştır.");
                    return View(model);
                }
                var checkUserEmail = myUserStore.Context.Set<ApplicationUser>()
                    .FirstOrDefault(x => x.Email == model.Email)?.Email;
                if (checkUserEmail!=null)
                {
                    ModelState.AddModelError("", "Bu email ile daha önceden kayıt yapılmıştır.");
                    return View(model);
                }

                // artık sisteme kayıt olabilir.
                var newUser = new ApplicationUser()
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    UserName = model.TCNumber
                };
                // aktivasyon kodu üretelim

                var activationCode = Guid.NewGuid().ToString().Replace("-", "");
                //artık ekleyelim.
                var createResult = myUserManager.CreateAsync(newUser, model.Password);

                if (createResult.Result.Succeeded)
                {
                    //görev başarıyla tamamlandıysa kişi aspnetusers tablosuna eklenmiştir.
                    // Yeni kayıt oldugu için bu kişiye pasif rol verilecek.
                    // Kişi emailine gelen aktivasyon koduna tıklarsa customer olabilir.

                   await myUserManager.AddToRoleAsync(newUser.Id, Roles.Passive.ToString());
                    PassiveUser mypassiveUser = new PassiveUser()
                    {
                        UserId = newUser.Id,
                        TCNumber = model.TCNumber,
                        IsDeleted = false,
                        LastActiveTime = DateTime.Now
                    };
                    await myPassiveUserRepo.InsertAsync(mypassiveUser);
                    //email gönderilecek.
                    //site adresini alıyoruz.
                    var siteURL = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                    await SiteSettings.SendMail(new MailModel()
                    {
                     To= newUser.Email,
                     Subject="ECommerceLite Site Aktivasyon Emaili",
                     Message=$"Merhaba {newUser.Name} { newUser.Surname},"+
                     $"<br/> Hesabınızı aktileştirmek için <b>" +
                     $"<a href='{siteURL}/Account/Activasyon?" +
                     $"code={activationCode}'> Aktivasyon Linkine </a></b> tıklayın"

                    });
                    //işlemler bitti
                    return RedirectToAction("Login", "Account", new { email = $"{newUser.Email}" });
                }
                else
                {
                    ModelState.AddModelError("", "Kayıt işleminde beklenmedik bir hata oluştu");
                    return View(model);
                }



            }
            catch (Exception ex)
            {

               
            }
        }
    }
}