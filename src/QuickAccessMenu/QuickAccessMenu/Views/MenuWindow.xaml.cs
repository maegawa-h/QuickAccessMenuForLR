using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using QuickAccessMenu.Extensions.ViewModels;

namespace QuickAccessMenu.Extensions.Views
{
    public partial class MenuWindow : Window
    {
        public MenuWindow(MenuWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            HideMenu();
        }

        public void HideMenu()
        {
            var animation = new DoubleAnimation(-Height, TimeSpan.FromMilliseconds(300));
            animation.Completed += (s, e) => this.Hide();
            this.BeginAnimation(TopProperty, animation);
        }

        public void Show(int topPos, int leftPos, Screen screen)
        {
            if (DataContext is MenuWindowViewModel vm)
            {
                this.Height = 60;
                this.Width = vm.LimitedIssueConfigs.Count * 130 + 40;

                var animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));

                this.Top = topPos;

                // 画面中央の位置を計算
                var screenWidth = screen.Bounds.Width;
                this.Left = (screenWidth - this.Width) / 2 + screen.Bounds.Left; // 左位置

                base.Show();
                this.BeginAnimation(TopProperty, animation);
            }
        }
    }
}