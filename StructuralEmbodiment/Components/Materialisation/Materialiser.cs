using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Cryptography;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using StructuralEmbodiment.Core.Materialisation;
using static Rhino.DocObjects.PhysicallyBasedMaterial;

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
            pManager.AddBooleanParameter("Include Deck Edges", "IDE", "Materialise the deck edges", GH_ParamAccess.item);

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
            pManager.AddBrepParameter("Cables", "C", "Cables of a structure", GH_ParamAccess.list);
            pManager.AddBrepParameter("Bars","B","Bars of a structure",GH_ParamAccess.list);
            pManager.AddBrepParameter("Deck", "D", "Deck of a bridge", GH_ParamAccess.list);
            pManager.AddBrepParameter("Arches", "A", "Arches of a bridge", GH_ParamAccess.list);
            pManager.AddBrepParameter("Towers", "T", "Towers of a bridge", GH_ParamAccess.list);

            pManager.AddGenericParameter("(test)", "t", "test", GH_ParamAccess.list);
            pManager.AddGenericParameter("(test2)", "t2", "test2", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Structure structure = null;
            DA.GetData("Structure", ref structure);
            int crossSection = 1;
            DA.GetData("Cross Section", ref crossSection);
            double multiplier = 1.0;
            DA.GetData("Multiplier", ref multiplier);
            Interval range = new Interval(structure.ForceRangeUnsigned[0], structure.ForceRangeUnsigned[1]);
            DA.GetData("Range", ref range);
            bool includeDeckEdges = false;
            DA.GetData("Include Deck Edges", ref includeDeckEdges);

            if (structure is Bridge)
            {
                double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
                var supports = new List<Point3d>(((Bridge)structure).Supports);
                Util.RemoveClosestToCenter(supports);
                Dictionary<Point3d, List<Member>> connectedMembersDict = Bridge.FindConncectedTrails(supports,((Bridge)structure).Members);

                List<Brep> deck = new List<Brep>();
                deck.AddRange(((Bridge)structure).LoftDeck(multiplier, range));
                DA.SetDataList("Deck", deck);

                List<Brep> trails = ((Bridge)structure).LoftTrails(connectedMembersDict, crossSection, multiplier, range, includeDeckEdges, tolerance * 10);
                DA.SetDataList("Arches", trails);

                List<List<Brep>> devs = ((Bridge)structure).LoftDeviations(crossSection, multiplier, range, tolerance * 10);
                List<Brep> cables = devs[0];
                List<Brep> bars = devs[1];
                DA.SetDataList("Cables", cables);
                DA.SetDataList("Bars", bars);

                List<Brep> tower = new List<Brep>();
                if (((Bridge)structure).TowerSupports != null) {
                    tower = ((Bridge)structure).LoftTower(crossSection, multiplier, range, tolerance);
                }
                DA.SetDataList("Towers", tower);

                
                List<LineCurve> lcrv = new List<LineCurve>();
              

                DA.SetDataList("(test)", lcrv);
                DA.SetDataList("(test2)", ((Bridge)structure).DeckOutlines);
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