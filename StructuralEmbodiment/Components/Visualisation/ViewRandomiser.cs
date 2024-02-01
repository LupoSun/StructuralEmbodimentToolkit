using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using StructuralEmbodiment.Core.Formfinding;
using StructuralEmbodiment.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class ViewRandomiser : GH_Component
    {
        Point3d camera = new Point3d(0, 0, 0);
        Point3d cameraTarget = new Point3d(0, 0, 0);
        Random rnd = new Random();

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ViewRandomiser()
          : base("View Randomiser", "VR",
              "Set random views around the centre point",
              "Structural Embodiment", "Visualisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Centre", "C", "A Structure or a manual point definition of the visual centre", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Eye Level", "EL", "Only set views with eye-level height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius", "RA", "Radius of the view circle", GH_ParamAccess.item);
            pManager.AddAngleParameter("Rotate", "RO", "Rotate the view area ()", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Visualise Area", "VA", "Visualise the view area", GH_ParamAccess.item);
            pManager.AddIntervalParameter("Longitude Range", "LR", "Longitudinal view range, from 0 to 180 degree", GH_ParamAccess.item);
            pManager.AddIntervalParameter("Latitude Range", "LT", "Latitude view range, from 0 to 180 degree", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Next View", "NV", "Refresh the next view", GH_ParamAccess.item);


            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            //pManager[8].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("View Areas", "VA", "View Areas for possible camera positions", GH_ParamAccess.list);
            pManager.AddGenericParameter("Camera Target", "CT", "Target of the camera", GH_ParamAccess.list);
            pManager.AddGenericParameter("Camera Position", "C", "Position of the camera", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Initialise data needed
            List<Brep> viewAreas = new List<Brep>();
            List<Curve> viewCurves = new List<Curve>();

            double eyeLevelHeight = 1.6;


            // Default values for inputs

            object centreFromInput = null;
            bool eyelevel = false;
            //int lensLength = 50;


            double radius = 1;
            double rotate = 0;
            bool visualiseArea = false;
            Interval longitudeRange = new Interval(0, 180);
            Interval latitudeRange = new Interval(1, 179);
            bool nextView = false;

            DA.GetData("Eye Level", ref eyelevel);
            if (!eyelevel) eyeLevelHeight = 0;
            //Get the data and do operations based on the input type
            DA.GetData("Centre", ref centreFromInput);
            if (centreFromInput is Point3d)
            {
                cameraTarget = (Point3d)centreFromInput;
            }
            else if (centreFromInput is GH_ObjectWrapper)
            {
                centreFromInput = ((GH_ObjectWrapper)centreFromInput).Value;
                if (centreFromInput is Structure)
                {
                    List<Point3d> pts = ((Structure)centreFromInput).Supports;
                    cameraTarget = new Point3d(
                        pts.Average(pt => pt.X),
                        pts.Average(pt => pt.Y),
                        pts.Average(pt => pt.Z));
                    double maxZ = pts.Max(pt => pt.Z);
                    cameraTarget = new Point3d(cameraTarget.X, cameraTarget.Y, maxZ + eyeLevelHeight);
                    if (centreFromInput is Bridge)
                    {
                        radius = ((Bridge)centreFromInput).DeckOutlines[0].PointAtStart.DistanceTo(((Bridge)centreFromInput).DeckOutlines[0].PointAtEnd);
                    }
                }
            }

            //DA.GetData("Lens Length", ref lensLength);
            DA.GetData("Radius", ref radius);
            if (radius <= 0) radius = 1;
            DA.GetData("Rotate", ref rotate);
            DA.GetData("Visualise Area", ref visualiseArea);
            DA.GetData("Longitude Range", ref longitudeRange);
            DA.GetData("Latitude Range", ref latitudeRange);
            DA.GetData("Next View", ref nextView);

            //Normalise and clamp the longitude and latitude range
            longitudeRange = StructuralEmbodiment.Core.Util.AdjustIntervalTo180(longitudeRange);
            Interval longitudeRange2 = new Interval(longitudeRange.Min + 180, longitudeRange.Max + 180);
            latitudeRange = StructuralEmbodiment.Core.Util.AdjustIntervalTo180(latitudeRange);

            //Create a the longitudinal arcs for the view area
            Circle lonCirc = new Circle(new Plane(cameraTarget, Vector3d.ZAxis), radius);
            
            lonCirc.Rotate(Core.Util.DegreesToRadians(rotate), Vector3d.ZAxis, cameraTarget);
            Arc lonArc1 = new Arc(lonCirc, Core.Util.DegreesToRadiansInterval(longitudeRange));
            Curve lonCrv1 = lonArc1.ToNurbsCurve();
            lonCrv1.Domain = new Interval(0, 1);

            Arc lonArc2 = new Arc(lonCirc, Core.Util.DegreesToRadiansInterval(longitudeRange2));
            Curve lonCrv2 = lonArc2.ToNurbsCurve();
            lonCrv2.Domain = new Interval(0, 1);

            //Operation based on if the view area is eye-level or not
            if (eyelevel)
            {
                Plane projectionPlane = new Plane(cameraTarget, Vector3d.ZAxis); // XY plane at the given point

                foreach (Curve crv in new Curve[] { lonCrv1, lonCrv2 })
                {
                    Curve projectedCurve = Curve.ProjectToPlane(crv, projectionPlane);
                    if (projectedCurve != null)
                    {
                        viewCurves.Add(projectedCurve);
                    }
                }



            }
            else
            {
                //Compute the latitudinal arcs
                Plane latPl1 = new Plane(cameraTarget, cameraTarget + new Vector3d(0, 0, 1), lonCrv1.PointAtStart);
                Circle latCirc1 = new Circle(latPl1, radius);
                Arc latArc1 = new Arc(latCirc1, Core.Util.DegreesToRadiansInterval(latitudeRange));
                Curve latCrv1 = latArc1.ToNurbsCurve();
                viewAreas.AddRange(new SweepOneRail().PerformSweep(lonCrv1, latCrv1));

                Plane latPl2 = new Plane(cameraTarget, cameraTarget + new Vector3d(0, 0, 1), lonCrv2.PointAtStart);
                Circle latCirc2 = new Circle(latPl2, radius);
                Arc latArc2 = new Arc(latCirc2, Core.Util.DegreesToRadiansInterval(latitudeRange));
                Curve latCrv2 = latArc2.ToNurbsCurve();
                viewAreas.AddRange(new SweepOneRail().PerformSweep(lonCrv2, latCrv2));

            }
            //List<object> test = new List<object>() { lonCrv1, lonCrv2, latCrv1, latCrv2 };
            //List<object> test2 = new List<object>() { };

            if (visualiseArea)
            {
                if (eyelevel) DA.SetDataList("View Areas", viewCurves);
                else DA.SetDataList("View Areas", viewAreas);

            }

            if (eyelevel)
            {
                //Randomly select a curve 
                int curveIndex = rnd.Next(viewCurves.Count);
                Curve selectedCurve = viewCurves[curveIndex];
                //Randomly select a point on the curve
                double t;
                selectedCurve.LengthParameter(rnd.NextDouble() * selectedCurve.GetLength(), out t);
                Point3d randomPointOnCurve = selectedCurve.PointAt(t);

                if (!nextView)camera = randomPointOnCurve;
                nextView = false;
            }
            else
            {
                if (!nextView) camera = Core.Util.SampleRandomPointOnBreps(viewAreas, rnd);
                nextView = false;
            }
            //Core.Visualisation.Util.RedrawView(camera, cameraTarget, lensLength);
            //nextView = false;  
            DA.SetDataList("Camera Target", new List<Point3d>() { cameraTarget });
            DA.SetDataList("Camera Position", new List<Point3d>() { camera });



            //DA.SetDataList("(Test)", test);
            //DA.SetDataList("(Test2)", test2);

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
                return Resources.VIS_ViewRandomiser;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("459DB466-8C62-4477-A612-9FA63882C195"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }
    }
}