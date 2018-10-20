using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;

namespace bayoen
{
    public partial class MainWindow
    {
        public class Display2Grid : Grid
        {
            public Display2Grid()
            {
                this.PanelImage = new Image()
                {
                    Height = 68,
                    Width = 406,
                    Margin = new Thickness(0),
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

                this.SubDisplayGrid = new Grid()
                {
                    Margin = new Thickness(0, 0, 0, 92),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Visibility = Visibility.Collapsed,
                };
                this.Children.Add(this.SubDisplayGrid);

                this.SubDisplayPanelImage = new Image()
                {
                    Width = 180,
                    Height = 29,
                    Margin = new Thickness(0, 0, 0, 4),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                this.SubDisplayPanelImage.SetBitmap(bayoen.Properties.Resources.SubPanelGoalString);
                this.SubDisplayGrid.Children.Add(this.SubDisplayPanelImage);

                this.SubDisplayContainerPanel = new StackPanel()
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Orientation = Orientation.Horizontal,                    
                };
                this.SubDisplayGrid.Children.Add(this.SubDisplayContainerPanel);

                TextBlock SubDisplayTFTextBlock = new TextBlock()
                {
                    Text = "GOAL",
                    FontSize = 20,
                    Margin = new Thickness(3, 2, 6, 0),
                    FontWeight = FontWeights.ExtraBold,
                    FontFamily = new System.Windows.Media.FontFamily("Arial"),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                this.SubDisplayContainerPanel.Children.Add(SubDisplayTFTextBlock);

                this.SubDisplaySymbolImage = new Image()
                {
                    Width = 17,
                    Height = 17,
                    VerticalAlignment = VerticalAlignment.Center,
                };                
                this.SubDisplayContainerPanel.Children.Add(this.SubDisplaySymbolImage);

                this.SubDisplayTextBlock = new TextBlock()
                {
                    Text = "-",
                    FontSize = 20,
                    Margin = new Thickness(3, 2, 0, 0),
                    FontWeight = FontWeights.ExtraBold,
                    FontFamily = new System.Windows.Media.FontFamily("Arial"),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                this.SubDisplayContainerPanel.Children.Add(this.SubDisplayTextBlock);

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

            public Grid SubDisplayGrid;
            public Image SubDisplayPanelImage;
            public StackPanel SubDisplayContainerPanel;
            public Image SubDisplaySymbolImage;
            public TextBlock SubDisplayTextBlock;

            public List<Container> Inners;
            public List<Container> Outers;

            private bool fitFlag;
            private DisplayModes _mode;
            private DisplayModes Mode
            {
                get => this._mode;
                set
                {
                    //if (value == this._mode)
                    //{
                    //    return;
                    //}

                    this.Inners.ForEach(x => this.ContainerPanel.Children.Remove(x));
                    this.Outers.ForEach(x => this.ContainerPanel.Children.Remove(x));

                    //if (this._mode >= DisplayModes.Game_and_Star)
                    //{
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
                    //}
                    else
                    //{
                        if (value >= DisplayModes.Game_and_Star)
                        {
                            PanelImage.SetBitmap(bayoen.Properties.Resources.PanelScore2LongStrong);
                        }
                    //}

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

            public void DisplayGoal(int goal, GoalTypes type)
            {
                if (this.SubDisplayGrid.Visibility == Visibility.Collapsed) this.SubDisplayGrid.Visibility = Visibility.Visible;
                if (type == GoalTypes.Star)
                {
                    this.SubDisplaySymbolImage.SetBitmap(bayoen.Properties.Resources.StarPlus);
                }
                else if (type == GoalTypes.Crown)
                {
                    this.SubDisplaySymbolImage.SetBitmap(bayoen.Properties.Resources.CrownLight);
                }
                else
                {
                    throw new System.InvalidOperationException();
                }

                this.SubDisplayTextBlock.Text = (goal == 0) ? ("-") : (goal.ToString());
            }

            public void HideGoal()
            {
                if (this.SubDisplayGrid.Visibility == Visibility.Visible) this.SubDisplayGrid.Visibility = Visibility.Collapsed;
                this.SubDisplayTextBlock.Text = "-";
            }

            public void Set(int outerScore0, int innerScore0, int innerScore1, int outerScore1, ContainerImages outerImage0, ContainerImages innerImage0, ContainerImages innerImage1, ContainerImages outerImage1)
            {
                this.Set_Old(outerScore0, innerScore0, innerScore1, outerScore1);
                this.Set(outerImage0, innerImage0, innerImage1, outerImage1);
            }

            public void Set_Old(int innerScore0, int innerScore1)
            {
                this.Inners[0].Score = innerScore0;
                this.Inners[1].Score = innerScore1;
            }

            public void Set_Old(int outerScore0, int innerScore0, int innerScore1, int outerScore1)
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


            public void Set(List<int> current, List<int> counted, List<int> crowns, GoalTypes goalType, int goalScore)
            {
                if (goalType == GoalTypes.None)
                {
                    this.Inners.ForEach(x => x.ContainerState = ContainerStates.Idle);
                    this.Outers.ForEach(x => x.ContainerState = ContainerStates.Idle);
                }

                if (this.Mode == DisplayModes.Game_and_Star_plus)
                {
                    this.Outers[0].Score = crowns[0];
                    this.Inners[0].Score = counted[0];
                    this.Inners[1].Score = counted[1];
                    this.Outers[1].Score = crowns[1];

                    if (goalType == GoalTypes.Star)
                    {
                             if (counted[0]  < goalScore) { this.Inners[0].ContainerState = ContainerStates.Idle;     this.Outers[0].ContainerState = ContainerStates.Idle;    }
                        else if (counted[0] == goalScore) { this.Inners[0].ContainerState = ContainerStates.Goal;     this.Outers[0].ContainerState = ContainerStates.Goal;    }
                        else if (counted[0] >  goalScore) { this.Inners[0].ContainerState = ContainerStates.Broken;   this.Outers[0].ContainerState = ContainerStates.Broken;  }
                                                                                                                           
                             if (counted[1]  < goalScore) { this.Inners[1].ContainerState = ContainerStates.Idle;     this.Outers[1].ContainerState = ContainerStates.Idle;    }
                        else if (counted[1] == goalScore) { this.Inners[1].ContainerState = ContainerStates.Goal;     this.Outers[1].ContainerState = ContainerStates.Goal;    }
                        else if (counted[1] >  goalScore) { this.Inners[1].ContainerState = ContainerStates.Broken;   this.Outers[1].ContainerState = ContainerStates.Broken;  }
                    }
                    else if (goalType == GoalTypes.Crown)
                    {
                             if (crowns[0]  < goalScore) { this.Inners[0].ContainerState = ContainerStates.Idle;     this.Outers[0].ContainerState = ContainerStates.Idle;    }
                        else if (crowns[0] == goalScore) { this.Inners[0].ContainerState = ContainerStates.Goal;     this.Outers[0].ContainerState = ContainerStates.Goal;    }
                        else if (crowns[0] >  goalScore) { this.Inners[0].ContainerState = ContainerStates.Broken;   this.Outers[0].ContainerState = ContainerStates.Broken;  }
                                 
                             if (crowns[1]  < goalScore) { this.Inners[1].ContainerState = ContainerStates.Idle;     this.Outers[1].ContainerState = ContainerStates.Idle;    }
                        else if (crowns[1] == goalScore) { this.Inners[1].ContainerState = ContainerStates.Goal;     this.Outers[1].ContainerState = ContainerStates.Goal;    }
                        else if (crowns[1] >  goalScore) { this.Inners[1].ContainerState = ContainerStates.Broken;   this.Outers[1].ContainerState = ContainerStates.Broken;  }
                    }
                }
                else if (this.Mode == DisplayModes.Game_and_Star)
                {
                    this.Outers[0].Score = crowns[0];
                    this.Inners[0].Score = current[0];
                    this.Inners[1].Score = current[1];
                    this.Outers[1].Score = crowns[1];

                    if (goalType == GoalTypes.Star)
                    {
                        this.Inners.ForEach(x => x.ContainerState = ContainerStates.Idle);
                        this.Outers.ForEach(x => x.ContainerState = ContainerStates.Idle);
                    }
                    else if (goalType == GoalTypes.Crown)
                    {
                             if (crowns[0]  < goalScore) { this.Inners[0].ContainerState = ContainerStates.Idle;     this.Outers[0].ContainerState = ContainerStates.Idle;    }
                        else if (crowns[0] == goalScore) { this.Inners[0].ContainerState = ContainerStates.Goal;     this.Outers[0].ContainerState = ContainerStates.Goal;    }
                        else if (crowns[0] >  goalScore) { this.Inners[0].ContainerState = ContainerStates.Broken;   this.Outers[0].ContainerState = ContainerStates.Broken;  }
                                 
                             if (crowns[1]  < goalScore) { this.Inners[1].ContainerState = ContainerStates.Idle;     this.Outers[1].ContainerState = ContainerStates.Idle;    }
                        else if (crowns[1] == goalScore) { this.Inners[1].ContainerState = ContainerStates.Goal;     this.Outers[1].ContainerState = ContainerStates.Goal;    }
                        else if (crowns[1] >  goalScore) { this.Inners[1].ContainerState = ContainerStates.Broken;   this.Outers[1].ContainerState = ContainerStates.Broken;  }
                    }
                }
                else if (this.Mode == DisplayModes.Game_only)
                {
                    this.Inners[0].Score = crowns[0];
                    this.Inners[1].Score = crowns[1];

                    if (goalType == GoalTypes.Star)
                    {
                        this.Inners.ForEach(x => x.ContainerState = ContainerStates.Idle);
                        this.Outers.ForEach(x => x.ContainerState = ContainerStates.Idle);
                    }
                    else if (goalType == GoalTypes.Crown)
                    {
                             if (crowns[0]  < goalScore) { this.Inners[0].ContainerState = ContainerStates.Idle;     }
                        else if (crowns[0] == goalScore) { this.Inners[0].ContainerState = ContainerStates.Goal;     }
                        else if (crowns[0] >  goalScore) { this.Inners[0].ContainerState = ContainerStates.Broken;   }
                                 
                             if (crowns[1]  < goalScore) { this.Inners[1].ContainerState = ContainerStates.Idle;     }
                        else if (crowns[1] == goalScore) { this.Inners[1].ContainerState = ContainerStates.Goal;     }
                        else if (crowns[1] >  goalScore) { this.Inners[1].ContainerState = ContainerStates.Broken;   }
                    }
                }
                else if (this.Mode == DisplayModes.Star_plus_only)
                {
                    this.Inners[0].Score = counted[0];
                    this.Inners[1].Score = counted[1];

                    if (goalType == GoalTypes.Star)
                    {
                             if (counted[0]  < goalScore) { this.Inners[0].ContainerState = ContainerStates.Idle;     }
                        else if (counted[0] == goalScore) { this.Inners[0].ContainerState = ContainerStates.Goal;     }
                        else if (counted[0] >  goalScore) { this.Inners[0].ContainerState = ContainerStates.Broken;   }
                                                                                                                      
                             if (counted[1]  < goalScore) { this.Inners[1].ContainerState = ContainerStates.Idle;     }
                        else if (counted[1] == goalScore) { this.Inners[1].ContainerState = ContainerStates.Goal;     }
                        else if (counted[1] >  goalScore) { this.Inners[1].ContainerState = ContainerStates.Broken;   }
                    }
                    else if (goalType == GoalTypes.Crown)
                    {
                        this.Inners.ForEach(x => x.ContainerState = ContainerStates.Idle);
                        this.Outers.ForEach(x => x.ContainerState = ContainerStates.Idle);
                    }
                }
                else //if (this.Mode == DisplayModes.Star_plus_and_Star)
                {
                    this.Outers[0].Score = counted[0];
                    this.Inners[0].Score = current[0];
                    this.Inners[1].Score = current[1];
                    this.Outers[1].Score = counted[1];

                    if (goalType == GoalTypes.Star)
                    {
                             if (counted[0]  < goalScore) { this.Inners[0].ContainerState = ContainerStates.Idle;     this.Outers[0].ContainerState = ContainerStates.Idle;    }
                        else if (counted[0] == goalScore) { this.Inners[0].ContainerState = ContainerStates.Goal;     this.Outers[0].ContainerState = ContainerStates.Goal;    }
                        else if (counted[0] >  goalScore) { this.Inners[0].ContainerState = ContainerStates.Broken;   this.Outers[0].ContainerState = ContainerStates.Broken;  }

                             if (counted[1]  < goalScore) { this.Inners[1].ContainerState = ContainerStates.Idle;     this.Outers[1].ContainerState = ContainerStates.Idle;    }
                        else if (counted[1] == goalScore) { this.Inners[1].ContainerState = ContainerStates.Goal;     this.Outers[1].ContainerState = ContainerStates.Goal;    }
                        else if (counted[1] >  goalScore) { this.Inners[1].ContainerState = ContainerStates.Broken;   this.Outers[1].ContainerState = ContainerStates.Broken;  }
                    }
                    else if (goalType == GoalTypes.Crown)
                    {
                        this.Inners.ForEach(x => x.ContainerState = ContainerStates.Idle);
                        this.Outers.ForEach(x => x.ContainerState = ContainerStates.Idle);
                    }
                }
            }
        }
    }
}



