using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using wf = System.Windows.Forms;

namespace bayoen
{
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// bayoen-star
        /// </summary>
        public MainWindow()
        {
            this.InitializePreferences();
            this.InitializeComponent();
            this.InitializeLayouts();
            this.InitializeTimer();
            this.InitializeVariables();
            this.Status("Ready");
            Thread.Sleep(1000);

            this.CheckContainers();
            this.ToMonitors();
            this.CheckUpdate();
            this.IsStatOn = true;
        }

        public Display2Grid MainDisplay;
        public Display2Grid OverlayDisplay;
        public List<TextBox> Monitors;

        public static Version currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        public Version latestVersion;

        public static string versionText = string.Format(" - Beta v{0}", currentVersion);
        public const string pptName = "puyopuyotetris";
        public const string prefName = "pref.json";
        public const string exportFolderName = "export";
        public const string dataJSONName = "data.json";
        public Preferences preferences;
        public wf::NotifyIcon Notify;
        public MetroWindow Setting;
        public MetroWindow Overlay;

        public VAMemory pptMemory;
        public RECT currentRect, oldRect;
        public int scoreAddress;
        public Process[] PPTProcesses;
        public DispatcherTimer timer;

        public int winCount;
        public int winMatch;
        public List<int> currentStar;
        public List<int> oldStar;
        public List<int> countingStar;
        public List<int> countingCrown;

        private int _goalScore;
        public int GoalScore
        {
            get => this._goalScore;
            set
            {
                int refined = Math.Min(Math.Max(0, value), 9999);

                if (this.GoalType == GoalTypes.None)
                {
                    this.MainDisplay.HideGoal();
                    this.OverlayDisplay.HideGoal();
                }
                else
                {
                    this.MainDisplay.DisplayGoal(refined, this.GoalType);
                    this.OverlayDisplay.DisplayGoal(refined, this.GoalType);
                }

                this._goalScore = refined;
            }
        }

        private GoalTypes _goalType;
        public GoalTypes GoalType
        {
            get => this._goalType;
            set
            {
                if (value == GoalTypes.None)
                {
                    this.MainDisplay.HideGoal();
                    this.OverlayDisplay.HideGoal();
                }   
                else
                {
                    this.MainDisplay.DisplayGoal(this.GoalScore, value);
                    this.OverlayDisplay.DisplayGoal(this.GoalScore, value);
                }

                this._goalType = value;
            }
        }

        public DisplayModes _mode;
        public DisplayModes Mode
        {
            get => this._mode;
            set
            {
                //if (value == this._mode)
                //{
                //    return;
                //}

                this.MainDisplay.SetMode(value, this.preferences.IsFitToScore.Value);
                this.OverlayDisplay.SetMode(value, this.preferences.IsFitToScore.Value);                

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


        private bool IsGoalOver
        {
            get
            {
                if (this.GoalScore == 0)
                {
                    return false;
                }

                if (this.GoalType == GoalTypes.None)
                {
                    return false;
                }
                else if (this.GoalType == GoalTypes.Star)
                {
                    return this.countingStar.FindIndex(x => x >= this.GoalScore) > -1;
                }
                else // if (this.GoalType == GoalTypes.Crown)
                {
                    return this.countingCrown.FindIndex(x => x >= this.GoalScore) > -1;
                }
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

        public bool IsGoogleOn
        {
            get
            {
                try
                {
                    using (var client = new System.Net.WebClient())
                    using (client.OpenRead("http://clients3.google.com/generate_204"))
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
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
            InitializeTopMenu();
            InitializeSettingWindow();
            InitializeNotifyIcon();
            InitializeDisplay();

            void InitializeTopMenu()
            {
                MenuItem ResetMenuItem = BuildMenu("Reset", "appbar_new");
                ResetMenuItem.Click += ResetMenuItem_ClickAsync;
                this.TopCompositeCollection.Add(ResetMenuItem);

                MenuItem GoalMenuItem = BuildMenu("Goal", "appbar_controller_xbox");
                this.TopCompositeCollection.Add(GoalMenuItem);

                List<MenuItem> GoalItems = new List<MenuItem>();
                MenuItem SetGoalItem = new MenuItem()
                {
                    Header = "Set...",
                    ToolTip = "Set goal with its score and type; 목표를 입력하고 설정합니다",
                };
                SetGoalItem.Click += (sender, e) =>
                {
                    this.GoalFlyout.IsOpen = true;
                };
                GoalItems.Add(SetGoalItem);
                this.SetGoalButton.Click += (sender, e) =>
                {
                    this.preferences.GoalType = (GoalTypes)(this.GoalTypeComboBox.SelectedIndex);
                    this.preferences.GoalScore = (int)this.GoalScoreNumericUpDown.Value.Value;

                    this.GoalType = this.preferences.GoalType.Value;
                    this.GoalScore = this.preferences.GoalScore.Value;
                    this.GoalFlyout.IsOpen = false;
                    System.Media.SystemSounds.Hand.Play();
                };
                this.ClearGoalButton.Click += (sender, e) =>
                {
                    this.GoalTypeComboBox.SelectedIndex = 0;
                    this.GoalScoreNumericUpDown.Value = 0;
                };
                List<Tuple<GoalTypes, System.Drawing.Bitmap>> GoalTypeSets = new List<Tuple<GoalTypes, System.Drawing.Bitmap>>()
                {
                    new Tuple<GoalTypes, System.Drawing.Bitmap>( GoalTypes.None,  null),
                    new Tuple<GoalTypes, System.Drawing.Bitmap>( GoalTypes.Star, bayoen.Properties.Resources.StarPlus),
                    new Tuple<GoalTypes, System.Drawing.Bitmap>( GoalTypes.Crown, bayoen.Properties.Resources.CrownLight),
                };
                List<ComboBoxItem> GoalTypeItemList = new List<ComboBoxItem>();
                foreach (Tuple<GoalTypes, System.Drawing.Bitmap> tokenGoalType in GoalTypeSets)
                {
                    StackPanel TokenGoalTypeStackPanel = new StackPanel()
                    {
                        Margin = new Thickness(0),
                        Orientation = Orientation.Horizontal,
                    };

                    Image TokenGoalTypeImage = new Image()
                    {
                        Width = 14,
                        Height = 14,
                        Margin = new Thickness(3, 2, 5, 2),
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    if (tokenGoalType.Item2 != null) TokenGoalTypeImage.SetBitmap(tokenGoalType.Item2);
                    TokenGoalTypeStackPanel.Children.Add(TokenGoalTypeImage);

                    TextBlock TokenGoalTypeTextBlock = new TextBlock()
                    {
                        Text = tokenGoalType.Item1.ToString(),
                        FontWeight = FontWeights.Normal,
                        Margin = new Thickness(2),
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    TokenGoalTypeStackPanel.Children.Add(TokenGoalTypeTextBlock);

                    ComboBoxItem TokenGoalTypeItem = new ComboBoxItem()
                    {
                        Background = Brushes.Black,
                        Content = TokenGoalTypeStackPanel,
                    };
                    GoalTypeItemList.Add(TokenGoalTypeItem);
                }
                this.GoalTypeComboBox.ItemsSource = GoalTypeItemList;
                MenuItem RemoveGoalItem = new MenuItem()
                {
                    Header = "Remove",
                    ToolTip = "Remove goal; 목표를 취소합니다",
                };
                RemoveGoalItem.Click += (sender, e) =>
                {
                    this.preferences.GoalType = GoalTypes.None;
                    this.preferences.GoalScore = 0;

                    this.GoalType = this.preferences.GoalType.Value;
                    this.GoalScore = this.preferences.GoalScore.Value;
                };
                GoalItems.Add(RemoveGoalItem);
                GoalMenuItem.ItemsSource = GoalItems;

                if (this.preferences.GoalType == null)
                {
                    this.preferences.GoalType = GoalTypes.None;
                }
                if (this.preferences.GoalScore == null)
                {
                    this.preferences.GoalScore = 0;
                }

                this.GoalTypeComboBox.SelectedIndex = (int)this.preferences.GoalType.Value;
                this.GoalScoreNumericUpDown.Value = this.preferences.GoalScore.Value;

                MenuItem OverlayMenuItem = BuildMenu("Overlay", "appbar_app_plus");
                OverlayMenuItem.Click += OverlayMenuItem_Click;
                this.TopCompositeCollection.Add(OverlayMenuItem);

                MenuItem SettingMenuItem = BuildMenu("Settings", "appbar_settings");
                SettingMenuItem.Click += SettingMenuItem_Click;
                this.TopCompositeCollection.Add(SettingMenuItem);

                MenuItem ModeMenuItem = BuildMenu("Mode", "appbar_list");
                this.TopCompositeCollection.Add(ModeMenuItem);

                List<MenuItem> ModeItems = new List<MenuItem>();
                MenuItem Mode1Item = new MenuItem()
                {
                    Header = "1. Star+",
                    ToolTip = "Count STARs and display (hidden GAMEs); 별을 세어 보여줍니다 (게임은 숨깁니다)",
                    IsCheckable = true,
                };
                Mode1Item.Click += (sender, e) =>
                {
                    this.Mode = DisplayModes.Star_plus_only;
                    this.CheckContainers();

                    this.preferences.DisplayMode = this.Mode;
                    ModeItems.ForEach(x => x.IsChecked = false);
                    (sender as MenuItem).IsChecked = true;
                };
                ModeItems.Add(Mode1Item);

                MenuItem Mode2Item = new MenuItem()
                {
                    Header = "2. Game",
                    ToolTip = "Count GAMEs and display (hidden STARs); 게임을 세어 보여줍니다 (별은 숨깁니다)",
                    IsCheckable = true,
                };
                Mode2Item.Click += (sender, e) =>
                {
                    this.Mode = DisplayModes.Game_only;
                    this.CheckContainers();

                    this.preferences.DisplayMode = this.Mode;
                    ModeItems.ForEach(x => x.IsChecked = false);
                    (sender as MenuItem).IsChecked = true;
                };
                ModeItems.Add(Mode2Item);

                MenuItem Mode3Item = new MenuItem()
                {
                    Header = "3. Game & Star",
                    ToolTip = "Count only GAMEs not STARs just display; 게임만 세고 별은 그대로 보여줍니다",
                    IsCheckable = true,
                };
                Mode3Item.Click += (sender, e) =>
                {
                    this.Mode = DisplayModes.Game_and_Star;
                    this.CheckContainers();

                    this.preferences.DisplayMode = this.Mode;
                    ModeItems.ForEach(x => x.IsChecked = false);
                    (sender as MenuItem).IsChecked = true;
                };
                ModeItems.Add(Mode3Item);

                MenuItem Mode4Item = new MenuItem()
                {
                    Header = "4. Game & Star+",
                    ToolTip = "Count/Display both GAMEs and STARs; 게임과 별을 함께 세어 보여줍니다",
                    IsCheckable = true,
                };
                Mode4Item.Click += (sender, e) =>
                {
                    this.Mode = DisplayModes.Game_and_Star_plus;
                    this.CheckContainers();

                    this.preferences.DisplayMode = this.Mode;
                    ModeItems.ForEach(x => x.IsChecked = false);
                    (sender as MenuItem).IsChecked = true;
                };
                ModeItems.Add(Mode4Item);

                MenuItem Mode5Item = new MenuItem()
                {
                    Header = "5. Star & Star+",
                    ToolTip = "Couont/Display STARs (hidden GAMEs); 별을 세고 현재 별을 보여줍니다 (게임은 숨깁니다)",
                    IsCheckable = true,
                };
                Mode5Item.Click += (sender, e) =>
                {
                    this.Mode = DisplayModes.Star_plus_and_Star;
                    this.CheckContainers();

                    this.preferences.DisplayMode = this.Mode;
                    ModeItems.ForEach(x => x.IsChecked = false);
                    (sender as MenuItem).IsChecked = true;
                };
                ModeItems.Add(Mode5Item);

                ModeMenuItem.ItemsSource = ModeItems;

                if (this.preferences.DisplayMode == null)
                {
                    this.preferences.DisplayMode = DisplayModes.Game_and_Star_plus;
                    (ModeItems[0] as MenuItem).IsChecked = true;
                }
                else
                {
                    int modeIndex = (int)this.preferences.DisplayMode.Value;
                    (ModeItems[modeIndex] as MenuItem).IsChecked = true;
                }


                MenuItem BuildMenu(string header, string appbar)
                {
                    return new MenuItem()
                    {
                        Header = header,
                        Icon = new Rectangle()
                        {
                            Width = 15,
                            Height = 15,
                            Fill = Brushes.White,
                            Margin = new Thickness(8, 8, 2, 8),
                            OpacityMask = new VisualBrush()
                            {
                                AlignmentX = AlignmentX.Center,
                                AlignmentY = AlignmentY.Center,
                                Stretch = Stretch.Uniform,
                                Visual = TryFindResource(appbar) as Visual,
                            },
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                        },
                    };
                }
            }

            void InitializeSettingWindow()
            {
                this.Setting = new MetroWindow()
                {
                    Title = "settings",
                    TitleCharacterCasing = CharacterCasing.Normal,

                    Height = 270,
                    Width = 520,
                    ResizeMode = ResizeMode.NoResize,

                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Topmost = true,
                };

                this.Setting.MouseLeftButtonDown += (sender, e) =>
                {
                    this.Setting.DragMove();
                };

                this.Setting.Closing += (sender, e) =>
                {
                    e.Cancel = true;
                    this.Setting.Hide();
                };

                WrapPanel SettingPanel = new WrapPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                this.Setting.Content = SettingPanel;

                #region MainGroup
                GroupBox MainGroupBox = new GroupBox()
                {
                    Header = "Main",
                    Margin = new Thickness(5),
                };
                SettingPanel.Children.Add(MainGroupBox);

                WrapPanel MainGroupPanel = new WrapPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10),
                };
                MainGroupBox.Content = MainGroupPanel;

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
                        this.Setting.Topmost = false;
                        this.Topmost = true;
                        this.Setting.Topmost = true;
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
                MainGroupPanel.Children.Add(TopMostCheckBox);

                CheckBox HideOfflineCheckBox = new CheckBox()
                {
                    Content = "Hide panels when PPT is offline",
                    Margin = new Thickness(5),
                    ToolTip = "뿌요뿌요 테트리스가 꺼져있을 때 점수판을 숨깁니다",
                };
                HideOfflineCheckBox.Click += (sender, e) =>
                {
                    this.preferences.IsHideOffline = !this.preferences.IsHideOffline;

                    if (this.MainDisplay.Visibility == Visibility.Collapsed) this.MainDisplay.Visibility = Visibility.Visible;
                    if (this.OverlayDisplay.Visibility == Visibility.Hidden) this.OverlayDisplay.Visibility = Visibility.Visible;
                };
                if (this.preferences.IsHideOffline == null)
                {
                    this.preferences.IsHideOffline = false;
                }                
                HideOfflineCheckBox.IsChecked = this.preferences.IsHideOffline.Value;
                MainGroupPanel.Children.Add(HideOfflineCheckBox);

                StackPanel ExportTextPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                CheckBox ExportTextCheckBox = new CheckBox()
                {
                    Content = "Export Texts",
                    Margin = new Thickness(5),
                    ToolTip = "Export these texts: (#: 1, 2, 3, 4)\n텍스트 파일로 저장합니다:\n\tStar#.txt,\n\tStarPlus#.txt,\n\tCrown#.txt,\n\tWinCount.txt",
                };
                ExportTextCheckBox.Click += (sender, e) =>
                {
                    this.preferences.ExportText = !this.preferences.ExportText;

                    if (!System.IO.Directory.Exists(exportFolderName))
                    {
                        System.IO.Directory.CreateDirectory(exportFolderName);
                    }

                    if (this.preferences.ExportText.Value)
                    {
                        this.Export();
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
                    if (!Directory.Exists(exportFolderName))
                    {
                        Directory.CreateDirectory(exportFolderName);
                    }

                    Process.Start(exportFolderName);
                };
                ExportTextPanel.Children.Add(ExportTextFolderButton);
                MainGroupPanel.Children.Add(ExportTextPanel);
           
                CheckBox FitScoreBoardCheckBox = new CheckBox()
                {
                    Content = "Fit/Cover to score board",
                    Margin = new Thickness(5),
                    ToolTip = "Resize to cover the original scoreboard;\n기존의 점수판을 덮을 수 있게 크기를 조절합니다",
                };
                FitScoreBoardCheckBox.Click += (sender, e) =>
                {
                    this.preferences.IsFitToScore = !this.preferences.IsFitToScore;

                    if (this.Mode < DisplayModes.Game_and_Star)
                    {
                        DisplayModes tokenMode = this.Mode;
                        this.Mode = DisplayModes.Game_and_Star;
                        this.Mode = tokenMode;
                    }
                    
                };
                if (this.preferences.IsFitToScore == null)
                {
                    this.preferences.IsFitToScore = false;
                }
                FitScoreBoardCheckBox.IsChecked = this.preferences.IsFitToScore.Value;
                MainGroupPanel.Children.Add(FitScoreBoardCheckBox);

                StackPanel ChromaKeyPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                TextBlock ChromaKeyTextBlock = new TextBlock()
                {
                    Text = "- Chroma key",
                    Margin = new Thickness(5, 5, 0, 5),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                ChromaKeyPanel.Children.Add(ChromaKeyTextBlock);
                List<Tuple<ChromaKeys, Brush>> ChromaSets = new List<Tuple<ChromaKeys, Brush>>()
                {
                    new Tuple<ChromaKeys, Brush>( ChromaKeys.None,  new SolidColorBrush(Color.FromRgb(37, 37, 37))),
                    new Tuple<ChromaKeys, Brush>( ChromaKeys.Magenta, Brushes.Magenta),
                    new Tuple<ChromaKeys, Brush>( ChromaKeys.Green, Brushes.Green),
                    new Tuple<ChromaKeys, Brush>( ChromaKeys.Blue, Brushes.Blue),
                };
                ComboBoxItem TokenAccentItem;
                List<ComboBoxItem> AccentItemList = new List<ComboBoxItem>();
                foreach (Tuple<ChromaKeys, Brush> tokenChroma in ChromaSets)
                {
                    TokenAccentItem = new ComboBoxItem()
                    {
                        Background = Brushes.Black,
                        Content = new StackPanel()
                        {
                            Margin = new Thickness(0),
                            Orientation = Orientation.Horizontal,                            
                        },
                    };
                    (TokenAccentItem.Content as StackPanel).Children.Add(new Rectangle()
                    {
                        Width = 12,
                        Height = 12,
                        Fill = tokenChroma.Item2,
                        Margin = new Thickness(4, 0, 4, 0),
                        OpacityMask = new VisualBrush()
                        {
                            AlignmentX = AlignmentX.Center,
                            AlignmentY = AlignmentY.Center,
                            Stretch = Stretch.Uniform,
                            Visual = TryFindResource("appbar_3d_obj") as Visual,
                        },
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    });

                    (TokenAccentItem.Content as StackPanel).Children.Add(new TextBlock()
                    {
                        Text = tokenChroma.Item1.ToString(),
                        Margin = new Thickness(2),
                    });

                    AccentItemList.Add(TokenAccentItem);
                }
                ComboBox ChromaKeyComboBox = new ComboBox()
                {
                    Width = 110,
                    Margin = new Thickness(5),
                    Background = Brushes.Black,
                    ItemsSource = AccentItemList,                    
                    VerticalAlignment = VerticalAlignment.Center,
                };
                ChromaKeyPanel.Children.Add(ChromaKeyComboBox);
                ChromaKeyComboBox.SelectionChanged += (sender, e) =>
                {
                    this.preferences.ChromaKey = (ChromaKeys)ChromaKeyComboBox.SelectedIndex;
                    this.Background = ChromaSets[ChromaKeyComboBox.SelectedIndex].Item2;
                };
                if (this.preferences.ChromaKey == null)
                {
                    if (this.preferences.IsChromaKey == null)
                    {
                        this.preferences.ChromaKey = ChromaKeys.None;
                    }
                    else
                    {
                        if (this.preferences.IsChromaKey.Value)
                        {
                            this.preferences.ChromaKey = ChromaKeys.None;
                        }
                        else
                        {
                            this.preferences.ChromaKey = ChromaKeys.Magenta;
                        }

                        this.preferences.IsChromaKey = null;
                    }                    
                }
                this.Background = ChromaSets[(int)this.preferences.ChromaKey].Item2;
                ChromaKeyComboBox.SelectedIndex = (int)this.preferences.ChromaKey;

                MainGroupPanel.Children.Add(ChromaKeyPanel);
                #endregion

                #region MonitorGroup

                this.Monitors = new List<TextBox>();

                GroupBox MonitorGroupBox = new GroupBox()
                {
                    Header = "Monitor",
                    Margin = new Thickness(5),
                };
                SettingPanel.Children.Add(MonitorGroupBox);

                WrapPanel MonitorGroupPanel = new WrapPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10),
                    Height = 150,
                };
                MonitorGroupBox.Content = MonitorGroupPanel;

                double setHeight = 22;
                double setWidth1 = 60;
                double setWidth2 = 40;

                StackPanel Star1Set = SetText("Star 1", setHeight, setWidth1, setWidth2, false);
                MonitorGroupPanel.Children.Add(Star1Set);                
                StackPanel StarPlus1Set = SetText("Star+ 1", setHeight, setWidth1, setWidth2, true);
                MonitorGroupPanel.Children.Add(StarPlus1Set);
                StackPanel Crown1Set = SetText("Crown 1", setHeight, setWidth1, setWidth2, true);
                MonitorGroupPanel.Children.Add(Crown1Set);
                StackPanel WinCountSet = SetText("Win Count", setHeight, setWidth1, setWidth2, false);
                MonitorGroupPanel.Children.Add(WinCountSet);
                StackPanel Star2Set = SetText("Star 2", setHeight, setWidth1, setWidth2, false);
                MonitorGroupPanel.Children.Add(Star2Set);
                StackPanel StarPlus2Set = SetText("Star+ 2", setHeight, setWidth1, setWidth2, true);
                MonitorGroupPanel.Children.Add(StarPlus2Set);
                StackPanel Crown2Set = SetText("Crown 2", setHeight, setWidth1, setWidth2, true);
                MonitorGroupPanel.Children.Add(Crown2Set);

                StackPanel ButtonSet = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(25, 5, 5, 5),
                };
                Button MonitorLoadButton = new Button()
                {
                    Content = "Load",
                    Width = 40,
                    Margin = new Thickness(0,0,5,0),
                };
                MonitorLoadButton.Click += (sender, e) =>
                {
                    this.ToMonitors();
                };
                MonitorLoadButton.SetResourceReference(Control.StyleProperty, "AccentedSquareButtonStyle");
                ButtonSet.Children.Add(MonitorLoadButton);
                Button MonitorSaveButton = new Button()
                {
                    Content = "Save",
                    Width = 40,
                    Margin = new Thickness(0),
                };
                MonitorSaveButton.Click += (sender, e) =>
                {
                    this.FromMonitors();

                    this.CheckContainers();
                    this.CheckOverlay();
                    this.Save();
                    this.Export();
                };
                ButtonSet.Children.Add(MonitorSaveButton);
                MonitorSaveButton.SetResourceReference(Control.StyleProperty, "AccentedSquareButtonStyle");
                MonitorGroupPanel.Children.Add(ButtonSet);

                this.Monitors.Add(Star1Set.Children[1] as TextBox);
                this.Monitors.Add(Star2Set.Children[1] as TextBox);
                this.Monitors.Add(StarPlus1Set.Children[1] as TextBox);
                this.Monitors.Add(StarPlus2Set.Children[1] as TextBox);
                this.Monitors.Add(Crown1Set.Children[1] as TextBox);
                this.Monitors.Add(Crown2Set.Children[1] as TextBox);
                this.Monitors.Add(WinCountSet.Children[1] as TextBox);

                this.Monitors.ForEach(x => x.Text = "?");

                #endregion

                StackPanel SetText(string header, double height, double width1, double width2, bool isEnabled)
                {
                    TextBlock tokenTextBlock = new TextBlock()
                    {
                        Text = header,
                        TextAlignment = TextAlignment.Right,
                        Margin = new Thickness(0),                        
                        Width = width1,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    Border tokenBorder = new Border()
                    {
                        Height = height,
                        Child = tokenTextBlock,
                    };
                    TextBox tokenTextBox = new TextBox()
                    {
                        TextAlignment = TextAlignment.Center,
                        Padding = new Thickness(0,2,0,0),
                        Margin = new Thickness(5,0,0,0),
                        Height = height,
                        Width = width2,
                        IsEnabled = isEnabled,
                    };
                    StackPanel tokenPanel = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(5),
                    };
                    tokenPanel.Children.Add(tokenBorder);
                    tokenPanel.Children.Add(tokenTextBox);

                    return tokenPanel;
                }
            }

            void InitializeNotifyIcon()
            {
                this.Notify = new wf::NotifyIcon()
                {
                    Visible = true,
                    Icon = bayoen.Properties.Resources.dailycarbuncle_174030608386,
                    Text = "bayoen-star",
                };

                this.Notify.MouseDoubleClick += (sender, e) =>
                {
                    ShowMainWindow();
                };

                this.Notify.ContextMenu = new wf::ContextMenu();

                wf::MenuItem OpenMenu = new wf::MenuItem()
                {
                    Text = "Open",
                };
                OpenMenu.Click += (sender, e) =>
                {
                    ShowMainWindow();
                };
                this.Notify.ContextMenu.MenuItems.Add(OpenMenu);

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

                        + Environment.NewLine + "and also thank you all PPT communities!" + Environment.NewLine
                        , "Acknowledgement");
                };
                this.Notify.ContextMenu.MenuItems.Add(AckMenu);


                wf::MenuItem ExitMenu = new wf::MenuItem()
                {
                    Text = "Exit",
                };
                ExitMenu.Click += (sender, e) =>
                {
                    this.preferences.Save(prefName);
                    this.Save();
                    this.Notify.Visible = false;
                    Environment.Exit(0);
                };
                this.Notify.ContextMenu.MenuItems.Add(ExitMenu);

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

            void InitializeDisplay()
            {
                this.MainDisplay = new Display2Grid();
                this.Content = this.MainDisplay;

                if (IsPPTOn)
                {
                    GetWindowRect(this.PPTProcesses.Single().MainWindowHandle, ref this.currentRect);
                }
                else
                {
                    this.currentRect = new RECT()
                    {
                        Left = -1,
                        Top = -1,
                        Right = -1,
                        Bottom = -1,
                    };
                }

                this.currentRect = new RECT(this.oldRect);

                this.OverlayDisplay = new Display2Grid();
                this.Overlay = new MetroWindow()
                {
                    Title = "bayoen-star-overlay",
                    BorderThickness = new Thickness(0),
                    AllowsTransparency = true,
                    Background = new SolidColorBrush() { Opacity = 0 },
                    ResizeMode = ResizeMode.NoResize,
                    TitlebarHeight = 0,
                    Topmost = true,
                };

                Rect workingRect = System.Windows.SystemParameters.WorkArea;
                bool isOverlayBroken = false;
                if (this.preferences.Overlay == null)
                {
                    isOverlayBroken = true;
                }
                else
                {
                    if (this.preferences.Overlay.Count != 5) isOverlayBroken = true;
                    else if (this.preferences.Overlay[0] < 0 || this.preferences.Overlay[0] > 1) isOverlayBroken = true;                    
                }

                if (isOverlayBroken)
                {                    
                    this.Overlay.Height = this.Height;
                    this.Overlay.Width = this.Width;
                    this.Overlay.Top = (workingRect.Height - this.Overlay.Height) / 2 + workingRect.Top;
                    this.Overlay.Left = (workingRect.Width - this.Overlay.Width) / 2 + workingRect.Left;
                    this.preferences.Overlay = new List<double>() { 1, 0, 0, this.Overlay.Left, this.Overlay.Top };
                }
                else
                {
                    this.Overlay.LayoutTransform = new ScaleTransform(this.preferences.Overlay[0], this.preferences.Overlay[0]);
                    this.Overlay.Height = this.Height * this.preferences.Overlay[0];
                    this.Overlay.Width = this.Width * this.preferences.Overlay[0];

                    if (IsPPTOn)
                    {
                        this.Overlay.Left = this.currentRect.Left - this.preferences.Overlay[1];
                        this.Overlay.Top = this.currentRect.Top - this.preferences.Overlay[2];
                    }
                    else
                    {
                        this.Overlay.Left = this.preferences.Overlay[3];
                        this.Overlay.Top = this.preferences.Overlay[4];
                    }
                    
                }

                double delta = 0.035;
                this.Overlay.PreviewMouseWheel += (sender, e) =>
                {
                    if (this.preferences.IsOverlayFixed.Value)
                    {
                        return;
                    }

                    if (e.Delta > 0)
                    {
                        this.preferences.Overlay[0] = Math.Min(1, this.preferences.Overlay[0] + delta);
                    }
                    else if (e.Delta < 0)
                    {
                        this.preferences.Overlay[0] = Math.Max(0.4, this.preferences.Overlay[0] - delta);
                    }

                    this.Overlay.LayoutTransform = new ScaleTransform(this.preferences.Overlay[0], this.preferences.Overlay[0], this.Overlay.Left + (this.Overlay.Width / 2), this.Overlay.Top + (this.Overlay.Height / 2));
                    this.Overlay.Height = this.Height * this.preferences.Overlay[0];
                    this.Overlay.Width = this.Width * this.preferences.Overlay[0];
                };

                this.Overlay.PreviewMouseLeftButtonDown += (sender, e) =>
                {
                    if (this.preferences.IsOverlayFixed.Value)
                    {
                        return;
                    }
                    this.Overlay.DragMove();
                };
                this.Overlay.PreviewMouseLeftButtonUp += (sender, e) =>
                {
                    if (this.preferences.IsOverlayFixed.Value)
                    {
                        return;
                    }
                    this.preferences.Overlay[1] = this.currentRect.Left - this.Overlay.Left;
                    this.preferences.Overlay[2] = this.currentRect.Top - this.Overlay.Top;
                    this.preferences.Overlay[3] = this.Overlay.Left;
                    this.preferences.Overlay[4] = this.Overlay.Top;
                };
                this.Overlay.Closing += (sender, e) =>
                {
                    e.Cancel = true;
                    this.Overlay.Hide();
                };

                this.Overlay.Content = this.OverlayDisplay;
                this.Overlay.ContextMenu = new ContextMenu();
                MenuItem OverlayFixMenu = new MenuItem()
                {
                    Header = "Fixed",
                    IsCheckable = true,
                };
                OverlayFixMenu.Click += (sender, e) =>
                {
                    this.preferences.IsOverlayFixed = !this.preferences.IsOverlayFixed;
                    OverlayFixMenu.IsChecked = this.preferences.IsOverlayFixed.Value;
                };
                this.Overlay.ContextMenu.Items.Add(OverlayFixMenu);

                if (this.preferences.IsOverlayFixed == null)
                {
                    this.preferences.IsOverlayFixed = false;
                }
                OverlayFixMenu.IsChecked = this.preferences.IsOverlayFixed.Value;

                MenuItem OverlayResetMenu = new MenuItem()
                {
                    Header = new TextBlock()
                    {
                        Text = "Reset",
                        TextDecorations = TextDecorations.Underline,                        
                    },
                    ToolTip = "You must do 'Shift + Click'!; 쉬프트키를 누르고 클릭하셔야 합니다!",
                };
                OverlayResetMenu.Click += (sender, e) =>
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        this.Reset();
                    }                    
                };
                this.Overlay.ContextMenu.Items.Add(OverlayResetMenu);

                MenuItem OverlayCloseMenu = new MenuItem()
                {
                    Header = "Close",
                };
                OverlayCloseMenu.Click += (sender, e) =>
                {
                    this.Overlay.Hide();
                };
                this.Overlay.ContextMenu.Items.Add(OverlayCloseMenu);

                if (this.preferences.DisplayMode == null)
                {
                    this.preferences.DisplayMode = DisplayModes.Game_and_Star_plus;
                }

                this._mode = (this.preferences.DisplayMode == DisplayModes.Game_and_Star_plus) ? (DisplayModes.Game_and_Star) : (DisplayModes.Game_and_Star_plus);
                this.Mode = this.preferences.DisplayMode.Value;
            }
        }

        private void InitializeTimer()
        {
            if (this.preferences.Period == null)
            {
                this.preferences.Period = 333;
            }
            else
            {
                this.preferences.Period = Math.Min(50, this.preferences.Period.Value); // at least 50 ms
            }

            this.timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, this.preferences.Period.Value),
            };
            this.timer.Tick += (e, sender) =>
            {
                this.CountingStars();
            };                       
        }

        private void InitializeVariables()
        {
            if (this.IsGoogleOn)
            {
                using (System.Net.WebClient web = new System.Net.WebClient() { Encoding = Encoding.UTF8 })
                {
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                    string rawString = web.DownloadString("https://github.com/bayoen/bayoen-star-exe/releases/latest");
                    string latestVersion = System.Text.RegularExpressions.Regex.Match(rawString, @"<a class=""js-selected-navigation-item selected reponav-item""((.|\n)*?)>", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups[1].Value.Split('/').Last();
                    this.latestVersion = Version.Parse(latestVersion.Remove(latestVersion.Length - 1));
                }
            }
            
            this.pptMemory = new VAMemory(pptName);

            this.oldStar = new List<int>() { -1, -1 };
            this.currentStar = new List<int>() { -1, -1 };


            if (File.Exists(dataJSONName))
            {
                Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(dataJSONName, Encoding.Unicode));
                Newtonsoft.Json.Linq.JToken star1Token = json.SelectToken("StarPlus1");
                Newtonsoft.Json.Linq.JToken star2Token = json.SelectToken("StarPlus2");
                Newtonsoft.Json.Linq.JToken crown1Token = json.SelectToken("Crown1");
                Newtonsoft.Json.Linq.JToken crown2Token = json.SelectToken("Crown2");
                Newtonsoft.Json.Linq.JToken winCountToken = json.SelectToken("WinCount");
                Newtonsoft.Json.Linq.JToken winMatchToken = json.SelectToken("WinMatch");

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
                this.winMatch = -1;
            }

            this.GoalType = this.preferences.GoalType.Value;
            this.GoalScore = this.preferences.GoalScore.Value;
        }

        private void CountingStars()
        {
            if (IsPPTOn)
            {
                if (this.preferences.IsHideOffline.Value)
                {
                    if (this.MainDisplay.Visibility == Visibility.Collapsed) this.MainDisplay.Visibility = Visibility.Visible;
                    if (this.OverlayDisplay.Visibility == Visibility.Hidden) this.OverlayDisplay.Visibility = Visibility.Visible;
                }                
                this.scoreAddress = this.pptMemory.ReadInt32(new IntPtr(0x14057F048)) + 0x38;

                this.CheckOverlay();                
            }
            else
            {
                if (this.preferences.IsHideOffline.Value)
                {
                    if (this.MainDisplay.Visibility == Visibility.Visible) this.MainDisplay.Visibility = Visibility.Collapsed;
                    if (this.OverlayDisplay.Visibility == Visibility.Visible) this.OverlayDisplay.Visibility = Visibility.Hidden;
                }                    
                this.Status("Offline");
                return;
            }

            if (IsInPlay)
            {
                for (int playerIndex = 0; playerIndex < 2; playerIndex++)
                {
                    this.currentStar[playerIndex] = this.pptMemory.ReadInt32(new IntPtr(scoreAddress) + playerIndex * 0x4);
                }
                this.winCount = this.pptMemory.ReadInt32(new IntPtr(scoreAddress) + 0x10);
            }
            else
            {
                for (int playerIndex = 0; playerIndex < 2; playerIndex++)
                {
                    this.currentStar[playerIndex] = -1;
                    this.oldStar[playerIndex] = -1;
                }
                this.winCount = -1;

                this.CheckContainers();
                this.Save();

                this.Status("Ready");
                return;
            }

            if (IsGoalOver)
            {
                this.Status("Goal!");
                return;
            }

            List<int> gradients = this.currentStar.Zip(this.oldStar, (a, b) => a - b).ToList();
            if (gradients.IndexOf(1) > -1)
            {
                for (int playerIndex = 0; playerIndex < 2; playerIndex++)
                {
                    if (this.oldStar[playerIndex] < 0) break;

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

                        this.Export();
                    }
                    this.Save();
                }
            }
            
            this.CheckContainers();
            this.Status("Working");

            for (int playerIndex = 0; playerIndex < 2; playerIndex++)
            {
                this.oldStar[playerIndex] = this.currentStar[playerIndex];
            }
        }
        
        private void Export()
        {
            File.WriteAllText(exportFolderName + '\\' + "Star1.txt", this.currentStar[0].ToString(), Encoding.UTF8);
            File.WriteAllText(exportFolderName + '\\' + "Star2.txt", this.currentStar[1].ToString(), Encoding.UTF8);
            File.WriteAllText(exportFolderName + '\\' + "StarPlus1.txt", this.countingStar[0].ToString(), Encoding.UTF8);
            File.WriteAllText(exportFolderName + '\\' + "StarPlus2.txt", this.countingStar[1].ToString(), Encoding.UTF8);
            File.WriteAllText(exportFolderName + '\\' + "Crown1.txt", this.countingCrown[0].ToString(), Encoding.UTF8);
            File.WriteAllText(exportFolderName + '\\' + "Crown2.txt", this.countingCrown[1].ToString(), Encoding.UTF8);
            File.WriteAllText(exportFolderName + '\\' + "WinCount1.txt", this.winCount.ToString(), Encoding.UTF8);
        }

        private void Save()
        {
            Newtonsoft.Json.Linq.JObject json = new Newtonsoft.Json.Linq.JObject()
            {
                ["Star1"] = this.currentStar[0],
                ["Star2"] = this.currentStar[1],
                ["StarPlus1"] = this.countingStar[0],
                ["StarPlus2"] = this.countingStar[1],
                ["Crown1"] = this.countingCrown[0],
                ["Crown2"] = this.countingCrown[1],
                ["WinCount"] = this.winCount,
                ["WinMatch"] = this.winMatch,
            };
            File.WriteAllText(dataJSONName, json.ToString(), Encoding.UTF8);
        }

        private void CheckContainers()
        {
            this.MainDisplay.Set(this.currentStar, this.countingStar, this.countingCrown, this.GoalType, this.GoalScore);
            this.OverlayDisplay.Set(this.currentStar, this.countingStar, this.countingCrown, this.GoalType, this.GoalScore);
        }

        private void ToMonitors()
        {
            this.Monitors[0].Text = SetText(this.currentStar[0]);
            this.Monitors[1].Text = SetText(this.currentStar[1]);
            this.Monitors[2].Text = SetText(this.countingStar[0]);
            this.Monitors[3].Text = SetText(this.countingStar[1]);
            this.Monitors[4].Text = SetText(this.countingCrown[0]);
            this.Monitors[5].Text = SetText(this.countingCrown[1]);
            this.Monitors[6].Text = SetText(this.winCount);

            string SetText(int number)
            {
                return (number < 0) ? ("-") : (number.ToString());
            }
        }

        private void FromMonitors()
        {
            this.countingStar[0] = int.Parse(this.Monitors[2].Text);
            this.countingStar[1] = int.Parse(this.Monitors[3].Text);
            this.countingCrown[0] = int.Parse(this.Monitors[4].Text);
            this.countingCrown[1] = int.Parse(this.Monitors[5].Text);
        }

        private void CheckOverlay()
        {
            GetWindowRect(this.PPTProcesses.Single().MainWindowHandle, ref this.currentRect);

            this.FixOverlay();

            this.oldRect = new RECT(this.currentRect);
        }

        private void FixOverlay()
        {
            if (RECT.Equals(this.currentRect, this.oldRect))
            {
                return;
            }

            if (!this.preferences.IsOverlayFixed.Value)
            {
                return;
            }

            if (this.Overlay.WindowState != WindowState.Normal)
            {
                return;
            }

            this.Overlay.Left = this.currentRect.Left - this.preferences.Overlay[1];
            this.Overlay.Top = this.currentRect.Top - this.preferences.Overlay[2];
        }

        private void Reset()
        {
            this.countingStar = new List<int>() { 0, 0 };
            this.countingCrown = new List<int>() { 0, 0 };

            this.CheckContainers();
            this.ToMonitors();
            this.Save();
            System.Media.SystemSounds.Hand.Play();
        }

        private void CheckUpdate()
        {
            if (!IsGoogleOn) return;

            if (currentVersion == latestVersion)
            {
                // Latest: do nothing
            }
            else if (currentVersion < latestVersion)
            {
                // Old
                this.Notify.BalloonTipClicked += Notify_BalloonTipClicked_GoDownloadPage;
                this.Notify.ShowBalloonTip(2000, "This is old version!", "Click to go download page!\nand please remove this version", wf::ToolTipIcon.None);
            }
            else // if (version > LatestVersion)
            {
                // in Dev.: do nothing
                this.Notify.ShowBalloonTip(2000, "This is version in development!", "There's no problem, right?", wf::ToolTipIcon.None);
            }
        }

        private void Notify_BalloonTipClicked_GoDownloadPage(object sender, EventArgs e)
        {
            Process.Start("https://github.com/bayoen/bayoen-star-exe/releases/latest");
            this.Notify.BalloonTipClicked -= Notify_BalloonTipClicked_GoDownloadPage;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            this.TopContextMenu.IsOpen = true;
        }

        private async void ResetMenuItem_ClickAsync(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync("Do clear?", "", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                this.Reset();
            }
        }

        private void OverlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.ViewOverlay();
        }

        private void ViewOverlay()
        {
            this.Overlay.Show();
            if (this.Overlay.WindowState == WindowState.Minimized)
            {
                this.Overlay.WindowState = WindowState.Normal;
            }
            this.Overlay.Activate();
        }

        private void SettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Point mousePoint = System.Windows.Forms.Control.MousePosition;

            this.Setting.Show();
            this.Setting.Left = Math.Max(0, mousePoint.X - 50);
            this.Setting.Top = Math.Max(0, mousePoint.Y - 50);
            if (this.Setting.WindowState == WindowState.Minimized)
            {
                this.Setting.WindowState = WindowState.Normal;
            }
            this.Setting.Activate();
        }

        private void TopContextMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.O)
            {
                this.ViewOverlay();
                this.TopContextMenu.IsOpen = false;
            }
            else if (e.Key == Key.R)
            {
                this.Reset();
                this.TopContextMenu.IsOpen = false;
            }
        }

        private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.M)
            {
                this.TopContextMenu.IsOpen = true;
            }
        }

        private void MetroWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
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
                this.Notify.BalloonTipClicked -= Notify_BalloonTipClicked_GoDownloadPage;
                this.Notify.ShowBalloonTip(2000, "Closing → Minimizing", "Minimized into system tray\nPlease right-click icon!", wf::ToolTipIcon.None);
                preferences.EverClosed = true;
            }
        }

        public struct RECT
        {
            /// <summary>
            /// x position of upper-left corner
            /// </summary>
            public int Left;

            /// <summary>
            /// y position of upper-left corner
            /// </summary>
            public int Top;

            /// <summary>
            /// x position of lower-right corner
            /// </summary>
            public int Right;

            /// <summary>
            /// y position of lower-right corner
            /// </summary>
            public int Bottom;

            public int Width
            {
                get => this.Right - this.Left;
            }
            public int Height
            {
                get => this.Bottom - this.Top;
            }

            public RECT(RECT rect)
            {
                this.Left = rect.Left;
                this.Top = rect.Top;
                this.Right = rect.Right;
                this.Bottom = rect.Bottom;
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref RECT rectangle);

        private void Status(string s)
        {
            this.StatusTextBlock.Text = s + versionText;
        }

        public enum DisplayModes : int
        {
            Star_plus_only = 0,
            Game_only = 1,
            Game_and_Star = 2,
            Game_and_Star_plus = 3,
            Star_plus_and_Star = 4,
        }
        public enum ChromaKeys : int
        {
            None = 0,
            Green = 1,
            Blue = 2,
            Magenta = 3,
        }
        public enum GoalTypes : int
        {
            None = 0,
            Star = 1,
            Crown = 2,
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
