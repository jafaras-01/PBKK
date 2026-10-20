using Microsoft.Data.Sqlite;
using System.IO;

namespace BookingTempatWPF;

public static class Database
{
    private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "booking_tempat.db");
    private static string ConnectionString => $"Data Source={DbPath}";

    public static SqliteConnection Open()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();
        return connection;
    }

    public static void Initialize()
    {
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Rooms (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Location TEXT NOT NULL,
    Capacity INTEGER NOT NULL CHECK(Capacity > 0),
    PricePerHour NUMERIC NOT NULL CHECK(PricePerHour >= 0),
    Facilities TEXT NOT NULL DEFAULT '',
    IsActive INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS Bookings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BookingCode TEXT NOT NULL UNIQUE,
    CustomerName TEXT NOT NULL,
    CustomerPhone TEXT NOT NULL,
    RoomId INTEGER NOT NULL,
    StartAt TEXT NOT NULL,
    EndAt TEXT NOT NULL,
    ParticipantCount INTEGER NOT NULL,
    EventName TEXT NOT NULL,
    TotalAmount NUMERIC NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Pending',
    DecisionNote TEXT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY(RoomId) REFERENCES Rooms(Id)
);
CREATE INDEX IF NOT EXISTS IX_Bookings_Room_Time_Status ON Bookings(RoomId, StartAt, EndAt, Status);
";
        cmd.ExecuteNonQuery();

        using var count = con.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM Rooms;";
        if (Convert.ToInt32(count.ExecuteScalar()) == 0)
        {
            using var seed = con.CreateCommand();
            seed.CommandText = @"
INSERT INTO Rooms(Name, Location, Capacity, PricePerHour, Facilities) VALUES
('Aula Merdeka','Gedung A, Lantai 1',100,250000,'Proyektor, AC, Sound system'),
('Ruang Rapat Kenanga','Gedung B, Lantai 2',20,100000,'TV, Whiteboard, AC'),
('Hall Nusantara','Gedung Utama',250,500000,'Panggung, Proyektor, Sound system, AC');";
            seed.ExecuteNonQuery();
        }
    }

    public static List<Room> GetRooms(bool activeOnly = false)
    {
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT Id,Name,Location,Capacity,PricePerHour,Facilities,IsActive FROM Rooms " +
                          (activeOnly ? "WHERE IsActive=1 " : "") + "ORDER BY Name;";
        using var reader = cmd.ExecuteReader();
        var result = new List<Room>();
        while (reader.Read())
            result.Add(new Room
            {
                Id = reader.GetInt32(0), Name = reader.GetString(1), Location = reader.GetString(2),
                Capacity = reader.GetInt32(3), PricePerHour = reader.GetDecimal(4),
                Facilities = reader.GetString(5), IsActive = reader.GetInt32(6) == 1
            });
        return result;
    }

    public static void SaveRoom(Room room)
    {
        using var con = Open();
        using var cmd = con.CreateCommand();
        if (room.Id == 0)
        {
            cmd.CommandText = @"INSERT INTO Rooms(Name,Location,Capacity,PricePerHour,Facilities,IsActive)
VALUES($name,$location,$capacity,$price,$facilities,$active);";
        }
        else
        {
            cmd.CommandText = @"UPDATE Rooms SET Name=$name,Location=$location,Capacity=$capacity,
PricePerHour=$price,Facilities=$facilities,IsActive=$active WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", room.Id);
        }
        cmd.Parameters.AddWithValue("$name", room.Name.Trim());
        cmd.Parameters.AddWithValue("$location", room.Location.Trim());
        cmd.Parameters.AddWithValue("$capacity", room.Capacity);
        cmd.Parameters.AddWithValue("$price", room.PricePerHour);
        cmd.Parameters.AddWithValue("$facilities", room.Facilities.Trim());
        cmd.Parameters.AddWithValue("$active", room.IsActive ? 1 : 0);
        cmd.ExecuteNonQuery();
    }

    public static List<Booking> GetBookings(string status = "Semua")
    {
        using var con = Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"SELECT b.Id,b.BookingCode,b.CustomerName,b.CustomerPhone,b.RoomId,r.Name,
b.StartAt,b.EndAt,b.ParticipantCount,b.EventName,b.TotalAmount,b.Status,b.DecisionNote,b.CreatedAt
FROM Bookings b JOIN Rooms r ON r.Id=b.RoomId " +
            (status == "Semua" ? "" : "WHERE b.Status=$status ") + "ORDER BY b.CreatedAt DESC;";
        if (status != "Semua") cmd.Parameters.AddWithValue("$status", status);
        using var reader = cmd.ExecuteReader();
        var result = new List<Booking>();
        while (reader.Read())
            result.Add(new Booking
            {
                Id = reader.GetInt32(0), BookingCode = reader.GetString(1), CustomerName = reader.GetString(2),
                CustomerPhone = reader.GetString(3), RoomId = reader.GetInt32(4), RoomName = reader.GetString(5),
                StartAt = DateTime.Parse(reader.GetString(6)), EndAt = DateTime.Parse(reader.GetString(7)),
                ParticipantCount = reader.GetInt32(8), EventName = reader.GetString(9), TotalAmount = reader.GetDecimal(10),
                Status = reader.GetString(11), DecisionNote = reader.IsDBNull(12) ? null : reader.GetString(12),
                CreatedAt = DateTime.Parse(reader.GetString(13))
            });
        return result;
    }

    public static string CreateBooking(string customer, string phone, int roomId, DateTime start, DateTime end,
        int participants, string eventName)
    {
        if (string.IsNullOrWhiteSpace(customer) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(eventName))
            throw new InvalidOperationException("Nama pemesan, nomor telepon, dan nama acara wajib diisi.");
        if (start <= DateTime.Now) throw new InvalidOperationException("Waktu mulai harus di masa depan.");
        if (end <= start) throw new InvalidOperationException("Waktu selesai harus setelah waktu mulai.");
        if (participants <= 0) throw new InvalidOperationException("Jumlah peserta harus lebih dari nol.");

        using var con = Open();
        using var tx = con.BeginTransaction();
        using var roomCmd = con.CreateCommand();
        roomCmd.Transaction = tx;
        roomCmd.CommandText = "SELECT Capacity,PricePerHour,IsActive FROM Rooms WHERE Id=$id;";
        roomCmd.Parameters.AddWithValue("$id", roomId);
        using var rr = roomCmd.ExecuteReader();
        if (!rr.Read()) throw new InvalidOperationException("Tempat tidak ditemukan.");
        var capacity = rr.GetInt32(0);
        var price = rr.GetDecimal(1);
        var active = rr.GetInt32(2) == 1;
        rr.Close();
        if (!active) throw new InvalidOperationException("Tempat sedang tidak aktif.");
        if (participants > capacity) throw new InvalidOperationException($"Kapasitas tempat hanya {capacity} orang.");

        using var conflict = con.CreateCommand();
        conflict.Transaction = tx;
        conflict.CommandText = @"SELECT COUNT(*) FROM Bookings
WHERE RoomId=$room AND Status='Approved' AND $start < EndAt AND $end > StartAt;";
        conflict.Parameters.AddWithValue("$room", roomId);
        conflict.Parameters.AddWithValue("$start", start.ToString("O"));
        conflict.Parameters.AddWithValue("$end", end.ToString("O"));
        if (Convert.ToInt32(conflict.ExecuteScalar()) > 0)
            throw new InvalidOperationException("Jadwal bentrok dengan booking yang sudah disetujui. Pilih waktu lain.");

        var hours = (decimal)(end - start).TotalHours;
        var total = Math.Ceiling(hours) * price;
        var code = $"BK-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
        using var insert = con.CreateCommand();
        insert.Transaction = tx;
        insert.CommandText = @"INSERT INTO Bookings(BookingCode,CustomerName,CustomerPhone,RoomId,StartAt,EndAt,
ParticipantCount,EventName,TotalAmount,Status,CreatedAt)
VALUES($code,$customer,$phone,$room,$start,$end,$participants,$event,$total,'Pending',$created);";
        insert.Parameters.AddWithValue("$code", code);
        insert.Parameters.AddWithValue("$customer", customer.Trim());
        insert.Parameters.AddWithValue("$phone", phone.Trim());
        insert.Parameters.AddWithValue("$room", roomId);
        insert.Parameters.AddWithValue("$start", start.ToString("O"));
        insert.Parameters.AddWithValue("$end", end.ToString("O"));
        insert.Parameters.AddWithValue("$participants", participants);
        insert.Parameters.AddWithValue("$event", eventName.Trim());
        insert.Parameters.AddWithValue("$total", total);
        insert.Parameters.AddWithValue("$created", DateTime.Now.ToString("O"));
        insert.ExecuteNonQuery();
        tx.Commit();
        return code;
    }

    public static void DecideBooking(int id, string decision, string? note = null)
    {
        if (decision is not ("Approved" or "Rejected")) throw new ArgumentException("Keputusan tidak valid.");
        if (decision == "Rejected" && string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("Alasan penolakan wajib diisi.");

        using var con = Open();
        using var tx = con.BeginTransaction();
        using var get = con.CreateCommand();
        get.Transaction = tx;
        get.CommandText = "SELECT RoomId,StartAt,EndAt,Status FROM Bookings WHERE Id=$id;";
        get.Parameters.AddWithValue("$id", id);
        using var reader = get.ExecuteReader();
        if (!reader.Read()) throw new InvalidOperationException("Booking tidak ditemukan.");
        int roomId = reader.GetInt32(0);
        string start = reader.GetString(1);
        string end = reader.GetString(2);
        string status = reader.GetString(3);
        reader.Close();
        if (status != "Pending") throw new InvalidOperationException("Booking ini sudah diproses.");

        if (decision == "Approved")
        {
            using var conflict = con.CreateCommand();
            conflict.Transaction = tx;
            conflict.CommandText = @"SELECT COUNT(*) FROM Bookings WHERE RoomId=$room AND Id<>$id
AND Status='Approved' AND $start < EndAt AND $end > StartAt;";
            conflict.Parameters.AddWithValue("$room", roomId);
            conflict.Parameters.AddWithValue("$id", id);
            conflict.Parameters.AddWithValue("$start", start);
            conflict.Parameters.AddWithValue("$end", end);
            if (Convert.ToInt32(conflict.ExecuteScalar()) > 0)
                throw new InvalidOperationException("Tidak dapat menyetujui: jadwal bertabrakan dengan booking yang sudah disetujui.");
        }

        using var update = con.CreateCommand();
        update.Transaction = tx;
        update.CommandText = "UPDATE Bookings SET Status=$status,DecisionNote=$note WHERE Id=$id AND Status='Pending';";
        update.Parameters.AddWithValue("$status", decision);
        update.Parameters.AddWithValue("$note", string.IsNullOrWhiteSpace(note) ? DBNull.Value : note.Trim());
        update.Parameters.AddWithValue("$id", id);
        if (update.ExecuteNonQuery() != 1) throw new InvalidOperationException("Status booking berubah. Muat ulang data.");
        tx.Commit();
    }
}
