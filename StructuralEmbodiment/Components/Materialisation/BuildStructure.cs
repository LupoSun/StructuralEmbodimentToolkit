using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using StructuralEmbodiment.Core.Materialisation;
using StructuralEmbodiment.Properties;

namespace StructuralEmbodiment.Components.Materialisation
{
    public class BuildStructure : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public BuildStructure()
          : base("Build Structure", "BS",
              "Build the structure object given data",
              "Structural Embodiment", "Materialisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("• Edge As Points", "• EP", "Each edge described as start and end points",GH_ParamAccess.tree);
            pManager.AddNumberParameter("• Forces", "• F", "Forces on each edge",GH_ParamAccess.list);
            pManager.AddBooleanParameter("• Deck Labels", "• DL", "True if the edge is a deck",GH_ParamAccess.list);
            pManager.AddBooleanParameter("• Trail Deviation Labels", "• TL", "Weather a edge is a trail",GH_ParamAccess.list);
            pManager.AddPointParameter("• Nodes", "• N", "Nodes of the structure",GH_ParamAccess.list);
            pManager.AddBooleanParameter("• Deck Node Labels", "• DNL", "True if the node is a deck node",GH_ParamAccess.list);
            pManager.AddBooleanParameter("• Support Labels", "• SL", "True if a Node is a support", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure","S","Structure object",GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Get the data
            GH_Structure<GH_Point> tree = new GH_Structure<GH_Point>();
            DA.GetDataTree("• Edge As Points", out tree);
            DataTree<Point3d> EdgeAsPoints = new DataTree<Point3d>();
            foreach (GH_Path path in tree.Paths)
            {
                List<GH_Point> points = (List<GH_Point>)tree.get_Branch(path);
                foreach (GH_Point ghPoint in points)
                {
                    EdgeAsPoints.Add(ghPoint.Value, path);
                }
            }

            List<double> forces = new List<double>();
            DA.GetDataList("• Forces", forces);
            List<bool> deckLabels = new List<bool>();
            DA.GetDataList("• Deck Labels", deckLabels);
            List<bool> trailDevLabels = new List<bool>();
            DA.GetDataList("• Trail Deviation Labels", trailDevLabels);
            List<Point3d> nodes = new List<Point3d>();
            DA.GetDataList("• Nodes", nodes);
            List<bool> deckNodeLabels = new List<bool>();
            DA.GetDataList("• Deck Node Labels", deckNodeLabels);
            List<bool> supportLabels = new List<bool>();
            DA.GetDataList("• Support Labels", supportLabels);


            List<Member> members = new List<Member>();
            for (int cemid = 0; cemid < EdgeAsPoints.BranchCount; cemid++)
            {
                List<Point3d> oneEdgeAsPoints = EdgeAsPoints.Branch(cemid);
                double force = forces[cemid];
                bool isDeck = deckLabels[cemid];
                bool isTrail = trailDevLabels[cemid];

                EdgeType edgeType;
                MemberType memberType;
                if (isTrail) edgeType = EdgeType.TrailEdge;
                else edgeType = EdgeType.DeviationEdge;

                if (isDeck) memberType = MemberType.Deck;
                else memberType = MemberType.NonDeck;

                members.Add(new Member(cemid, force, oneEdgeAsPoints, memberType, edgeType));

            }
            //remove the middle point
            List<Point3d> CEMsupports = nodes.Where((item, index) => supportLabels[index]).ToList();
            List<Point3d> DeckNodes = nodes.Where((item, index) => deckNodeLabels[index]).ToList();
            List<Point3d> supports = new List<Point3d>(CEMsupports);
            Util.RemoveClosestToCenter(supports);

            //find the deck support points
            List<Point3d> deckSupports = supports.Intersect(DeckNodes).ToList();
            List<Point3d> nonDeckSupports = supports.Except(DeckNodes).ToList();

            Bridge bridge = new Bridge(members, CEMsupports, deckSupports, nonDeckSupports);

            DA.SetData("Structure", bridge);


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
                return Resources.MAT_BuildStructure;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("028E66F1-73FA-4D1B-9C97-DD2C34963D87"); }
        }
    }
}