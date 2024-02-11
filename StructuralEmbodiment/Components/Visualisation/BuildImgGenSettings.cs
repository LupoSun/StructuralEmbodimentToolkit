using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Newtonsoft.Json.Linq;
using StructuralEmbodiment.Core.GrasshopperAsyncComponent;
using StructuralEmbodiment.Core.Visualisation;
using StructuralEmbodiment.Properties;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;


namespace StructuralEmbodiment.Components.Visualisation
{
    public class BuildImgGenSettings : GH_AsyncComponent
    {
        public bool loadSDModelPressed = false;
        /// <summary>
        /// Initializes a new instance of the BuildImageSettings class.
        /// </summary>
        public BuildImgGenSettings()
          : base("Build Image Settings", "ImgSetting",
              "Compile the settings for the image generation",
              "Structural Embodiment", "Visualisation")
        {
            BaseWorker = new ImgGenSettingsWorker(this, SDWebUISetting.Instance);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("• Prompt", "• P", "Describe the image to generate", GH_ParamAccess.item);
            pManager.AddTextParameter("Negative Prompt", "NP", "Describe the image not to generate", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Random Seed", "RS", "The random seed for the generation", GH_ParamAccess.item, -1);
            pManager.AddIntegerParameter("Batch Size", "BS", "Number of images in one geration", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Steps", "ST", "Number of sampling steps", GH_ParamAccess.item, 20);
            pManager.AddNumberParameter("CFG Scale", "CFG", "Classifier Free Guidance Scale - how strongly the image should conform to prompt - lower values produce more creative results", GH_ParamAccess.item, 7);
            pManager.AddIntegerParameter("Width", "W", "Pixel width of the image", GH_ParamAccess.item, 512);
            pManager.AddIntegerParameter("Height", "H", "Pixel height of the image", GH_ParamAccess.item, 512);
            pManager.AddTextParameter("SD Model", "SDM", "StableDiffusion model", GH_ParamAccess.item);
            pManager.AddTextParameter("Sampler", "SP", "Sampling method", GH_ParamAccess.item);
            pManager.AddTextParameter("LoRAs", "LoRAs", "LoRA models", GH_ParamAccess.list);
            pManager.AddNumberParameter("LoRA Multipliers", "LoRA M", "LoRA multipliers", GH_ParamAccess.list);
            pManager.AddGenericParameter("Guides", "G", "Guidances for generation", GH_ParamAccess.list);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;
            pManager[10].Optional = true;
            pManager[11].Optional = true;
            pManager[12].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Settings", "IGS", "Settings for image Generation", GH_ParamAccess.item);
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.VIS_BuildImageSettings;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("EEA43DB2-F06D-4AAB-AA69-F66C4927001E"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Cancel Current Operation", (s, e) =>
            {
                RequestCancellation();
            });
        }

        public override void CreateAttributes()
        {
            base.m_attributes = new BuildImgGenSettings_ButtonAttributes(this);

        }

        private class ImgGenSettingsWorker : WorkerInstance
        {
            public ImageGenerationSetting ImgGenSettings { get; set; }

            BuildImgGenSettings Owner;
            SDWebUISetting SDWebUISetting;

            public string Prompt { get; set; }
            public string NegativePrompt { get; set; }
            public int RandomSeed { get; set; }
            public string Sampler { get; set; }
            public int BatchSize { get; set; }
            public int Steps { get; set; }
            public double CFGScale { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public List<string> LoRAs { get; set; }
            public List<double> LoRAMultipliers { get; set; }
            public List<ControlNetSetting> Guides { get; set; }

            public string SDModel { get; set; }



            public ImgGenSettingsWorker(BuildImgGenSettings owner, SDWebUISetting sDWebUISetting) : base(null)
            {
                this.Owner = owner;
                this.SDWebUISetting = sDWebUISetting;
            }

            public override WorkerInstance Duplicate() => new ImgGenSettingsWorker(this.Owner, this.SDWebUISetting);

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                // Check if the operation has been cancelled
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    this.Prompt = Core.Util.AddLoRAsToPrompt(this.Prompt, this.LoRAs, this.LoRAMultipliers);
                    this.ImgGenSettings = new ImageGenerationSetting(SDWebUISetting.Instance);
                    this.ImgGenSettings.InitialiseSettings(this.Prompt, this.NegativePrompt, this.RandomSeed, this.Sampler, this.BatchSize, this.Steps, this.CFGScale, this.Width, this.Height);
                    foreach (var guide in this.Guides)
                    {
                        if (guide.Settings != null)
                        {
                            this.ImgGenSettings.Settings = Core.Util.AddControlNet(this.ImgGenSettings.Settings, guide.Settings);
                        }
                    }
                    Done();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                if (this.Owner.loadSDModelPressed)
                {
                    this.Owner.loadSDModelPressed = false;
                    if (this.SDModel != null || this.SDModel == "")
                    {
                        this.SDWebUISetting.SDWebUIOptions(SDModel).Wait();
                    }
                    else
                    {
                        throw new Exception("No SD Model selected");
                    }

                }

            }

            public override void SetData(IGH_DataAccess DA)
            {
                DA.SetData("Settings", this.ImgGenSettings);
            }

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                // Get and set the input data
                string _prompt = "";
                DA.GetData("• Prompt", ref _prompt);
                string _negPrompt = "";
                DA.GetData("Negative Prompt", ref _negPrompt);
                int _randomSeed = -1;
                DA.GetData("Random Seed", ref _randomSeed);
                int _batchSize = 1;
                DA.GetData("Batch Size", ref _batchSize);
                double _cfgScale = 7;
                DA.GetData("CFG Scale", ref _cfgScale);
                int _steps = 20;
                DA.GetData("Steps", ref _steps);
                int _width = 512;
                DA.GetData("Width", ref _width);
                int _height = 512;
                DA.GetData("Height", ref _height);
                string _sampler = "DPM++ 2M Karras";
                DA.GetData("Sampler", ref _sampler);
                List<string> _loRAs = new List<string>();
                DA.GetDataList("LoRAs", _loRAs);
                List<double> _loRAMultipliers = new List<double>();
                DA.GetDataList("LoRA Multipliers", _loRAMultipliers);
                List<ControlNetSetting> _guides = new List<ControlNetSetting>();
                DA.GetDataList("Guides", _guides);

                string _sdModel = "";
                DA.GetData("SD Model", ref _sdModel);

                this.Prompt = _prompt;
                this.NegativePrompt = _negPrompt;
                this.RandomSeed = _randomSeed;
                this.Sampler = _sampler;
                this.BatchSize = _batchSize;
                this.Steps = _steps;
                this.CFGScale = _cfgScale;
                this.Width = _width;
                this.Height = _height;
                this.LoRAs = _loRAs;
                this.LoRAMultipliers = _loRAMultipliers;
                this.Guides = _guides;

                this.SDModel = _sdModel;

            }

        }
    }

    #region GH_ComponentAttributes interface

    public class BuildImgGenSettings_ButtonAttributes : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        BuildImgGenSettings owner = null;

        public BuildImgGenSettings_ButtonAttributes(BuildImgGenSettings owner) : base(owner)
        {
            this.owner = owner;

        }

        protected override void Layout()
        {
            base.Layout();
            var buttonWidth = 120;
            var bezel = 5;

            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(this.Bounds);
            rec0.Height += (22 * 1 + 8 + bezel);
            System.Drawing.Rectangle rec1 = rec0;
            rec1.Y = rec1.Bottom - 22 - 8 - bezel;
            rec1.Height = 22 + 8;
            rec1.Width = buttonWidth;
            var x = rec0.Right - rec0.Width / 2 - rec1.Width / 2;
            rec1.X = x;
            rec1.Inflate(-2, -2);

            Bounds = rec0;
            ButtonBounds = rec1;

        }
        private System.Drawing.Rectangle ButtonBounds { get; set; }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Pink, "Load SD Model", new int[] { 7, 7, 0, 7 }, 4);
                button.Render(graphics, Selected, Owner.Locked, false);

            }
        }
        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                System.Drawing.RectangleF rec = ButtonBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    if (this.owner != null)
                    {
                        this.owner.loadSDModelPressed = true;
                        owner.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }
    }
    #endregion GH_ComponentAttributes interface
}