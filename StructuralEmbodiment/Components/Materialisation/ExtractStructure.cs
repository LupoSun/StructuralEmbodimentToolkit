using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using StructuralEmbodimentToolkit.Core.Formfinding;
using StructuralEmbodimentToolkit.Properties;

namespace StructuralEmbodimentToolkit.Components.Materialisation
{
    public class ExtractStructure : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ExtractStructure()
          : base("Extract Structure", "ES",
              "Extract the gemometrical information from a structure",
              "Structural Embodiment", "Materialisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("• Structure", "• S", "The Structure to extract information from", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Members", "M", "Members of the structure", GH_ParamAccess.tree);
            pManager.AddPointParameter("Supports", "S", "Supports of the structure", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Force Range", "FR", "Force Range of the structure", GH_ParamAccess.item);
            pManager.AddIntervalParameter("Force Range Unsigned", "FRU", "Force Range Unsigned of the structure", GH_ParamAccess.item);

            pManager.AddPointParameter("Deck Supports", "DS", "Deck Supports of the structure", GH_ParamAccess.list);
            pManager.AddPointParameter("Non Deck Supports", "NDS", "Non Deck Supports of the structure", GH_ParamAccess.list);
            pManager.AddPointParameter("Column Supports", "TS", "Tower Supports of the structure", GH_ParamAccess.list);
            pManager.AddCurveParameter("Deck Outlines", "DO", "Deck Outlines of the structure", GH_ParamAccess.list);
            pManager.AddCurveParameter("Trail Edges","TE", "Trail Edges of the structure", GH_ParamAccess.list);
            pManager.AddCurveParameter("Deviation Edges", "DE", "Deviation Edges of the structure", GH_ParamAccess.list);
            pManager.AddCurveParameter("Boundary", "B", "Boundary of the structure", GH_ParamAccess.list);
            pManager.AddCurveParameter("Borders", "BD", "Borders of the structure", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Mesh of the structure", GH_ParamAccess.item);
            //pManager.AddGenericParameter("test", "t", "test", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Structure structure = null;
            DA.GetData("• Structure", ref structure);

            
            //Bridge structure = input as Bridge;
            
            DA.SetData("Force Range", structure.ForceRange);
            DA.SetData("Force Range Unsigned", structure.ForceRangeUnsigned);
            
            if (structure is Bridge) {
                var pathMembers = new GH_Path(0);
                var treeMembers = new GH_Structure<IGH_Goo>();
                foreach (Member member in structure.Members)
                {
                    IGH_Goo ghGoo = GH_Convert.ToGoo(member);
                    treeMembers.Append(ghGoo, pathMembers);
                }
                DA.SetDataTree(0,treeMembers);

                DA.SetDataList("Supports", structure.Supports);
                DA.SetDataList("Deck Supports", ((Bridge)structure).DeckSupports);
                DA.SetDataList("Non Deck Supports", ((Bridge)structure).NonDeckSupports);
                //DA.SetDataList("Column Supports", ((Bridge)structure).TowerSupports);
                DA.SetDataList("Deck Outlines", ((Bridge)structure).DeckOutlines);
                DA.SetDataList("Trail Edges", ((Bridge)structure).TrailEdges);
                DA.SetDataList("Deviation Edges", ((Bridge)structure).DeviationEdges);
            } else if (structure is Roof)
            {
                
                Roof roof = (Roof)structure;
                
                var treeMembers = new GH_Structure<IGH_Goo>();
                for (int i = 0; i < roof.SortedMembers.Count; i++)
                {
                    var path = new GH_Path(i);
                    var branch = new List<IGH_Goo>();
                    foreach (Member item in roof.SortedMembers[i])
                    {
                        IGH_Goo ghGoo = GH_Convert.ToGoo(item);
                        if (ghGoo != null)
                        {
                            branch.Add(ghGoo);
                        }
                    }

                    treeMembers.AppendRange(branch, path);
                }
                DA.SetDataTree(0, treeMembers);
                DA.SetDataList("Supports", roof.Supports);
                DA.SetDataList("Boundary", roof.Boundary);
                DA.SetDataList("Borders", roof.Borders);

                var treeSortedSupports = new GH_Structure<IGH_Goo>();
                for (int i = 0; i < roof.SortedSupports.Count; i++)
                {
                    var path = new GH_Path(i);
                    var branch = new List<IGH_Goo>();
                    foreach (Point3d item in roof.SortedSupports[i])
                    {
                        IGH_Goo ghGoo = new GH_Point(item);
                        if (ghGoo != null)
                        {
                            branch.Add(ghGoo);
                        }
                    }

                    treeSortedSupports.AppendRange(branch, path);
                }
                DA.SetDataTree(6, treeSortedSupports);
                DA.SetData("Mesh", roof.SurfaceMesh);

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
                return Resources.MAT_ExtractStructure;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8FCA03FF-8C40-4692-AEEB-3C3542853FC5"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }
    }
}