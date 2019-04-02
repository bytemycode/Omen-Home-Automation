using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Omen
{
    public enum KeyModifier
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        WinKey = 8
    }

    public class KeyCombo
    {
        private static int IDRegistry = 0;

        public readonly int InternalID;
        public readonly KeyModifier Modifiers;
        public readonly Keys Key;

        public KeyCombo(Keys key)
        {
            InternalID = IDRegistry++;
            Key = key;
        }

        public KeyCombo(KeyModifier modifiers, Keys key)
        {
            InternalID = IDRegistry++;
            Key = key;
            Modifiers = modifiers;
        }

        public bool IsValid(KeyModifier modifiers, Keys key)
        {
            return Modifiers == 0 || ((Modifiers & modifiers) == modifiers && Key == key);
        }

    }
    public partial class KeyComboParser : Form
    {
        #region Defines

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public class HotKeyEventArgs : EventArgs
        {
            public readonly KeyCombo Combo;

            public HotKeyEventArgs(KeyCombo combo)
            {
                Combo = combo;
            }
        }

        public delegate void HotKeyCallback(object sender, HotKeyEventArgs args);

        #endregion

        private List<KeyCombo> RegisteredHotKeys = new List<KeyCombo>();
        public event HotKeyCallback OnHotKeyReceived;
        
        public KeyComboParser()
        {

        }

        ~KeyComboParser()
        {
            foreach(KeyCombo keyCombo in RegisteredHotKeys)
            {
                UnregisterHotKey(Handle, keyCombo.InternalID);
            }

            RegisteredHotKeys.Clear();
        }

        public bool RegisterHotKey(KeyCombo hotkey)
        {
            if(!RegisteredHotKeys.Contains(hotkey))
            {
                bool rez = RegisterHotKey(Handle, hotkey.InternalID, (int)hotkey.Modifiers, hotkey.Key.GetHashCode());
                if(rez)
                {
                    RegisteredHotKeys.Add(hotkey);
                    return true;
                }
            }

            return false;
        }

        public bool UnregisterHotKey(KeyCombo hotkey)
        {
            if (RegisteredHotKeys.Contains(hotkey))
            {
                RegisteredHotKeys.Remove(hotkey);
                UnregisterHotKey(Handle, hotkey.InternalID);
                return true;
            }

            return false;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                int id = m.WParam.ToInt32();
                foreach(KeyCombo combo in RegisteredHotKeys)
                {
                    if(id == combo.InternalID)
                    {
                        OnHotKeyReceived?.Invoke(this, new HotKeyEventArgs(combo));
                    }
                }
            }

            base.WndProc(ref m);
        }
    }
}
