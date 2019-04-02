using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace Omen
{
    public static class ExtMath
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {

        VolumeWindow    VolWindow;
        ProjectorStatus ProjectorWindow;
        KeyComboParser  Parser;

        KeyCombo ProjectorCombo = new KeyCombo(KeyModifier.WinKey | KeyModifier.Shift, Keys.O);
        KeyCombo VolUpCombo = new KeyCombo(KeyModifier.WinKey, Keys.VolumeUp);
        KeyCombo VolDownCombo = new KeyCombo(KeyModifier.WinKey, Keys.VolumeDown);

        bool ProjectorTest = false;
        float VolumeTest = 0.5f;

        public App()
        {

        }

        void HotKeyCallback(object sender, KeyComboParser.HotKeyEventArgs args)
        {
            if (args.Combo == ProjectorCombo)
            {
                ProjectorTest = !ProjectorTest;
                ProjectorWindow.Show(ProjectorTest);
                // TO-DO Call Service
            }
            else if (args.Combo == VolUpCombo)
            {
                VolumeTest = ExtMath.Clamp(VolumeTest + 0.014f, 0.0f, 1.0f);
                VolWindow.Show(VolumeTest);
                // TO-DO Call Service
            }
            else if (args.Combo == VolDownCombo)
            {
                VolumeTest = ExtMath.Clamp(VolumeTest - 0.014f, 0.0f, 1.0f);
                VolWindow.Show(VolumeTest);
                // TO-DO Call Service
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Color accentColor = UXTheme.GetAccentColor();
            SolidColorBrush accentBrush = new SolidColorBrush(accentColor);
            Resources["Windows.Accent"] = accentBrush;

            VolWindow = new VolumeWindow();
            ProjectorWindow = new ProjectorStatus();
            Parser = new KeyComboParser();

            Parser.RegisterHotKey(ProjectorCombo);
            Parser.RegisterHotKey(VolUpCombo);
            Parser.RegisterHotKey(VolDownCombo);
            Parser.OnHotKeyReceived += HotKeyCallback;

            base.OnStartup(e);
        }
    }
}
