﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Inventory_Management_Web_Application.Models;


namespace Inventory_Management_Web_Application.Controllers
{
    public class YazilimUrunController : Controller
    {
        InventoryContext db = new InventoryContext();
        // GET: YazilimUrun

        [HttpGet]
        public ActionResult Listesi()
        {
            ViewBag.ayarlar = db.Ayarlar.FirstOrDefault();
            return View(db.YazılımUrun.ToList());
        }

        public PartialViewResult altKategoriDropdown(int id)
        {
            var altkategoriler = db.AltKategori.Where(x => x.AnaKategorID == id).ToList();
            ViewBag.altkategoriler = new SelectList(altkategoriler, "ID", "KategoriAdi");
            return PartialView();
        }

        [HttpGet]
        public ActionResult Ekle()
        {
            var anakategoriler = db.AnaKategori.ToList();
            ViewBag.anakategoriler = new SelectList(anakategoriler, "ID", "KategoriAdi");
            return View();
        }

        [HttpPost]
        public ActionResult Ekle(YazılımUrun u)
        {
            int Lastid = 0;
            if (db.YazılımUrun != null)
            {
                Lastid = db.YazılımUrun.Max(x => x.ID);
            }
            Lastid = db.Urun.Max(x => x.ID);
            string urunKodu = u.altKategoriID.ToString() + "1000" + DateTime.Now.Year.ToString() + (Lastid + 1).ToString();
            u.UrunID = urunKodu;
            db.YazılımUrun.Add(u);
            db.SaveChanges();
            return RedirectToAction("Listesi");
        }

        [HttpPost]
        public ActionResult Sil(int id)
        {
            YazılımUrun b = db.YazılımUrun.Where(x => x.ID == id).SingleOrDefault();
            if (b == null)
            {
                return Json(false);
            }
            else
            {
                try
                {
                    db.YazılımUrun.Remove(b);
                    db.SaveChanges();
                    return Json(true);
                }
                catch (Exception)
                {
                    return Json("FK");
                }

            }
        }

        [HttpGet]
        public ActionResult Guncelle(int id)
        {
            YazılımUrun u = db.YazılımUrun.Where(x => x.ID == id).FirstOrDefault();

            var anakategoriler = db.AltKategori.ToList();
            ViewBag.anakategoriler = new SelectList(anakategoriler, "ID", "KategoriAdi");
            if (u == null)
            {
                return RedirectToAction("Hata", "Admin");
            }
            return View(u);
        }

        [HttpPost]
        public ActionResult Guncelle(YazılımUrun u)
        {
            YazılımUrun gu = db.YazılımUrun.Where(x => x.ID == u.ID).FirstOrDefault();
            if (gu == null)
            {
                return RedirectToAction("Hata", "Admin");
            }
            gu.altKategoriID = u.altKategoriID;
            gu.UrunAdi = u.UrunAdi;
            gu.Aciklama = u.Aciklama;
            gu.KeyAdet = u.KeyAdet;
            gu.LisansBaslangicTarihi = u.LisansBaslangicTarihi;
            gu.LisansBitisTarihi = u.LisansBitisTarihi;
            gu.UrunSeriNo = u.UrunSeriNo;
            db.SaveChanges();
            return RedirectToAction("Listesi");
        }

        // urun çıkarma
        public ActionResult stokCikar(int id)
        {
            YazılımUrun u = db.YazılımUrun.Where(x => x.ID == id).SingleOrDefault();
            if (u.KeyAdet == 0)
            {
                ViewBag.hata = "Bu ürün için stok bulunmamaktadır.";
                return RedirectToAction("Listesi");
            }
            if (u == null)
            {
                return RedirectToAction("Hata", "Admin");
            }

            var urunSepet = (App_Classes.YazilimUrunCikis)Session["YazilimUrun"];
            if (urunSepet == null)
            {
                urunSepet = new App_Classes.YazilimUrunCikis();
                Session["YazilimUrun"] = urunSepet;
            }
            urunSepet.ListeyeEkle(u);
            return RedirectToAction("Listesi");
        }

        [HttpGet]
        public ActionResult stokCikarView()
        {
            var personeller = db.TeslimAlanPersonel.Select(x => new
            {
                ID = x.ID,
                adiSoyadi = x.Adi + " " + x.Soyadi
            });
            var personellerVeren = db.Personel.Select(x => new
            {
                ID = x.ID,
                adiSoyadi = x.Adi + " " + x.Soyadi
            });
            var urunbirimler = db.UrunBirim.ToList();
            ViewBag.teslimalanlar = new SelectList(personeller, "ID", "adiSoyadi");
            ViewBag.teslimverenler = new SelectList(personellerVeren, "ID", "adiSoyadi");
            return View();
        }
        [HttpPost]
        public ActionResult stokCikarView(UrunCikis uc)
        {
            int Lastid = 0;
            if (db.UrunCikis.Count() != 0)
            {
                Lastid = db.UrunCikis.Max(x => x.ID);
            }
            int CikisNumarasi = 1000 + DateTime.Now.Year + (Lastid + 1);
            var urunler = (App_Classes.YazilimUrunCikis)Session["YazilimUrun"];
            List<YazılımUrun> liste = urunler.HepsiniGetir();
            List<YazılımUrun> temp = new List<YazılımUrun>();
            foreach (YazılımUrun item in liste)
            {
                if (temp.Where(x => x.ID == item.ID).SingleOrDefault() != null)
                {
                    continue;
                }
                YazılımUrun stokDus = db.YazılımUrun.Where(x => x.ID == item.ID).FirstOrDefault();
                if (stokDus.KeyAdet == 0)
                {
                    ViewBag.hatali = "Çıkarılacak ürünler arasında stok miktarı 0 olan ürünler bulanmaktadır.";
                    return View();
                }
                stokDus.KeyAdet = stokDus.KeyAdet - liste.Where(x => x.ID == item.ID).ToList().Count;
                db.SaveChanges();
                uc.YazilimUrunID = item.ID;
                uc.CikisNumarasi = CikisNumarasi;
                uc.CikanMictar = liste.Where(x => x.ID == item.ID).ToList().Count;
                db.UrunCikis.Add(uc);
                db.SaveChanges();
                temp.Add(item);
            }
            urunler.ListeTemizle();
            Session.Remove("YazilimUrun");
            return RedirectToAction("CikisBasarili", uc);
        }

        [HttpGet]
        public ActionResult stokEkleView(int id)
        {
            var tedarikciler = db.Tedarikci.Select(x => new
            {
                ID = x.ID,
                TedarikciAdi = x.FirmaAdi
            });
            var personeller = db.Personel.Select(x => new
            {
                ID = x.ID,
                adiSoyadi = x.Adi + " " + x.Soyadi
            });
            var urunbirimler = db.UrunBirim.ToList();
            ViewBag.tedarikciler = new SelectList(tedarikciler, "ID", "TedarikciAdi");
            ViewBag.personeller = new SelectList(personeller, "ID", "adiSoyadi");

            YazılımUrun eklenecekUrun = db.YazılımUrun.Where(x => x.ID == id).FirstOrDefault();
            return View(eklenecekUrun);
        }

        [HttpPost]
        public ActionResult stokEkle(UrunGiris veri)
        {
            var urun = db.YazılımUrun.FirstOrDefault(x => x.ID == veri.YazilimUrunID);
            urun.KeyAdet = urun.KeyAdet + veri.AlinanMiktar;
            veri.UrunID = null;
            db.UrunGiris.Add(veri);
            db.SaveChanges();
            return RedirectToAction("urunGirisleri","Urun");
        }
    }
}