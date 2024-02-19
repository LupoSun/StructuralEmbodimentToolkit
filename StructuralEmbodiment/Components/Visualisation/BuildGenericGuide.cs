using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System;

namespace StructuralEmbodimentToolkit.Components.Visualisation
{
    public class BuildGenericGuide : GH_Component
    {
        public bool builGuidePressed = false;

        #region Methods of GH_Component interface

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BuildGenericGuide()
            : base("Build Generic Guide", "GG",
                "Building the genric guide for the image generation",
              "Structural Embodiment", "Visualisation")
        {
        }

        public override void CreateAttributes()
        {
            base.m_attributes = new BuildGenericGuide_ButtonAttributes(this);

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("• Preprocessor", "• PP", "Preprocessor for the guiding image", GH_ParamAccess.item);
            pManager.AddTextParameter("• CN Model", "CNM", "ControlNet model", GH_ParamAccess.item);
            pManager.AddNumberParameter("Control Weight", "CW", "Control Weight", GH_ParamAccess.item,1);

            pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Guide", "G", "Guides for image Generation", GH_ParamAccess.item);
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

            if (builGuidePressed)
            {
                string preprocessor = "";
                bool s1 = DA.GetData("• Preprocessor", ref preprocessor);
                string cNModel = "";
                bool s2 = DA.GetData("• CN Model", ref cNModel);
                double controlWeight = 1;
                DA.GetData("Control Weight", ref controlWeight);
                if (s1 && s2)
                {
                    var capture = Core.Util.CaptureView();
                    var genericSetting = new Core.Visualisation.ControlNetSetting(capture);
                    genericSetting.SetGenericGuide(preprocessor,cNModel,controlWeight);
                    DA.SetData("Guide", genericSetting);
                    DA.SetData("Guiding Image", capture);
                    builGuidePressed = false;
                    this.Message = "Guide Generated ✓";
                }else this.Message = "Please provide the\nPreprocessor and CN Model";
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
                return Properties.Resources.VIS_BuildGenericGuide;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("194FE162-F4AA-465C-8288-F6F8A7C27E59"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }


        #endregion Methods of GH_Component interface


    }

    #region GH_ComponentAttributes interface

    public class BuildGenericGuide_ButtonAttributes : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        GH_Component thisowner = null;

        public BuildGenericGuide_ButtonAttributes(GH_Component owner) : base(owner)
        {
            thisowner = owner;

        }

        protected override void Layout()
        {
            base.Layout();
            var buttonWidth = 120;
            var bezel = 5;

            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(this.Bounds);
            rec0.Height += (22 * 1 + 8+bezel);
            System.Drawing.Rectangle rec1 = rec0;
            rec1.Y = rec1.Bottom - 22 - 8 - bezel;
            rec1.Height = 22 + 8;
            rec1.Width = buttonWidth;
            var x = rec0.Right - rec0.Width / 2 - rec1.Width / 2;
            rec1.X = x;
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
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Pink, "Build Generic Guide", new int[] { 7, 7, 0, 7 }, 4);
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
                    var owner = this.Owner as BuildGenericGuide;
                    if (owner != null)
                    {
                        owner.builGuidePressed = true;
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