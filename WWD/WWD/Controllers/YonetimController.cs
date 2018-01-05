using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WWD.App_classes;
using WWD.Models;
namespace WWD.Controllers
{
    public class YonetimController : Controller
    {
        DonationDB ctx = new DonationDB();
        // GET: Yonetim

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Giris()
        {
            return View();
        }
        public ActionResult Kayit()
        {
            return View();
        }
        public ActionResult Reset()
        {
            return View();
        }
        //Login çalışıyor
        [HttpPost]
        public ActionResult Giris(Kullanici kl, string rememberMe)
        {
            bool sonuc = Membership.ValidateUser(kl.UserAdi, kl.Parola);
            if (sonuc)
            {
                if (rememberMe == "on")
                {
                    FormsAuthentication.RedirectFromLoginPage(kl.UserAdi, true);
                }
                else
                {
                    FormsAuthentication.RedirectFromLoginPage(kl.UserAdi, false);
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.mesaj = "Password or Username is not correct";
                return View("Giris");
            }

        }
        public ActionResult Cikis()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Resetle(Kullanici kl, string Parola2)
        {
            if (kl.Parola != Parola2)
            {
                ViewBag.mes = "Not Equal Passwords";
                return RedirectToAction("Reset");
            }
            else
            {
                MembershipUser mu = Membership.GetUser(kl.UserAdi);
                if (mu.PasswordQuestion == kl.GizliSoru)
                {
                    string pwd = mu.ResetPassword(kl.GizliCevap);
                    mu.ChangePassword(pwd, kl.Parola);
                    return RedirectToAction("Giris");
                }
                else
                {
                    ViewBag.mesaj = "Incorrect Informations";
                    return View();
                }
            }



        }

        [HttpPost]
        public ActionResult Kayit(Kullanici kl, string Parola2)
        {
            if (kl.Parola != Parola2)
            {
                ViewBag.mes = "Not Equal Passwords";
                return View();
            }
            else
            {
                MembershipCreateStatus durum;
                MembershipUser ASS = Membership.CreateUser(kl.UserAdi, kl.Parola, kl.Email, kl.GizliSoru, kl.GizliCevap, true, out durum);
                string mesaj = " ";
                switch (durum)
                {
                    case MembershipCreateStatus.Success:
                        break;
                    case MembershipCreateStatus.InvalidUserName:
                        mesaj += "Invalid User Name";
                        break;
                    case MembershipCreateStatus.InvalidPassword:
                        mesaj += "Invalid Password";
                        break;
                    case MembershipCreateStatus.InvalidQuestion:
                        mesaj += "Invalid Secret Question";
                        break;
                    case MembershipCreateStatus.InvalidAnswer:
                        mesaj += "Invalid Secret Answer";
                        break;
                    case MembershipCreateStatus.InvalidEmail:
                        mesaj += "Invalid E-mail";
                        break;
                    case MembershipCreateStatus.DuplicateUserName:
                        mesaj += "Already This User Name Exist";
                        break;
                    case MembershipCreateStatus.DuplicateEmail:
                        mesaj += "Already This E-mail Exist";
                        break;
                    case MembershipCreateStatus.UserRejected:
                        mesaj += "Rejected";
                        break;
                    case MembershipCreateStatus.InvalidProviderUserKey:
                        break;
                    case MembershipCreateStatus.DuplicateProviderUserKey:
                        break;
                    case MembershipCreateStatus.ProviderError:
                        break;
                    default:
                        break;
                }
                ViewBag.mesaj = mesaj;
                if (durum == MembershipCreateStatus.Success)
                {
                    Guid id = (Guid)Membership.GetUser(kl.UserAdi).ProviderUserKey;
                    Roles.AddUserToRole(kl.UserAdi, "UYE");
                    kl.ProfilFoto = null;
                    kl.IsAdmin = false;
                    kl.IsDonator = false;
                    kl.IsLeader = false;
                    kl.KullaniciId = id;
                    ctx.Kullanicis.Add(kl);
                    ctx.SaveChanges();
                    return RedirectToAction("Giris", "Yonetim");
                }
            }


            return View();
        }
        [Authorize(Roles = "Boss")]
        public ActionResult RolEkle()
        {
            ViewBag.rol = Roles.GetAllRoles().ToList();
            ViewBag.admin = Membership.GetAllUsers();
            return View();
        }
        [Authorize(Roles = "Boss")]
        [HttpPost]
        public ActionResult RolEle(string roladi)
        {
            Roles.CreateRole(roladi);
            return View("RolEkle");
        }
        [Authorize(Roles = "Boss")]
        [HttpPost]
        public ActionResult RolAta(string KullaniciAdi, string RolAdi)
        {
            Roles.AddUserToRole(KullaniciAdi, RolAdi);
            return View("RolEkle");
        }
        [Authorize(Roles = "Boss")]
        public ActionResult Bekleyenler()
        {
            ViewBag.bekleyenler = ctx.Kampanyas.Where(x => x.OnaylandiMi == false).ToList();
            return View();
        }
        [Authorize(Roles = "Boss")]
        public ActionResult DevamEdenler()
        {
            ViewBag.devamedenler = ctx.Kampanyas.Where(x => x.OnaylandiMi == true && x.BasariliMi == false).ToList();
            return View();
        }
        [Authorize(Roles = "Boss")]
        public ActionResult Bitenler()
        {
            ViewBag.bitenler = ctx.Kampanyas.Where(x => x.BasariliMi == true).ToList();
            return View();
        }
        [Authorize(Roles = "Boss")]
        public ActionResult Onaylandi(int id)
        {

            Kampanya ka = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
            ka.OnaylandiMi = true;
            ctx.SaveChanges();
            return RedirectToAction("Bekleyenler");

        }
        [Authorize(Roles = "Boss")]
        public ActionResult Rededildi(int id)
        {
            List<Stuff> st = ctx.Stuffs.Where(x => x.KampanyaID == id).ToList();
            List<Donate> dn = ctx.Donates.Where(x => x.KampanyaID == id).ToList();
            Kampanya ka = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
            if (ka != null)
            {
                for (int i = 0; i < st.Count(); i++)
                {
                    ctx.Stuffs.Remove(st[i]);


                    ctx.SaveChanges();
                }
                if (dn != null)
                {
                    for (int i = 0; i < dn.Count(); i++)
                    {
                        ctx.Donates.Remove(dn[i]);


                        ctx.SaveChanges();
                    }
                }

                ctx.Kampanyas.Remove(ka);
                ctx.SaveChanges();
                return RedirectToAction("Bekleyenler");
            }


            return RedirectToAction("Bekleyenler");

        }
        public ActionResult EditBekleyenler()
        {
            var mad = ctx.Kampanyas.Where(x => x.ResimEditMi == true|| x.StuffEditMi==true||x.AcikEditlendiMi==true).ToList();

            return View(mad);
        }
        public ActionResult EditBekleyenlerDetay(int id)
        {
            //     Edit edt = ctx.Edits.FirstOrDefault(x => x.KampanyaID == id);
            //     ViewBag.yeni = ctx.Stuffs.FirstOrDefault(x => x.StuffId == edt.StuffID).StuffName;
            //     ViewBag.yeni2 = ctx.Stuffs.FirstOrDefault(x => x.StuffId == edt.StuffID).Miktari;
            //     ViewBag.yeni3 = ctx.Stuffs.FirstOrDefault(x => x.StuffId == edt.StuffID).Birim;
            //     DegUrun dg = ctx.DegUruns.FirstOrDefault(x => x.EditID == edt.EditId);
            //
            //     //   int hg = dg.UrunID;
            //
            //     Stuff jk = ctx.Stuffs.FirstOrDefault(x => x.StuffId == dg.UrunID);
            //     //  string ff= 
            //     ViewBag.eski = jk.StuffName;
            //     ViewBag.eski2 = jk.Miktari;
            //     ViewBag.eski3 = jk.Birim;
            //     //Stuff ad = ctx.Stuffs.FirstOrDefault(x=>x.StuffId==DEG.UrunID);
            //     //ViewBag.eski = ad.StuffName;
            //     // ViewBag.eski = ctx.Stuffs.FirstOrDefault(edt.DegUruns.FirstOrDefault(y=>y.UrunID==stf.StuffId));
            //     var data = ctx.Edits.FirstOrDefault(x => x.KampanyaID == id);
            //     ViewBag.asd = ctx.Stuffs.Where(x => x.Kampanya.StuffID == data.StuffID);
            //    
            StuffEd ed = ctx.StuffEds.FirstOrDefault(y => y.KampanyaID == id);
            
            ViewBag.eskistuff = ctx.Stuffs.FirstOrDefault(x=>x.StuffId==ed.gizli).StuffName;
            ViewBag.eskistuff1 = ctx.Stuffs.FirstOrDefault(x => x.StuffId == ed.gizli).Miktari;
            ViewBag.eskistuff2 = ctx.Stuffs.FirstOrDefault(x => x.StuffId == ed.gizli).Birim;

            ViewBag.yeni = ed.StuffName;
            ViewBag.yeni2 = ed.Miktari;
            ViewBag.yeni3 = ed.Birim;


            AciklamaEd fg = ctx.AciklamaEds.FirstOrDefault(x=>x.KampanyaID==id);
            if (fg!=null)
            {
                ViewBag.acik = fg.Aciklamasi;
            }
           

            ResimEd dr = ctx.ResimEds.FirstOrDefault(x=>x.KampanyaID==id);
            if (dr!=null)
            {
                ViewBag.resim = dr.ResimYolK;

            }

            var data = ctx.Kampanyas.FirstOrDefault(x=>x.KampanyaId==id);
            return View(data);
        }
        public ActionResult AddedBekleyenler()
        {
            ViewBag.eklenenler = ctx.Kampanyas.Where(x => x.EklendiMi == true).ToList();
            return View();
        }
        //gereksiz yazdım sanırım
        [HttpPost]
        public ActionResult AddedBekleyenler(int id)
        {
            return View();
        }
        public ActionResult AddedBekleyenlerDetay(int id)
        {
              ViewBag.stfname = TempData["StuffName"];
              ViewBag.biri = TempData["Birim"];
              ViewBag.mik = TempData["Miktari"];
              ViewBag.res = TempData["ResimID"];
          
             List<string> da1 = ViewBag.stfname;
             List<string> da2 = ViewBag.biri;
             List<int> da3 = ViewBag.mik;
             List<HttpPostedFileBase> da4 = ViewBag.res;

              TempData["23"] = da1;
              TempData["24"] = da2;
              TempData["25"] = da3;
              TempData["26"] = da4;
         
          
           Kampanya ka = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
            return View(ka);
        }
        public ActionResult ADDOk1(int id)
        {
            Kampanya kl = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
           
            
          List<string> ade = (List<string>)TempData["23"];
          List<string> ade2 = (List<string>)TempData["24"];
          List<int> ade3 = (List<int>)TempData["25"];
            Stuff stf = new Stuff();
           
            for (int i = 0; i < ade.Count(); i++)
            {
                stf.KampanyaID = kl.KampanyaId;
                stf.StuffName = ade[i];
                stf.Birim = ade2[i];
                stf.Miktari = ade3[i];
                stf.KalanMiktar = 0;
                ctx.Stuffs.Add(stf);
                
            }

            List<HttpPostedFileBase> rsm = (List<HttpPostedFileBase>)TempData["26"];
            Resim rrr = new Resim();
            if (rsm[0] != null)
            {
                for (int i = 0; i < rsm.Count(); i++)
                {
                    string name = "/Content/Uploads/Olaylar/" + Guid.NewGuid() + Path.GetExtension(rsm[i].FileName);
                    rsm[i].SaveAs(Server.MapPath(name));
                    rrr.ResimBuyukYol = name;
                    rrr.ResimKucukYol = name;
                    rrr.KampanyaID = id;
                    ctx.Resims.Add(rrr);
                    try
                    {
                        ctx.SaveChanges();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        foreach (var entityValidationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in entityValidationErrors.ValidationErrors)
                            {
                                Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                            }
                        }
                    }
                }
            }
         

            kl.EklendiMi = false;
            ctx.SaveChanges();
            return View("AddedBekleyenler");




        }
        public ActionResult AddedDelete(int id)
        {
            Kampanya kl = ctx.Kampanyas.FirstOrDefault(x=>x.EklendiMi==false);
            return View("AddedBekleyenler");
        }
    }
}