# Tempatindan — Sistem Informasi Booking Tempat (WPF)

Aplikasi desktop WPF dengan C# dan SQLite untuk pengajuan booking tempat serta persetujuan admin. Versi ini tidak memiliki tarif, total biaya, pembayaran, atau fitur transaksi.

## Fitur
- Melihat daftar tempat aktif, lokasi, kapasitas, dan fasilitas.
- Mengajukan booking dengan nama pemesan, nomor telepon, tempat, nama acara, jumlah peserta, tanggal, dan jam.
- Validasi waktu, kapasitas tempat, serta bentrok jadwal yang sudah disetujui.
- Status booking: Pending, Approved, dan Rejected.
- Admin dapat menyetujui atau menolak booking. Alasan penolakan wajib diisi.
- Mengelola tempat: tambah/edit, lokasi, kapasitas, fasilitas, dan status aktif.
- Database SQLite dibuat otomatis dengan nama `booking_tempat_nopayment.db` di folder aplikasi. Database lama tidak dihapus atau diubah.

## Tempat default
- CCWS
- IUP
- Kelas 105 Teknik Informatika
- Kelas 107 Teknik Informatika
- Aula Handayani
- Lab Pemograman 1
- Lab Pemograman 2

## Cara menjalankan
1. Buka `Tempatindan.csproj` menggunakan Visual Studio 2022 atau lebih baru dengan workload .NET Desktop Development.
2. Pastikan SDK .NET 8 terpasang dan internet tersedia saat restore NuGet pertama kali.
3. Jalankan **Build > Restore NuGet Packages**, lalu **Build Solution**.
4. Jalankan dengan tombol **F5**.

Aplikasi menggunakan `Microsoft.Data.Sqlite` versi 8.0.11. Tidak ada proses pembayaran atau penghitungan harga di versi ini.
