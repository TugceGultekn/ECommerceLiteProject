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
using Microsoft.Owin.Security;

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
            //giriş yapmış biri olarak giriş yapma sayfası gelmesin home ındex gelsin
            if (MembershipTools.GetUser() != null)
            {
                return RedirectToAction("Index", "Home");
            }
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

                // aktivasyon kodu üretelim

                var activationCode = Guid.NewGuid().ToString().Replace("-", "");
                // artık sisteme kayıt olabilir.

                var newUser = new ApplicationUser()
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    UserName = model.Email,
                    ActivationCode=activationCode
                };
               
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
                     $"<a href='{siteURL}/Account/Activation?" +
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
                // loglama yapılacak.

                ModelState.AddModelError("", "Beklenmedik bir hata oluştu. Tekrar deneyin");
                return View(model);
               
            }
        }

        [HttpGet]
        public async  Task<ActionResult> Activation(string code)
        {
            try
            {
                var user =
                    myUserStore.Context.Set<ApplicationUser>()
                    .FirstOrDefault(x => x.ActivationCode == code);
                if (user==null)
                {
                    ViewBag.ActivationResult = "Aktivasyon işlemi başarısız. Sistem yöneticisinden yeniden isteyin.";
                    return View();
                }
                //user bulundu.
                if (user.EmailConfirmed) //zaten aktifleşmiş mi?
                {
                    ViewBag.ActivationResult = "Aktivasyon işleminiz zaten gerçekleşmiştir. Giriş yaparak sistemi kullanabilirsiniz.";
                    return View();
                }
                user.EmailConfirmed = true;
                await myUserStore.UpdateAsync(user);
                await myUserStore.Context.SaveChangesAsync();
                //bu kişi artık aktif
                PassiveUser passiveUser = myPassiveUserRepo.AsQueryable().FirstOrDefault(x => x.UserId == user.Id);
                if (passiveUser!= null)
                {
                    passiveUser.IsDeleted = true;
                    myPassiveUserRepo.Update(passiveUser);
                        

                    Customer customer = new Customer()
                    {
                        UserId=user.Id,
                        TCNumber= passiveUser.TCNumber,
                        IsDeleted=false,
                        LastActiveTime=DateTime.Now
                    };
                   await myCustomerRepo.InsertAsync(customer);
                    //Aspnetuser tablosuna da bu kişinin customer oldugunu bildirmemiz gerek.
                    myUserManager.RemoveFromRole(user.Id, Roles.Passive.ToString());
                    myUserManager.AddToRole(user.Id, Roles.Customer.ToString());
                    //işlem bitti başarılı olduguna dair mesaj gönderelim
                    ViewBag.ActivationResult = $"Merhaba Sayın {user.Name} {user.Surname},aktifleştirme işleminiz başarılıdır. Giriş yapıp sistemi kullanabilirsiniz.";
                    return View();
                }
                return View();
            }
            catch (Exception ex)
            {
                //loglama yapılacak.
                ModelState.AddModelError("", "Beklenmedik hata oluştu");
                return View();
                
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult UserProfile()
        {
            //login olmuş kişinin id bilgisini alalım.
            var user = myUserManager.FindById(HttpContext.User.Identity.GetUserId());
            if (user!=null)
            {
                ProfileViewModel model = new ProfileViewModel()
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    //TCNumber = user.UserName
                };
                return View(model);
            }
            //user null ise (temkinli davrandık)
            ModelState.AddModelError("", "Beklenmedik bir sorun oluştu :( Giriş yapıp tekrar deneyin.");
            return View();




            //kişiyi bulacagız ve mevcut bilgilerini profilviewmodele atayıp sayfaya göndereceğiz.
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserProfile(ProfileViewModel model)
        {
            try
            {
                //sisteme kayıt olmuş ve login ile giriş  yapmış kişi hesabıma tıkladı.
                // bilgilerini gördü. bilgilerinde değişiklik yaptı. biz burada kontrol edeceğiz. yapılan değişiklikleri tespit edip
                // db mizi güncelleyeceğiz.
                var user = myUserManager.FindById(HttpContext.User.Identity.GetUserId());
                if (user==null)
                {
                    ModelState.AddModelError("", "mevcut kullanıcı bilgilerine ulaşılamadığı için işlem yapamıyoruz.");
                    return View(model);
                }
                // bir user herhangi bir bilgisini değiştirecekse parolasını bilmek zorunda 
                //bu nedenle model ile gelen parola dbdeki parola ile eşleşiyor mu diye bakmak lazım.
                if (myUserManager.PasswordHasher.VerifyHashedPassword(user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError("", "Mevcut şifrenizi yanlış girdiğiniz için bilgilerinizi güncelleyemedik. Lütfen tekrar deneyin.");
                    return View(model);
                }
                //başarılıysa yani parolayı doğru yazdı. bilgilerini güncelleyeceğiz.
                user.Name = model.Name;
                user.Surname = model.Surname;
                await myUserManager.UpdateAsync(user);
                ViewBag.Result = "Bilgileriniz güncellendi";
                var updateModel = new ProfileViewModel()
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    //TCNumber = user.UserName,
                    Email = user.Email
                };
                return View(updateModel);
            }
            catch (Exception ex)
            {

                //loglanacak
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu. Tekrar deneyiniz.");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult UpdatePassword()
        {   
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePassword(PasswordChangeViewModel model)
        {
            try
            {
                //mevcut login olmuş kişinin ıd sini veriyor. O id ile manager kişiyi dbden bulup getiriyor.
                var user = myUserManager.FindById(HttpContext.User.Identity.GetUserId());
                //Ya eski ve yeni girdiği şifre aynıysa?
                if (myUserManager.PasswordHasher.VerifyHashedPassword
                    (user.PasswordHash, model.NewPassword) == PasswordVerificationResult.Success)
                {
                    //bu kişi mevcut şifresinin aynısını yeni şifre olarak yutturmmaya çalışıyor.
                    ModelState.AddModelError("", "Yeni şifreniz mevcut şifrenizle aynı olamaz.");
                    return View(model);
                }

                //Yeni şifre ile şifre tekrarı uyuşuyor mu?
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Şifreler uyuşmuyor!");
                    return View(model);
                }
               
                //Acaba mevcut şifresini doğru yazdı mı?
                var checkCurrent = myUserManager.Find(user.UserName, model.Password);
                if (checkCurrent==null)
                {
                    //mevcut şifesini yanlış yazmış!
                    ModelState.AddModelError("", "mevcut şifrenizi yanlış girdiniz yeni şifre oluşturma işleminiz başarısız oldu. tekrar deneyin.");
                    return View(model);
                }
                //artık şifresini değiştirebilir.
                await myUserStore.SetPasswordHashAsync(user, myUserManager.PasswordHasher.HashPassword(model.NewPassword));
                await myUserManager.UpdateAsync(user);

                //şifre değiştirdikten sonra sistemden atalım!
                TempData["PasswordUpdated"] = "Parolanız değiştirildi.";
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Login", "Account",
                    new { email = user.Email });
            }
            catch (Exception ex)
            {

                // ex loglanacak
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu. Tekrar deneyin.");
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RecoverPassword(ProfileViewModel model)
        {
            try
            {
                // Şifresini unutmuş.
                
                var user = myUserStore.Context.Set<ApplicationUser>().FirstOrDefault(x => x.Email == model.Email);
                
               
                if (user==null)
                {
                    ViewBag.RecoverPassword = "Sistemde böyle bir kullanıcı olmadıgı için size yeni bir şifre gönderemiyoruz.Lütfen önce sisteme kayıt olun.";
                    return View(model);
                }
                //random şifre oluştur.
                var randomPassword = CreateRandomNewPassword();
                await myUserStore.SetPasswordHashAsync(user, myUserManager.PasswordHasher.HashPassword(randomPassword));
                await myUserStore.UpdateAsync(user);
                //email gönderilecek.
                //site adresini alıyoruz.
                var siteURL = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                await SiteSettings.SendMail(new MailModel()
                {
                    To = user.Email,
                    Subject = "ECommerceLite - Şifre Yenilendi!",
                    Message = $"Merhaba {user.Name} { user.Surname}," +
                 $"<br/> Yeni şifreniz:<b> {randomPassword}</b>Sisteme Giriş yapmak için<b>" +
                 $"<a href='{siteURL}/Account/Login?" +
                 $"email={user.Email}'> BURAYA </a></b> tıklayınız..."

                });
                //işlemler bitti
                ViewBag.RecoverPassword = " Email adresinize şifre gönderilmiştir.";
                return View();


            }
            catch (Exception ex)
            {

                // ex loglanacak
                ViewBag.RecoverPasswordResult = "Sistemsel bir hata oluştu. Tekrar deneyin";
                return View(model);
            }
        }


        [HttpGet]
        public ActionResult Login(string ReturnUrl, string email)
        {
            try
            {
                //giriş yapmış biri olarak giriş yapma sayfası gelmesin home ındex gelsin
                if (MembershipTools.GetUser()!=null)
                {
                    return RedirectToAction("Index", "Home");
                }
                //To Do: Sayfa patlamazsa if kontrolüne gerek yok. test ederken bakılacak.
                var model = new LoginViewModel()
                {
                    ReturnUrl = ReturnUrl,
                    Email = email
                };
                return View(model);
            }
            catch (Exception ex)
            {

                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var user = await myUserManager.FindAsync(model.Email, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email ya da şifrenizi yanlış girdiniz.");
                    return View(model);
                }
                //Userı buldu ama rolü pasif ise sisteme giremesin.
                if (user.Roles.FirstOrDefault().RoleId == myRoleManager.FindByName(Enum.GetName(typeof(Roles), Roles.Passive)).Id)
                {
                    ViewBag.Result = "Sistemi kullanmak için aktivasyon yapmanız gerekmektedir. Emailinize gödnerilen aktivasyon linkine tıklanıyınız.";
                    return View(model);
                }
                //artık login olabilir.

                var authManager = HttpContext.GetOwinContext().Authentication;
                var userIdentity = await myUserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                authManager.SignIn(new AuthenticationProperties()
                {IsPersistent = model.RememberMe}, userIdentity
                  );
                ////2. yol
                //AuthenticationProperties authProperties = new AuthenticationProperties();
                //authProperties.IsPersistent = model.RememberMe;
                //authManager.SignIn(authProperties, userIdentity);

                // Giriş yaptı. peki nereye gidecek. 
                //Herkes rolüne uygun sayfaya gidecek.
                if (user.Roles.FirstOrDefault().RoleId== myRoleManager.FindByName(Enum.GetName(typeof(Roles),Roles.Admin)).Id)
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                if (user.Roles.FirstOrDefault().RoleId == myRoleManager.FindByName(Enum.GetName(typeof(Roles), Roles.Customer)).Id)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (string.IsNullOrEmpty(model.ReturnUrl))
                {
                    return RedirectToAction("Index", "Home");
                }
                //Returnurl dolu ise
                var url = model.ReturnUrl.Split('/'); //stringi split ettik
                if (url.Length==4)
                {
                    return RedirectToAction(url[2], url[1], new { id = url[3] });
                }
                else
                {
                    //Örn: return redirecttoaction("userprofile","Account");
                    return RedirectToAction(url[2], url[1]);
                }

            }
            catch (Exception ex ) 
            {

                ModelState.AddModelError("", "Beklenmedik hata oluştu tekrar deneyin");
                return View(model);
            }
        }
    




        [Authorize]
        public ActionResult Logout()
        {
            Session.Clear();
            var user = MembershipTools.GetUser();
            HttpContext.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Login", "Account", new { email=user.Email});
        }

    }
}