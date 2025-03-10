using LightningReview.ExtensionFramework;
using QuickAccessMenu.Extensions.Views;

namespace QuickAccessMenu.Extensions.Model
{
    public class SwitchActivateStateCommand : ExtensionCommand
    {
        public SwitchActivateStateCommand()
        {
            Title = "クイックアクセスを有効化";
        }

        /// <summary>
        /// 実行処理
        /// </summary>
        /// <param name="parameter"></param>
        protected override void OnExecute(object parameter = null)
        {
            if (Extension is QuickAccessMenu quickAccessMenu)
            {
                quickAccessMenu.IsActive = !quickAccessMenu.IsActive;
                if (quickAccessMenu.IsActive)
                {
                    quickAccessMenu.MainWindow = new MainWindow(quickAccessMenu);
                }
                else
                {
                    quickAccessMenu.MainWindow.Close();
                }
            }
        }
    }
}