using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MouseMover.Properties;

namespace MouseMover
{
    static class Program
    {
        private const Int32 STEPS = 3;
        private const Int32 WIDTH = 1920;
        private const Int32 HEIGHT = 1200;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MouseMoverApplication());
        }

        public class MouseMoverApplication : ApplicationContext
        {
            private readonly NotifyIcon trayIcon;
            private readonly HotKey startKey;
            private readonly HotKey stopKey;
            private CancellationTokenSource tokenSource;
            private Task task;

            public MouseMoverApplication()
            {
                MenuItem[] menuItems = new MenuItem[]
                {
                    new MenuItem("Exit", this.Exit)
                };
                this.trayIcon = new NotifyIcon
                {
                    Icon = Resources.Icon,
                    ContextMenu = new ContextMenu(menuItems),
                    Visible = true
                };
                this.startKey = new HotKey(Key.D, KeyModifier.Ctrl | KeyModifier.Alt, this.MoveCursor);
                this.stopKey = new HotKey(Key.X, KeyModifier.Ctrl | KeyModifier.Alt, this.Cancel);
            }

            private void MoveCursor()
            {
                if (task == null || task.IsCompleted)
                {
                    this.tokenSource?.Dispose();
                    this.tokenSource = new CancellationTokenSource();
                    task = Task.Factory.StartNew(() =>
                    {
                        for (int i = 0; i < WIDTH; i++)
                        {
                            for (int j = 0; j < HEIGHT; j += HEIGHT - 1)
                            {
                                if (this.tokenSource.Token.IsCancellationRequested)
                                {
                                    return;
                                }
                                Thread.Sleep(10);
                                Cursor.Position = new Point(i, j);
                            }
                        };
                    }, this.tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                }
            }

            private void Cancel()
            {
                this.tokenSource?.Cancel();
            }

            private void Exit(object sender, EventArgs e)
            {
                this.Exit();
            }

            private void Exit()
            {
                this.tokenSource?.Cancel();
                this.tokenSource?.Dispose();
                this.startKey.Dispose();
                this.stopKey.Dispose();
                this.trayIcon.Dispose();
                Application.Exit();
            }
        }
    }
}
