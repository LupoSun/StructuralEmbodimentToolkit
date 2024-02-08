using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Rhino.Geometry;
using StructuralEmbodiment.Core.GrasshopperAsyncComponent;
using StructuralEmbodiment.Core.Visualisation;
using StructuralEmbodiment.Properties;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class AIVisualiser : GH_AsyncComponent
    {
        private List<Bitmap> ImagePersistent;
        /// <summary>
        /// Initializes a new instance of the Visualiser class.
        /// </summary>
        public AIVisualiser()
          : base("AI Visualiser", "Vis",
              "AI Visualisation Generator by StableDiffusion",
              "Structural Embodiment", "Visualisation")
        {
            ImagePersistent = new List<Bitmap>();
            BaseWorker = new VisualiserWorker(ImagePersistent);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("• Settings", "• S", "Settings for image Generation", GH_ParamAccess.item);
            pManager.AddBooleanParameter("• Generate", "• G", "Generate the images", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Images", "I", "Generated Images",GH_ParamAccess.list);
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
                return Resources.VIS_SDVisualiser;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E88FEE74-326A-442C-BF93-C8396E1479C1"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Cancel Current Generation", (s, e) =>
            {
                //TODO: Add cancellation logic for the server
                RequestCancellation();
            });

        }

        private class VisualiserWorker: WorkerInstance
        {
            List<Bitmap> Images;
            public ImageGenerationSetting ImgGenSettings { get; set; }

            public ImageRequest imageRequest { get; set; }
            public bool Generate { get; set; }
            

            public VisualiserWorker(List<Bitmap> imagePersistent): base(null) { this.Images = imagePersistent; }

            public override WorkerInstance Duplicate() => new VisualiserWorker(Images);

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                // Check if the operation has been cancelled
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                if (Generate)
                {
                    imageRequest = new ImageRequest(ImgGenSettings.ServerUrl, ImgGenSettings.Settings, ImgGenSettings.Client);
                    Task imageRequestTask = imageRequest.GenerateImage(GenerationMode.text2img);
                    imageRequestTask.Wait();
                    this.Images.AddRange(imageRequest.Images);

                }
                
                Done();
            }
            public override void SetData(IGH_DataAccess DA)
            {
                DA.SetDataList("Images", this.Images);
            }
            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                ImageGenerationSetting _imgGenSettings = null;
                DA.GetData("• Settings", ref _imgGenSettings);
                bool _generate = false;
                DA.GetData("• Generate", ref _generate);

                ImgGenSettings = _imgGenSettings;
                Generate = _generate;
            }
        }
    }
}