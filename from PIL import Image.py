from PIL import Image
import os

def cut_image_vertical(image_path, n, output_folder="output"):
    img = Image.open(image_path)
    width, height = img.size

    if width % n != 0:
        print("⚠️ Chiều rộng không chia hết cho n, ảnh cuối có thể lệch")

    part_width = width // n

    os.makedirs(output_folder, exist_ok=True)

    for i in range(n):
        left = i * part_width
        right = (i + 1) * part_width if i != n - 1 else width

        cropped = img.crop((left, 0, right, height))
        cropped.save(f"{output_folder}/part_{i+1}.png")

    print(f"✅ Đã cắt ảnh thành {n} phần theo WIDTH")

# ====== SỬ DỤNG ======
cut_image_vertical("ground.png", 53)
