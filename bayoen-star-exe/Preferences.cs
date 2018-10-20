using System.Collections.Generic;
using System.Text;
using System.IO;

using js = Newtonsoft.Json;
using jl = Newtonsoft.Json.Linq;

namespace bayoen
{
    public partial class MainWindow
    {
        public class Preferences
        {
            // Active
            public bool? IsTopMost;
            public bool? IsChromaKey;
            public ChromaKeys? ChromaKey;
            public bool? ExportText;
            public bool? IsFitToScore;
            public bool? IsOverlayFixed;
            public DisplayModes? DisplayMode;

            public GoalTypes? GoalType;
            public int? GoalScore;

            // Passive
            public List<double> Overlay;
            public bool? EverClosed;

            // Escape
            public int? Period;
            public bool? OverlapPass;

            public Preferences()
            {
                this.Clear();
            }

            public void Clear()
            {
                this.IsTopMost = null;
                //this.IsChromaKey = null;
                this.ChromaKey = null;
                this.ExportText = null;
                this.IsFitToScore = null;
                this.IsOverlayFixed = null;

                this.Overlay = null;
                this.DisplayMode = null;

                this.EverClosed = null;
                this.OverlapPass = false;
                this.Period = 333;
            }

            public static Preferences Load(string src)
            {
                Preferences output = null;
                bool brokenFlag = false;
                if (File.Exists(src))
                {
                    string rawString = File.ReadAllText(src, Encoding.Unicode);

                    try
                    {
                        output = js::JsonConvert.DeserializeObject<Preferences>(rawString, new js::JsonSerializerSettings() { NullValueHandling = js::NullValueHandling.Ignore, });
                    }
                    catch
                    {
                        brokenFlag = true;
                    }
                }
                else
                {
                    brokenFlag = true;
                }

                if (brokenFlag)
                {
                    output = new Preferences();
                    File.WriteAllText(src, output.ToJSON().ToString(), Encoding.Unicode);
                }

                return output;
            }

            public bool Save(string dst)
            {
                try
                {
                    File.WriteAllText(dst, this.ToJSON().ToString(), Encoding.Unicode);
                }
                catch
                {
                    return false;
                }

                return true;
            }

            public jl::JObject ToJSON()
            {
                return jl::JObject.Parse(js::JsonConvert.SerializeObject(this, new js::JsonSerializerSettings() { NullValueHandling = js::NullValueHandling.Ignore, }));
            }

            public static Preferences FromJSON(jl::JObject jobject)
            {
                return js::JsonConvert.DeserializeObject<Preferences>(jobject.ToString(), new js::JsonSerializerSettings() { NullValueHandling = js::NullValueHandling.Ignore, });
            }
        }
    }

}



