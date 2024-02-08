using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Data;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;

using Rhino.Geometry;
using Rhino.Display;
using Rhino;
using Grasshopper.Kernel.Types;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class BuildSegGuide : GH_Component 
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
        public BuildSegGuide()
            : base("Build Seg Guide", "SegG",
                "Building the segmentation guide for the image generation",
              "Structural Embodiment", "Visualisation")
        {
        }

        public override void CreateAttributes()
        {
            base.m_attributes = new BuildSegGuide_ButtonAttributes(this,"SE_Seg");
            
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
            pManager.AddGenericParameter("Seg Guide", "SegG", "Guides for image Generation", GH_ParamAccess.item);
            pManager.AddGenericParameter("Guiding Image", "GImg", "Guiding image", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //check if the display mode exists
            var existingMode = Rhino.Display.DisplayModeDescription.FindByName("SE_Seg");
            if (existingMode != null) this.Message = "Seg Display Mode ✓";
            else this.Message = "Please initialise the\nSeg Display Mode";

            if (buildDepthGuidePressed)
            {
                var capture = Core.Util.CaptureView();
                var segSetting = new Core.Visualisation.ControlNetSetting(capture);
                segSetting.SetSeg();
                DA.SetData("Seg Guide", segSetting);
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
                return Properties.Resources.VIS_BuildSegGuide;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C5FDA43D-D847-4315-BE29-CE694EBFD9CE"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }


        #endregion Methods of GH_Component interface


    }

    #region GH_ComponentAttributes interface

    public class BuildSegGuide_ButtonAttributes : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        GH_Component thisowner = null;
        System.Drawing.RectangleF ButtonBounds2;
        System.Drawing.RectangleF ButtonBounds3;
        string SegDisplayModeName;
        
        public BuildSegGuide_ButtonAttributes(GH_Component owner,string SegDisplayModeName) : base(owner) { 
            thisowner = owner; 
            this.SegDisplayModeName = SegDisplayModeName;
            
        }

        protected override void Layout()
        {
            base.Layout();

            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(this.Bounds);
            rec0.Height += (22 * 3+8);
            System.Drawing.Rectangle rec1 = rec0;
            System.Drawing.Rectangle rec2 = rec0;
            System.Drawing.Rectangle rec3 = rec0;
            rec1.Y = rec1.Bottom - 66 - 8   ;
            rec1.Height = 22;
            rec1.Inflate(-2, -2);
            Bounds = rec0;
            ButtonBounds = rec1;

            rec2.Y = rec2.Bottom - 44-8;
            rec2.Height = 22;
            rec2.Inflate(-2, -2);
            ButtonBounds2 = rec2;

            rec3.Y = rec3.Bottom - 22-8;
            rec3.Height = 22+8;
            rec3.Inflate(-2, -2);
            ButtonBounds3 = rec3;
        }
        private System.Drawing.Rectangle ButtonBounds { get; set; }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.White, "Clear Display Mode", new int[] {7,7,0,7 },4);
                button.Render(graphics, Selected, Owner.Locked, false);
                GH_Capsule button2 = GH_Capsule.CreateTextCapsule(ButtonBounds2, ButtonBounds2, GH_Palette.White, "Set Display Mode", new int[] { 7, 7, 0, 7 }, 4);
                button2.Render(graphics, Selected, Owner.Locked, false);
                GH_Capsule button3 = GH_Capsule.CreateTextCapsule(ButtonBounds3, ButtonBounds3, GH_Palette.Pink, "Build Seg Guide", new int[] { 7, 7, 0, 7 }, 4);
                button3.Render(graphics, Selected, Owner.Locked, false);

            }
        }
        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                System.Drawing.RectangleF rec = ButtonBounds;
                rec.Inflate(-4, -4);
                System.Drawing.RectangleF rec2 = ButtonBounds2;
                System.Drawing.RectangleF rec3 = ButtonBounds3;
                if (rec.Contains(e.CanvasLocation))
                {
                    var checkDisplayMode = Rhino.Display.DisplayModeDescription.FindByName(this.SegDisplayModeName);

                    Core.Util.SetDisplayModeToCurrentViewport("Shaded");
                    var displayMode = DisplayModeDescription.FindByName(this.SegDisplayModeName);
                    if (displayMode != null) DisplayModeDescription.DeleteDisplayMode(displayMode.Id);
                    base.Owner.ExpireSolution(true);
                    return GH_ObjectResponse.Handled;

                }
                else if(rec2.Contains(e.CanvasLocation))
                {
                    Core.Util.CreateSegDisplayMode(this.SegDisplayModeName);
                    Core.Util.SetDisplayModeToCurrentViewport(this.SegDisplayModeName);
                    base.Owner.ExpireSolution(true);
                    return GH_ObjectResponse.Handled;
                }
                else if (rec3.Contains(e.CanvasLocation))
                {
                    var currentDisplayMode = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.DisplayMode;
                    if (currentDisplayMode.EnglishName!= this.SegDisplayModeName)
                    {
                        MessageBox.Show("Current view is not set up correctly, please click on \"Set Display Mode\" button");
                    }
                    else
                    {
                        var owner = this.Owner as BuildSegGuide;
                        if (owner != null)
                        {
                            owner.buildDepthGuidePressed = true;
                            owner.ExpireSolution(true);
                        }
                    }       
                    
                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }
    }
    #endregion GH_ComponentAttributes interface
}