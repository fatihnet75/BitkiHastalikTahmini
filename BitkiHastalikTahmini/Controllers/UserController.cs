using BitkiHastalikTahmini.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace BitkiHastalikTahmini.Controllers
{
    public class UserController : Controller
    {
        private readonly MongoDbContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public UserController(MongoDbContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _clientFactory = clientFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Kullanıcı listesini görüntüle
        public IActionResult Index()
        {
            var users = _context.Users.Find(_ => true).ToList();

            // Şehir ve ay listelerini ViewBag'e ekle
            ViewBag.Cities = GetCitiesList();
            ViewBag.Months = GetMonthsList();

            return View(users); // Index.cshtml dosyasını açacak
        }

        // Yeni kullanıcı ekleme sayfasını göster
        public IActionResult Create()
        {
            return View();
        }

        // Yeni kullanıcı kaydetme işlemi
        [HttpPost]
        public IActionResult Create(User user)
        {
            try
            {
                _context.Users.InsertOne(user);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Kullanıcı kaydedilirken hata: {ex.Message}";
                return View(user);
            }
        }

        // Toprak Analizi API'sini çağır
        [HttpPost]
        public async Task<IActionResult> AnalyzeSoil(double azot, double fosfor, double potasyum, double ph, string il, int ay, string apiUrl, int trialCount = 0)
        {
            try
            {
                // API'a gönderilecek verileri hazırla
                var data = new
                {
                    Nitrogen = azot,
                    Phosphorus = fosfor,
                    Potassium = potasyum,
                    Ph = ph,
                    City = il,
                    Month = ay
                };

                // HTTP istemcisini oluştur
                var client = _clientFactory.CreateClient();

                // API endpoint'ini oluştur (sonundaki slash'i temizle)
                var endpoint = $"{apiUrl.TrimEnd('/')}/api/soil-analysis";

                // JSON içeriğini oluştur
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // POST isteğini gönder
                var response = await client.PostAsync(endpoint, content);

                // Başarılı yanıt kontrolü
                if (response.IsSuccessStatusCode)
                {
                    // Yanıtı oku ve deserialize et
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Deserializasyon hatasını yakala
                    try
                    {
                        var result = JsonSerializer.Deserialize<SoilAnalysisResult>(responseString, _jsonOptions);

                        // ViewBag'e sonuçları ekle
                        ViewBag.ShowResults = true;
                        ViewBag.AnalysisType = "soil";
                        ViewBag.ResultCrop = result?.Crop ?? "Bilinmeyen Bitki";
                        ViewBag.Confidence = result?.Confidence ?? 0;
                        ViewBag.Explanation = result?.Explanation ?? "Açıklama bulunamadı";
                    }
                    catch (Exception jsonEx)
                    {
                        // JSON deserializasyon hatası
                        ViewBag.ShowResults = true;
                        ViewBag.AnalysisType = "soil";
                        ViewBag.ResultCrop = "Hata";
                        ViewBag.Confidence = 0;
                        ViewBag.Explanation = $"API yanıtı işlenirken hata oluştu: {jsonEx.Message}. Yanıt: {responseString}";

                        // Loglama ekleyebilirsiniz
                        Console.WriteLine($"JSON Deserializasyon hatası: {jsonEx.Message}");
                        Console.WriteLine($"API yanıtı: {responseString}");
                    }

                    // Kullanıcının girdiği değerleri ViewBag'e ekle
                    ViewBag.Azot = azot;
                    ViewBag.Fosfor = fosfor;
                    ViewBag.Potasyum = potasyum;
                    ViewBag.Ph = ph;
                    ViewBag.Il = il;
                    ViewBag.AyAdi = GetMonthName(ay);
                    ViewBag.ApiUrl = apiUrl;
                }
                else
                {
                    // Hata durumunda
                    var errorContent = await response.Content.ReadAsStringAsync();

                    ViewBag.ShowResults = true;
                    ViewBag.AnalysisType = "soil";
                    ViewBag.ResultCrop = "Hata";
                    ViewBag.Confidence = 0;
                    ViewBag.Explanation = $"API'dan yanıt alınamadı. Hata kodu: {(int)response.StatusCode} - {response.StatusCode}. Detay: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                // İstisna durumunda
                ViewBag.ShowResults = true;
                ViewBag.AnalysisType = "soil";
                ViewBag.ResultCrop = "Hata";
                ViewBag.Confidence = 0;
                ViewBag.Explanation = $"İşlem sırasında bir hata oluştu: {ex.Message}";

                // İç hata ayrıntıları (debug amaçlı)
                if (ex.InnerException != null)
                {
                    ViewBag.Explanation += $" İç hata: {ex.InnerException.Message}";
                }
            }

            // Şehir ve ay listelerini yeniden yükle
            ViewBag.Cities = GetCitiesList();
            ViewBag.Months = GetMonthsList();

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(IFormFile plantImage, string apiUrl, int trialCount = 0)
        {
            try
            {
                // Görüntü kontrolü - daha kapsamlı
                if (plantImage == null || plantImage.Length == 0)
                {
                    ViewBag.ShowResults = true;
                    ViewBag.AnalysisType = "image";
                    ViewBag.ResultCrop = "Hata";
                    ViewBag.Confidence = 0;
                    ViewBag.Explanation = "Lütfen bir görüntü seçin.";
                    SetViewBagLists();
                    return View("Index");
                }

                // Görüntü boyutu kontrolü (örneğin 5MB ile sınırla)
                if (plantImage.Length > 5 * 1024 * 1024)
                {
                    ViewBag.ShowResults = true;
                    ViewBag.AnalysisType = "image";
                    ViewBag.ResultCrop = "Hata";
                    ViewBag.Confidence = 0;
                    ViewBag.Explanation = "Görüntü boyutu çok büyük (maksimum 5MB).";
                    SetViewBagLists();
                    return View("Index");
                }

                // HTTP istemcisini oluştur
                var client = _clientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30); // Zaman aşımı ayarla

                // API endpoint'ini oluştur
                var endpoint = $"{apiUrl.TrimEnd('/')}/api/image-analysis";

                // Multipart form içeriği oluştur
                using var formContent = new MultipartFormDataContent();
                using var memoryStream = new MemoryStream();

                // Görüntü dosyasını okuyup byte dizisine dönüştür
                await plantImage.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                // Görüntü içeriğini form'a ekle
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(plantImage.ContentType);
                formContent.Add(imageContent, "image", plantImage.FileName);

                // POST isteğini gönder
                var response = await client.PostAsync(endpoint, formContent);

                // Base64 formatında görüntü verisini sakla
                var base64Image = Convert.ToBase64String(imageBytes);
                ViewBag.ImageData = base64Image;

                // Başarılı yanıt kontrolü
                if (response.IsSuccessStatusCode)
                {
                    // Yanıtı oku ve deserialize et
                    var responseString = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var result = JsonSerializer.Deserialize<ImageAnalysisResult>(responseString, _jsonOptions);

                        // ViewBag'e sonuçları ekle
                        ViewBag.ShowResults = true;
                        ViewBag.AnalysisType = "image";
                        ViewBag.ResultCrop = result?.Disease ?? "Bilinmeyen Hastalık";
                        ViewBag.Confidence = result?.Confidence ?? 0;
                        ViewBag.Explanation = result?.Explanation ?? "Açıklama bulunamadı";
                    }
                    catch (Exception jsonEx)
                    {
                        // JSON deserializasyon hatası
                        ViewBag.ShowResults = true;
                        ViewBag.AnalysisType = "image";
                        ViewBag.ResultCrop = "Hata";
                        ViewBag.Confidence = 0;
                        ViewBag.Explanation = $"API yanıtı işlenirken hata oluştu: {jsonEx.Message}";

                        // Loglama
                        Console.WriteLine($"JSON Deserializasyon hatası: {jsonEx.Message}");
                        Console.WriteLine($"API yanıtı: {responseString}");
                    }
                }
                else
                {
                    // Hata durumunda
                    var errorContent = await response.Content.ReadAsStringAsync();

                    ViewBag.ShowResults = true;
                    ViewBag.AnalysisType = "image";
                    ViewBag.ResultCrop = "Hata";
                    ViewBag.Confidence = 0;
                    ViewBag.Explanation = $"API'dan yanıt alınamadı. Hata kodu: {(int)response.StatusCode} - {response.StatusCode}";

                    // Detaylı hata mesajı (isteğe bağlı)
                    if (!string.IsNullOrEmpty(errorContent))
                    {
                        ViewBag.Explanation += $". Detay: {errorContent}";
                    }
                }
            }
            catch (TaskCanceledException)
            {
                ViewBag.ShowResults = true;
                ViewBag.AnalysisType = "image";
                ViewBag.ResultCrop = "Hata";
                ViewBag.Confidence = 0;
                ViewBag.Explanation = "İstek zaman aşımına uğradı. Lütfen daha sonra tekrar deneyin.";
            }
            catch (Exception ex)
            {
                // İstisna durumunda
                ViewBag.ShowResults = true;
                ViewBag.AnalysisType = "image";
                ViewBag.ResultCrop = "Hata";
                ViewBag.Confidence = 0;
                ViewBag.Explanation = $"İşlem sırasında bir hata oluştu: {ex.Message}";

                // İç hata ayrıntıları (debug amaçlı)
                if (ex.InnerException != null)
                {
                    ViewBag.Explanation += $" İç hata: {ex.InnerException.Message}";
                }
            }

            SetViewBagLists();
            return View("Index");
        }

        // ViewBag listelerini ayarlayan yardımcı metot
        private void SetViewBagLists()
        {
            ViewBag.Cities = GetCitiesList();
            ViewBag.Months = GetMonthsList();
        }

        private List<string> GetCitiesList()
        {
            return new List<string>
            {
                "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Aksaray", "Amasya", "Ankara", "Antalya", "Ardahan", "Artvin",
                "Aydın", "Balıkesir", "Bartın", "Batman", "Bayburt", "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur",
                "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli", "Diyarbakır", "Düzce", "Edirne", "Elazığ", "Erzincan",
                "Erzurum", "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkari", "Hatay", "Iğdır", "Isparta", "İstanbul",
                "İzmir", "Kahramanmaraş", "Karabük", "Karaman", "Kars", "Kastamonu", "Kayseri", "Kırıkkale", "Kırklareli", "Kırşehir",
                "Kilis", "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Mardin", "Mersin", "Muğla", "Muş",
                "Nevşehir", "Niğde", "Ordu", "Osmaniye", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop", "Sivas",
                "Şanlıurfa", "Şırnak", "Tekirdağ", "Tokat", "Trabzon", "Tunceli", "Uşak", "Van", "Yalova", "Yozgat", "Zonguldak"
            };
        }

        private List<string> GetMonthsList()
        {
            return new List<string>
            {
                "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
                "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"
            };
        }

        // CSV dosyasından değer alma yardımcı metodu
        private float GetValueFromCsv(string filePath, string city, int month)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"CSV dosyası bulunamadı: {filePath}");
            }

            string[] lines = System.IO.File.ReadAllLines(filePath);

            // CSV satırlarını döngüyle ara
            foreach (var line in lines.Skip(1)) // İlk satırı atla (başlık satırı)
            {
                var values = line.Split(',');
                if (values.Length >= 13 && values[0].Trim().Equals(city, StringComparison.OrdinalIgnoreCase))
                {
                    // Ay değerini al (1-12 arası)
                    if (month >= 1 && month <= 12 && values.Length > month)
                    {
                        if (float.TryParse(values[month], NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
                        {
                            return result;
                        }
                    }
                }
            }

            // Değer bulunamazsa varsayılan değer döndür
            return 0;
        }

        // Ay adını döndüren yardımcı metot
        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Ocak",
                2 => "Şubat",
                3 => "Mart",
                4 => "Nisan",
                5 => "Mayıs",
                6 => "Haziran",
                7 => "Temmuz",
                8 => "Ağustos",
                9 => "Eylül",
                10 => "Ekim",
                11 => "Kasım",
                12 => "Aralık",
                _ => "Bilinmeyen Ay"
            };
        }
    }

    // API yanıtlarını deserialize etmek için model sınıfları
    public class SoilAnalysisResult
    {
        [JsonPropertyName("crop")]
        public string Crop { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; }
    }

    public class ImageAnalysisResult
    {
        [JsonPropertyName("disease")]
        public string Disease { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; }
    }
}