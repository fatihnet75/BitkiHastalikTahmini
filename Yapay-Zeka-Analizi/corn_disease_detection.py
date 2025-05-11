import tkinter as tk
from tkinter import filedialog, Label, Button, Toplevel
from PIL import Image, ImageTk
import tensorflow as tf
import numpy as np

# Modeli yükleme
model = tf.keras.models.load_model("corn_disease_cnn_model.h5")

# Sınıf etiketlerini tanımlama
class_labels = {0: "Yanık", 1: "Paslı Yaprak", 2: "Lekeli Yaprak", 3: "Sağlıklı"}

# Sınıf önerilerini tanımlama
recommendations = {
    "Yanık": "Düzenli ve yeterli sulama yapın.\nGübre kullanın, toprağın besin ihtiyacı eksik olduğunda yapraklar yanık olur.",
    "Paslı Yaprak": "Bitki stresini azaltın (susuzluk, aşırı sıcak gibi faktörlere dikkat edin).\nDengeli gübreleme yapın.\nEnfekte yaprakları elde toplayıp yakarak veya çöpe atarak uzaklaştırın.",
    "Lekeli Yaprak": "Aşırı sulama yapmayın.\nEkim rotasyonu yapın.\nYağmurlama sulama yerine damla sulama tercih edin.\nHer yıl aynı yere mısır ekmeyin.\nFasulye, buğday gibi farklı bitkilerle dönüşümlü ekim yaparak patojenin toprakta kalmasını engelleyin.",
    "Sağlıklı": "Bitkiniz sağlıklı görünüyor. Düzenli bakım ve gözlem yapmaya devam edin!"
}

# Tkinter ana pencere
window = tk.Tk()
window.title("Mısır Yaprağı Hastalık Tespiti")
window.geometry("600x400+400+200")

# Arka plan resmi
bg_image_path = "assortment-colored-plant-leaves.jpg"
try:
    bg_image = Image.open(bg_image_path)
    bg_image = bg_image.resize((600, 400))
    bg_photo = ImageTk.PhotoImage(bg_image)
    background_label = Label(window, image=bg_photo)
    background_label.image = bg_photo
    background_label.place(relwidth=1, relheight=1)
except Exception as e:
    print(f"Arka plan yüklenirken hata: {e}")
    window.configure(bg="lightblue")

# Görüntü seçme işlevi
def load_image():
    try:
        file_path = filedialog.askopenfilename(
            title="Görüntü Seçin",
            filetypes=[("JPEG Dosyaları", "*.jpg;*.jpeg"), ("PNG Dosyaları", "*.png"), ("Tüm Dosyalar", "*.*")]
        )

        if not file_path:
            print("Dosya seçilmedi.")
            return

        # Seçilen görüntü ile tahmin yapma
        open_result_window(file_path)
    except Exception as e:
        print(f"Görüntü seçilirken hata: {e}")

def open_result_window(image_path):
    result_window = Toplevel(window)
    result_window.title("Tahmin Sonucu")
    result_window.geometry("600x600+650+450")  # Yükseklik biraz artırıldı öneriler için

    # Farklı bir arka plan
    new_bg_image_path = "WhatsApp Image 2025-01-14 at 17.47.33.jpeg"
    try:
        bg_image = Image.open(new_bg_image_path)
        bg_image = bg_image.resize((400, 500))
        bg_photo = ImageTk.PhotoImage(bg_image)
        background_label = Label(result_window, image=bg_photo)
        background_label.image = bg_photo
        background_label.place(relwidth=1, relheight=1)
    except Exception as e:
        print(f"Arka plan yüklenirken hata: {e}")
        result_window.configure(bg="lightyellow")

    # Görüntü
    img = Image.open(image_path)
    img_resized = img.resize((300, 300))
    img_tk = ImageTk.PhotoImage(img_resized)
    img_label = Label(result_window, image=img_tk, bg="#ffffff")
    img_label.image = img_tk
    img_label.pack(pady=10)

    # Tahmin yapma
    img_height, img_width = 128, 128
    image = tf.keras.utils.load_img(image_path, target_size=(img_height, img_width))
    image_array = tf.keras.utils.img_to_array(image) / 255.0
    image_array = np.expand_dims(image_array, axis=0)

    predictions = model.predict(image_array)
    predicted_class = np.argmax(predictions, axis=1)[0]
    predicted_label = class_labels[predicted_class]
    confidence = np.max(predictions) * 100

    # Tahmin sonucu
    result_label = Label(result_window, text=f"Tahmin Sonucu: {predicted_label}", font=("Helvetica", 14, "bold"), bg="#ffffff")
    result_label.pack(pady=5)

    confidence_label = Label(result_window, text=f"Doğruluk Oranı: %{confidence:.2f}", font=("Helvetica", 12), bg="#ffffff")
    confidence_label.pack(pady=5)

    # Tahmine göre önerileri göster
    recommendation_text = recommendations.get(predicted_label, "Öneri bulunamadı.")
    recommendation_label = Label(result_window, text=f"Öneriler:\n{recommendation_text}", font=("Helvetica", 11), justify="left", wraplength=350, bg="#ffffff")
    recommendation_label.pack(pady=10)

    # Kapat butonu
    close_button = Button(result_window, text="Kapat", command=result_window.destroy, bg="#4682B4", fg="white", font=("Arial", 12, "bold"))
    close_button.pack(pady=10)

# Buton
btn_load = Button(window, text="Resim Seç", command=load_image, bg="#5CA4FF", fg="white", padx=10, pady=5, font=("Tahoma", 12, "bold"))
btn_load.place(relx=0.8, rely=0.1, anchor="center")

# Tkinter döngüsü
window.mainloop()
