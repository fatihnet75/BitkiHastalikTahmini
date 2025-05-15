# Bitki Hastalık Tahmini Projesi 🌱

Bu proje, yapay zeka ve makine öğrenmesi kullanarak bitki hastalıklarını tespit eden ve tahmin eden bir web uygulamasıdır. ASP.NET Core MVC ve TensorFlow.NET kullanılarak geliştirilmiştir.

## 🚀 Özellikler

- Yapay zeka ile bitki hastalığı tespiti
- Gerçek zamanlı görüntü analizi
- Hastalık tahmin API'si
- Kullanıcı dostu arayüz
- Mobil uyumlu tasarım
- Detaylı hastalık raporları

## 🛠️ Teknolojiler

- ASP.NET Core 8.0
- TensorFlow.NET
- MongoDB
- RESTful API
- Bootstrap 5
- xUnit & Moq

## 📋 Gereksinimler

- .NET 8.0 SDK
- MongoDB
- Visual Studio 2022
- Git

## 🔧 Kurulum

1. Projeyi klonlayın:
```bash
git clone https://github.com/fatihnet75/BitkiHastalikTahmini.git
```

2. Bağımlılıkları yükleyin:
```bash
dotnet restore
```

3. Uygulamayı çalıştırın:
```bash
dotnet run
```

## 🤖 Yapay Zeka Özellikleri

- **Görüntü İşleme**: OpenCV ile bitki yapraklarının analizi
- **Derin Öğrenme**: CNN modeli ile hastalık sınıflandırması
- **Tahmin Modeli**: TensorFlow.NET ile eğitilmiş özel model
- **Veri Analizi**: Hastalık geçmişi ve trend analizi

## 📡 API Yapısı

### Hastalık Tespiti API'si
```http
POST /api/disease/detect
Content-Type: multipart/form-data

Response:
{
    "disease": "Yaprak Yanıklığı",
    "confidence": 0.95,
    "recommendations": ["İlaçlama yapın", "Sulama azaltın"]
}
```

### Tahmin API'si
```http
GET /api/disease/predict?plantType=Domates&region=Akdeniz

Response:
{
    "riskLevel": "Yüksek",
    "potentialDiseases": ["Mildiyö", "Yaprak Yanıklığı"],
    "preventiveMeasures": ["İlaçlama", "Havalandırma"]
}
```

## 📁 Proje Yapısı

```
BitkiHastalikTahmini/
├── Controllers/         # MVC & API Controller'lar
├── Models/             # Veri modelleri
├── Services/           # AI & İş mantığı servisleri
├── AI/                 # Yapay zeka modelleri
└── Tests/             # Test projesi
```

## 👥 Kullanıcı Rolleri

1. **Admin**
   - AI model yönetimi
   - Sistem ayarları
   - Kullanıcı yönetimi

2. **Kullanıcı**
   - Hastalık tespiti
   - Tahmin görüntüleme
   - Rapor oluşturma

## 📞 İletişim

Fatih Net - [@fatihnet75](https://github.com/fatihnet75)

Proje Linki: [https://github.com/fatihnet75/BitkiHastalikTahmini](https://github.com/fatihnet75/BitkiHastalikTahmini) 