Tempatindan - versi revisi pengguna

Perubahan:
- Antarmuka utama, warna, dan gaya visual dipertahankan.
- Sidebar kini bisa digunakan untuk berpindah ke Buat Reservasi, Riwayat Booking, dan Daftar Ruangan.
- Fitur persetujuan admin dan pengelolaan ruangan dihilangkan dari tampilan pengguna.
- Booking baru langsung tercatat sebagai Approved karena aplikasi tidak lagi menggunakan alur persetujuan admin.
- Bentrok jadwal dicek terhadap booking Pending maupun Approved pada ruangan dan rentang waktu yang sama.
- Daftar ruangan hanya menampilkan ruangan aktif.
- Database lama yang disertakan tetap berada di jalur output sebelumnya agar data lokal dapat dipertahankan.

Cara membuka:
1. Ekstrak ZIP.
2. Buka folder Tempatindan.
3. Buka Tempatindan.csproj di Visual Studio.
4. Restore NuGet packages jika diminta, lalu jalankan aplikasi.

Catatan:
- Project ini menggunakan .NET 8 WPF dan Microsoft.Data.Sqlite.
- Pengujian build tidak dapat dijalankan di lingkungan ini karena .NET SDK tidak tersedia. File XAML sudah diperiksa agar valid secara XML dan semua event UI yang digunakan memiliki handler.
