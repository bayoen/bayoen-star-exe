using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ok = Octokit;
using bayoen.material;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

namespace bayoen.star.updater
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Hide();
            this.Background = new SolidColorBrush(Color.FromRgb(25, 25, 25)) { Opacity = 0.95 };
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
            this.LogoImage.SetBitmap(bayoen.star.updater.Properties.Resources.BayoenEN);
            this.Show();

            this.launcherWorker = new BackgroundWorker();
            this.launcherWorker.DoWork += LauncherWorker_DoWork;

            this.launcherWorker.RunWorkerAsync();                       
        }

        private const string bayoenStarName = "bayoen-star.exe";
        private const string versionDataName = "bayoen-star-version.dat";
        private const string coreListName = "Resources/bayoen-star-updater-list.dat";
        private const string updatingFolderName = "__update__";
        private static string UpdatarName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name + ".exe";
        private Version currentVersion = null;

        public BackgroundWorker launcherWorker;
        public BayoenMessage tokenMessage;

        private void LauncherWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (IsGoogleOn)
            {
                this.CheckLocalVersion();

                this.UpdatorTextBlock.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    this.UpdatorTextBlock.Text += string.Format(" {0}", this.currentVersion.ToString());
                }));
                Thread.Sleep(1);

                #region CheckReleases::
                ok::GitHubClient client;
                ok::Release latest;
                bool nextBigVersionFlag = false;

                try
                {
                    client = new ok::GitHubClient(new ok::ProductHeaderValue("bayoen"));
                    //latest = client.Repository.Release.GetLatest("bayoen", "bayoen-star-exe").Result;

                    List<ok.Release> releases = client.Repository.Release.GetAll("bayoen", "bayoen-star-exe").Result.ToList();

                    int nextBigVersion = releases.FindIndex(x => Version.Parse(x.TagName.GetVersionNumbers()) >= new Version(0, 2));

                    if (nextBigVersion == 0)
                    {
                        latest = null;
                    }
                    else if (nextBigVersion > 0)
                    {
                        nextBigVersionFlag = true;
                        latest = releases[nextBigVersion - 1];
                    }
                    else
                    {
                        latest = releases.Last();
                    }                    
                }
                catch
                {
                    latest = null;
                }

                if (latest == null)
                {                    
                    MessageBox.Show("We can't find any latest version", "Warning");
                    Environment.Exit(0);
                }
                else
                {
                    Version latestVersion = Version.Parse(latest.TagName.GetVersionNumbers());
                    
                    if (currentVersion <= latestVersion)
                    {
                        this.UpdatorTextBlock.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            this.UpdatorTextBlock.Text += string.Format(" to {0}...", latestVersion.ToString());
                            if (nextBigVersionFlag)
                            {
                                this.UpdatorTextBlock.Text += $"\nWe can not update modules to upper versions!\nPlease visit 'https://bayoen.github.io/star/'";
                            }
                        }));
                        Thread.Sleep(1);

                        List<string> fileNameList = new List<string>();
                        foreach (ok::ReleaseAsset assetToken in latest.Assets)
                        {
                            using (System.Net.WebClient web = new System.Net.WebClient())
                            {
                                string url = assetToken.BrowserDownloadUrl;
                                string fileName = url.Split('/').Last();
                                web.DownloadFile(url, fileName);
                                fileNameList.Add(fileName);
                            }
                        }

                        List<string> UpdaterList = File.ReadAllText(coreListName, Encoding.UTF8).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        string rootPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                        string updatingFolderPath = System.IO.Path.Combine(rootPath, updatingFolderName);
                        if (Directory.Exists(updatingFolderPath)) Directory.Delete(updatingFolderPath, true);
                        foreach (string fileNameToken in fileNameList)
                        {
                            if (fileNameToken.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                            {
                                using (ZipArchive readingToken = ZipFile.Open(fileNameToken, ZipArchiveMode.Update))
                                {                                    
                                    readingToken.ExtractToDirectory(updatingFolderPath);
                                }
                                if (File.Exists(fileNameToken)) File.Delete(fileNameToken);
                            }
                        }

                        if (Directory.Exists(updatingFolderPath))
                        {
                            List<string> fileList = Directory.GetFiles(updatingFolderPath, "*.*", SearchOption.AllDirectories).ToList();
                            bool updaterFlag = fileList.Remove(System.IO.Path.Combine(updatingFolderPath, UpdatarName));

                            foreach (string filePath in fileList)
                            {
                                string token = filePath.Replace(updatingFolderPath + '\\', "");
                                if (UpdaterList.IndexOf(token) > -1) continue;

                                if (File.Exists(filePath))
                                {
                                    string targetPath = System.IO.Path.Combine(rootPath, System.IO.Path.GetFileName(filePath));
                                    if (File.Exists(targetPath))
                                    {
                                        File.SetAttributes(targetPath, FileAttributes.Normal);
                                        File.Delete(targetPath);
                                    }
                                    File.Move(filePath, targetPath);
                                }
                            }

                            if (!updaterFlag) Directory.Delete(updatingFolderPath);
                        }

                        File.WriteAllText(versionDataName, latestVersion.ToString());
                    }
                    else //if (currentVersion > latestVersion)
                    {
                        MessageBox.Show("Hm... we are doing well, right?", "Warning");
                    }
                }

                #endregion

                if (File.Exists(bayoenStarName)) Process.Start(bayoenStarName);
                Environment.Exit(0);
            }
            else
            {
                System.Media.SystemSounds.Hand.Play();
            }

        }        

        private bool IsGoogleOn
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

        private void CheckLocalVersion()
        {
            // Check local version
            if (File.Exists(versionDataName))
            {
                try
                {
                    this.currentVersion = Version.Parse(File.ReadAllText(versionDataName, Encoding.ASCII));
                }
                catch
                {
                    this.currentVersion = new Version(0, 0, 0, 0);
                }
            }
            else
            {
                this.currentVersion = new Version(0, 0, 0, 0);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Media.SystemSounds.Hand.Play();
            //e.Cancel = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }            
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

        public static string GetVersionNumbers(this string input)
        {
            return new string(input.Where(c => char.IsDigit(c) || c == '.').ToArray());
        }
    }
}
