using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using LightningReview.ExtensionFramework;
using System.Text.Json;
using QuickAccessMenu.Extensions.Views;
using QuickAccessMenu.Extensions.Model;

namespace QuickAccessMenu.Extensions
{
    /// <summary>
    /// エクステンションの説明
    /// </summary>
    [ExtensionExport("QuickAccessMenu", "クイックアクセスメニュー", "QuickAccessMenu")]
    public class QuickAccessMenu : Extension
    {
        private bool isActive;

        #region プロパティ

        public string CommandType { get; set; } = string.Empty;

        public ObservableCollection<IssueConfig> IssueConfigs { get; set; }

        public MainWindow MainWindow { get; set; }

        private string DllPath { get; set; }

        private string SettingFilePath { get; set; }

        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    SaveSettings();
                }
            }
        }

        #endregion

        #region オーバーライド

        #region 設定ファイルの読み込み

        public void OpenIssueConfigs()
        {
            var configFilePath = Path.Combine(SettingFilePath, "extension.DensoCreate.QuickAccessMenu.json");
            if (File.Exists(configFilePath))
            {
                Process.Start(configFilePath);
            }
        }

        public void LoadIssueConfigs()
        {
            var configFilePath = Path.Combine(SettingFilePath, "extension.DensoCreate.QuickAccessMenu.json");
            if (!File.Exists(configFilePath))
            {
                var baseConfigFilePath = Path.Combine(DllPath, "extension.DensoCreate.QuickAccessMenu.json");
                File.Copy(baseConfigFilePath, configFilePath);
            }

            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true // 必要に応じて、末尾のカンマも許容する
            };

            var json = File.ReadAllText(configFilePath);
            var config = JsonSerializer.Deserialize<QuickAccessMenuConfig>(json, options);

            IssueConfigs = config.Buttons;
            IsActive = config.IsActive;
        }

        public void SaveSettings()
        {
            var configFilePath = Path.Combine(SettingFilePath, "extension.DensoCreate.QuickAccessMenu.json");

            // JSONファイルを読み込む
            var json = File.ReadAllText(configFilePath);

            // IsActiveの値を更新するための正規表現パターン
            var pattern = @"""IsActive"":\s*(true|false)";
            var replacement = $@"""IsActive"": {IsActive.ToString().ToLower()}";

            // 文字列置換でIsActiveの値を更新
            var updatedJson = System.Text.RegularExpressions.Regex.Replace(json, pattern, replacement);

            // 更新されたJSONをファイルに書き込む
            File.WriteAllText(configFilePath, updatedJson);
        }

        #endregion

        /// <summary>
        /// アクティベート処理
        /// </summary>
        protected override void OnActivate()
        {
            DllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            SettingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DENSO CREATE", "Lightning Review");

            LoadIssueConfigs();
            if (IsActive)
            {
                MainWindow = new MainWindow(this);
            }

            // ツールメニューに
            RegisterMenu()
                .Location("ReviewWindow/MainMenu")
                .Parent("ツール")
                .Before("プロパティの表示順を変更")
                .WithSeparatorAfter()
                .Command<SwitchActivateStateCommand>()
                .SetCheckableMenu(IsActive)
                .Build();

            // イベントハンドラの登録
            App.IssueNew += AppOnIssueNew;
        }

        /// <summary>
        /// ディアクティベート処理
        /// </summary>
        protected override void OnDeactivate()
        {
            // イベントハンドラの解除
            App.IssueNew -= AppOnIssueNew;
        }

        #endregion

        #region イベントハンドラ

        /// <summary>
        /// 新規追加時の処理
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void AppOnIssueNew(object arg1, IssueEventArgs arg2)
        {
            var issue = arg2.Issues.FirstOrDefault();
            if (issue == null) return;

            var config = IssueConfigs.FirstOrDefault(c => c.ID == CommandType);
            if (config != null)
            {
                var issueType = issue.GetType();
                foreach (var prop in config.Properties.GetType().GetProperties())
                {
                    var issueProp = issueType.GetProperty(prop.Name);
                    var propValue = prop.GetValue(config.Properties)?.ToString();
                    if (!string.IsNullOrEmpty(propValue) && issueProp != null)
                    {
                        if (issueProp.PropertyType == typeof(DateTime) ||
                           (issueProp.PropertyType.IsGenericType && issueProp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && issueProp.PropertyType.GetGenericArguments()[0] == typeof(DateTime)))

                        {
                            DateTime date;
                            switch (propValue)
                            {
                                case "Today":
                                    date = DateTime.Today;
                                    break;
                                case "Day":
                                    date = DateTime.Today.AddDays(1);
                                    break;
                                case "Week":
                                    date = DateTime.Today.AddDays(7);
                                    break;
                                case "Month":
                                    date = DateTime.Today.AddMonths(1);
                                    break;
                                default:
                                    if (!DateTime.TryParse(propValue, out date))
                                    {
                                        continue;
                                    }
                                    break;
                            }
                            issueProp.SetValue(issue, date);
                        }
                        else
                        {
                            issueProp.SetValue(issue, propValue);
                        }
                    }
                }
            }

            CommandType = string.Empty;
        }

        #endregion

    }
}
