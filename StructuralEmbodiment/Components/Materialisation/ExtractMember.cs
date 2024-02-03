using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using StructuralEmbodiment.Core.Formfinding;

namespace StructuralEmbodiment.Components.Materialisation
{
    public class ExtractMember : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ExtractMember class.
        /// </summary>
        public ExtractMember()
          : base("ExtractMember", "EM",
              "Extract the gemometrical information from a member",
              "Structural Embodiment", "Materialisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("• Member", "• M", "The Member to extract information from", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("ID", "ID", "ID of the member", GH_ParamAccess.item);
            pManager.AddCurveParameter("Line", "L", "Line of the member", GH_ParamAccess.item);
            pManager.AddNumberParameter("Force", "F", "Internal force of the member", GH_ParamAccess.item);
            pManager.AddTextParameter("Member Type", "MT", "Member type of the member", GH_ParamAccess.item);
            pManager.AddTextParameter("Edge Type", "ET", "Edge type of the member (CEM)", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Member member = null;
            bool success = DA.GetData("• Member", ref member);
            if (success && member is Member)
            {
                Member m = member as Member;
                DA.SetData("ID", m.id);
                DA.SetData("Line", new LineCurve(m.EdgeAsPoints[0], m.EdgeAsPoints[1]));
                DA.SetData("Force", m.Force);
                DA.SetData("Member Type", m.MemberType);
                DA.SetData("Edge Type", m.EdgeType);
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
                return Properties.Resources.MAT_ExtractMember;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("60713226-6FC2-4D91-8F34-A161F7E126B0"); }
        }
    }
}