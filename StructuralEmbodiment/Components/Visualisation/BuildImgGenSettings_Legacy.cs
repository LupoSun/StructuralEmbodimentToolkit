using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;
using StructuralEmbodiment.Core.GrasshopperAsyncComponent;
using StructuralEmbodiment.Core.Visualisation;
using StructuralEmbodiment.Properties;


namespace StructuralEmbodiment.Components.Visualisation
{
    public class BuildImgGenSettings_Legacy : GH_AsyncComponent
    {
        string ServerURL = "http://127.0.0.1:7860";
        /// <summary>
        /// Initializes a new instance of the BuildImageSettings class.
        /// </summary>
        public BuildImgGenSettings_Legacy()
          : base("Build Image Settings", "ImgSetting",
              "Compile the settings for the image generation",
              "Structural Embodiment", "Visualisation")
        {
            BaseWorker = new ImgGenSettingsWorker(ServerURL);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("• Prompt", "• P", "Describe the image to generate", GH_ParamAccess.item);
            pManager.AddTextParameter("Negative Prompt", "NP", "Describe the image not to generate", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Random Seed", "RS", "The random seed for the generation", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Batch Size", "BS", "Number of images in one geration", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Steps", "S", "Number of sampling steps", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Width", "W", "Pixel width of the image", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Height", "H", "Pixel height of the image", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;


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
                return Resources.VIS_BuildImageSettings_Legacy;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5BCF32B0-45B5-45B3-9F21-41D5570F063D"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.septenary; }
        }

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Cancel Current Operation", (s, e) =>
            {
                RequestCancellation();
            });
            Menu_AppendTextItem(menu, "SD Server URL", OnKeyDown, null, false);

            //TODO: Debug this function
            void OnKeyDown(GH_MenuTextBox sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    this.ServerURL = sender.Text;
                    ((ImgGenSettingsWorker)BaseWorker).UpdateServerURL(this.ServerURL);
                    ExpireSolution(true);
                }
                
            }
        }

        private class ImgGenSettingsWorker: WorkerInstance 
        {
            public ImageGenerationSetting ImgGenSettings { get; set; }

            string url = "http://127.0.0.1:7860";

            public string Prompt { get; set; }
            public string NegativePrompt { get; set; }
            public int RandomSeed { get; set; }
            public string Sampler { get; set; }
            public int BatchSize { get; set; }
            public int Steps { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            


            public ImgGenSettingsWorker(string url) : base(null) { this.url = url; }

            public void UpdateServerURL(string newUrl)
            {
                this.url = newUrl;
            }
            public override WorkerInstance Duplicate() => new ImgGenSettingsWorker(url);

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                // Check if the operation has been cancelled
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    Task<ImageGenerationSetting> imgSettingsTask = ImageGenerationSetting.CreateImageSettingsObject(url);
                    ImageGenerationSetting imgSettings = imgSettingsTask.Result;
                    imgSettings.InitialiseSettings(Prompt, NegativePrompt, RandomSeed, Sampler, BatchSize, Steps, Width, Height);
                    this.ImgGenSettings = imgSettings;
                } catch (Exception e)
                {
                    throw new Exception(e.Message);
                }   
                Done();


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
                int _steps = 20;
                DA.GetData("Steps", ref _steps);
                int _width = 512;
                DA.GetData("Width", ref _width);
                int _height = 512;
                DA.GetData("Height", ref _height);

                this.Prompt = _prompt;
                this.NegativePrompt = _negPrompt;
                this.RandomSeed = _randomSeed;
                this.Sampler = "DPM++ 2M Karras";
                this.BatchSize = _batchSize;
                this.Steps = _steps;
                this.Width = _width;
                this.Height = _height;
            }
            

        }
    }
}