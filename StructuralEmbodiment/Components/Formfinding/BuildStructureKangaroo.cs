using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using StructuralEmbodiment.Core;
using StructuralEmbodiment.Core.Formfinding;

namespace StructuralEmbodiment.Components.Formfinding
{
    public class BuildStructureKangaroo : GH_Component
    {
        const double maxDistance = double.MaxValue;

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
            pManager.AddCurveParameter("• Edges", "• E", "Edges of the structure as tree, where each branch represent a continuous stripe of edges", GH_ParamAccess.tree);
            pManager.AddNumberParameter("• Forces", "• F", "Forces on each edge as tree corresponding to the structure of the Edges input", GH_ParamAccess.tree);
            pManager.AddCurveParameter("• Boundary", "• Bdy", "Boundary of the structure, lines or closed polylines", GH_ParamAccess.list);
            pManager.AddCurveParameter("• Borders", "• Bdr", "The hard border of the structure, it needs to be a subset of the boundary lines", GH_ParamAccess.list);
            pManager.AddPointParameter("Supports", "S", "Extra supports of the structure ", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Naked Supports", "NS", "True if the naked vertices of the mesh are supports. By default false", GH_ParamAccess.item,true);

            pManager[5].Optional = true;
            pManager[6].Optional = true;
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
            double tolerance = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            List<Member> members = new List<Member>();
            List<Point3d> nakedSupports = new List<Point3d>();
            var sortedMembers = new List<List<Member>>(); 


            //handling the inputs
            Mesh surfaceMesh = null;
            if(DA.GetData("• Mesh", ref surfaceMesh))
            {
                var nakedVertexStatus = surfaceMesh.GetNakedEdgePointStatus();
                nakedSupports = surfaceMesh.Vertices.Where((x, i) => nakedVertexStatus[i]).Select(x => (Point3d)x).ToList();
            }

            GH_Structure<GH_Curve> edgesGHStructure = new GH_Structure<GH_Curve>();
            List<List<Curve>> edges = new List<List<Curve>>();
            if(DA.GetDataTree("• Edges", out edgesGHStructure))
            {
                foreach (GH_Path path in edgesGHStructure.Paths)
                {
                    List<Curve> edgesInBranch = new List<Curve>();
                    foreach (GH_Curve line in edgesGHStructure.get_Branch(path))
                    {
                        edgesInBranch.Add(line.Value);
                    }
                    edges.Add(edgesInBranch);
                }

            }

            GH_Structure<GH_Number> forcesGHStructure = new GH_Structure<GH_Number>();
            List<List<double>> forces = new List<List<double>>();
            if (DA.GetDataTree("• Forces", out forcesGHStructure))
            {
                foreach (GH_Path path in forcesGHStructure.Paths)
                {
                    List<double> forcesInBranch = new List<double>();
                    foreach (GH_Number force in forcesGHStructure.get_Branch(path))
                    {
                        forcesInBranch.Add(force.Value);
                    }
                    forces.Add(forcesInBranch);
                }

            }

            //Compare if edges and forces have the same structure
            if (!(edges.Count == forces.Count && edges.Zip(forces, (subList1, subList2) => subList1.Count == subList2.Count).All(equal => equal)))
            {
                throw new Exception("The number of edges and forces in each branch of the Edges and Forces input needs to be the same");
            }

            List<LineCurve> boundaries = new List<LineCurve>();
            List<Curve> inputBoundary = new List<Curve>();
            if (DA.GetDataList("• Boundary", inputBoundary))
            {
                foreach (Curve curve in inputBoundary)
                {
                    if (curve is PolylineCurve && curve.IsClosed) {
                        var segments = ((PolylineCurve)curve).DuplicateSegments();
                        boundaries.AddRange(segments.OfType<LineCurve>());
                    }else if (curve is LineCurve)
                    {
                        boundaries.Add((LineCurve)curve);
                    } else throw new Exception("Boundary needs to be a closed polyline or a list of line curves");
                }
            }

            List<LineCurve> borders = new List<LineCurve>();
            List<Curve> inputBorder = new List<Curve>();
            if (DA.GetDataList("• Borders", inputBorder))
            {
                foreach (Curve curve in inputBorder)
                {
                    if (curve is LineCurve)
                    {
                        borders.Add((LineCurve)curve);
                    }
                    else throw new Exception("Borders needs to be line curves");
                }
            }

            List<List<Point3d>> sortedSupports = new List<List<Point3d>>();
            GH_Structure<GH_Point> supportsGHStructure = new GH_Structure<GH_Point>();
            if (DA.GetDataTree("Supports", out supportsGHStructure))
            {
                PointCloud meshVertices = new PointCloud(surfaceMesh.Vertices.Select(v=>(Point3d)v));
                foreach (GH_Path path in supportsGHStructure.Paths)
                {
                    List<Point3d> supportsInBranch = new List<Point3d>();
                    foreach (GH_Point support in supportsGHStructure.get_Branch(path))
                    {
                        int id = meshVertices.ClosestPoint(support.Value);
                        supportsInBranch.Add(meshVertices[id].Location);
                    }
                    sortedSupports.Add(supportsInBranch);
                }

            }

            bool includeNakedSupports = true;
            DA.GetData("Naked Supports", ref includeNakedSupports);

            List<Point3d> supports = new List<Point3d>();
            if (includeNakedSupports) { 
                supports.AddRange(nakedSupports); 
                supports.AddRange(sortedSupports.SelectMany(x => x));
            }
            else
            {
                 supports.AddRange(sortedSupports.SelectMany(x => x));
            }

            //create the members
            for (int i = 0; i < edges.Count; i++)
            {
                List<Curve> edgesInBranch = edges[i];
                List<double> forcesInBranch = forces[i];
                var membersInBranch = new List<Member>();
                for (int j = 0; j < edgesInBranch.Count; j++)
                {
                    Curve edge = edgesInBranch[j];
                    List<Point3d> edgeAsPoints = new List<Point3d>() {edge.PointAtStart,edge.PointAtEnd};
                    double force = forcesInBranch[j];
                    if (Util.DoesLineConnectPoints((LineCurve)edge, sortedSupports, tolerance))
                    {
                        membersInBranch.Add(new Member(-1, force, edgeAsPoints, MemberType.Generic, EdgeType.GenericEdge));
                    }
                    else
                    {
                        membersInBranch.Add(new Member(-1, force, edgeAsPoints, MemberType.RoofBeam, EdgeType.GenericEdge));
                    }
                }
                sortedMembers.Add(membersInBranch);
                members.AddRange(membersInBranch);
            }

            var structure = new Roof(members, supports,surfaceMesh, sortedMembers, sortedSupports, nakedSupports, boundaries, borders);
            DA.SetData("Structure", structure);

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
                return Properties.Resources.FF_BuildStructureKangaroo;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E892A382-840C-4B0F-89E7-D9682472182F"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }
    }
}