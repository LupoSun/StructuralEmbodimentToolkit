using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class SetCamera : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SetCamera class.
        /// </summary>
        public SetCamera()
          : base("SetCamera", "SC",
              "Set camera in the active Rhino viewport",
              "Structural Embodiment", "Visualisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("• Camera Target", "CT", "Target of the camera", GH_ParamAccess.item);
            pManager.AddPointParameter("• Camera Position", "CL", "Location of the camera", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Lens Length", "LL", "Lens length of the camera, by defult 50mm", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Set Camera", "SC", "Set the camera", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Camera Direction", "CD", "Direction of the camera", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d cameraPosition = new Point3d();
            Point3d cameraTarget = new Point3d();
            int lensLength = 15;
            bool setCamera = false;


            bool s0 = DA.GetData("• Camera Target", ref cameraTarget);
            bool s1 = DA.GetData("• Camera Position", ref cameraPosition);
            bool s2 = DA.GetData("Lens Length", ref lensLength);
            bool s3 = DA.GetData("Set Camera", ref setCamera);
            if (s0 && s1 && s3)
            {
                if (setCamera)
                {
                    Core.Visualisation.Util.RedrawView(cameraPosition, cameraTarget, lensLength);
                    setCamera = false;
                }

                Vector3d cameraDirection = cameraTarget - cameraPosition;

                DA.SetData("Camera Direction", cameraDirection);
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
            get { return new Guid("6FD466A8-BE64-48F3-9BE7-62AE7DBA32B4"); }
        }
    }
}