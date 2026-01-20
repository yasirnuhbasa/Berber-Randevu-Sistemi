# ✂️ Premium Berber - Randevu Yönetim Sistemi

Web Tabanlı Programlama Dersi Final Projesi - Rıdvan Yasir NUHBAŞA - 132230033

## 🎯 Proje Amacı
Bu projenin temel amacı, geleneksel berber randevu süreçlerini dijitalleştirerek hem işletme sahipleri hem de müşteriler için zaman kaybını önlemektir. Karmaşık telefon trafiğini ortadan kaldırarak, 7/24 erişilebilir, kullanıcı dostu ve yönetilebilir bir randevu ekosistemi oluşturmaktır.

## 👥 Hedef Kullanıcı Kitlesi
* **Müşteriler:** Sıra beklemeden, dilediği berberden ve saatten randevu almak isteyen son kullanıcılar.
* **İşletme Sahipleri (Admin):** Randevuları, personeli (berberleri) ve verilen hizmetleri tek bir panelden yönetmek isteyen işletmeciler.

## 🎬 Senaryo ve Kullanım
Uygulama iki temel rol üzerine kurgulanmıştır:

1.  **Müşteri Senaryosu:**
    * Kullanıcı sisteme üye olur ve giriş yapar.
    * "Randevu Al" ekranından tarih seçimi yapar.
    * Sistem, o tarihteki uygun saatleri ve müsait berberleri listeler (Dolu saatler engellenir).
    * Kullanıcı istediği hizmeti (Saç, Sakal vb.) seçerek randevusunu onaylar.
    * "Randevularım" ekranından gelecek randevularını takip edebilir.

2.  **Yönetici (Admin) Senaryosu:**
    * Admin paneline erişir.
    * **Dashboard:** Günlük randevu özetlerini görüntüler ve filtreler.
    * **Berber Yönetimi:** Dükkanda çalışan berberleri ekler, çıkarır veya "İzinde/Pasif" moduna alır.
    * **Hizmet Yönetimi:** Fiyat ve süre bilgilerini günceller.
    * **Kullanıcı Yönetimi:** Kayıtlı müşterileri görüntüler ve gerekirse siler.

## 🛠 Kullanılan Teknolojiler
Bu proje **ASP.NET Core MVC** mimarisi kullanılarak geliştirilmiştir.

* **Dil:** C# (.NET 8.0)
* **Mimari:** MVC (Model-View-Controller)
* **Veritabanı:** MS SQL Server (Entity Framework Core - Code First)
* **Front-End:** Bootstrap 5, HTML5, CSS3, JavaScript
* **Diğer Araçlar:** FontAwesome (İkonlar), Google Maps Embed API

## 📹 Tanıtım Videosu
Projenin çalışır halini ve kod yapısını anlatan tanıtım videosu:
[YOUTUBE_LINKI_BURAYA_GELECEK]
