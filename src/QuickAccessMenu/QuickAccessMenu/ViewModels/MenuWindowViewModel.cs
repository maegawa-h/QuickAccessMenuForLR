using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using LightningReview.ExtensionFramework;
using QuickAccessMenu.Extensions.Model;
using QuickAccessMenu.Extensions.Views;

namespace QuickAccessMenu.Extensions.ViewModels
{
    public class MenuWindowViewModel : ViewModelBase
    {

        #region 内部フィールド

        private QuickAccessMenu m_Extension;

        #endregion

        #region 公開プロパティ

        public ObservableCollection<IssueConfig> IssueConfigs
        {
            get { return m_Extension.IssueConfigs; }
            set
            {
                if (m_Extension.IssueConfigs != value)
                {
                    m_Extension.IssueConfigs = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<IssueConfig> LimitedIssueConfigs
        {
            get
            {
                return new ObservableCollection<IssueConfig>(IssueConfigs.Take(5));
            }
        }

        #endregion

        #region コンストラクタ

        public MenuWindowViewModel(QuickAccessMenu extension)
        {
            m_Extension = extension;
        }

        #endregion

        #region 公開コマンド

        public ICommand OpenSettingsCommand => new DelegateCommand(OpenSettings, () => true);
        public ICommand LoadSettingsCommand => new DelegateCommand(LoadSettings, () => true);
        public ICommand AddIssueCommand => new DelegateCommand<string>(AddIssue, () => true);

        #endregion

        #region コマンドの実装
        private void OpenSettings()
        {
            m_Extension.OpenIssueConfigs();
        }

        private void LoadSettings()
        {
            m_Extension.LoadIssueConfigs();
            HideMenu();
            OnPropertyChanged(nameof(LimitedIssueConfigs));
        }

        private void AddIssue(string commandType)
        {
            AddIssueInternal();
            m_Extension.CommandType = commandType;
        }

        private void AddIssueInternal()
        {
            var reviewWindow = m_Extension.App.ActiveReviewWindow;
            if (reviewWindow == null)
            {
                // アクティブなレビューウィンドウがない場合、新しくレビューウィンドウを立ち上げる
                reviewWindow = m_Extension.App.NewReviewWindow();
            }

            var targetDocument = Enumerable.FirstOrDefault<IDocument>(m_Extension.App.ActiveReviewWindow.Review.Documents);
            if (targetDocument == null)
            {
                // 対象のドキュメントが見つからない場合はドキュメントを追加します
                targetDocument = m_Extension.App.ActiveReviewWindow.Review.AddDocument();
            }

            var owner = m_Extension.App.ActiveReviewWindow.GetWindow();
            MoveToBack(owner);
            owner.WindowState = WindowState.Minimized;

            reviewWindow.ShowQuickReviewWindow(targetDocument.OutlineTree.Path);
            
            // 子ウィンドウを取得
            var child = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window.DataContext == this);

            if (child != null)
            {
                // 親子関係を解除
                child.Owner = null;

                // 子ウィンドウをタスクバーに表示する場合
                child.ShowInTaskbar = true;

                // 必要に応じて、位置を調整
                child.Topmost = true; // 子ウィンドウを最前面に表示する場合

                // 子ウィンドウの位置を指定したディスプレイの中央に移動
                child.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        #endregion

        private void HideMenu()
        {
            var menuWindow = Application.Current.Windows.OfType<MenuWindow>().FirstOrDefault(window => window.DataContext == this);
            if (menuWindow != null)
            {
                menuWindow.Hide();
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        private void MoveToBack(Window window)
        {
            var handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;

            SetWindowPos(handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }

    }
}