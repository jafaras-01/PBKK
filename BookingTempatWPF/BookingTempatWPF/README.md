# Sistem Informasi Booking Tempat — WPF + C#

Aplikasi desktop WPF untuk pengelolaan penyewaan tempat/ruangan dengan database SQLite.

## Fitur
- Melihat katalog tempat aktif, lokasi, kapasitas, tarif, dan fasilitas.
- Mengajukan booking dengan data pemesan, acara, tanggal, jam, dan peserta.
- Status awal booking `Pending`.
- Admin dapat menyetujui atau menolak booking; alasan penolakan wajib diisi.
- Validasi kapasitas, tanggal/jam, dan bentrok dengan booking yang sudah disetujui.
- CRUD sederhana tempat: tambah, edit, aktif/nonaktif.
- Database SQLite otomatis dibuat di folder aplikasi dan diisi data contoh pada peluncuran pertama.

## Persyaratan
- Windows
- Visual Studio 2022 dengan workload **.NET desktop development**
- .NET 8 SDK

## Cara menjalankan
1. Ekstrak ZIP.
2. Buka `BookingTempatWPF.csproj` menggunakan Visual Studio 2022.
3. Restore NuGet packages bila diminta.
4. Jalankan dengan `F5`.
5. Database `booking_tempat.db` dibuat otomatis di folder output aplikasi.

Alternatif menggunakan terminal PowerShell dari folder proyek:
```powershell
dotnet restore
dotnet run
```

## Alur demo
1. Tab **Buat Booking**: isi nama, telepon, tempat, acara, jumlah peserta, tanggal dan jam.
2. Klik **Ajukan Booking**. Kode booking dan status Pending ditampilkan.
3. Tab **Kelola Booking (Admin)**: pilih pengajuan, klik **Setujui** atau **Tolak**.
4. Tab **Kelola Tempat**: pilih tempat pada tabel untuk edit, atau klik Form Baru untuk menambah tempat.

## Catatan penting
- Ini adalah starter project/demo lokal, belum aplikasi produksi multi-pengguna.
- Belum ada login/role sungguhan, sehingga tab admin masih dapat dibuka oleh siapa pun yang menjalankan aplikasi. Tambahkan autentikasi dan otorisasi sebelum dipakai operasional.
- Belum ada pembayaran online, notifikasi email/WhatsApp, laporan ekspor, kalender visual, audit log, dan jadwal perawatan.
- Untuk sistem multi-user, pindahkan database ke server (mis. SQL Server/PostgreSQL) dan implementasikan transaksi/constraint konflik jadwal di server.
- Harga dihitung per jam yang dibulatkan ke atas. Sesuaikan kebijakan tarif sesuai kebutuhan bisnis.
