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
        /// bayoen-star 0.4
        /// </summary>
        public MainWindow()
        {
            this.InitializePreferences();
            this.InitializeComponent();
            this.InitializeLayouts();
            this.InitializeTimer(333);
            this.InitializeVariables();
            this.Status("Ready");
            Thread.Sleep(1000);

            this.CheckContainers();
            this.ToMonitors();
            this.IsStatOn = true;
        }

        public DisplayGrid MainDisplay;
        public DisplayGrid OverlayDisplay;
        public List<TextBox> Monitors;

        public const string versionText = " - Beta v0.0.7";
        public const string pptName = "puyopuyotetris";
        public const string prefName = "pref.json";
        public const string exportFolderName = "export";
        public const string dataJSONName = "data.json";
        public Preferences preferences;
        public wf::NotifyIcon Notify;
        public MetroWindow Setting;
        public MetroWindow Overlay;


        public VAMemory pptMemory;
        public RECT pptRect, oldRect;
        public int scoreAddress;
        public Process[] PPTProcesses;
        public DispatcherTimer timer;

        public int winCount;
        public List<int> currentStar;
        public List<int> oldStar;
        public List<int> countingStar;
        public List<int> countingCrown;       

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
            InitializeTopMenu();
            InitializeSettingWindow();
            InitializeNotifyIcon();
            InitializeDisplay();

            void InitializeTopMenu()
            {
                MenuItem ResetMenuItem = BuildMenu("Reset", "appbar_new");
                ResetMenuItem.Click += ResetMenuItem_ClickAsync;
                this.TopCompositeCollection.Add(ResetMenuItem);

                MenuItem OverlayMenuItem = BuildMenu("Show Overlay", "appbar_app_plus");
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
                    Title = "Settings",
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
                MainGroupPanel.Children.Add(ChromaKeyCheckBox);

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

                        + Environment.NewLine + "and also thank you PPT KOR community!" + Environment.NewLine
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
                this.MainDisplay = new DisplayGrid();
                this.Content = this.MainDisplay;

                if (IsPPTOn)
                {
                    GetWindowRect(this.PPTProcesses.Single().MainWindowHandle, ref this.pptRect);
                }
                else
                {
                    this.pptRect = new RECT()
                    {
                        Left = -1,
                        Top = -1,
                        Right = -1,
                        Bottom = -1,
                    };
                }

                this.pptRect = new RECT(this.oldRect);

                this.OverlayDisplay = new DisplayGrid();
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
                        this.Overlay.Left = this.pptRect.Left - this.preferences.Overlay[1];
                        this.Overlay.Top = this.pptRect.Top - this.preferences.Overlay[2];
                    }
                    else
                    {
                        this.Overlay.Left = this.preferences.Overlay[3];
                        this.Overlay.Top = this.preferences.Overlay[4];
                    }
                    
                }

                double delta = 0.05;
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
                    this.preferences.Overlay[1] = this.pptRect.Left - this.Overlay.Left;
                    this.preferences.Overlay[2] = this.pptRect.Top - this.Overlay.Top;
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
                if (this.MainDisplay.Visibility == Visibility.Collapsed) this.MainDisplay.Visibility = Visibility.Visible;
                if (this.OverlayDisplay.Visibility == Visibility.Hidden) this.OverlayDisplay.Visibility = Visibility.Visible;
                this.scoreAddress = this.pptMemory.ReadInt32(new IntPtr(0x14057F048)) + 0x38;

                this.CheckOverlay();                
            }
            else
            {
                if (this.MainDisplay.Visibility == Visibility.Visible) this.MainDisplay.Visibility = Visibility.Collapsed;
                if (this.OverlayDisplay.Visibility == Visibility.Visible) this.OverlayDisplay.Visibility = Visibility.Hidden;
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
                this.Status("Ready");
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

                        this.Export();
                    }

                    this.ToMonitors();
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
            File.WriteAllText(exportFolderName + '\\' + "Star1.txt", this.currentStar[0].ToString(), Encoding.Unicode);
            File.WriteAllText(exportFolderName + '\\' + "Star2.txt", this.currentStar[1].ToString(), Encoding.Unicode);
            File.WriteAllText(exportFolderName + '\\' + "StarPlus1.txt", this.countingStar[0].ToString(), Encoding.Unicode);
            File.WriteAllText(exportFolderName + '\\' + "StarPlus2.txt", this.countingStar[1].ToString(), Encoding.Unicode);
            File.WriteAllText(exportFolderName + '\\' + "Crown1.txt", this.countingCrown[0].ToString(), Encoding.Unicode);
            File.WriteAllText(exportFolderName + '\\' + "Crown2.txt", this.countingCrown[1].ToString(), Encoding.Unicode);
            File.WriteAllText(exportFolderName + '\\' + "WinCount1.txt", this.winCount.ToString(), Encoding.Unicode);
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
            };
            File.WriteAllText(dataJSONName, json.ToString(), Encoding.Unicode);
        }

        private void CheckContainers()
        {
            if (this.Mode == DisplayModes.Game_and_Star_plus)
            {
                this.MainDisplay.Set(this.countingCrown[0], this.countingStar[0], this.countingStar[1], this.countingCrown[1]);
                this.OverlayDisplay.Set(this.countingCrown[0], this.countingStar[0], this.countingStar[1], this.countingCrown[1]);
            }
            else if (this.Mode == DisplayModes.Game_and_Star)
            {
                this.MainDisplay.Set(this.countingCrown[0], this.currentStar[0], this.currentStar[1], this.countingCrown[1]);
                this.OverlayDisplay.Set(this.countingCrown[0], this.currentStar[0], this.currentStar[1], this.countingCrown[1]);
            }
            else if (this.Mode == DisplayModes.Game_only)
            {
                this.MainDisplay.Set(this.countingCrown[0], this.countingCrown[1]);
                this.OverlayDisplay.Set(this.countingCrown[0], this.countingCrown[1]);
            }
            else if (this.Mode == DisplayModes.Star_plus_only)
            {
                this.MainDisplay.Set(this.countingStar[0], this.countingStar[1]);
                this.OverlayDisplay.Set(this.countingStar[0], this.countingStar[1]);
            }
            else //if (this.Mode == DisplayModes.Star_plus_and_Star)
            {
                this.MainDisplay.Set(this.countingStar[0], this.currentStar[0], this.currentStar[1], this.countingStar[1]);
                this.OverlayDisplay.Set(this.countingStar[0], this.currentStar[0], this.currentStar[1], this.countingStar[1]);
            }
        }

        private void ToMonitors()
        {
            this.Monitors[0].Text = this.currentStar[0].ToString();
            this.Monitors[1].Text = this.currentStar[1].ToString();
            this.Monitors[2].Text = this.countingStar[0].ToString();
            this.Monitors[3].Text = this.countingStar[1].ToString();
            this.Monitors[4].Text = this.countingCrown[0].ToString();
            this.Monitors[5].Text = this.countingCrown[1].ToString();
            this.Monitors[6].Text = this.winCount.ToString();
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
            GetWindowRect(this.PPTProcesses.Single().MainWindowHandle, ref this.pptRect);

            this.FixOverlay();

            this.oldRect = new RECT(this.pptRect);
        }

        private void FixOverlay()
        {
            if (RECT.Equals(this.pptRect, this.oldRect))
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

            this.Overlay.Left = this.pptRect.Left - this.preferences.Overlay[1];
            this.Overlay.Top = this.pptRect.Top - this.preferences.Overlay[2];
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
                this.countingStar = new List<int>() { 0, 0 };
                this.countingCrown = new List<int>() { 0, 0 };

                this.CheckContainers();
                this.ToMonitors();
                this.Save();
                System.Media.SystemSounds.Hand.Play();
            }
        }

        private void OverlayMenuItem_Click(object sender, RoutedEventArgs e)
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
