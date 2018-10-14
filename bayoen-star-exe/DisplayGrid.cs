using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;

namespace bayoen
{
    public partial class MainWindow
    {
        public class DisplayGrid : Grid
        {
            public DisplayGrid()
            {
                this.PanelImage = new Image()
                {
                    Height = 68,
                    Width = 406,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                this.Children.Add(this.PanelImage);

                this.DashBlock = new TextBlock()
                {
                    Text = ":",
                    FontSize = 30,
                    Margin = new Thickness(0, 0, 0, 8),
                    FontWeight = FontWeights.ExtraBold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                this.Children.Add(this.DashBlock);

                this.ContainerPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                this.Children.Add(this.ContainerPanel);

                this.Inners = new List<Container>()
                {
                    new Container() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, },
                    new Container() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, },
                };

                this.Outers = new List<Container>()
                {
                    new Container() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, },
                    new Container() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, },
                };
                this.ContainerPanel.Children.Add(this.Outers[0]);
                this.ContainerPanel.Children.Add(this.Inners[0]);
                this.ContainerPanel.Children.Add(this.Inners[1]);
                this.ContainerPanel.Children.Add(this.Outers[1]);
            }
           
            public Image PanelImage;
            public TextBlock DashBlock;
            public StackPanel ContainerPanel;

            public List<Container> Inners;
            public List<Container> Outers;

            private bool fitFlag;
            private DisplayModes _mode;
            private DisplayModes Mode
            {
                get => this._mode;
                set
                {
                    if (value == this._mode)
                    {
                        return;
                    }

                    this.Inners.ForEach(x => this.ContainerPanel.Children.Remove(x));
                    this.Outers.ForEach(x => this.ContainerPanel.Children.Remove(x));

                    if (this._mode >= DisplayModes.Game_and_Star)
                    {
                        if (value < DisplayModes.Game_and_Star)
                        {
                            if (fitFlag)
                            {
                                PanelImage.SetBitmap(bayoen.Properties.Resources.PanelScore2FitStrong);
                            }
                            else
                            {
                                PanelImage.SetBitmap(bayoen.Properties.Resources.PanelScore2ShortStrong);
                            }                            
                        }
                    }
                    else
                    {
                        if (value >= DisplayModes.Game_and_Star)
                        {
                            PanelImage.SetBitmap(bayoen.Properties.Resources.PanelScore2LongStrong);
                        }
                    }

                    if (value == DisplayModes.Star_plus_only)
                    {
                        if (fitFlag)
                        {
                            this.Inners[0].Margin = new Thickness(0, 0, 30, 0);
                            this.Inners[1].Margin = new Thickness(30, 0, 0, 0);
                        }
                        else
                        {
                            this.Inners[0].Margin = new Thickness(0, 0, 5, 0);
                            this.Inners[1].Margin = new Thickness(5, 0, 0, 0);
                        }

                        this.ContainerPanel.Children.Add(this.Inners[0]);
                        this.Inners[0].ContainerOrientation = ContainerOrientations.Left;
                        this.Inners[0].ContainerImage = ContainerImages.StarPlus;

                        this.ContainerPanel.Children.Add(this.Inners[1]);
                        this.Inners[1].ContainerOrientation = ContainerOrientations.Right;
                        this.Inners[1].ContainerImage = ContainerImages.StarPlus;
                    }
                    else if (value == DisplayModes.Game_only)
                    {
                        if (fitFlag)
                        {
                            this.Inners[0].Margin = new Thickness(0, 0, 30, 0);
                            this.Inners[1].Margin = new Thickness(30, 0, 0, 0);
                        }
                        else
                        {
                            this.Inners[0].Margin = new Thickness(0, 0, 5, 0);
                            this.Inners[1].Margin = new Thickness(5, 0, 0, 0);
                        }

                        this.ContainerPanel.Children.Add(this.Inners[0]);
                        this.Inners[0].ContainerOrientation = ContainerOrientations.Left;
                        this.Inners[0].ContainerImage = ContainerImages.CrownLight;

                        this.ContainerPanel.Children.Add(this.Inners[1]);
                        this.Inners[1].ContainerOrientation = ContainerOrientations.Right;
                        this.Inners[1].ContainerImage = ContainerImages.CrownLight;
                    }
                    else if (value == DisplayModes.Game_and_Star)
                    {
                        this.ContainerPanel.Children.Add(this.Outers[0]);
                        this.Outers[0].Margin = new Thickness(0);
                        this.Outers[0].ContainerOrientation = ContainerOrientations.Left;
                        this.Outers[0].ContainerImage = ContainerImages.CrownLight;

                        this.ContainerPanel.Children.Add(this.Inners[0]);
                        this.Inners[0].Margin = new Thickness(0, 0, 5, 0);
                        this.Inners[0].ContainerOrientation = ContainerOrientations.Left;
                        this.Inners[0].ContainerImage = ContainerImages.StarPlain;

                        this.ContainerPanel.Children.Add(this.Inners[1]);
                        this.Inners[1].Margin = new Thickness(5, 0, 0, 0);
                        this.Inners[1].ContainerOrientation = ContainerOrientations.Right;
                        this.Inners[1].ContainerImage = ContainerImages.StarPlain;

                        this.ContainerPanel.Children.Add(this.Outers[1]);
                        this.Outers[1].Margin = new Thickness(0);
                        this.Outers[1].ContainerOrientation = ContainerOrientations.Right;
                        this.Outers[1].ContainerImage = ContainerImages.CrownLight;
                    }
                    else if (value == DisplayModes.Game_and_Star_plus)
                    {
                        this.ContainerPanel.Children.Add(this.Outers[0]);
                        this.Outers[0].Margin = new Thickness(0);
                        this.Outers[0].ContainerOrientation = ContainerOrientations.Left;
                        this.Outers[0].ContainerImage = ContainerImages.CrownLight;

                        this.ContainerPanel.Children.Add(this.Inners[0]);
                        this.Inners[0].Margin = new Thickness(0, 0, 5, 0);
                        this.Inners[0].ContainerOrientation = ContainerOrientations.Left;
                        this.Inners[0].ContainerImage = ContainerImages.StarPlus;

                        this.ContainerPanel.Children.Add(this.Inners[1]);
                        this.Inners[1].Margin = new Thickness(5, 0, 0, 0);
                        this.Inners[1].ContainerOrientation = ContainerOrientations.Right;
                        this.Inners[1].ContainerImage = ContainerImages.StarPlus;

                        this.ContainerPanel.Children.Add(this.Outers[1]);
                        this.Outers[1].Margin = new Thickness(0);
                        this.Outers[1].ContainerOrientation = ContainerOrientations.Right;
                        this.Outers[1].ContainerImage = ContainerImages.CrownLight;
                    }
                    else //if (value == DisplayModes.Star_plus_and_Star)
                    {
                        this.ContainerPanel.Children.Add(this.Outers[0]);
                        this.Outers[0].Margin = new Thickness(0);
                        this.Outers[0].ContainerOrientation = ContainerOrientations.Left;
                        this.Outers[0].ContainerImage = ContainerImages.StarPlus;

                        this.ContainerPanel.Children.Add(this.Inners[0]);
                        this.Inners[0].Margin = new Thickness(0, 0, 5, 0);
                        this.Inners[0].ContainerOrientation = ContainerOrientations.Left;
                        this.Inners[0].ContainerImage = ContainerImages.StarPlain;

                        this.ContainerPanel.Children.Add(this.Inners[1]);
                        this.Inners[1].Margin = new Thickness(5, 0, 0, 0);
                        this.Inners[1].ContainerOrientation = ContainerOrientations.Right;
                        this.Inners[1].ContainerImage = ContainerImages.StarPlain;

                        this.ContainerPanel.Children.Add(this.Outers[1]);
                        this.Outers[1].Margin = new Thickness(0);
                        this.Outers[1].ContainerOrientation = ContainerOrientations.Right;
                        this.Outers[1].ContainerImage = ContainerImages.StarPlus;
                    }

                    this._mode = value;
                }
            }

            public void SetMode(DisplayModes mode, bool fit)
            {
                this.fitFlag = fit;
                this.Mode = mode;
            }

            public void Set(int outerScore0, int innerScore0, int innerScore1, int outerScore1, ContainerImages outerImage0, ContainerImages innerImage0, ContainerImages innerImage1, ContainerImages outerImage1)
            {
                this.Set(outerScore0, innerScore0, innerScore1, outerScore1);
                this.Set(outerImage0, innerImage0, innerImage1, outerImage1);
            }

            public void Set(int innerScore0, int innerScore1)
            {
                this.Inners[0].Score = innerScore0;
                this.Inners[1].Score = innerScore1;
            }

            public void Set(int outerScore0, int innerScore0, int innerScore1, int outerScore1)
            {
                this.Outers[0].Score = outerScore0;
                this.Inners[0].Score = innerScore0;
                this.Inners[1].Score = innerScore1;
                this.Outers[1].Score = outerScore1;
            }

            public void Set(ContainerImages outerImage0, ContainerImages innerImage0, ContainerImages innerImage1, ContainerImages outerImage1)
            {
                this.Outers[0].ContainerImage = outerImage0;
                this.Inners[0].ContainerImage = innerImage0;
                this.Inners[1].ContainerImage = innerImage1;
                this.Outers[1].ContainerImage = outerImage1;
            }

        }
    }
}



