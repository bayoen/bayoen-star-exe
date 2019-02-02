using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace bayoen.material
{
    public class BayoenMessage : StackPanel
    {
        public BayoenMessage()
        {
            this.Initialize();
        }

        public BayoenMessage(System.Drawing.Bitmap bitmap, string header, string message) : this(bitmap, header, message, DateTime.Now) { }

        public BayoenMessage(System.Drawing.Bitmap bitmap, string header, string message, DateTime datetime)
        {
            this.Initialize();

            this.HeaderBitmap = bitmap;
            if (header != null) this.HeaderText = header;
            if (message != null) this.MessageText = message;
            this.Time = datetime;

            if (this.Time.DayOfYear < DateTime.Now.DayOfYear)
            {
                this.TimeTextBlock.Text = this.Time.ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("en-US"));
            }
            else
            {
                this.TimeTextBlock.Text = this.Time.ToString("tt hh:mm:ss", CultureInfo.CreateSpecificCulture("en-US"));
            }
        }

        public void Initialize()
        {
            this.Orientation = Orientation.Horizontal;
            this.Margin = new Thickness(5);

            this.HeaderImage = new Image()
            {
                Height = 100,
                Width = 100,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Top,
            };
            RenderOptions.SetBitmapScalingMode(this.HeaderImage, BitmapScalingMode.Fant);
            this.Children.Add(this.HeaderImage);

            this.MessageStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
            };
            this.Children.Add(this.MessageStackPanel);

            this.HeaderTextBlock = new TextBlock()
            {
                FontSize = 14,
                Margin = new Thickness(20, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            this.MessageStackPanel.Children.Add(this.HeaderTextBlock);

            this.MessageGrid = new Grid()
            {
                Margin = new Thickness(5),
            };
            this.MessageStackPanel.Children.Add(this.MessageGrid);

            this.MessageTextBlock = new TextBlock()
            {
                FontSize = 12,
                Margin = new Thickness(10),
                MaxWidth = 200,
                TextWrapping = TextWrapping.Wrap,
            };
            this.MessageColl = new ObservableCollection<string>();
            this.MessageColl.CollectionChanged += MessageColl_CollectionChanged;

            this.MessageBorder = new Border()
            {
                Child = this.MessageTextBlock,
                CornerRadius = new CornerRadius(0, 10, 10, 10),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Margin = new Thickness(15, 0, 0, 0),
            };
            this.MessageGrid.Children.Add(this.MessageBorder);

            this.ModifyingPolyline = new Polyline()
            {
                Points = new PointCollection()
                    {
                        new Point(15,0),
                        new Point(15,15),
                    },
                Stroke = new SolidColorBrush(Color.FromRgb(37, 37, 37)),
                StrokeThickness = 2,
                Margin = new Thickness(1, 1, 0, 0),
            };
            this.MessageGrid.Children.Add(this.ModifyingPolyline);

            this.TimeTextBlock = new TextBlock()
            {
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 7),
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            this.Children.Add(this.TimeTextBlock);

            this.TailPolyline = new Polyline()
            {
                Points = new PointCollection()
                    {
                        new Point(16,0),
                        new Point(0,0),
                        new Point(15,15),

                    },
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Margin = new Thickness(1, 1, 0, 0),
            };
            this.MessageGrid.Children.Add(this.TailPolyline);

            this._messageDirection = BayoenMessageDirection.Null;
            this.MessageDirection = BayoenMessageDirection.Left;
        }

        public Image HeaderImage;

        public StackPanel MessageStackPanel;
        public TextBlock HeaderTextBlock;

        public Grid MessageGrid;
        public TextBlock MessageTextBlock;
        public Border MessageBorder;

        public DateTime Time;
        public TextBlock TimeTextBlock;

        public Polyline TailPolyline;
        public Polyline ModifyingPolyline;

        public ObservableCollection<string> MessageColl;

        private BayoenMessageDirection _messageDirection;
        public BayoenMessageDirection MessageDirection
        {
            get => this._messageDirection;
            set
            {
                if (value == BayoenMessageDirection.Null)
                {
                    return;
                }
                else if (value == this._messageDirection)
                {
                    return;
                }

                if (value == BayoenMessageDirection.Left)
                {
                    this.LayoutTransform = new ScaleTransform(1, 1);
                    this.HeaderTextBlock.LayoutTransform = new ScaleTransform(1, 1);
                    this.MessageTextBlock.LayoutTransform = new ScaleTransform(1, 1);
                    this.TimeTextBlock.LayoutTransform = new ScaleTransform(1, 1);
                }
                else
                {
                    this.LayoutTransform = new ScaleTransform(-1, 1);
                    this.HeaderTextBlock.LayoutTransform = new ScaleTransform(-1, 1);
                    this.MessageTextBlock.LayoutTransform = new ScaleTransform(-1, 1);
                    this.TimeTextBlock.LayoutTransform = new ScaleTransform(-1, 1);
                }

                this._messageDirection = value;
            }
        }

        private System.Drawing.Bitmap _headerBitmap;
        public System.Drawing.Bitmap HeaderBitmap
        {
            get => this._headerBitmap;
            set
            {                
                using (System.IO.MemoryStream streamToken = new System.IO.MemoryStream())
                {
                    value.Save(streamToken, System.Drawing.Imaging.ImageFormat.Png);
                    streamToken.Position = 0;

                    BitmapImage bitmapImageToken = new BitmapImage();
                    bitmapImageToken.BeginInit();
                    bitmapImageToken.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImageToken.StreamSource = streamToken;
                    bitmapImageToken.EndInit();

                    this.HeaderImage.Source = bitmapImageToken;
                    bitmapImageToken.Freeze();
                }

                this._headerBitmap = value;
            }
        }

        private double _headerImageHeight;
        public double HeaderImageHeight
        {
            get => this._headerImageHeight;
            set
            {
                this.HeaderImage.Height = value;
                this._headerImageHeight = value;
            }
        }

        private double _headerImageWidth;
        public double HeaderImageWidth
        {
            get => this._headerImageWidth;
            set
            {
                this.HeaderImage.Width = value;
                this._headerImageWidth = value;
            }
        }

        private string _headerText;
        public string HeaderText
        {
            get => this._headerText;
            set
            {
                this.HeaderTextBlock.Text = value;
                this._headerText = value;
            }
        }

        private string _messageText;
        public string MessageText
        {
            get => this._messageText;
            set
            {
                this.MessageTextBlock.Text = value;
                this._messageText = value;
            }
        }

        private void MessageColl_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {            
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (this.MessageColl.Count <= 1)
                {
                    this.MessageTextBlock.Text += e.NewItems[0];
                }
                else
                {
                    this.MessageTextBlock.Text += Environment.NewLine + e.NewItems[0];
                }
                
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                this.MessageTextBlock.Text = string.Format(Environment.NewLine, this.MessageColl);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                this.MessageTextBlock.Text = string.Format(Environment.NewLine, this.MessageColl);
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                this.MessageTextBlock.Text = string.Format(Environment.NewLine, this.MessageColl);
            }
            else //if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.MessageTextBlock.Text = "";
            }
        }

        public void Add(string s)
        {
            this.MessageColl.Add(s);
        }

        //public Newtonsoft.Json.Linq.JObject ToJSON()
        //{
        //    Newtonsoft.Json.Linq.JObject json = new Newtonsoft.Json.Linq.JObject();



        //    return json;
        //}

        public static BayoenMessage FromJSON()
        {
            BayoenMessage bm = new BayoenMessage();



            return bm;
        }
    }

    public enum BayoenMessageDirection
    {
        Null,
        Left,
        Right,
    }
}
