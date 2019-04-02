using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Omen
{
    /// <summary>
    /// Interaction logic for VolumeWindow.xaml
    /// </summary>
    public partial class VolumeWindow : Window
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public VolumeWindow()
        {
            InitializeComponent();

            dispatcherTimer.Tick += OnTimeout;
        }

        public void Show(float volume)
        {
            int volumeInteger = (int)(volume * 100);
            VolumeSlider.Value = volumeInteger;
            VolumeLabel.Content = volumeInteger;

            dispatcherTimer.Stop();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1.5);
            dispatcherTimer.Start();

            BeginAnimation(OpacityProperty, null);
            Opacity = 1;

            Show();
        }

        private void OnTimeout(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();
            Close();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
            anim.Completed += (s, _) =>
            {
                this.Hide();
            };

            BeginAnimation(OpacityProperty, anim);
        }
    }
}