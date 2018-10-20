using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using js = Newtonsoft.Json;
using jl = Newtonsoft.Json.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace bayoen
{
    public partial class MainWindow
    {
        public class Container : StackPanel
        {
            public Container()
            {
                this.Orientation = Orientation.Horizontal;

                this.ScoreBorder = new Border()
                {
                    Width = 50,
                    Height = 35,
                };
                this.Children.Add(this.ScoreBorder);

                this.ScoreText = new TextBlock()
                {
                    FontFamily = new System.Windows.Media.FontFamily("Arial"),
                    Width = 50,                    
                    FontWeight = FontWeights.ExtraBold,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0,3,0,0),
                };
                this.ScoreBorder.Child = this.ScoreText;


                this.SymbolImage = new System.Windows.Controls.Image()
                {
                    Height = 36,
                    Width = 36,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                this.Children.Add(this.SymbolImage);

                this._containerOrientation = ContainerOrientations.Right;
                this.ContainerOrientation = ContainerOrientations.Left;

                this._containerImage = ContainerImages.StarPlus;
                this.ContainerImage = ContainerImages.StarPlain;

                this._containerState = ContainerStates.Broken;
                this.ContainerState = ContainerStates.Idle;

                this.IsValid = true;
                this.Score = 0;
            }
            
            public TextBlock ScoreText;
            public Border ScoreBorder;
            public System.Windows.Controls.Image SymbolImage;

            private int _score;
            public int Score
            {
                get => this._score;
                set
                {
                    if (!this.IsValid) return;

                    // FontSize adjustment
                    if (value >= 1000)
                    {
                        this.ScoreText.FontSize = 15;
                    }
                    else if (value >= 100)
                    {
                        this.ScoreText.FontSize = 20;
                    }
                    else
                    {
                        this.ScoreText.FontSize = 30;
                    }

                    // Display value adjustment
                    if (value >= 9999)
                    {
                        this.ScoreText.Text = "9999";
                        this._score = 9999;
                    }
                    else if (value > -1)
                    {
                        this.ScoreText.Text = value.ToString();
                        this._score = value;
                    }
                    else
                    {
                        this.ScoreText.Text = "-";
                        this._score = -1;
                    }                    
                }
            }

            private ContainerOrientations _containerOrientation;
            public ContainerOrientations ContainerOrientation
            {
                get => this._containerOrientation;
                set
                {
                    if (value == this._containerOrientation)
                    {
                        return;
                    }

                    this.Children.Remove(this.SymbolImage);

                    if (value == ContainerOrientations.Left)
                    {
                        this.Children.Insert(0, this.SymbolImage);
                    }
                    else
                    {
                        this.Children.Insert(1, this.SymbolImage);
                    }


                    this._containerOrientation = value;
                }
            }

            private ContainerImages _containerImage;
            public ContainerImages ContainerImage
            {
                get => this._containerImage;
                set
                {
                    if (value == this._containerImage)
                    {
                        return;
                    }

                    Bitmap bitmapToken;
                    if (value == ContainerImages.StarPlain)
                    {
                        bitmapToken = bayoen.Properties.Resources.StarPlain;
                    }
                    else if (value == ContainerImages.StarPlus)
                    {
                        bitmapToken = bayoen.Properties.Resources.StarPlus;
                    }
                    else //if (value == ContainerImages.CrownLight)
                    {
                        bitmapToken = bayoen.Properties.Resources.CrownLight;
                    }
                    this.SymbolImage.SetBitmap(bitmapToken);

                    this._containerImage = value;
                }
            }

            private ContainerStates _containerState;
            public ContainerStates ContainerState
            {
                get => this._containerState;
                set
                {
                    //if (value == this._containerState)
                    //{
                    //    return;
                    //}

                    if (value == ContainerStates.Idle)
                    {
                        this.ScoreText.Foreground = System.Windows.Media.Brushes.White;
                    }
                    else if (value == ContainerStates.Goal)
                    {
                        this.ScoreText.Foreground = System.Windows.Media.Brushes.Gold;
                    }
                    else // if (value == ContainerStates.Broken)
                    {
                        this.ScoreText.Foreground = System.Windows.Media.Brushes.Crimson;
                    }

                    this._containerState = value;
                }
            }

            private bool _isValid;
            public bool IsValid
            {
                get => this._isValid;
                set
                {
                    if (value)
                    {
                        this.ScoreText.Text = this.Score.ToString();
                    }
                    else
                    {
                        this.ScoreText.Text = "-";
                    }

                    this._isValid = value;
                }
            }

            public void Clear()
            {
                this.Score = -1;
            }

            public void Set()
            {

            }
        }

        public enum ContainerOrientations
        {
            Left,
            Right,
        }

        public enum ContainerImages
        {
            StarPlain,
            StarPlus,
            CrownLight,
        }

        public enum ContainerStates : int
        {
            Idle = 0,
            Goal = 1,
            Broken = 2,
        }
    }

}



