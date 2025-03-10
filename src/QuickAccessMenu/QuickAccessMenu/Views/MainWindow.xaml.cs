using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using QuickAccessMenu.Extensions.Model;
using QuickAccessMenu.Extensions.ViewModels;

namespace QuickAccessMenu.Extensions.Views
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private MenuWindow sideMenu;

        public MainWindow(QuickAccessMenu extension)
        {
            InitializeComponent();

            // サイドメニューウィンドウを作成
            sideMenu = new MenuWindow(new MenuWindowViewModel(extension));
            sideMenu.Hide();

            // タイマー設定
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50) // 50msごとに監視
            };
            timer.Tick += Timer_Tick;
            timer.Start();

            // Closingイベントにハンドラを追加
            this.Closing += MainWindow_Closing;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // マウス位置を取得
            var mousePos = MouseHelper.GetMousePosition();

            // 各ディスプレイのエッジを確認
            foreach (var screen in Screen.AllScreens)
            {
                var bounds = screen.Bounds;
                if((mousePos.Y >= bounds.Top && mousePos.Y <= bounds.Top + 10)
                && (bounds.Left <= mousePos.X &&  mousePos.X <= bounds.Left + bounds.Width))
                {
                    // 上端にマウスがある場合、メニューを表示
                    ShowMenu(bounds, screen);
                }
            }
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // タイマーを停止
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
            }

            // サイドメニューを閉じる
            if (sideMenu != null)
            {
                sideMenu.Close();
            }
        }

        private void ShowMenu(System.Drawing.Rectangle screenBounds, Screen screen)
        {
            if (!sideMenu.IsVisible)
            {
                sideMenu.Show(screenBounds.Top, screenBounds.Left, screen);
            }
        }
    }
}