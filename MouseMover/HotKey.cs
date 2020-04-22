using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace MouseMover
{
    public sealed class HotKey : IDisposable
    {
        private static Dictionary<Int32, HotKey> registeredKeys;
        private static IMessageFilter messageFilter;

        static HotKey()
        {
            registeredKeys = new Dictionary<Int32, HotKey>();
            messageFilter = new MessageFilter();
        }

        [DllImport("user32.dll")]
        private static extern Boolean RegisterHotKey(IntPtr hWnd, Int32 id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern Boolean UnregisterHotKey(IntPtr hWnd, Int32 id);

        public const Int32 WmHotKey = 0x0312;

        public Key Key { get; }
        public KeyModifier KeyModifiers { get; }
        public Action Action { get; }

        private Int32 id;

        public HotKey(Key k, KeyModifier keyModifiers, Action action)
        {
            this.Key = k;
            this.KeyModifiers = keyModifiers;
            this.Action = action ?? throw new ArgumentNullException(nameof(action));
            Application.AddMessageFilter(messageFilter);
            this.Register();
        }

        private void Register()
        {
            Int32 virtualKeyCode = KeyInterop.VirtualKeyFromKey(this.Key);
            this.id = virtualKeyCode + ((Int32)this.KeyModifiers * 0x10000);
            RegisterHotKey(IntPtr.Zero, this.id, (UInt32)this.KeyModifiers, (UInt32)virtualKeyCode);
            registeredKeys.Add(this.id, this);
        }

        private void Unregister()
        {
            UnregisterHotKey(IntPtr.Zero, this.id);
            registeredKeys.Remove(this.id);
        }

        public void Dispose()
        {
            this.Unregister();
        }

        private class MessageFilter : IMessageFilter
        {
            public Boolean PreFilterMessage(ref Message m)
            {
                if (m.Msg == WmHotKey)
                {
                    if (registeredKeys.TryGetValue((Int32)m.WParam, out HotKey hotKey))
                    {
                        hotKey.Action.Invoke();
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
