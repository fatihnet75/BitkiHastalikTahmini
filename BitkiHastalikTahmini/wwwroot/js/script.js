// Sabit değişkenler
const MAX_TRIALS = 2;
let trialCounts = {
    soil: parseInt(localStorage.getItem('soilTrialCount') || '0'),
    image: parseInt(localStorage.getItem('imageTrialCount') || '0')
};

// Kullanıcı durumu kontrolü
let isLoggedIn = false;
let isAdmin = false;

// Sayfa yüklendiğinde
document.addEventListener('DOMContentLoaded', () => {
    updateTrialCounts();
    const currentYearElement = document.getElementById('currentYear');
    if (currentYearElement) {
        currentYearElement.textContent = new Date().getFullYear();
    }
    setupSidebarToggle();
    checkLoginStatus();
    loadContent();

    // Profil sayfası için
    updateProfileView();
    updateProfileStats();

    // Admin paneli için
    if (window.location.pathname.includes('admin.html')) {
        if (!isAdmin) {
            window.location.href = 'admin-login.html';
        }
        updateAdminStats();

        // İçerik yönetimi form işleyicisi
        const contentForm = document.getElementById('contentManagementForm');
        if (contentForm) {
            contentForm.addEventListener('submit', handleContentManagement);
        }
    }
});

// Sidebar toggle işlevi
function setupSidebarToggle() {
    const sidebarToggle = document.getElementById('sidebar-toggle');
    const sidebar = document.querySelector('.sidebar');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', () => {
            sidebar.classList.toggle('active');
        });

        // Sidebar dışına tıklandığında kapanma
        document.addEventListener('click', (e) => {
            if (!sidebar.contains(e.target) && !sidebarToggle.contains(e.target) && sidebar.classList.contains('active')) {
                sidebar.classList.remove('active');
            }
        });
    }
}

// Deneme sayılarını güncelle
function updateTrialCounts() {
    const soilTrialsElement = document.getElementById('soilTrials');
    const imageTrialsElement = document.getElementById('imageTrials');

    if (soilTrialsElement) {
        soilTrialsElement.textContent = `Kalan deneme: ${MAX_TRIALS - trialCounts.soil}`;
    }

    if (imageTrialsElement) {
        imageTrialsElement.textContent = `Kalan deneme: ${MAX_TRIALS - trialCounts.image}`;
    }
}

// Bölümleri aç/kapat
function toggleSection(section) {
    const panel = document.getElementById(`${section}Panel`);
    if (panel) {
        if (panel.style.display === 'none') {
            panel.style.display = 'block';
        } else {
            panel.style.display = 'none';
        }
    }
}

// Hata mesajını göster
function showError(message) {
    const errorDiv = document.getElementById('trialError');
    if (errorDiv) {
        errorDiv.textContent = message;
        errorDiv.style.display = 'block';
        setTimeout(() => {
            errorDiv.style.display = 'none';
        }, 3000);
    }
}

// Toprak analizi
function handleSoilAnalysis(event) {
    event.preventDefault();

    if (trialCounts.soil >= MAX_TRIALS && !isLoggedIn) {
        showError('Deneme haklarınızı bitirdiniz. Lütfen daha fazlası için giriş yapınız.');
        return;
    }

    const formData = new FormData(event.target);
    const data = {
        ph: formData.get('ph'),
        moisture: formData.get('moisture'),
        fertilizer: formData.get('fertilizer')
    };

    // Backend'e gönderilecek veri
    const result = analyzeSoilData(data);

    const soilResultText = document.getElementById('soilResultText');
    const soilResult = document.getElementById('soilResult');

    if (soilResultText && soilResult) {
        soilResultText.textContent = result;
        soilResult.style.display = 'block';
    }

    // Deneme sayısını güncelle (sadece giriş yapmamış kullanıcılar için)
    if (!isLoggedIn) {
        trialCounts.soil++;
        localStorage.setItem('soilTrialCount', trialCounts.soil);
        updateTrialCounts();
    }
}

// Görüntü analizi
function handleImageAnalysis(event) {
    event.preventDefault();

    if (trialCounts.image >= MAX_TRIALS && !isLoggedIn) {
        showError('Deneme haklarınızı bitirdiniz. Lütfen daha fazlası için giriş yapınız.');
        return;
    }

    // Backend'e gönderilecek görüntü işleme
    const result = analyzeImage();

    const imageResultText = document.getElementById('imageResultText');
    const imageResult = document.getElementById('imageResult');

    if (imageResultText && imageResult) {
        imageResultText.textContent = result;
        imageResult.style.display = 'block';
    }

    // Deneme sayısını güncelle (sadece giriş yapmamış kullanıcılar için)
    if (!isLoggedIn) {
        trialCounts.image++;
        localStorage.setItem('imageTrialCount', trialCounts.image);
        updateTrialCounts();
    }
}

// Görüntü önizleme
function handleImageChange(event) {
    const file = event.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const preview = document.getElementById('imagePreview');
            if (preview) {
                preview.innerHTML = `<img src="${e.target.result}" alt="Seçilen bitki">`;
            }
        };
        reader.readAsDataURL(file);
    }
}

// Backend olmadan örnek analiz fonksiyonları
function analyzeSoilData(data) {
    // Bu fonksiyon normalde backend'e istek atacak
    return `Toprak Analiz Sonucu:\nPH: ${data.ph}\nNem: ${data.moisture}%\nGübre: ${data.fertilizer} kg/m²\n\nÖrnek sonuç: Toprak değerleri normal aralıkta.`;
}

function analyzeImage() {
    // Bu fonksiyon normalde backend'e istek atacak
    return "Görüntü Analiz Sonucu: Bitkiniz sağlıklı görünüyor.";
}

// Login/Register işlemleri
function toggleLoginModal() {
    const loginModal = document.getElementById('loginModal');
    const registerModal = document.getElementById('registerModal');

    if (loginModal && registerModal) {
        registerModal.style.display = 'none';
        loginModal.style.display = loginModal.style.display === 'none' ? 'flex' : 'none';
    }
}

function toggleRegisterModal() {
    const loginModal = document.getElementById('loginModal');
    const registerModal = document.getElementById('registerModal');

    if (loginModal && registerModal) {
        loginModal.style.display = 'none';
        registerModal.style.display = registerModal.style.display === 'none' ? 'flex' : 'none';
    }
}

// Login durumunu kontrol et
function checkLoginStatus() {
    isLoggedIn = localStorage.getItem('isLoggedIn') === 'true';
    isAdmin = localStorage.getItem('isAdmin') === 'true';
    updateHeaderButtons();
}

// Header butonlarını güncelle
function updateHeaderButtons() {
    const loginBtn = document.querySelector('.login-button');
    const logoutBtn = document.querySelector('.logout-button');

    if (loginBtn) {
        loginBtn.style.display = isLoggedIn ? 'none' : 'block';
    }
    if (logoutBtn) {
        logoutBtn.style.display = isLoggedIn ? 'block' : 'none';
    }
}

// Admin girişi
function handleAdminLogin(event) {
    event.preventDefault();
    const formData = new FormData(event.target);
    const username = formData.get('username');
    const password = formData.get('password');

    if (username === 'admin' && password === 'admin123') {
        localStorage.setItem('isAdmin', 'true');
        localStorage.setItem('isLoggedIn', 'true');
        window.location.href = 'admin.html';
    } else {
        const errorDiv = document.getElementById('adminLoginError');
        if (errorDiv) {
            errorDiv.textContent = 'Geçersiz kullanıcı adı veya şifre!';
            errorDiv.style.display = 'block';
        }
    }
}

// Profil görünümünü güncelle
function updateProfileView() {
    const guestView = document.getElementById('guestView');
    const userView = document.getElementById('userView');

    if (guestView && userView) {
        if (isLoggedIn) {
            guestView.style.display = 'none';
            userView.style.display = 'block';
        } else {
            guestView.style.display = 'block';
            userView.style.display = 'none';
        }
    }
}

// Login işlemi
function handleLogin(event) {
    event.preventDefault();
    const formData = new FormData(event.target);
    const data = {
        email: formData.get('email'),
        password: formData.get('password')
    };

    // Örnek login işlemi
    localStorage.setItem('isLoggedIn', 'true');
    isLoggedIn = true;

    // Kullanıcı bilgilerini localStorage'a kaydet
    localStorage.setItem('userEmail', data.email);

    updateHeaderButtons();
    updateProfileView();
    toggleLoginModal();

    // Deneme haklarını sıfırla (opsiyonel)
    if (isLoggedIn) {
        trialCounts.soil = 0;
        trialCounts.image = 0;
        localStorage.setItem('soilTrialCount', '0');
        localStorage.setItem('imageTrialCount', '0');
        updateTrialCounts();
    }
}

// Register işlemi - Eksik olan fonksiyon
function handleRegister(event) {
    event.preventDefault();
    const formData = new FormData(event.target);
    const data = {
        fullName: formData.get('fullName'),
        email: formData.get('email'),
        password: formData.get('password'),
        passwordConfirm: formData.get('passwordConfirm')
    };

    // Şifre kontrolü
    if (data.password !== data.passwordConfirm) {
        alert('Şifreler eşleşmiyor!');
        return;
    }

    // Kullanıcı bilgilerini localStorage'a kaydet
    localStorage.setItem('userFullName', data.fullName);
    localStorage.setItem('userEmail', data.email);
    localStorage.setItem('userPassword', data.password); // Gerçek uygulamada şifre şifrelenmelidir!

    // Giriş durumunu güncelle
    localStorage.setItem('isLoggedIn', 'true');
    isLoggedIn = true;

    // UI güncellemesi
    updateHeaderButtons();
    updateProfileView();
    toggleRegisterModal();

    // Hoşgeldiniz mesajı göster
    alert(`Hoş geldiniz, ${data.fullName}! Başarıyla kayıt oldunuz.`);
}

// Logout işlemi
function handleLogout() {
    localStorage.removeItem('isLoggedIn');
    localStorage.removeItem('isAdmin');
    isLoggedIn = false;
    isAdmin = false;
    updateHeaderButtons();
    updateProfileView();
    if (window.location.pathname.includes('admin.html')) {
        window.location.href = 'index.html';
    }
}

// Profil sayfası fonksiyonları
function updateProfileStats() {
    const soilCount = document.getElementById('soilAnalysisCount');
    const imageCount = document.getElementById('imageAnalysisCount');
    const userNameElement = document.getElementById('userName');
    const userEmailElement = document.getElementById('userEmail');

    if (soilCount && imageCount) {
        soilCount.textContent = localStorage.getItem('soilTrialCount') || '0';
        imageCount.textContent = localStorage.getItem('imageTrialCount') || '0';
    }

    // Profil sayfasında kullanıcı bilgilerini göster
    if (userNameElement && isLoggedIn) {
        userNameElement.textContent = localStorage.getItem('userFullName') || 'Kullanıcı';
    }

    if (userEmailElement && isLoggedIn) {
        userEmailElement.textContent = localStorage.getItem('userEmail') || 'kullanici@ornek.com';
    }
}

// Admin paneli fonksiyonları
function updateAdminStats() {
    // Backend'den istatistikleri al
    const stats = {
        totalUsers: 100,
        totalAnalysis: 250,
        todayAnalysis: 25,
        activeUsers: 15
    };

    const totalUsersElement = document.getElementById('totalUsers');
    const totalAnalysisElement = document.getElementById('totalAnalysis');
    const todayAnalysisElement = document.getElementById('todayAnalysis');
    const activeUsersElement = document.getElementById('activeUsers');

    if (totalUsersElement) totalUsersElement.textContent = stats.totalUsers;
    if (totalAnalysisElement) totalAnalysisElement.textContent = stats.totalAnalysis;
    if (todayAnalysisElement) todayAnalysisElement.textContent = stats.todayAnalysis;
    if (activeUsersElement) activeUsersElement.textContent = stats.activeUsers;
}

// Modal kapatma işlemleri
document.addEventListener('DOMContentLoaded', () => {
    const closeButtons = document.querySelectorAll('.close-modal');
    closeButtons.forEach(button => {
        button.addEventListener('click', () => {
            const modal = button.closest('.modal');
            if (modal) {
                modal.style.display = 'none';
            }
        });
    });

    // Login butonuna tıklandığında
    const loginButtons = document.querySelectorAll('.login-button');
    loginButtons.forEach(button => {
        button.addEventListener('click', toggleLoginModal);
    });

    // Logout butonuna tıklandığında
    const logoutButton = document.querySelector('.logout-button');
    if (logoutButton) {
        logoutButton.addEventListener('click', handleLogout);
    }
});

// İçerik yönetimi
function loadContent() {
    // Footer metni
    const footerText = localStorage.getItem('footerText') || 'Bitkilerinizin sağlığı bizim önceliğimizdir.';
    const footerTextElement = document.getElementById('footerText');
    if (footerTextElement) {
        footerTextElement.textContent = footerText;
    }

    if (window.location.pathname.includes('admin.html')) {
        // Admin panelindeki form alanlarını doldur
        const adminFooterTextElement = document.getElementById('footerText');
        const aboutTitleElement = document.getElementById('aboutTitle');
        const aboutContentElement = document.getElementById('aboutContent');
        const feature1TitleElement = document.getElementById('feature1Title');
        const feature1ContentElement = document.getElementById('feature1Content');
        const feature2TitleElement = document.getElementById('feature2Title');
        const feature2ContentElement = document.getElementById('feature2Content');
        const feature3TitleElement = document.getElementById('feature3Title');
        const feature3ContentElement = document.getElementById('feature3Content');
        const feature4TitleElement = document.getElementById('feature4Title');
        const feature4ContentElement = document.getElementById('feature4Content');

        if (adminFooterTextElement) adminFooterTextElement.value = footerText;

        // Hakkında sayfası içerikleri
        if (aboutTitleElement) aboutTitleElement.value = localStorage.getItem('aboutTitle') || 'Dr.PlantAI Nedir?';
        if (aboutContentElement) aboutContentElement.value = localStorage.getItem('aboutContent') ||
            'Dr.PlantAI, yapay zeka teknolojisini kullanarak bitki hastalıklarını tespit eden ve toprak analizleri yapan yenilikçi bir platformdur. Amacımız, çiftçilere ve bitki yetiştiricilerine hızlı, doğru ve ekonomik çözümler sunmaktır.';

        // Özellikler
        if (feature1TitleElement) feature1TitleElement.value = localStorage.getItem('feature1Title') || 'Görüntü Analizi';
        if (feature1ContentElement) feature1ContentElement.value = localStorage.getItem('feature1Content') ||
            'Yapay zeka destekli görüntü analizi ile bitki hastalıklarını hızlı ve doğru şekilde tespit ediyoruz.';

        if (feature2TitleElement) feature2TitleElement.value = localStorage.getItem('feature2Title') || 'Toprak Analizi';
        if (feature2ContentElement) feature2ContentElement.value = localStorage.getItem('feature2Content') ||
            'Toprak değerlerini analiz ederek en uygun gübre ve bakım önerilerini sunuyoruz.';

        if (feature3TitleElement) feature3TitleElement.value = localStorage.getItem('feature3Title') || 'Hızlı Sonuç';
        if (feature3ContentElement) feature3ContentElement.value = localStorage.getItem('feature3Content') ||
            'Saniyeler içinde analiz sonuçlarını alın ve çözüm önerilerimizden faydalanın.';

        if (feature4TitleElement) feature4TitleElement.value = localStorage.getItem('feature4Title') || 'Uzman Desteği';
        if (feature4ContentElement) feature4ContentElement.value = localStorage.getItem('feature4Content') ||
            'Tarım uzmanlarımız tarafından hazırlanan öneriler ve çözümler.';
    }

    if (window.location.pathname.includes('about.html')) {
        // Hakkında sayfasındaki içerikleri güncelle
        const aboutTitleElement = document.querySelector('.about-section h2');
        const aboutContentElement = document.querySelector('.about-section p');

        if (aboutTitleElement) {
            aboutTitleElement.textContent = localStorage.getItem('aboutTitle') || 'Dr.PlantAI Nedir?';
        }

        if (aboutContentElement) {
            aboutContentElement.textContent = localStorage.getItem('aboutContent') ||
                'Dr.PlantAI, yapay zeka teknolojisini kullanarak bitki hastalıklarını tespit eden ve toprak analizleri yapan yenilikçi bir platformdur.';
        }

        const features = document.querySelectorAll('.feature-card');
        if (features.length >= 4) {
            if (features[0].querySelector('h3')) {
                features[0].querySelector('h3').textContent = localStorage.getItem('feature1Title') || 'Görüntü Analizi';
            }
            if (features[0].querySelector('p')) {
                features[0].querySelector('p').textContent = localStorage.getItem('feature1Content') ||
                    'Yapay zeka destekli görüntü analizi ile bitki hastalıklarını hızlı ve doğru şekilde tespit ediyoruz.';
            }

            if (features[1].querySelector('h3')) {
                features[1].querySelector('h3').textContent = localStorage.getItem('feature2Title') || 'Toprak Analizi';
            }
            if (features[1].querySelector('p')) {
                features[1].querySelector('p').textContent = localStorage.getItem('feature2Content') ||
                    'Toprak değerlerini analiz ederek en uygun gübre ve bakım önerilerini sunuyoruz.';
            }

            if (features[2].querySelector('h3')) {
                features[2].querySelector('h3').textContent = localStorage.getItem('feature3Title') || 'Hızlı Sonuç';
            }
            if (features[2].querySelector('p')) {
                features[2].querySelector('p').textContent = localStorage.getItem('feature3Content') ||
                    'Saniyeler içinde analiz sonuçlarını alın ve çözüm önerilerimizden faydalanın.';
            }

            if (features[3].querySelector('h3')) {
                features[3].querySelector('h3').textContent = localStorage.getItem('feature4Title') || 'Uzman Desteği';
            }
            if (features[3].querySelector('p')) {
                features[3].querySelector('p').textContent = localStorage.getItem('feature4Content') ||
                    'Tarım uzmanlarımız tarafından hazırlanan öneriler ve çözümler.';
            }
        }
    }
}

// İçerik yönetimi form işleme
function handleContentManagement(event) {
    event.preventDefault();

    // Footer metni
    const footerTextElement = document.getElementById('footerText');
    if (footerTextElement) {
        localStorage.setItem('footerText', footerTextElement.value);
    }

    // Hakkında sayfası içerikleri
    const aboutTitleElement = document.getElementById('aboutTitle');
    const aboutContentElement = document.getElementById('aboutContent');
    const feature1TitleElement = document.getElementById('feature1Title');
    const feature1ContentElement = document.getElementById('feature1Content');
    const feature2TitleElement = document.getElementById('feature2Title');
    const feature2ContentElement = document.getElementById('feature2Content');
    const feature3TitleElement = document.getElementById('feature3Title');
    const feature3ContentElement = document.getElementById('feature3Content');
    const feature4TitleElement = document.getElementById('feature4Title');
    const feature4ContentElement = document.getElementById('feature4Content');

    if (aboutTitleElement) localStorage.setItem('aboutTitle', aboutTitleElement.value);
    if (aboutContentElement) localStorage.setItem('aboutContent', aboutContentElement.value);

    // Özellikler
    if (feature1TitleElement) localStorage.setItem('feature1Title', feature1TitleElement.value);
    if (feature1ContentElement) localStorage.setItem('feature1Content', feature1ContentElement.value);

    if (feature2TitleElement) localStorage.setItem('feature2Title', feature2TitleElement.value);
    if (feature2ContentElement) localStorage.setItem('feature2Content', feature2ContentElement.value);

    if (feature3TitleElement) localStorage.setItem('feature3Title', feature3TitleElement.value);
    if (feature3ContentElement) localStorage.setItem('feature3Content', feature3ContentElement.value);

    if (feature4TitleElement) localStorage.setItem('feature4Title', feature4TitleElement.value);
    if (feature4ContentElement) localStorage.setItem('feature4Content', feature4ContentElement.value);

    // Sayfadaki içeriği güncelle
    loadContent();

    // Başarı mesajı göster
    alert('İçerikler başarıyla güncellendi!');
}