using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class BuildDepthGuide : GH_Component
    {
        public bool buildDepthGuidePressed = false;

        #region Methods of GH_Component interface

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BuildDepthGuide()
            : base("Build Depth Guide", "DptG",
                "Building the depth guide for the image generation",
              "Structural Embodiment", "Visualisation")
        {
        }

        public override void CreateAttributes()
        {
            base.m_attributes = new BuildDepthGuide_ButtonAttributes(this);

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Depth Guide", "DptG", "Guides for image Generation", GH_ParamAccess.item);
            pManager.AddGenericParameter("Guiding Image", "GImg", "Guiding image", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            this.Message = "Ready to Capture ✓";

            if (buildDepthGuidePressed)
            {
                var capture = Core.Util.CaptureDepthView();
                var depthSetting = new Core.Visualisation.ControlNetSetting(capture);
                depthSetting.SetDepthMap();
                DA.SetData("Depth Guide", depthSetting);
                DA.SetData("Guiding Image", capture);
                buildDepthGuidePressed = false;
            }

        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("49112463-E60B-4B3B-92A2-3D1EB916A007"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }


        #endregion Methods of GH_Component interface


    }

    #region GH_ComponentAttributes interface

    public class BuildDepthGuide_ButtonAttributes : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        GH_Component thisowner = null;

        public BuildDepthGuide_ButtonAttributes(GH_Component owner) : base(owner)
        {
            thisowner = owner;

        }

        protected override void Layout()
        {
            base.Layout();

            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(this.Bounds);
            rec0.Height += (22 * 1 + 8);
            System.Drawing.Rectangle rec1 = rec0;
            rec1.Y = rec1.Bottom - 22 - 8;
            rec1.Height = 22 + 8;
            rec1.Inflate(-2, -2);

            Bounds = rec0;
            ButtonBounds = rec1;

        }
        private System.Drawing.Rectangle ButtonBounds { get; set; }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Pink, "Build Depth Guide", new int[] { 7, 7, 0, 7 }, 4);
                button.Render(graphics, Selected, Owner.Locked, false);

            }
        }
        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                System.Drawing.RectangleF rec = ButtonBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    var owner = this.Owner as BuildDepthGuide;
                    if (owner != null)
                    {
                        owner.buildDepthGuidePressed = true;
                        owner.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }
    }
    #endregion GH_ComponentAttributes interface
}