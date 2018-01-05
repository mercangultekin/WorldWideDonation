using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WWD.App_classes;
using WWD.Models;
namespace WWD.Controllers
{
    public class KampanyaController : Controller
    {
        // GET: Kampanya
        DonationDB ctx = new DonationDB();
        [Authorize(Roles = "UYE")]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated != true)
            {
                TempData["adad"] = "You must Log in";
            }
            ViewBag.kategoriler = ctx.Kategoris.ToList();

            return View();
        }
        public ActionResult KampanyaDetay(int id)
        {
            ViewBag.katwid = ctx.Kategoris.Take(12).OrderByDescending(x => x.KategoriId);
            var data = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
            ViewBag.addd = ctx.Stuffs.Where(x => x.KampanyaID == id);
            ViewBag.fu = ctx.Resims.Where(x => x.KampanyaID == id).Take(4).OrderByDescending(y => y.ResimId).ToList();
            return View(data);
        }
        //Çoklu resimi dene
        [Authorize(Roles = "UYE")]

        public ActionResult KampanyaBaslat(Kampanya ka, HttpPostedFileBase kampresim, string[] StuffName, int[] Miktari, string[] Birim, HttpPostedFileBase[] ResimID)
        {

            if (kampresim != null)
            {
                Image img = Image.FromStream(kampresim.InputStream);
                int width = Convert.ToInt32(ConfigurationManager.AppSettings["kampKucukW"]);
                int height = Convert.ToInt32(ConfigurationManager.AppSettings["kampKucukh"]);
                string name = "/Content/Uploads/KucukFoto" + Guid.NewGuid() + Path.GetExtension(kampresim.FileName);
                Bitmap bm = new Bitmap(img, width, height);
                bm.Save(Server.MapPath(name));
                ka.KampanyaFoto = name;
                int wd = Convert.ToInt32(ConfigurationManager.AppSettings["kampBuyukW"]);
                int hd = Convert.ToInt32(ConfigurationManager.AppSettings["kampBuyukH"]);
                string name2 = "/Content/Uploads/BuyukFoto" + Guid.NewGuid() + Path.GetExtension(kampresim.FileName);
                Bitmap bm2 = new Bitmap(img, wd, hd);
                bm.Save(Server.MapPath(name2));
                ka.KampanyaFotoB = name2;
            }
            //eşya nesnesini yukarıda oluşturmamım bir sebebi var ama ne mk
            Stuff esya = new Stuff();
            Guid baslatanId = new Guid();
            baslatanId = ctx.Kullanicis.FirstOrDefault(x => x.UserAdi == User.Identity.Name).KullaniciId;
            ka.Baslatan = baslatanId;
            ka.BasariliMi = false;
            ka.OnaylandiMi = false;
            ka.ResimEditMi = false;
            ka.AcikEditlendiMi = false;
            ka.StuffEditMi = false;
            ka.BaslamaTarihi = DateTime.Now;

            // ka.StuffID = 1;
            //ka.DonateID = 0;

            ctx.Kampanyas.Add(ka);
            ctx.SaveChanges();
            int d = ka.KampanyaId;
            Resim rsm = new Resim();
            if (ResimID[0] != null)
            {
                for (int i = 0; i < ResimID.Count(); i++)
                {
                    string name = "/Content/Uploads/Olaylar/" + Guid.NewGuid() + Path.GetExtension(ResimID[i].FileName);

                    // Image img = Image.FromStream(ResimID[i].InputStream);
                    //  Bitmap bm = new Bitmap(img);   
                    ResimID[i].SaveAs(Server.MapPath(name));
                    rsm.ResimBuyukYol = name;
                    rsm.ResimKucukYol = name;
                    rsm.KampanyaID = d;
                    ctx.Resims.Add(rsm);
                    ctx.SaveChanges();
                }
            }

            if (rsm.ResimId != 0)
            {
                ka.ResimID = rsm.ResimId;

            }

            for (int i = 0; i < StuffName.Count() && i < Birim.Count() && i < Miktari.Count(); i++)
            {
                esya.Birim = Birim[i];
                esya.Miktari = Miktari[i];
                esya.StuffName = StuffName[i];

                esya.KampanyaID = 0;
                esya.KalanMiktar = 0;
                if (esya.KampanyaID != -1)
                {
                    esya.KampanyaID = d;
                }
                ctx.Stuffs.Add(esya);
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
            // ctx.Kampanyas.Add(ka);
            //AHANDA BURADA HATA VERİYOR
            // ka.StuffID = esya.StuffId;
            // ctx.Kampanyas.Add(ka);
            ctx.SaveChanges();
            return RedirectToAction("KampanyaDetay/" + ka.KampanyaId);
        }
        //Kargo Takip No su Denenmedi
        
        //ajax ile
        [Authorize(Roles = "UYE")]
        [HttpPost]
        public void BagisYap(int[] GonderiMiktari, int[] StuffID, int[] KampanyaID)
        {


            Donate gelen = new Donate();
            if (User.Identity.IsAuthenticated)
            {
                for (int i = 0; i < GonderiMiktari.Count() && i < StuffID.Count() && i < KampanyaID.Count(); i++)
                {
                    gelen.GonderiMiktari = GonderiMiktari[i];
                    gelen.KampanyaID = KampanyaID[i];
                    gelen.StuffID = StuffID[i];
                    Guid bagisci = new Guid();
                    bagisci = ctx.Kullanicis.FirstOrDefault(x => x.UserAdi == User.Identity.Name).KullaniciId;
                    gelen.GonderdiMi = false;
                    gelen.VardiMi = false;
                    gelen.GonderenID = bagisci;
                    gelen.BasladiMi = true;
                    if (gelen.GonderiMiktari == 0)
                    {
                        gelen.GonderiMiktari = 0;
                        gelen.StuffID = 41;
                        gelen.KampanyaID = 65;
                        bagisci = ctx.Kullanicis.FirstOrDefault(x => x.UserAdi == "sell").KullaniciId;
                        gelen.GonderenID = bagisci;
                        gelen.GonderdiMi = false;
                        gelen.VardiMi = false;
                        gelen.BasladiMi = true;
                    }
                    ctx.Donates.Add(gelen);
                    ctx.SaveChanges();
                }

                //kargo takip nosunu null yap,
                // gelen.KargoTakipNo = a + "WWD" + b;
                //   ViewBag.kargotakip = gelen.KargoTakipNo;
                // ctx.Donates.Add(gelen);
                //   ctx.SaveChanges();
                //   ViewBag.kargotakip
            }
            else
            {
                ViewBag.mej = "You must Log in";

            }
            int a = gelen.DonateId;
            int b = gelen.KampanyaID;
            TempData["kargo"] = a + "WWD" + b;
            string c = HttpContext.User.Identity.Name;
            Guid id = (Guid)Membership.GetUser(c).ProviderUserKey;
            //return RedirectToAction("Index", "Kullanici", new { @id = id });
        }
        //db yi değiştirdim donate ve stuff arasına relation ve stuff a coloumn ekledim
        [Authorize(Roles = "UYE")]
        [HttpPost]
        public void Onayla(int id, int tr)
        {
            Stuff fd = ctx.Stuffs.FirstOrDefault(x => x.StuffId == tr);
            Donate dt = ctx.Donates.FirstOrDefault(x => x.DonateId == id);
            // Stuff sta = ctx.Stuffs.FirstOrDefault(y=>y.KampanyaID==dt.KampanyaID);
            Kampanya kak = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == dt.KampanyaID);
            List<Stuff> daat = ctx.Stuffs.Where(x => x.KampanyaID == dt.KampanyaID).ToList();
            if (dt.Stuff.StuffId == tr)
            {
                int d = fd.Miktari;
                int a = dt.GonderiMiktari;
                int b = fd.KalanMiktar;//Giden miktar
                int c = d - a;
                // dt.Stuff.KalanMiktar = b + a;
                if (c >= 0)
                {
                    fd.KalanMiktar = b + a;
                    if (b != d)
                    {
                        int jaasdd = 0;
                        for (int i = 0; i < daat.Count(); i++)
                        {
                            // int[] ad;
                            // List<Stuff> adsad = ctx.Stuffs.Where(x => x.KampanyaID == sta.KampanyaID).ToList();
                            int f = daat[i].KalanMiktar;
                            int g = daat[i].Miktari;

                            int aff = g - f;
                            // aff -= 1;

                            if (aff == 0)
                            {
                                jaasdd = jaasdd + 1;
                            }
                            if (jaasdd == daat.Count())
                            {
                                kak.BasariliMi = true;
                                ctx.SaveChanges();
                            }
                        }
                        dt.GonderdiMi = true;
                        ctx.SaveChanges();
                    }
                }
            }
        }
        //ajax kullandım
        [Authorize(Roles = "UYE")]
        [HttpPost]
        public void Red(int id)
        {
            Donate dt = ctx.Donates.FirstOrDefault(x => x.DonateId == id);
            ctx.Donates.Remove(dt);
            ctx.SaveChanges();
        }
        [Authorize(Roles = "UYE")]
        [HttpPost]
        public ActionResult GlnBagsOnay(int id, string KargoTakipNo)
        {
            Donate dd = ctx.Donates.FirstOrDefault(x => x.DonateId == id);
            dd.KargoTakipNo = KargoTakipNo;
            dd.VardiMi = true;
            ctx.SaveChanges();

            string a = HttpContext.User.Identity.Name;
            Guid add = (Guid)Membership.GetUser(a).ProviderUserKey;

            return RedirectToAction("Index", "Kullanici", new { @id = add });
        }
        [Authorize(Roles = "UYE")]
        [HttpPost]
        public void KampanyaSil(int id)
        {
            Kampanya kk = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
            ctx.Kampanyas.Remove(kk);
            ctx.SaveChanges();

        }
        [Authorize(Roles = "UYE")]
        public ActionResult Yenile(int id)
        {
            Kampanya data = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
            return View(data);
        }
        //  [Authorize(Roles = "UYE")]
        // [HttpPost]
        //    public ActionResult editkaydet(int id, int[] gizlideger, string Aciklama, string[] StuffName, int[] Miktari, string[] Birim, HttpPostedFileBase resim12)
        //    {
        //        Kampanya kam = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
        //        Edit et = new Edit();
        //        et.EditAciklama = Aciklama;
        //        et.KampanyaID = kam.KampanyaId;
        //        if (resim12 != null)
        //        {
        //            Image img = Image.FromStream(resim12.InputStream);
        //            int width = Convert.ToInt32(ConfigurationManager.AppSettings["kampKucukW"]);
        //            int height = Convert.ToInt32(ConfigurationManager.AppSettings["kampKucukh"]);
        //            string name = "/Content/Uploads/KucukFoto" + Guid.NewGuid() + Path.GetExtension(resim12.FileName);
        //            Bitmap bm = new Bitmap(img, width, height);
        //            bm.Save(Server.MapPath(name));
        //            et.KampanyaFotoK = name;
        //            int wd = Convert.ToInt32(ConfigurationManager.AppSettings["kampBuyukW"]);
        //            int hd = Convert.ToInt32(ConfigurationManager.AppSettings["kampBuyukH"]);
        //            string name2 = "/Content/Uploads/BuyukFoto" + Guid.NewGuid() + Path.GetExtension(resim12.FileName);
        //            Bitmap bm2 = new Bitmap(img, wd, hd);
        //            bm.Save(Server.MapPath(name2));
        //            et.KampanyaFotoB = name2;
        //        }
        //        Stuff dat = new Stuff();//yeni ürün
        //        ctx.Edits.Add(et);
        //        ctx.SaveChanges();
        //        for (int i = 0; i < StuffName.Count() && i < Birim.Count() && i < Miktari.Count(); i++)
        //        {
        //            if (Birim[i] != null && Miktari[i] != 0 && StuffName[i] != "")
        //            {
        //                DegUrun deg = new DegUrun();
        //                deg.UrunID = gizlideger[i];
        //                deg.EditID = et.EditId;
        //                ctx.DegUruns.Add(deg);
        //                ctx.SaveChanges();
        //            }
        //            if (Birim[i] != null)
        //            {
        //                dat.Birim = Birim[i];
        //            }
        //            else
        //            {
        //                dat.Birim = "Null";
        //            }
        //            if (Miktari[i] != 0)
        //            {
        //                dat.Miktari = Miktari[i];
        //            }
        //            else
        //            {
        //                dat.Miktari = 00;
        //            }
        //            if (StuffName[i] != "")
        //            {
        //                dat.StuffName = StuffName[i];
        //            }
        //            else
        //            {
        //                dat.StuffName = "Null";
        //            }
        //            dat.EditID = et.EditId;
        //            dat.KampanyaID = 0;
        //            dat.KalanMiktar = 0;
        //            if (dat.KampanyaID != -1)
        //            {
        //                dat.KampanyaID = 65;
        //            }
        //            ctx.Stuffs.Add(dat);
        //            ctx.SaveChanges();
        //            et.StuffID = dat.StuffId;
        //        }
        //        //ctx.Edits.Add(et);
        //        ctx.SaveChanges();
        //        string a = HttpContext.User.Identity.Name;
        //        Guid add = (Guid)Membership.GetUser(a).ProviderUserKey;
        //        kam.EditlendiMi = true;
        //        ctx.SaveChanges();
        //        return RedirectToAction("Index", "Kullanici", new { @id = add });
        //    }
        //[HttpPost]
        //  public ActionResult EditOk1(int id)
        //  {
        //      Kampanya kam = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
        //       Edit et = ctx.Edits.FirstOrDefault(x => x.KampanyaID == id);
        //
        //       List<Stuff> st = ctx.Stuffs.Where(x => x.EditID == et.EditId).ToList();//yeni ürün
        //
        //       List<DegUrun> def = ctx.DegUruns.Where(x => x.EditID == et.EditId).ToList();//eski ürün
        //
        //       for (int i = 0; i < def.Count(); i++)
        //       {
        //           if (st[i].StuffId == def[i].UrunID)
        //             
        //           {
        //               st[i].Birim = def[i].Edit.Stuff.Birim;
        //               st[i].Miktari = def[i].Edit.Stuff.Miktari;
        //               st[i].StuffName = def[i].Edit.Stuff.StuffName;
        //               ctx.SaveChanges();
        //           }
        //
        //       }
        //       //kam.Stuff.StuffName = et.Stuff.StuffName;
        //       //kam.Stuff.Miktari = et.Stuff.Miktari;
        //       //kam.Stuff.Birim = et.Stuff.Birim;
        //
        //       if (et.EditAciklama != null)
        //       {
        //           kam.Aciklama = et.EditAciklama;
        //       }
        //
        //       if (et.KampanyaFotoK != null)
        //       {
        //           kam.KampanyaFoto = et.KampanyaFotoK;
        //       }
        //       if (et.KampanyaFotoB != null)
        //       {
        //           kam.KampanyaFotoB = et.KampanyaFotoB;
        //       }
        //
        //       kam.EditlendiMi = false;
        //       ctx.SaveChanges();
        //       return RedirectToAction("EditBekleyenler", "Yonetim");
        //   }
        //[HttpPost]
        //      public ActionResult EditDelete(int id)
        //      {
        //          Kampanya kat = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
        //          kat.EditlendiMi = false;
        //          Edit ed = ctx.Edits.FirstOrDefault(x => x.KampanyaID == id);
        //          //  Stuff df = ctx.Stuffs.FirstOrDefault(x => x.EditID == ed.EditId);
        //          //ctx.Stuffs.Remove(df);
        //          ctx.Edits.Remove(ed);
        //          try
        //          {
        //              ctx.SaveChanges();
        //          }
        //          catch (DbEntityValidationException ex)
        //          {
        //              foreach (var entityValidationErrors in ex.EntityValidationErrors)
        //              {
        //                  foreach (var validationError in entityValidationErrors.ValidationErrors)
        //                  {
        //                      Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
        //                  }
        //              }
        //          }
        //          return RedirectToAction("EditBekleyenler", "Yonetim");
        //      }
        public ActionResult YeniEkle(int id)
        {

            return View();
        }
        [HttpPost]
        public ActionResult YeniEkle(int id, string[] StuffName, string[] Birim, int[] Miktari, HttpPostedFileBase[] ResimID)
        {
            Kampanya kam = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);



            List<string> deneme1 = new List<string>();
            for (int i = 0; i < StuffName.Count(); i++)
            {
                string da = StuffName[i];
                deneme1.Add(da);
            }
            List<string> deneme2 = new List<string>();
            for (int i = 0; i < Birim.Count(); i++)
            {
                string da = Birim[i];
                deneme2.Add(da);
            }
            List<int> deneme3 = new List<int>();
            for (int i = 0; i < Miktari.Count(); i++)
            {
                int da = Miktari[i];
                deneme3.Add(da);
            }
            List<HttpPostedFileBase> deneme4 = new List<HttpPostedFileBase>();
            for (int i = 0; i < ResimID.Count(); i++)
            {
                HttpPostedFileBase da = ResimID[i];
                deneme4.Add(da);
            }


            TempData["StuffName"] = deneme1;
            TempData["Birim"] = deneme2;
            TempData["Miktari"] = deneme3;
            TempData["ResimID"] = deneme4;



            Resim rsm = new Resim();
            if (ResimID[0] != null)
            {
                for (int i = 0; i < ResimID.Count(); i++)
                {
                    string name = "/Content/Uploads/Olaylar/" + Guid.NewGuid() + Path.GetExtension(ResimID[i].FileName);
                    ResimID[i].SaveAs(Server.MapPath(name));
                    rsm.ResimBuyukYol = name;
                    rsm.ResimKucukYol = name;
                    rsm.KampanyaID = id;
                    ctx.Resims.Add(rsm);
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

            kam.EklendiMi = true;
            ctx.SaveChanges();




            return View();
        }
        [HttpPost]
        public void Fotoedit(int id,HttpPostedFileBase resim12)
        {
            Kampanya kat = ctx.Kampanyas.FirstOrDefault(x=>x.KampanyaId==id);
            kat.ResimEditMi = true;
            ResimEd rrr = new ResimEd();
            rrr.KampanyaID = id;
            if (resim12 != null)
            {
                Image img = Image.FromStream(resim12.InputStream);
                int width = Convert.ToInt32(ConfigurationManager.AppSettings["kampKucukW"]);
                int height = Convert.ToInt32(ConfigurationManager.AppSettings["kampKucukh"]);
                string name = "/Content/Uploads/KucukFoto" + Guid.NewGuid() + Path.GetExtension(resim12.FileName);
                Bitmap bm = new Bitmap(img, width, height);
                bm.Save(Server.MapPath(name));
                rrr.ResimYolK = name;
                int wd = Convert.ToInt32(ConfigurationManager.AppSettings["kampBuyukW"]);
                int hd = Convert.ToInt32(ConfigurationManager.AppSettings["kampBuyukH"]);
                string name2 = "/Content/Uploads/BuyukFoto" + Guid.NewGuid() + Path.GetExtension(resim12.FileName);
                Bitmap bm2 = new Bitmap(img, wd, hd);
                bm.Save(Server.MapPath(name2));
                rrr.ResimYolB = name2;
            }
            ctx.ResimEds.Add(rrr);
            ctx.SaveChanges();
        }
     
        public void Stuffedit(int id,string StuffName,string Birim,int Miktari,int gizlideger)
        {
            Kampanya ka = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
            StuffEd fer = new StuffEd();
            fer.gizli = gizlideger;
            fer.Birim = Birim;
            fer.KampanyaID = id;
            fer.Miktari = Miktari;
            fer.StuffName = StuffName;
            ctx.StuffEds.Add(fer);
            ka.StuffEditMi = true;
            ctx.SaveChanges();
        }
        [HttpPost]
        public void Acikedit(int id, string Aciklama)
        {
            Kampanya ka=ctx.Kampanyas.FirstOrDefault(x=>x.KampanyaId==id);
            AciklamaEd ac = new AciklamaEd();
            ac.KampanyaID = id;
            ac.Aciklamasi = Aciklama;
            ctx.AciklamaEds.Add(ac);
            ka.AcikEditlendiMi = true;
            ctx.SaveChanges();
        }
        public void editok(int id)
        {
            Kampanya ka = ctx.Kampanyas.FirstOrDefault(x=>x.KampanyaId==id);
            StuffEd st = ctx.StuffEds.FirstOrDefault(x=>x.KampanyaID==id);
            Stuff gst = ctx.Stuffs.FirstOrDefault(x=>x.StuffId==st.gizli);
            if (gst!=null)
            {
                gst.StuffName = st.StuffName;
                gst.Miktari = st.Miktari;
                gst.Birim = st.Birim;
                ka.StuffEditMi = false;
            }
           
            ResimEd re = ctx.ResimEds.FirstOrDefault(x=>x.KampanyaID==id);
            if (re!=null)
            {
                ka.KampanyaFoto = re.ResimYolK;
                ka.KampanyaFotoB = re.ResimYolB;
                ka.ResimEditMi = false;
            }
           
            AciklamaEd dd = ctx.AciklamaEds.FirstOrDefault(x=>x.KampanyaID==id);
            if (dd!=null)
            {
                ka.Aciklama = dd.Aciklamasi;
                ka.AcikEditlendiMi = false;
                ka.Aciklama = dd.Aciklamasi;
            }
          
            ctx.SaveChanges();
        }
        public void editdelete(int id)
        {
            Kampanya ka = ctx.Kampanyas.FirstOrDefault(x => x.KampanyaId == id);
            ka.AcikEditlendiMi = false;
            ka.ResimEditMi = false;
            ka.StuffEditMi = false;
            ctx.SaveChanges();
        }
        }
}