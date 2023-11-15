using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructuralEmbodiment.Core.Visualisation
{
    public class ControlNetSetting
    {
        public Bitmap CannySourceImage { get; set; }
        public Bitmap DepthMapSourceImage { get; set; }
        public JObject CannySettings { get; set; }
        public JObject DepthMapSettings { get; set; }

        public ControlNetSetting()
        {
            
        }

        public void SetCanny(int width= -1, int height = -1) {
            this.CannySourceImage = Util.CaptureView(width, height);
            string encodedImage = Util.ReadImage(new Bitmap(this.CannySourceImage));
            this.CannySettings = JObject.FromObject(new
            {
                enabled = true,
                module = "canny",
                model = "canny",
                weight = 0.75,
                image = encodedImage,
                resize_mode = 1,
                lowvram = false,
                processor_res = 512,
                threshold_a = 200,
                threshold_b = 100,
                guidance_start = 0.0,
                guidance_end = 1.0,
                control_mode = 0,
                pixel_perfect = false
            });
        }
        public void SetDepthMap(int width = -1, int height = -1) { 
            this.DepthMapSourceImage = Util.CaptureDepthView(width, height);
            string encodedImage = Util.ReadImage(new Bitmap (this.DepthMapSourceImage));
            this.DepthMapSettings = JObject.FromObject(new
            {
                enabled = true,
                //module = "depth",
                model = "depth",
                weight = 1,
                image = encodedImage,
                resize_mode = 1,
                lowvram = false,
                processor_res = 512,
                threshold_a = 200,
                threshold_b = 100,
                guidance_start = 0.0,
                guidance_end = 1.0,
                control_mode = 0,
                pixel_perfect = false
            });
        }

        public override string ToString()
        {
            string repr = "";
            if (this.CannySettings != null) repr += this.CannySettings.ToString();
            if (this.DepthMapSettings != null) repr += this.DepthMapSettings.ToString();
            return repr;
        }
    }
}
