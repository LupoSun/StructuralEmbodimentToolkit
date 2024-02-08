using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using StructuralEmbodiment.Core;
using StructuralEmbodiment.Core.Visualisation;
using StructuralEmbodiment.Properties;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class BuildImgGenGuides_Legacy : GH_Component
    {
        ControlNetSetting controlNetSettings;
        List<Bitmap> images;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public BuildImgGenGuides_Legacy()
          : base("Build Guidances_Legacy", "GenGuides_L",
              "Building the guidances for the image generation",
              "Structural Embodiment", "Visualisation")
        {
            this.controlNetSettings = new ControlNetSetting();
            this.images = new List<Bitmap>();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Guide by Curves", "GD", "Guide the image generation through depths", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Guide by Depth", "GC", "Guide the image generation through curves", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Build", "B", "Build the guidanes", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Guides", "G", "Guides for image Generation", GH_ParamAccess.item);
            pManager.AddGenericParameter("Images", "Img", "Guiding images", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
         
            bool canny = false;
            bool depthmap = false;
            bool build = false;
            DA.GetData("Guide by Curves", ref canny);
            DA.GetData("Guide by Depth", ref depthmap);
            DA.GetData("Build", ref build);

            if (build)
            {
                //clear the settings
                this.images.Clear();
                this.controlNetSettings = new ControlNetSetting();
                /*
                this.controlNetSettings.CannySourceImage = null;
                this.controlNetSettings.DepthMapSourceImage = null;
                this.controlNetSettings.CannySettings = null;
                this.controlNetSettings.DepthMapSettings = null;
                */
                if (canny)
                {
                    controlNetSettings.SetCanny();
                    images.Add(controlNetSettings.CannySourceImage);
                }
                if (depthmap)
                {
                    controlNetSettings.SetDepthMap();
                    images.Add(controlNetSettings.DepthMapSourceImage);
                }
            }

            DA.SetData("Guides", SDWebUISetting.Instance);
            
            //DA.SetData("Guides", controlNetSettings);
            DA.SetDataList("Images", images);

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
                return Util.ConvertToGrayscale(new Bitmap(Properties.Resources.VIS_BuildGuidance));
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D3F5708B-0F34-4330-85CF-1DC2AE977280"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.septenary; }
        }
    }
}