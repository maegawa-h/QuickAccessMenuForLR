using System.Windows.Media;
using Newtonsoft.Json;

namespace QuickAccessMenu.Extensions.Model
{
    public class IssueConfig
    {
        #region 公開プロパティ

        public string ID { get; set; }
        public string DisplayName { get; set; }
        public string Color { get; set; }
        public Properties Properties { get; set; }

        [JsonIgnore]
        public Brush BackgroundBrush => (Brush)new BrushConverter().ConvertFromString(Color);

        #endregion
    }

    public class Properties
    {
        public string Type { get; set; }
        public string Importance { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; }
        public string ReportedBy { get; set; }
        public string AssignedTo { get; set; }
        public string ConfirmedBy { get; set; }
        public string DueDate { get; set; }
    }
}