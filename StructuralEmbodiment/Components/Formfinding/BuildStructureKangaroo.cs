using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace StructuralEmbodiment.Components.Formfinding
{
    public class BuildStructureKangaroo : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BuildStructureKangaroo class.
        /// </summary>
        public BuildStructureKangaroo()
          : base("Build Structure Kangaroo", "BSK",
              "Build Structure from Kangaroo inputs",
              "Structural Embodiment", "Form-finding")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("• Mesh", "• M", "Mesh of the structure", GH_ParamAccess.item);
            pManager.AddLineParameter("• Edges", "• E", "Edges of the structure as tree, where each branch represent a continuous stripe of edges", GH_ParamAccess.tree);
            pManager.AddNumberParameter("• Forces", "• F", "Forces on each edge as tree corresponding to the structure of the Edges input", GH_ParamAccess.tree);
            pManager.AddCurveParameter("• Boundary", "• Bdy", "Boundary of the structure, lines or closed polylines", GH_ParamAccess.list);
            pManager.AddLineParameter("• Borders", "• Bdr", "The hard border of the structure, it needs to be a subset of the boundary lines", GH_ParamAccess.list);
            pManager.AddPointParameter("Supports", "S", "Extra supports of the structure ", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Naked Supports", "NS", "True if the naked vertices of the mesh are supports. By default false", GH_ParamAccess.item,true);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "S", "Structure object of Structural Embodiment assembly", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
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
            get { return new Guid("E892A382-840C-4B0F-89E7-D9682472182F"); }
        }
    }
}