using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using wf = System.Windows.Forms;

namespace bayoen
{
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// bayoen-star 0.2
        /// </summary>
        public MainWindow()
        {
            this.InitializePreferences();
            this.InitializeComponent();
            this.InitializeLayouts();
            this.InitializeTimer(333);
            this.InitializeVariables();
            Thread.Sleep(1000);

            this.CheckContainers();
            this.IsStatOn = true;
        }

        public List<Container> StarContainers;
        public List<Container> CrownContainers;

        public const string pptName = "puyopuyotetris";
        public const string prefName = "pref.json";
        public const string exportFolderName = "export";
        public const string dataJSONName = "data.json";
        public Preferences preferences;
        public wf::NotifyIcon notify;
        public MetroWindow setting;

        public VAMemory pptMemory;
        public int scoreAddress;
        public Process[] PPTProcesses;
        public DispatcherTimer timer;

        public int winCount;
        public List<int> currentStar;
        public List<int> oldStar;
        public List<int> countingStar;
        public List<int> countingCrown;
        
        public ContextMenu ModeContextMenu;

        public DisplayModes _mode;
        public DisplayModes Mode
        {
            get => this._mode;
            set
            {
                if (value == this._mode)
                {
                    return;
                }

                this.StarContainers.ForEach(x => this.TopPanel.Children.Remove(x));
                this.CrownContainers.ForEach(x => this.TopPanel.Children.Remove(x));

                if (this._mode >= DisplayModes.Game_only)
                {
                    if (value < DisplayModes.Game_only)
                    {
                        PanelImage.SetBitmap(bayoen.Properties.Resources.PanelScore2LongStrong);
                    }                        
                }
                else
                {
                    if (value >= DisplayModes.Game_only)
                    {
                        if (this.preferences.IsFirToScore.Value)
                        {
                            PanelImage.SetBitmap(bayoen.Properties.Resources.PanelScore2FitStrong);
                        }
                        else
                        {
                            PanelImage.SetBitmap(bayoen.Properties.Resources.PanelScore2ShortStrong);
                        }                        
                    }                        
                }


                if (value == DisplayModes.Game_and_Star_plus)
                {
                    this.TopPanel.Children.Add(this.CrownContainers[0]);
                    this.CrownContainers[0].Margin = new Thickness(0);
                    this.CrownContainers[0].ContainerOrientation = ContainerOrientations.Left;

                    this.TopPanel.Children.Add(this.StarContainers[0]);
                    this.StarContainers[0].Margin = new Thickness(0, 0, 5, 0);
                    this.StarContainers[0].ContainerOrientation = ContainerOrientations.Left;
                    this.StarContainers[0].ContainerImage = ContainerImages.StarPlus;

                    this.TopPanel.Children.Add(this.StarContainers[1]);
                    this.StarContainers[1].Margin = new Thickness(5, 0, 0, 0);
                    this.StarContainers[1].ContainerOrientation = ContainerOrientations.Right;
                    this.StarContainers[1].ContainerImage = ContainerImages.StarPlus;

                    this.TopPanel.Children.Add(this.CrownContainers[1]);
                    this.CrownContainers[1].Margin = new Thickness(0);
                    this.CrownContainers[1].ContainerOrientation = ContainerOrientations.Right;
                }
                else if (value == DisplayModes.Game_and_Star)
                {
                    this.TopPanel.Children.Add(this.CrownContainers[0]);
                    this.CrownContainers[0].Margin = new Thickness(0);
                    this.CrownContainers[0].ContainerOrientation = ContainerOrientations.Left;

                    this.TopPanel.Children.Add(this.StarContainers[0]);
                    this.StarContainers[0].Margin = new Thickness(0, 0, 5, 0);
                    this.StarContainers[0].ContainerOrientation = ContainerOrientations.Left;
                    this.StarContainers[0].ContainerImage = ContainerImages.StarPlain;

                    this.TopPanel.Children.Add(this.StarContainers[1]);
                    this.StarContainers[1].Margin = new Thickness(5, 0, 0, 0);
                    this.StarContainers[1].ContainerOrientation = ContainerOrientations.Right;
                    this.StarContainers[1].ContainerImage = ContainerImages.StarPlain;

                    this.TopPanel.Children.Add(this.CrownContainers[1]);
                    this.CrownContainers[1].Margin = new Thickness(0);
                    this.CrownContainers[1].ContainerOrientation = ContainerOrientations.Right;
                }
                else if (value == DisplayModes.Game_only)
                {
                    if (this.preferences.IsFirToScore.Value)
                    {
                        this.CrownContainers[0].Margin = new Thickness(0, 0, 30, 0);
                        this.CrownContainers[1].Margin = new Thickness(30, 0, 0, 0);
                    }
                    else
                    {
                        this.CrownContainers[0].Margin = new Thickness(0, 0, 5, 0);
                        this.CrownContainers[1].Margin = new Thickness(5, 0, 0, 0);
                    }

                    this.TopPanel.Children.Add(this.CrownContainers[0]);
                    this.CrownContainers[0].ContainerOrientation = ContainerOrientations.Left;

                    this.TopPanel.Children.Add(this.CrownContainers[1]);
                    this.CrownContainers[1].ContainerOrientation = ContainerOrientations.Right;
                }
                else if (value == DisplayModes.Star_plus_only)
                {
                    if (this.preferences.IsFirToScore.Value)
                    {
                        this.StarContainers[0].Margin = new Thickness(0, 0, 30, 0);
                        this.StarContainers[1].Margin = new Thickness(30, 0, 0, 0);
                    }
                    else
                    {
                        this.StarContainers[0].Margin = new Thickness(0, 0, 5, 0);
                        this.StarContainers[1].Margin = new Thickness(5, 0, 0, 0);
                    }

                    this.TopPanel.Children.Add(this.StarContainers[0]);
                    this.StarContainers[0].ContainerOrientation = ContainerOrientations.Left;
                    this.StarContainers[0].ContainerImage = ContainerImages.StarPlus;

                    this.TopPanel.Children.Add(this.StarContainers[1]);
                    this.StarContainers[1].ContainerOrientation = ContainerOrientations.Right;
                    this.StarContainers[1].ContainerImage = ContainerImages.StarPlus;
                }
                else // if (value == DisplayModes.Star_plus_only)
                {
                    if (this.preferences.IsFirToScore.Value)
                    {
                        this.StarContainers[0].Margin = new Thickness(0, 0, 30, 0);
                        this.StarContainers[1].Margin = new Thickness(30, 0, 0, 0);
                    }
                    else
                    {
                        this.StarContainers[0].Margin = new Thickness(0, 0, 5, 0);
                        this.StarContainers[1].Margin = new Thickness(5, 0, 0, 0);
                    }

                    this.TopPanel.Children.Add(this.StarContainers[0]);
                    this.StarContainers[0].ContainerOrientation = ContainerOrientations.Left;
                    this.StarContainers[0].ContainerImage = ContainerImages.StarPlain;

                    this.TopPanel.Children.Add(this.StarContainers[1]);
                    this.StarContainers[1].ContainerOrientation = ContainerOrientations.Right;
                    this.StarContainers[1].ContainerImage = ContainerImages.StarPlain;
                }

                

                this._mode = value;
            }
        }

        private bool IsPPTOn
        {
            get
            {
                this.PPTProcesses = Process.GetProcessesByName(pptName);

                if (this.PPTProcesses.Length != 1)
                {
                    return false;
                }
                return true;
            }
        }

        private bool IsInPlay
        {
            get
            {
                return (this.scoreAddress != 0x38);
            }
        }

        private bool _isStatOn;
        public bool IsStatOn
        {
            get => this._isStatOn;
            set
            {
                if (value)
                {
                    this.timer.Start();
                }
                else
                {
                    this.timer.Stop();
                }

                this._isStatOn = value;
            }
        }

        private void InitializePreferences()
        {
            this.preferences = Preferences.Load(prefName);

            if (!System.IO.Directory.Exists(exportFolderName))
            {
                System.IO.Directory.CreateDirectory(exportFolderName);
            }
        }

        private void InitializeLayouts()
        {
            InitializeSettingWindow();
            InitializeModeMenu();
            InitializeNotifyIcon();
            InitializePanel();

            void InitializeSettingWindow()
            {
                this.setting = new MetroWindow()
                {
                    Title = "Settings",
                    TitleCharacterCasing = CharacterCasing.Normal,

                    Height = 300,
                    Width = 300,
                    ResizeMode = ResizeMode.NoResize,

                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Topmost = true,
                };

                this.setting.Closing += (sender, e) =>
                {
                    e.Cancel = true;
                    this.setting.Hide();
                };

                WrapPanel SettingPanel = new WrapPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10),
                };
                this.setting.Content = SettingPanel;

                CheckBox TopMostCheckBox = new CheckBox()
                {
                    Content = "Always on Top",
                    Margin = new Thickness(5),
                    ToolTip = "항상 위로",
                };
                TopMostCheckBox.Click += (sender, e) =>
                {
                    this.preferences.IsTopMost = !this.preferences.IsTopMost;

                    if (this.preferences.IsTopMost.Value)
                    {
                        this.setting.Topmost = false;
                        this.Topmost = true;
                        this.setting.Topmost = true;
                    }
                    else
                    {
                        this.Topmost = false;
                    }
                };
                if (this.preferences.IsTopMost == null)
                {
                    this.preferences.IsTopMost = false;
                }
                this.Topmost = this.preferences.IsTopMost.Value;
                TopMostCheckBox.IsChecked = this.preferences.IsTopMost.Value;
                SettingPanel.Children.Add(TopMostCheckBox);

                CheckBox ChromaKeyCheckBox = new CheckBox()
                {
                    Content = "Enable Chroma Key (Magenta)",
                    Margin = new Thickness(5),
                    ToolTip = "Change background to Magenta for transmission;\n송출을 위해 배경을 자홍색으로 바꿉니다",
                };
                Brush NeroBrush = new SolidColorBrush(Color.FromRgb(37, 37, 37)); //new BrushConverter().ConvertFromString("#FF252525") as Brush;
                ChromaKeyCheckBox.Click += (sender, e) =>
                {
                    this.preferences.IsChromaKey = !this.preferences.IsChromaKey;
                    this.Background = (this.preferences.IsChromaKey.Value) ? (Brushes.Magenta) : (NeroBrush);
                };
                if (this.preferences.IsChromaKey == null)
                {
                    this.preferences.IsChromaKey = false;
                }
                this.Background = (this.preferences.IsChromaKey.Value)?(Brushes.Magenta): (NeroBrush);
                ChromaKeyCheckBox.IsChecked = this.preferences.IsChromaKey.Value;
                SettingPanel.Children.Add(ChromaKeyCheckBox);

                StackPanel ExportTextPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                CheckBox ExportTextCheckBox = new CheckBox()
                {
                    Content = "Export Texts",
                    Margin = new Thickness(5),
                    ToolTip = "Export these texts: (#: 1, 2, 3, 4)\n텍스트 파일로 저장합니다:\n\tStar#.txt,\n\tStarPlus#.txt,\n\tCrown#.txt,\n\tWinCount.txt,\n\tExport.json",
                };
                ExportTextCheckBox.Click += (sender, e) =>
                {
                    this.preferences.ExportText = !this.preferences.ExportText;

                    if (!System.IO.Directory.Exists(exportFolderName))
                    {
                        System.IO.Directory.CreateDirectory(exportFolderName);
                    }
                };
                if (this.preferences.ExportText == null)
                {
                    this.preferences.ExportText = false;
                }
                ExportTextCheckBox.IsChecked = this.preferences.ExportText.Value;
                ExportTextPanel.Children.Add(ExportTextCheckBox);
                Button ExportTextFolderButton = new Button()
                {
                    Content = "Folder",
                    Height = 10,
                    Margin = new Thickness(5,0,0,0),
                    ToolTip = "Open the folder where the file is stored;\n파일이 저장된 폴더를 엽니다",
                };
                ExportTextFolderButton.SetResourceReference(Control.StyleProperty, "AccentedSquareButtonStyle");
                ExportTextFolderButton.Click += (sender, e) =>
                {
                    if (!System.IO.Directory.Exists(exportFolderName))
                    {
                        System.IO.Directory.CreateDirectory(exportFolderName);
                    }

                    Process.Start(exportFolderName);
                };
                ExportTextPanel.Children.Add(ExportTextFolderButton);
                SettingPanel.Children.Add(ExportTextPanel);


                CheckBox FitScoreBoardCheckBox = new CheckBox()
                {
                    Content = "Fit/Cover to score board",
                    Margin = new Thickness(5),
                    ToolTip = "Resize to cover the original scoreboard;\n기존의 점수판을 덮을 수 있게 크기를 조절합니다",
                };
                FitScoreBoardCheckBox.Click += (sender, e) =>
                {
                    this.preferences.IsFirToScore = !this.preferences.IsFirToScore;

                    if (this.Mode >= DisplayModes.Game_only)
                    {
                        DisplayModes tokenMode = this.Mode;
                        this.Mode = DisplayModes.Game_and_Star;
                        this.Mode = tokenMode;
                    }
                    
                };
                if (this.preferences.IsFirToScore == null)
                {
                    this.preferences.IsFirToScore = false;
                }
                FitScoreBoardCheckBox.IsChecked = this.preferences.IsFirToScore.Value;
                SettingPanel.Children.Add(FitScoreBoardCheckBox);

            }

            void InitializeModeMenu()
            {
                this.ModeContextMenu = new ContextMenu()
                {

                };

                List<MenuItem> ModeItems = new List<MenuItem>();
                MenuItem Mode1Item = new MenuItem()
                {
                    Header = "1. Game & Star+",
                    ToolTip = "Count/Display both GAMEs and STARs; 게임과 별을 함께 세어 보여줍니다",
                    IsCheckable = true,
                };
                Mode1Item.Click += (sender, e) =>
                {
                    this.Mode = DisplayModes.Game_and_Star_plus;
                    this.CheckContainers();

                    this.preferences.DisplayMode = this.Mode;
                    ModeItems.ForEach(x => x.IsChecked = false);
                    (sender as MenuItem).IsChecked = true;
                };
                ModeItems.Add(Mode1Item);
                ModeContextMenu.Items.Add(Mode1Item);

                MenuItem Mode2Item = new MenuItem()
                {
                    Header = "2. Game & Star",
                    ToolTip = "Count only GAMEs not STARs just display; 게임만 세고 별은 그대로 보여줍니다",
                    IsCheckable = true,
                };
                Mode2Item.Click += (sender, e) =>
                {
                    this.Mode = DisplayModes.Game_and_Star;
                    this.CheckContainers();

                    this.preferences.DisplayMode = this.Mode;
                    ModeItems.ForEach(x => x.IsChecked = false);
                    (sender as MenuItem).IsChecked = true;
                };
                ModeItems.Add(Mode2Item);
                ModeContextMenu.Items.Add(Mode2Item);

                MenuItem Mode3Item = new MenuItem()
                {
                    Header = "3. Game",
                    ToolTip = "Count GAMEs and display (hidden STARs); 게임을 세어 보여줍니다 (별은 숨깁니다)",
                    IsCheckable = true,
                };
                Mode3Item.Click += (sender, e) =>
                {
                    this.Mode = DisplayModes.Game_only;
                    this.CheckContainers();

                    this.preferences.DisplayMode = this.Mode;
                    ModeItems.ForEach(x => x.IsChecked = false);
                    (sender as MenuItem).IsChecked = true;
                };
                ModeItems.Add(Mode3Item);
                ModeContextMenu.Items.Add(Mode3Item);

                MenuItem Mode4Item = new MenuItem()
                {
                    Header = "4. Star+",
                    ToolTip = "Count STARs and display (hidden GAMEs); 별을 세어 보여줍니다 (게임은 숨깁니다)",
                    IsCheckable = true,
                };
                Mode4Item.Click += (sender, e) =>
                {
                    this.Mode = DisplayModes.Star_plus_only;
                    this.CheckContainers();

                    this.preferences.DisplayMode = this.Mode;
                    ModeItems.ForEach(x => x.IsChecked = false);
                    (sender as MenuItem).IsChecked = true;
                };
                ModeItems.Add(Mode4Item);
                ModeContextMenu.Items.Add(Mode4Item);

                MenuItem Mode5Item = new MenuItem()
                {
                    Header = "5. Star",
                    ToolTip = "Display STARs (hidden GAMEs); 현재 별을 보여줍니다 (게임은 숨깁니다)",
                    IsCheckable = true,
                };
                Mode5Item.Click += (sender, e) =>
                {
                    this.Mode = DisplayModes.Star_only;
                    this.CheckContainers();

                    this.preferences.DisplayMode = this.Mode;
                    ModeItems.ForEach(x => x.IsChecked = false);
                    (sender as MenuItem).IsChecked = true;
                };
                ModeItems.Add(Mode5Item);
                ModeContextMenu.Items.Add(Mode5Item);

                this.ModeButton.ContextMenu = ModeContextMenu;

                if (this.preferences.DisplayMode == null)
                {
                    this.preferences.DisplayMode = DisplayModes.Game_and_Star_plus;
                    (ModeContextMenu.Items[0] as MenuItem).IsChecked = true;
                }
                else
                {
                    int modeIndex = (int)this.preferences.DisplayMode.Value;
                    (ModeContextMenu.Items[modeIndex] as MenuItem).IsChecked = true;
                }
                

            }

            void InitializeNotifyIcon()
            {
                this.notify = new wf::NotifyIcon()
                {
                    Visible = true,
                    Icon = bayoen.Properties.Resources.dailycarbuncle_174030608386,
                    Text = "bayoen-star",
                };

                this.notify.MouseDoubleClick += (sender, e) =>
                {
                    ShowMainWindow();
                };

                this.notify.ContextMenu = new wf::ContextMenu();

                wf::MenuItem OpenMenu = new wf::MenuItem()
                {
                    Text = "Open",
                };
                OpenMenu.Click += (sender, e) =>
                {
                    ShowMainWindow();
                };
                this.notify.ContextMenu.MenuItems.Add(OpenMenu);

                wf::MenuItem AckMenu = new wf::MenuItem()
                {
                    Text = "Ack.",
                };
                AckMenu.Click += (sender, e) =>
                {
                    MessageBox.Show("'bayoen~' is powered by:" + Environment.NewLine
                        + "Idea: Minacle, mat1jaczyyy" + Environment.NewLine
                        + "Icon: Get your Gu's (dailycarbuncle.tumblr.com)" + Environment.NewLine
                        + "IU: MahApps.Metro (mahapps.com)" + Environment.NewLine

                        + Environment.NewLine + "and made by SemiR4in (twitch.tv/semirain)" + Environment.NewLine
                        + "[ the.semirain@gmail.com ]" + Environment.NewLine

                        + Environment.NewLine + "and also thank you PPT KOR community!" + Environment.NewLine
                        , "Acknowledgement");
                };
                this.notify.ContextMenu.MenuItems.Add(AckMenu);


                wf::MenuItem ExitMenu = new wf::MenuItem()
                {
                    Text = "Exit",
                };
                ExitMenu.Click += (sender, e) =>
                {
                    this.preferences.Save(prefName);
                    this.notify.Visible = false;
                    Environment.Exit(0);
                };
                this.notify.ContextMenu.MenuItems.Add(ExitMenu);

                void ShowMainWindow()
                {
                    this.Show();
                    if (this.WindowState == WindowState.Minimized)
                    {
                        this.WindowState = WindowState.Normal;
                    }
                    this.Activate();
                }
            }

            void InitializePanel()
            {
                // Panel image
                this.PanelImage.SetBitmap(bayoen.Properties.Resources.PanelScore2LongStrong);
                this.PanelImage.Height = 68;
                this.PanelImage.Width = 406;

                // Containers
                this.StarContainers = new List<Container>()
                {
                    new Container() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, },
                    new Container() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, },
                };
                this.CrownContainers = new List<Container>()
                {
                    new Container() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, ContainerImage = ContainerImages.CrownLight },
                    new Container() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, ContainerImage = ContainerImages.CrownLight },
                };

                if (this.preferences.DisplayMode == null)
                {
                    this.preferences.DisplayMode = DisplayModes.Game_and_Star_plus;
                }
                this._mode = (this.preferences.DisplayMode == DisplayModes.Game_and_Star_plus) ? (DisplayModes.Game_and_Star) : (DisplayModes.Game_and_Star_plus);
                this.Mode = this.preferences.DisplayMode.Value;                
            }
        }

        private void InitializeTimer(int milliseconds)
        {
            this.timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, milliseconds),
            };
            this.timer.Tick += (e, sender) =>
            {
                this.CountingStars();
            };                       
        }

        private void InitializeVariables()
        {
            this.pptMemory = new VAMemory(pptName);

            this.oldStar = new List<int>() { 0, 0 };
            this.currentStar = new List<int>() { 0, 0 };


            if (File.Exists(dataJSONName))
            {
                Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(dataJSONName, Encoding.Unicode));
                Newtonsoft.Json.Linq.JToken star1Token = json.SelectToken("StarPlus1");
                Newtonsoft.Json.Linq.JToken star2Token = json.SelectToken("StarPlus2");
                Newtonsoft.Json.Linq.JToken crown1Token = json.SelectToken("Crown1");
                Newtonsoft.Json.Linq.JToken crown2Token = json.SelectToken("Crown2");
                Newtonsoft.Json.Linq.JToken winCountToken = json.SelectToken("WinCount");

                this.countingStar = new List<int>()
                {
                    (star1Token == null)?(0):(star1Token.ToObject<int>()),
                    (star2Token == null)?(0):(star2Token.ToObject<int>()),
                };
                this.countingCrown = new List<int>()
                {
                    (crown1Token == null)?(0):(crown1Token.ToObject<int>()),
                    (crown2Token == null)?(0):(crown2Token.ToObject<int>()),
                };
                this.winCount = (winCountToken == null) ? (-1) : (winCountToken.ToObject<int>());
            }
            else
            {
                this.countingStar = new List<int>() { 0, 0 };
                this.countingCrown = new List<int>() { 0, 0 };
                this.winCount = -1;
            }            
        }

        private void CountingStars()
        {
            if (IsPPTOn)
            {
                if (this.TopGrid.Visibility == Visibility.Collapsed) this.TopGrid.Visibility = Visibility.Visible;
                this.scoreAddress = this.pptMemory.ReadInt32(new IntPtr(0x14057F048)) + 0x38;
            }
            else
            {
                if (this.TopGrid.Visibility == Visibility.Visible) this.TopGrid.Visibility = Visibility.Collapsed;
                return;
            }

            if (IsInPlay)
            {
                for (int playerIndex = 0; playerIndex < 2; playerIndex++)
                {
                    this.currentStar[playerIndex] = this.pptMemory.ReadInt32(new IntPtr(scoreAddress) + playerIndex * 0x4);
                }
                this.winCount = this.pptMemory.ReadInt32(new IntPtr(scoreAddress) + 0x10);

                //this.StarContainers.ForEach(x => { if (!x.IsValid) { x.IsValid = true; } });
                //this.CrownContainers.ForEach(x => { if (!x.IsValid) { x.IsValid = true; } });
            }
            else
            {
                //this.StarContainers.ForEach(x => { if (x.IsValid) { x.IsValid = false; } });
                //this.CrownContainers.ForEach(x => { if (x.IsValid) { x.IsValid = false; } });
                return;
            }

            List<int> gradients = this.currentStar.Zip(this.oldStar, (a, b) => a - b).ToList();
            if (gradients.IndexOf(1) > -1)
            {
                for (int playerIndex = 0; playerIndex < 2; playerIndex++)
                {
                    if (gradients[playerIndex] == 1) this.countingStar[playerIndex]++;

                    if (this.winCount == this.currentStar[playerIndex])
                    {
                        this.countingCrown[playerIndex]++;
                    }

                    if (this.preferences.ExportText.Value)
                    {
                        if (!Directory.Exists(exportFolderName))
                        {
                            Directory.CreateDirectory(exportFolderName);
                        }

                        File.WriteAllText(exportFolderName + '\\' + "Star1.txt", this.currentStar[0].ToString(), Encoding.Unicode);
                        File.WriteAllText(exportFolderName + '\\' + "Star2.txt", this.currentStar[1].ToString(), Encoding.Unicode);
                        File.WriteAllText(exportFolderName + '\\' + "StarPlus1.txt", this.countingStar[0].ToString(), Encoding.Unicode);
                        File.WriteAllText(exportFolderName + '\\' + "StarPlus2.txt", this.countingStar[1].ToString(), Encoding.Unicode);
                        File.WriteAllText(exportFolderName + '\\' + "Crown1.txt", this.countingCrown[0].ToString(), Encoding.Unicode);
                        File.WriteAllText(exportFolderName + '\\' + "Crown2.txt", this.countingCrown[1].ToString(), Encoding.Unicode);
                        File.WriteAllText(exportFolderName + '\\' + "WinCount1.txt", this.winCount.ToString(), Encoding.Unicode);
                    }


                    Newtonsoft.Json.Linq.JObject json = new Newtonsoft.Json.Linq.JObject()
                    {
                        ["Star1"] = this.currentStar[0],
                        ["Star2"] = this.currentStar[1],
                        ["StarPlus1"] = this.countingStar[0],
                        ["StarPlus2"] = this.countingStar[1],
                        ["Crown1"] = this.countingCrown[0],
                        ["Crown2"] = this.countingCrown[1],
                        ["WinCount"] = this.winCount,
                    };
                    File.WriteAllText(dataJSONName, json.ToString(), Encoding.Unicode);
                }
            }

            this.CheckContainers();

            for (int playerIndex = 0; playerIndex < 2; playerIndex++)
            {
                this.oldStar[playerIndex] = this.currentStar[playerIndex];
            }
        }

        private void CheckContainers()
        {
            if (this.Mode == DisplayModes.Game_and_Star_plus)
            {
                this.StarContainers[0].Score = this.countingStar[0];
                this.StarContainers[1].Score = this.countingStar[1];

                this.CrownContainers[0].Score = this.countingCrown[0];
                this.CrownContainers[1].Score = this.countingCrown[1];
            }
            else if (this.Mode == DisplayModes.Game_and_Star)
            {
                this.StarContainers[0].Score = this.currentStar[0];
                this.StarContainers[1].Score = this.currentStar[1];

                this.CrownContainers[0].Score = this.countingCrown[0];
                this.CrownContainers[1].Score = this.countingCrown[1];
            }
            else if (this.Mode == DisplayModes.Game_only)
            {
                this.CrownContainers[0].Score = this.countingCrown[0];
                this.CrownContainers[1].Score = this.countingCrown[1];
            }
            else if (this.Mode == DisplayModes.Star_plus_only)
            {
                this.StarContainers[0].Score = this.countingStar[0];
                this.StarContainers[1].Score = this.countingStar[1];
            }
            else // if (this.Mode == DisplayModes.Star_plus_only)
            {
                this.StarContainers[0].Score = this.currentStar[0];
                this.StarContainers[1].Score = this.currentStar[1];
            }
        }

        private async void ClearButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync("Do clear?", "", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                this.countingStar = new List<int>() { 0, 0 };
                this.countingCrown = new List<int>() { 0, 0 };

                this.CheckContainers();
                System.Media.SystemSounds.Hand.Play();
            }
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            this.setting.Show();
            if (this.setting.WindowState == WindowState.Minimized)
            {
                this.setting.WindowState = WindowState.Normal;
            }
            this.setting.Activate();
        }

        private void ModeButton_Click(object sender, RoutedEventArgs e)
        {
            ModeContextMenu.IsOpen = true;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();

            if (preferences.EverClosed == null)
            {
                NotifyMinimizing();
            }
            else if (!preferences.EverClosed.Value)
            {
                NotifyMinimizing();
            }

            void NotifyMinimizing()
            {
                this.notify.ShowBalloonTip(2000, "Closing → Minimizing", "Minimized into system tray\nPlease right-click icon!", wf::ToolTipIcon.None);
                preferences.EverClosed = true;
            }
        }

        public enum DisplayModes : int
        {
            Game_and_Star_plus = 0,
            Game_and_Star = 1,
            Game_only = 2,
            Star_plus_only = 3,
            Star_only = 4,
        }
    }

    public static partial class ExtendedMethods
    {
        public static bool SetBitmap(this System.Windows.Controls.Image image, System.Drawing.Bitmap bitmap)
        {
            try
            {
                using (System.IO.MemoryStream streamToken = new System.IO.MemoryStream())
                {
                    bitmap.Save(streamToken, System.Drawing.Imaging.ImageFormat.Png);
                    streamToken.Position = 0;

                    BitmapImage bitmapImageToken = new BitmapImage();
                    bitmapImageToken.BeginInit();
                    bitmapImageToken.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImageToken.StreamSource = streamToken;
                    bitmapImageToken.EndInit();

                    image.Source = bitmapImageToken;
                    bitmapImageToken.Freeze();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
