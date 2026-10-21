using System.Windows;
using System.Globalization;
using System.Windows.Controls;

namespace Tempatindan;

public partial class MainWindow : Window
{
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
        AllRoomsGrid.ItemsSource = rooms.Where(r => r.IsActive).ToList();
        RoomCombo.ItemsSource = rooms.Where(r => r.IsActive).ToList();
        if (RoomCombo.Items.Count > 0 && RoomCombo.SelectedIndex < 0) RoomCombo.SelectedIndex = 0;
        RefreshBookings();
    }

    private void RefreshBookings()
    {
        string status = StatusFilter.SelectedItem as string ?? "Semua";
        BookingsGrid.ItemsSource = Database.GetBookings(status);
    }

    private void NavigateBooking_Click(object sender, RoutedEventArgs e) => NavigateToTab(0);
    private void NavigateHistory_Click(object sender, RoutedEventArgs e) => NavigateToTab(1);
    private void NavigateRooms_Click(object sender, RoutedEventArgs e) => NavigateToTab(2);

    private void NavigateToTab(int index)
    {
        MainTabs.SelectedIndex = index;
        var activeBackground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#302B55")!;
        var inactiveBackground = System.Windows.Media.Brushes.Transparent;
        var activeForeground = System.Windows.Media.Brushes.White;
        var inactiveForeground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#D0D3E0")!;
        var buttons = new[] { NavBookingButton, NavHistoryButton, NavRoomsButton };
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].Background = i == index ? activeBackground : inactiveBackground;
            buttons[i].Foreground = i == index ? activeForeground : inactiveForeground;
        }
        if (index == 1) RefreshBookings();
        if (index == 2) AllRoomsGrid.ItemsSource = Database.GetRooms(activeOnly: true);
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
            BookingMessage.Text = $"Booking berhasil dibuat. Kode: {code}. Status: Berhasil dibooking.";
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


}
