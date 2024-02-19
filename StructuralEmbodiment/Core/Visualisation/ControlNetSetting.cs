using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructuralEmbodimentToolkit.Core.Visualisation
{
    public class ControlNetSetting
    {
        //Obsolete
        public Bitmap CannySourceImage { get; set; }
        public Bitmap DepthMapSourceImage { get; set; }


        public Bitmap SourceImage { get; set; }
        public JObject CannySettings { get; set; }
        public JObject DepthMapSettings { get; set; }
        public JObject Settings { get; set; }


        //Obsolete
        public ControlNetSetting()
        {
        }
        public ControlNetSetting(Bitmap sourceImage)
        {
            this.SourceImage = sourceImage;
        }

        public void SetLineGuide(string lineModel, double cNWeight = 1, int width= -1, int height = -1) {
            string encodedImage = Util.ReadImage(new Bitmap(this.SourceImage));
            this.Settings = JObject.FromObject(new
            {
                enabled = true,
                //module = "canny",
                model = lineModel,
                weight = cNWeight,
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
        public void SetDepthGuide(string depthModel, double cNWeight = 1, int width = -1, int height = -1) { 
            //this.DepthMapSourceImage = Util.CaptureDepthView(width, height);
            string encodedImage = Util.ReadImage(new Bitmap (this.SourceImage));
            this.Settings = JObject.FromObject(new
            {
                enabled = true,
                //module = "depth",
                model = depthModel,
                weight = cNWeight,
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
        public void SetSegGuide(string segModel = "control_seg-fp16", double cNWeight = 1,int width = -1, int height = -1)
        {
            string encodedImage = Util.ReadImage(new Bitmap(this.SourceImage));
            this.Settings = JObject.FromObject(new
            {
                enabled = true,
                //module = "segmentation",
                model = segModel,
                weight = cNWeight,
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
        public void SetGenericGuide(string cNModule, string cNModel,double weight=1, int width = -1, int height = -1)
        {
            string encodedImage = Util.ReadImage(new Bitmap(this.SourceImage));
            this.Settings = JObject.FromObject(new
            {
                enabled = true,
                module = cNModule,
                model = cNModel,
                weight = weight,
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
            //if (this.CannySettings != null) repr += this.CannySettings.ToString();
            //if (this.DepthMapSettings != null) repr += this.DepthMapSettings.ToString();
            if (this.Settings != null) repr += this.Settings.ToString();
            else repr += "No guide set";
            return repr;
        }
    }
}
