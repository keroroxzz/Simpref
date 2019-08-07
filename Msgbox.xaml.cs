using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SimpRef
{
    /// <summary>
    /// Msgbox.xaml 的互動邏輯
    /// </summary>
    public partial class Msgbox : Window
    {
        String downloading_text;

        public Msgbox(String text)
        {
            InitializeComponent();
            Text.Text = text;
            Visibility = Visibility.Visible;
            Show();
            Activate();
        }

        public Msgbox(String text, bool isProgress)
        {
            InitializeComponent();

            if (isProgress)
            {
                downloading_text = text;
                ProgressBar.Visibility = Visibility.Visible;
                Text.Text = downloading_text + "0%";
            }
            else
                Text.Text = text;

            Visibility = Visibility.Visible;
            Show();
            Activate();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void UpdateProgressBar(object sender, DownloadProgressEventArgs e)
        {
            Text.Text = downloading_text+e.Progress + "%";
            ProgressBar.Width = e.Progress / 100.0 * (Width - 20);

            if (e.Progress == 100)
                Close();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
