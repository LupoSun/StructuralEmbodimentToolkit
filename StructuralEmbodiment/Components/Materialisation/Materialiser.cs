using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using StructuralEmbodiment.Core.Materialisation;

namespace StructuralEmbodiment.Components.Materialisation
{
    public class Materialiser : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Materialiser class.
        /// </summary>
        public Materialiser()
          : base("Materialiser", "MAT",
              "Materialise structures",
              "Structural Embodiment", "Materialisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "S", "The Structure to materialise", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Cross Section", "CS", "Cross Section of the members. O=Circular, 1=Rectangular", GH_ParamAccess.item);
            pManager.AddNumberParameter("Multiplier", "M", "Multiplier for the cross section", GH_ParamAccess.item);
            pManager.AddIntervalParameter("Range", "R", "Range of the cross section", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Result", "R", "Resulting Brep", GH_ParamAccess.list);
            pManager.AddGenericParameter("test", "t", "test", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Structure structure = null;
            DA.GetData("Structure", ref structure);
            int crossSection = 0;
            DA.GetData("Cross Section", ref crossSection);
            double multiplier = 1.0;
            DA.GetData("Multiplier", ref multiplier);
            Interval range = new Interval(structure.ForceRange[0], structure.ForceRange[1]);
            DA.GetData("Range", ref range);

            if (structure is Bridge)
            {
                Dictionary<Point3d, List<Member>> connectedMembersDict = ((Bridge)structure).FindConncectedTrails();

                //List<LineCurve> lines = connectedMembersDict.First().Value.Select(m => new LineCurve(m.EdgeAsPoints[0], m.EdgeAsPoints[1])).ToList();
                List<Plane> pls = ((Bridge)structure).ComputePerpendicularPlanesAtPoints(connectedMembersDict.First().Value);
                DA.SetDataList("test", pls);
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
            get { return new Guid("61B435BB-AFF1-4D23-A626-63C88A70D0D1"); }
        }
    }
}