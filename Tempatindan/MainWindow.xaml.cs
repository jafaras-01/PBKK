using System.Windows;
using System.Globalization;
using System.Windows.Controls;

namespace Tempatindan;

public partial class MainWindow : Window
{
    private int _editingRoomId;
    private bool _loadingFilter;

    public MainWindow()
    {
        InitializeComponent();
        try
        {
            Database.Initialize();
            var times = Enumerable.Range(0, 48).Select(i => $"{i / 2:00}:{(i % 2) * 30:00}").ToList();
            StartTimeCombo.ItemsSource = times;
            EndTimeCombo.ItemsSource = times;
            StartTimeCombo.SelectedItem = "09:00";
            EndTimeCombo.SelectedItem = "10:00";
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(1);
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(1);
            _loadingFilter = true;
            StatusFilter.ItemsSource = new[] { "Semua", "Pending", "Approved", "Rejected" };
            StatusFilter.SelectedIndex = 0;
            _loadingFilter = false;
            RefreshAll();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Gagal menyiapkan database: " + ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshAll()
    {
        var rooms = Database.GetRooms();
        RoomsGrid.ItemsSource = rooms.Where(r => r.IsActive).ToList();
        RoomCombo.ItemsSource = rooms.Where(r => r.IsActive).ToList();
        if (RoomCombo.Items.Count > 0 && RoomCombo.SelectedIndex < 0) RoomCombo.SelectedIndex = 0;
        ManageRoomsGrid.ItemsSource = rooms;
        RefreshBookings();
    }

    private void RefreshBookings()
    {
        string status = StatusFilter.SelectedItem as string ?? "Semua";
        BookingsGrid.ItemsSource = Database.GetBookings(status);
    }

    private void CreateBooking_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (RoomCombo.SelectedItem is not Room room) throw new InvalidOperationException("Pilih tempat terlebih dahulu.");
            if (StartDatePicker.SelectedDate is not DateTime sd || EndDatePicker.SelectedDate is not DateTime ed)
                throw new InvalidOperationException("Tanggal mulai dan selesai wajib dipilih.");
            if (StartTimeCombo.SelectedItem is not string st || EndTimeCombo.SelectedItem is not string et)
                throw new InvalidOperationException("Pilih jam mulai dan selesai.");
            var start = sd.Date + TimeSpan.Parse(st, CultureInfo.InvariantCulture);
            var end = ed.Date + TimeSpan.Parse(et, CultureInfo.InvariantCulture);
            if (!int.TryParse(ParticipantsBox.Text, out int participants))
                throw new InvalidOperationException("Jumlah peserta harus berupa angka.");
            string code = Database.CreateBooking(CustomerNameBox.Text, PhoneBox.Text, room.Id, start, end,
                participants, EventNameBox.Text);
            BookingMessage.Foreground = System.Windows.Media.Brushes.DarkGreen;
            BookingMessage.Text = $"Pengajuan berhasil dibuat. Kode: {code}. Status: Pending.";
            CustomerNameBox.Clear(); PhoneBox.Clear(); EventNameBox.Clear();
            RefreshBookings();
        }
        catch (Exception ex)
        {
            BookingMessage.Foreground = System.Windows.Media.Brushes.Firebrick;
            BookingMessage.Text = ex.Message;
        }
    }

    private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_loadingFilter && IsInitialized) RefreshBookings();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        try { RefreshAll(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Kesalahan"); }
    }

    private Booking? SelectedBooking() => BookingsGrid.SelectedItem as Booking;

    private void Approve_Click(object sender, RoutedEventArgs e)
    {
        var booking = SelectedBooking();
        if (booking == null) { MessageBox.Show("Pilih pengajuan booking terlebih dahulu."); return; }
        try
        {
            Database.DecideBooking(booking.Id, "Approved");
            RefreshBookings();
            MessageBox.Show("Booking berhasil disetujui.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Tidak dapat menyetujui", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private void Reject_Click(object sender, RoutedEventArgs e)
    {
        var booking = SelectedBooking();
        if (booking == null) { MessageBox.Show("Pilih pengajuan booking terlebih dahulu."); return; }
        var dialog = new ReasonWindow { Owner = this };
        if (dialog.ShowDialog() != true) return;
        try
        {
            Database.DecideBooking(booking.Id, "Rejected", dialog.Reason);
            RefreshBookings();
            MessageBox.Show("Booking berhasil ditolak.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Tidak dapat menolak", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private void SaveRoom_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(RoomNameBox.Text) || string.IsNullOrWhiteSpace(RoomLocationBox.Text))
                throw new InvalidOperationException("Nama dan lokasi wajib diisi.");
            if (!int.TryParse(RoomCapacityBox.Text, out int capacity) || capacity <= 0)
                throw new InvalidOperationException("Kapasitas harus berupa angka positif.");
            Database.SaveRoom(new Room
            {
                Id = _editingRoomId,
                Name = RoomNameBox.Text,
                Location = RoomLocationBox.Text,
                Capacity = capacity,
                Facilities = RoomFacilitiesBox.Text,
                IsActive = RoomActiveCheck.IsChecked == true
            });
            RoomMessage.Text = "Data tempat berhasil disimpan.";
            RefreshAll();
            ClearRoomForm();
        }
        catch (Exception ex) { RoomMessage.Text = ex.Message; }
    }

    private void NewRoom_Click(object sender, RoutedEventArgs e) => ClearRoomForm();

    private void ClearRoomForm()
    {
        _editingRoomId = 0;
        RoomNameBox.Clear(); RoomLocationBox.Clear(); RoomCapacityBox.Clear();
        RoomFacilitiesBox.Clear(); RoomActiveCheck.IsChecked = true;
        ManageRoomsGrid.SelectedItem = null;
    }

    private void ManageRoomsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ManageRoomsGrid.SelectedItem is not Room room) return;
        _editingRoomId = room.Id;
        RoomNameBox.Text = room.Name; RoomLocationBox.Text = room.Location;
        RoomCapacityBox.Text = room.Capacity.ToString();
        RoomFacilitiesBox.Text = room.Facilities; RoomActiveCheck.IsChecked = room.IsActive;
    }
}
