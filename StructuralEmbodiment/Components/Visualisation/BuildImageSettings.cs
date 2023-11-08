using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using StructuralEmbodiment.Core.GrasshopperAsyncComponent;
using StructuralEmbodiment.Core.Visualisation;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class BuildImageSettings : GH_AsyncComponent
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public BuildImageSettings()
          : base("Build Image Settings", "ImgSetting",
              "Compile the settings for the image generation",
              "Structural Embodiment", "Visualisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5BCF32B0-45B5-45B3-9F21-41D5570F063D"); }
        }


    }
}