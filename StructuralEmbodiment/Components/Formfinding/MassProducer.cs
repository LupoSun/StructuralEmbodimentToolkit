using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;

namespace StructuralEmbodiment.Components.Formfinding
{
    public class MassProducer : GH_Component
    {
        GH_Structure<IGH_Goo> data = new GH_Structure<IGH_Goo>();
        bool initialised = false;
        int structureID = 0;
        int viewID = 0;
        /// <summary>
        /// Initializes a new instance of the MassProducer class.
        /// </summary>
        public MassProducer()
          : base("MassProducer", "MP",
              "Mass produce data according to the Structural Embodiment Framework, work with the Trigger component",
              "Structural Embodiment", "Form-finding")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("• Data in", "DI", "Data in", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Structure Count", "nS", "Number of structures to produce", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("View Count", "nV", "Number of views to produce for each structure", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Production", "P", "Production", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Reset", "R", "Reset", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data out", "DO", "Data out", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Next View", "NV", "Next View", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Structure ID", "SID", "Current structure ID", GH_ParamAccess.item);
            pManager.AddIntegerParameter("View ID", "VID", "Current view ID", GH_ParamAccess.item);
            pManager.AddBooleanParameter("State", "S", "State", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // state increments by 1 each time the component is triggered
            //if (this.state != 0) this.state++;

            GH_Structure<IGH_Goo> volatileData = new GH_Structure<IGH_Goo>();
            int structureCount = 0;
            int viewCount = 0;
            bool production = false;
            bool reset = false;
            bool nextView = false;

            DA.GetDataTree("• Data in", out volatileData);
            DA.GetData("Structure Count", ref structureCount);
            DA.GetData("View Count", ref viewCount);
            DA.GetData("Production", ref production);
            DA.GetData("Reset", ref reset);

            GH_Structure<IGH_Goo> dataIn = volatileData.Duplicate();

            if (reset)
            {
                initialised = false;
                structureID = 0;
                viewID = 0;
                this.data = new GH_Structure<IGH_Goo>();
                reset = false;
                nextView = true;
            }

            if (!initialised)
            {
                this.data = dataIn;
            }

            if (this.structureID < structureCount && this.viewID <= viewCount)
            {
                if (production)
                {
                    nextView = true;
                    if (this.viewID < viewCount-1) {
                        
                        if (initialised) this.viewID++;
                    }
                    else
                    {
                        this.viewID = 0;
                        if(initialised) this.structureID++;
                        this.data = dataIn;
                    }
                }
                
                // Prevents the component outputting data after the last structure has been produced
                if(this.structureID < structureCount)
                {
                    DA.SetDataTree(0, this.data);
                    DA.SetData("Next View", nextView);
                    DA.SetData("Structure ID", structureID);
                    DA.SetData("View ID", viewID);
                    DA.SetData("State", this.initialised);
                }
                
                this.initialised = true;
            }

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
            get { return new Guid("5500868C-FF02-4229-94BA-A6EF00742391"); }
        }
    }
}