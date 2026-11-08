# AuliaPrakGrafkom2 — Algoritma Penggambaran Objek Primitif

Aplikasi **C# Windows Forms (.NET / GDI+)** untuk Praktikum 2 Grafika Komputer.
Enam algoritma penggambaran primitif diintegrasikan dalam satu GUI dan digambar
**piksel demi piksel** melalui helper `PutPixel` (tanpa `DrawLine`/`DrawEllipse`).

## Fitur

- **Garis:** Algoritma Dasar, DDA, Bresenham (8 oktan)
- **Lingkaran:** Algoritma Dasar, Polar, Bresenham (midpoint, simetri 8 oktan)
- Tiga kanvas 220 × 220 berdampingan untuk membandingkan hasil
- Back-buffer `Bitmap` per kanvas → gambar tidak hilang saat minimize/refresh
- Validasi input integer dan validasi radius
- Nilai default siap uji: garis `(20,20) → (180,150)`, lingkaran `(110,110)` r = `80`

## Prasyarat

- Windows 10/11
- [.NET SDK 10.0](https://dotnet.microsoft.com/download) (atau versi yang mendukung Windows Forms)

Cek instalasi:

```powershell
dotnet --version
dotnet --list-sdks
```

## Menjalankan Proyek

Dari folder root solusi (`AuliaPrakGrafkom2`):

```powershell
dotnet build .\AuliaPrakGrafkom2.slnx
dotnet run --project .\AuliaPrakGrafkom2\AuliaPrakGrafkom2.csproj
```

Atau langsung melalui executable hasil build:

```powershell
.\AuliaPrakGrafkom2\bin\Debug\net10.0-windows\AuliaPrakGrafkom2.exe
```

### Membuat Proyek Baru dari Nol (opsional)

Jika ingin membuat ulang proyek WinForms kosong:

```powershell
dotnet new winforms -n AuliaPrakGrafkom2
cd AuliaPrakGrafkom2
dotnet run
```

Lalu ganti isi `Program.cs` dan `Form1.cs` dengan kode pada proyek ini.

## Cara Menggunakan

1. Jalankan aplikasi.
2. **Bagian Garis:** isi `X1`, `Y1`, `X2`, `Y2` (bilangan bulat), lalu klik **Gambar**.
   Garis yang sama muncul di kanvas **DASAR**, **DDA**, dan **Bresenham**.
3. **Bagian Lingkaran:** isi `Xc`, `Yc`, dan `Radius`, lalu klik **Gambar**.
   Lingkaran muncul di kanvas **DASAR**, **Polar**, dan **Bresenham**.
4. Klik **Bersihkan** untuk mengosongkan kanvas pada bagian terkait.

## Struktur Berkas

```
AuliaPrakGrafkom2/
├── AuliaPrakGrafkom2.slnx
├── LAPORAN_PRAKTIKUM.md        # laporan praktikum (poin 8)
├── README.md                   # panduan ini
├── docs/                       # screenshot hasil pengujian
└── AuliaPrakGrafkom2/
    ├── Program.cs              # entry point aplikasi
    ├── Form1.cs                # GUI + seluruh algoritma
    ├── Form1.Designer.cs       # komponen dasar form
    └── AuliaPrakGrafkom2.csproj
```

## Catatan Implementasi

- `PutPixel` menggunakan `g.FillRectangle(brush, x, y, 2, 2)` agar piksel jelas terlihat.
- Semua koordinat digambar ke `Bitmap` back-buffer; event `Paint` hanya menyalin bitmap.
- `DrawLineDasar` menangani garis vertikal dan menukar titik bila `x1 > x2`.
- `DrawLineBresenham` memakai parameter keputusan integer `∇ = 2Δb − Δa` untuk semua 8 oktan.
- `DrawCircleBresenham` memakai `d = 3 − 2r` dan `DrawCirclePoints` untuk 8 titik simetris.
- `DrawCirclePolar` memakai `Δθ = min(1/r, 0.005)` agar busur rapat.
