using System.Windows;

namespace BookingTempatWPF;

public partial class ReasonWindow : Window
{
    public string Reason => ReasonBox.Text.Trim();
    public ReasonWindow() => InitializeComponent();

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Reason))
        {
            MessageBox.Show("Alasan penolakan wajib diisi.");
            return;
        }
        DialogResult = true;
        Close();
    }
}
