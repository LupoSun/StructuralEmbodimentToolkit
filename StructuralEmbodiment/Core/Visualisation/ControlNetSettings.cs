using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructuralEmbodiment.Core.Visualisation
{
    internal class ControlNetSettings
    {
        public Bitmap CannyImage { get; set; }
        public Bitmap DepthMapImage { get; set; }
        public JObject CannySettings { get; set; }
        public JObject DepthMapSettings { get; set; }

        public ControlNetSettings()
        {
            
        }

        public void SetCanny() { }
        public void SetDepthMap() { }
    }
}
