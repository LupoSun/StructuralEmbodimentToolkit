﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using StructuralEmbodiment.Core.Materialisation;
using System.Linq;

namespace StructuralEmbodiment.Components.Materialisation
{
    public class TerrainMaker : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TerrainMaker class.
        /// </summary>
        public TerrainMaker()
          : base("TerrainMaker", "TM",
              "Make tailored terrain for a structure",
              "Structural Embodiment", "Materialisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "S", "Structure to be embeded with the terrain", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "W", "Width of the terrain", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "L", "Length of the terrain", GH_ParamAccess.item);
            pManager.AddNumberParameter("Depth", "D", "Depth of the trench", GH_ParamAccess.item);
            pManager.AddNumberParameter("Slope", "SL", "Slope of the trench", GH_ParamAccess.item);

            //pManager[0].Optional = true;
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
            pManager.AddBrepParameter("Terrain", "T", "Terrain for the structure", GH_ParamAccess.list);
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            Structure structure = null;
            DA.GetData("Structure", ref structure);

            double width; 
            double length; 
            double depth;
            double slope;
    

            if (structure != null && structure is Bridge)
            {
                List<Point3d> deckStartPoints = new List<Point3d>();
                List<Point3d> deckEndPoints = new List<Point3d>();
                List<Point3d> nonDeckStartPoints = new List<Point3d>();
                List<Point3d> nonDeckEndPoints = new List<Point3d>();

                List<Curve> sections = new List<Curve>();
                List<Brep> terrains = new List<Brep>();
                Terrain terrain = new Terrain();

                Dictionary<Point3d, List<Member>> connectedMembersDict = Bridge.FindConncectedTrails(((Bridge)structure).Supports, ((Bridge)structure).Members);
                Dictionary<Point3d, List<Member>> bridgedConnectedMembersDict = ((Bridge)structure).BridgeTrailGaps(connectedMembersDict,tolerance * 10);
                
                foreach(KeyValuePair<Point3d, List<Member>> kvp in bridgedConnectedMembersDict)
                {
                    Point3d pt = kvp.Key;
                    List<Member> members = kvp.Value;
                    List<Point3d> membersPts = members.SelectMany(m => m.EdgeAsPoints).ToList();
                    membersPts = Point3d.SortAndCullPointList(membersPts, tolerance).ToList();
                    if (members.First().MemberType == MemberType.Deck)
                    {
                        deckStartPoints.Add(membersPts.First());
                        deckEndPoints.Add(membersPts.Last());
                    }
                    else { 
                        nonDeckStartPoints.Add(membersPts.First());
                        nonDeckEndPoints.Add(membersPts.Last());
                    }

                }

                

                //Setting the optional parameters
                width = Util.AveragePoint(deckStartPoints).DistanceTo(Util.AveragePoint(deckEndPoints));
                length = width;
                depth = width / 2;
                slope = width / 4;
                DA.GetData("Width", ref width);
                DA.GetData("Length", ref length);
                DA.GetData("Depth", ref depth);
                DA.GetData("Slope", ref slope);

                //Point3d midPoint = (Util.AveragePoint(deckStartPoints) + Util.AveragePoint(deckEndPoints)) / 2;
                //midPoint += (-Vector3d.ZAxis * depth*2);

                sections.AddRange(terrain.GenerateTerrain(deckStartPoints, deckEndPoints, nonDeckStartPoints, nonDeckEndPoints,
                    width, length, depth, slope, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis));
                terrain.LoftTerrain((-Vector3d.ZAxis * depth * 6), tolerance);

                DA.SetDataList("Terrain", Brep.JoinBreps(terrain.TerrainBreps, tolerance));
            }

            


            //DA.SetDataList("(Sections)", new Point3d[]{ deckStartPoints[0], Point3d.Unset, averageNotDeckStart});
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
            get { return new Guid("7C375D8D-A9DC-49B3-B751-965D704061E5"); }
        }
    }
}