using System.Collections.ObjectModel;

namespace QuickAccessMenu.Extensions.Model
{
    public class QuickAccessMenuConfig
    {
        public bool IsActive { get; set; }
        public ObservableCollection<IssueConfig> Buttons { get; set; }
    }
}