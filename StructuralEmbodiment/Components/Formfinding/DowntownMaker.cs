using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using StructuralEmbodiment.Core.Formfinding;
using StructuralEmbodiment.Core.Materialisation;
using System;
using System.Collections.Generic;

namespace StructuralEmbodiment.Components.Formfinding
{
    public class DowntownMaker : GH_Component
    {
        double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
        double angleTolerance = RhinoDoc.ActiveDoc.ModelAngleToleranceRadians;
        /// <summary>
        /// Initializes a new instance of the DowntownMaker class.
        /// </summary>
        public DowntownMaker()
          : base("DowntownMaker", "DM",
              "Given Boundary and borders, create a downtown setting for the structure",
              "Structural Embodiment", "Materialisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("• Structure", "• S", "The Structure to be embedded in the downtown settings", GH_ParamAccess.item);
            pManager.AddNumberParameter("Structure Height", "SH", "THe height of the buildings", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Site Height", "GH", "Height of the houses", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Gap Width", "GW", "Width of the gaps between the houses", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Number", "N", "Number of the houses", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Auxiliary Structure", "AS", "Create auxiliary structures", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("To 3D", "3D", "Create solid 3D geometry", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Saddle Roof", "SR", "Add saddle room on the site", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Buildings", "B", "Created Houses", GH_ParamAccess.list);
            pManager.AddBrepParameter("Auxiliary Structure", "AS", "Auxiliary Structure", GH_ParamAccess.list);
            pManager.AddBrepParameter("Ground", "G", "Ground for the downtown", GH_ParamAccess.list);
            pManager.AddGenericParameter("test", "t", "test", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Structure structure = null;
            DA.GetData("• Structure", ref structure);
            double structureHeight = 10;
            DA.GetData("Structure Height", ref structureHeight);
            double siteHeight = 10;
            DA.GetData("Site Height", ref siteHeight);
            double gapWidth = 1;
            DA.GetData("Gap Width", ref gapWidth);
            int number = 0;
            DA.GetData("Number", ref number);
            bool auxiliaryStructure = true;
            DA.GetData("Auxiliary Structure", ref auxiliaryStructure);
            bool to3D = false;
            DA.GetData("To 3D", ref to3D);
            bool saddleRoof = false;
            DA.GetData("Saddle Roof", ref saddleRoof);


            if (structure is Roof)
            {
                Roof roof = (Roof)structure;
                var boundary = Curve.JoinCurves(roof.Boundary);
                var houses = House.CreateHousesFromBorders(roof.Borders, (PolylineCurve)boundary[0], tolerance);

                var outlines = new List<PolylineCurve>();
                var brepHouses = new List<Brep>();
                var uniqueHouses = new List<House>();

                var centrePt = Core.Util.AveragePoint(new List<Point3d>(((PolylineCurve)boundary[0]).ToPolyline()));

                //Get the initial set of anchor points for comparison
                var initialAnchorPts = new List<Point3d>();
                foreach (var house in houses)
                {
                    bool isUnique = true;
                    foreach (var pt in initialAnchorPts)
                    {
                        if (pt.DistanceTo(house.AnchorPt) <= tolerance)
                        {
                            isUnique = false;
                            break;
                        }
                    }
                    if (isUnique)
                    {
                        initialAnchorPts.Add(house.AnchorPt);
                        uniqueHouses.Add(house);
                    }
                    uniqueHouses.AddRange(House.MultiplyHouse(house, number, gapWidth));
                }

                // Creating the Ground
                var groundPlane = new Plane(centrePt + new Vector3d(0, 0, -structureHeight), Vector3d.ZAxis);
                var planeWidth = boundary[0].GetBoundingBox(groundPlane).Diagonal.X * 10;
                Curve rectCurve = new Rectangle3d(groundPlane, new Interval(-planeWidth, planeWidth), new Interval(-planeWidth, planeWidth)).ToNurbsCurve();
                var ground = Brep.CreatePlanarBreps(rectCurve, tolerance);
                DA.SetDataList("Ground", ground);

                if (to3D)
                {
                    foreach (var house in uniqueHouses)
                    {
                        brepHouses.AddRange(house.LoftHouse(structureHeight, siteHeight, saddleRoof,tolerance,angleTolerance));
                    }
                    DA.SetDataList("Buildings", brepHouses);

                }
                else
                {

                    foreach (var house in uniqueHouses)
                    {
                        outlines.Add(house.Outline);
                    }
                    DA.SetDataList("Buildings", outlines);

                }


                

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
                return Properties.Resources.MAT_DowntownMaker;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1DFB8C68-502E-42E3-851A-5FD6C15DF743"); }
        }
    }
}