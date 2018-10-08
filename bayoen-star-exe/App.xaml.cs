using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace bayoen
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!CheckOverlapPass("pref.json"))
            {
                Process[] operatingProcesses = Process.GetProcesses();
                Process currentProcess = Process.GetCurrentProcess();

                List<Process> overlappedProcesses = operatingProcesses.ToList().FindAll(x => x.ProcessName == currentProcess.ProcessName);
                overlappedProcesses.RemoveAll(x => x.StartTime == currentProcess.StartTime);

                if (overlappedProcesses.Count >= 1)
                {
                    System.Media.SystemSounds.Hand.Play();
                    MessageBox.Show(string.Format("'bayoen' is already running:\n > {0}", overlappedProcesses.First().ProcessName), "Warning");
                    currentProcess.Kill();
                }
            }

            base.OnStartup(e);
        }

        private bool CheckOverlapPass(string prefName)
        {
            if (File.Exists(prefName))
            {
                string rawPref = File.ReadAllText(prefName, System.Text.Encoding.Unicode);
                string skipPattern = "\"OverlapPass\": true";
                if (rawPref.IndexOf(skipPattern) > -1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
