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
    document.getElementById('currentYear').textContent = new Date().getFullYear();
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

// Deneme sayılarını güncelle
function updateTrialCounts() {
    document.getElementById('soilTrials').textContent = `Kalan deneme: ${MAX_TRIALS - trialCounts.soil}`;
    document.getElementById('imageTrials').textContent = `Kalan deneme: ${MAX_TRIALS - trialCounts.image}`;
}

// Bölümleri aç/kapat
function toggleSection(section) {
    const panel = document.getElementById(`${section}Panel`);
    if (panel.style.display === 'none') {
        panel.style.display = 'block';
    } else {
        panel.style.display = 'none';
    }
}

// Hata mesajını göster
function showError(message) {
    const errorDiv = document.getElementById('trialError');
    errorDiv.textContent = message;
    errorDiv.style.display = 'block';
    setTimeout(() => {
        errorDiv.style.display = 'none';
    }, 3000);
}

// Toprak analizi
function handleSoilAnalysis(event) {
    event.preventDefault();

    if (trialCounts.soil >= MAX_TRIALS) {
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
    
    document.getElementById('soilResultText').textContent = result;
    document.getElementById('soilResult').style.display = 'block';

    // Deneme sayısını güncelle
    trialCounts.soil++;
    localStorage.setItem('soilTrialCount', trialCounts.soil);
    updateTrialCounts();
}

// Görüntü analizi
function handleImageAnalysis(event) {
    event.preventDefault();

    if (trialCounts.image >= MAX_TRIALS) {
        showError('Deneme haklarınızı bitirdiniz. Lütfen daha fazlası için giriş yapınız.');
        return;
    }

    // Backend'e gönderilecek görüntü işleme
    const result = analyzeImage();
    
    document.getElementById('imageResultText').textContent = result;
    document.getElementById('imageResult').style.display = 'block';

    // Deneme sayısını güncelle
    trialCounts.image++;
    localStorage.setItem('imageTrialCount', trialCounts.image);
    updateTrialCounts();
}

// Görüntü önizleme
function handleImageChange(event) {
    const file = event.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function(e) {
            const preview = document.getElementById('imagePreview');
            preview.innerHTML = `<img src="${e.target.result}" alt="Seçilen bitki">`;
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
    registerModal.style.display = 'none';
    loginModal.style.display = loginModal.style.display === 'none' ? 'flex' : 'none';
}

function toggleRegisterModal() {
    const loginModal = document.getElementById('loginModal');
    const registerModal = document.getElementById('registerModal');
    loginModal.style.display = 'none';
    registerModal.style.display = registerModal.style.display === 'none' ? 'flex' : 'none';
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
        errorDiv.textContent = 'Geçersiz kullanıcı adı veya şifre!';
        errorDiv.style.display = 'block';
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
    updateHeaderButtons();
    updateProfileView();
    toggleLoginModal();
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
    if (soilCount && imageCount) {
        soilCount.textContent = localStorage.getItem('soilTrialCount') || '0';
        imageCount.textContent = localStorage.getItem('imageTrialCount') || '0';
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

    document.getElementById('totalUsers').textContent = stats.totalUsers;
    document.getElementById('totalAnalysis').textContent = stats.totalAnalysis;
    document.getElementById('todayAnalysis').textContent = stats.todayAnalysis;
    document.getElementById('activeUsers').textContent = stats.activeUsers;
}

// Modal kapatma işlemleri
document.addEventListener('DOMContentLoaded', () => {
    const closeButtons = document.querySelectorAll('.close-modal');
    closeButtons.forEach(button => {
        button.addEventListener('click', () => {
            const modal = button.closest('.modal');
            modal.style.display = 'none';
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
    document.getElementById('footerText').textContent = footerText;
    
    if (window.location.pathname.includes('admin.html')) {
        // Admin panelindeki form alanlarını doldur
        document.getElementById('footerText').value = footerText;
        
        // Hakkında sayfası içerikleri
        document.getElementById('aboutTitle').value = localStorage.getItem('aboutTitle') || 'Dr.PlantAI Nedir?';
        document.getElementById('aboutContent').value = localStorage.getItem('aboutContent') || 
            'Dr.PlantAI, yapay zeka teknolojisini kullanarak bitki hastalıklarını tespit eden ve toprak analizleri yapan yenilikçi bir platformdur. Amacımız, çiftçilere ve bitki yetiştiricilerine hızlı, doğru ve ekonomik çözümler sunmaktır.';
        
        // Özellikler
        document.getElementById('feature1Title').value = localStorage.getItem('feature1Title') || 'Görüntü Analizi';
        document.getElementById('feature1Content').value = localStorage.getItem('feature1Content') || 
            'Yapay zeka destekli görüntü analizi ile bitki hastalıklarını hızlı ve doğru şekilde tespit ediyoruz.';
        
        document.getElementById('feature2Title').value = localStorage.getItem('feature2Title') || 'Toprak Analizi';
        document.getElementById('feature2Content').value = localStorage.getItem('feature2Content') || 
            'Toprak değerlerini analiz ederek en uygun gübre ve bakım önerilerini sunuyoruz.';
        
        document.getElementById('feature3Title').value = localStorage.getItem('feature3Title') || 'Hızlı Sonuç';
        document.getElementById('feature3Content').value = localStorage.getItem('feature3Content') || 
            'Saniyeler içinde analiz sonuçlarını alın ve çözüm önerilerimizden faydalanın.';
        
        document.getElementById('feature4Title').value = localStorage.getItem('feature4Title') || 'Uzman Desteği';
        document.getElementById('feature4Content').value = localStorage.getItem('feature4Content') || 
            'Tarım uzmanlarımız tarafından hazırlanan öneriler ve çözümler.';
    }

    if (window.location.pathname.includes('about.html')) {
        // Hakkında sayfasındaki içerikleri güncelle
        document.querySelector('.about-section h2').textContent = 
            localStorage.getItem('aboutTitle') || 'Dr.PlantAI Nedir?';
        document.querySelector('.about-section p').textContent = 
            localStorage.getItem('aboutContent');

        const features = document.querySelectorAll('.feature-card');
        if (features.length >= 4) {
            features[0].querySelector('h3').textContent = localStorage.getItem('feature1Title') || 'Görüntü Analizi';
            features[0].querySelector('p').textContent = localStorage.getItem('feature1Content');
            
            features[1].querySelector('h3').textContent = localStorage.getItem('feature2Title') || 'Toprak Analizi';
            features[1].querySelector('p').textContent = localStorage.getItem('feature2Content');
            
            features[2].querySelector('h3').textContent = localStorage.getItem('feature3Title') || 'Hızlı Sonuç';
            features[2].querySelector('p').textContent = localStorage.getItem('feature3Content');
            
            features[3].querySelector('h3').textContent = localStorage.getItem('feature4Title') || 'Uzman Desteği';
            features[3].querySelector('p').textContent = localStorage.getItem('feature4Content');
        }
    }
}

// İçerik yönetimi form işleme
function handleContentManagement(event) {
    event.preventDefault();
    
    // Footer metni
    const footerText = document.getElementById('footerText').value;
    localStorage.setItem('footerText', footerText);
    
    // Hakkında sayfası içerikleri
    localStorage.setItem('aboutTitle', document.getElementById('aboutTitle').value);
    localStorage.setItem('aboutContent', document.getElementById('aboutContent').value);
    
    // Özellikler
    localStorage.setItem('feature1Title', document.getElementById('feature1Title').value);
    localStorage.setItem('feature1Content', document.getElementById('feature1Content').value);
    
    localStorage.setItem('feature2Title', document.getElementById('feature2Title').value);
    localStorage.setItem('feature2Content', document.getElementById('feature2Content').value);
    
    localStorage.setItem('feature3Title', document.getElementById('feature3Title').value);
    localStorage.setItem('feature3Content', document.getElementById('feature3Content').value);
    
    localStorage.setItem('feature4Title', document.getElementById('feature4Title').value);
    localStorage.setItem('feature4Content', document.getElementById('feature4Content').value);
    
    // Sayfadaki içeriği güncelle
    loadContent();
    
    // Başarı mesajı göster
    alert('İçerikler başarıyla güncellendi!');
} 