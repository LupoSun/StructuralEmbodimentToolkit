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

using StructuralEmbodiment.Core.Visualisation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class SDInitialiser : GH_Component
    {
        public bool startWebUIPressed = false;
        public bool refreshStatusPressed = false;

        #region Methods of GH_Component interface
        /// <summary>
        /// Initializes a new instance of the SDInitialiser class.
        /// </summary>
        public SDInitialiser()
          : base("SD Initialiser", "SDI",
              "Initialise the StableDiffusion WebUI by AUTOMATIC1111 with api enabled",
              "Structural Embodiment", "Visualisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("• SD Location", "• SD", "Path to StableDiffusion WebUI folder", GH_ParamAccess.item);
            pManager.AddTextParameter("User URL", "CA", "Customised Adress to access the api. By default http://127.0.0.1:7860", GH_ParamAccess.item);
            pManager.AddTextParameter("Additional Arguments", "AA", "Additional Arguments for the WebUI, refer to https://github.com/AUTOMATIC1111/stable-diffusion-webui/wiki/Command-Line-Arguments-and-Settings#all-command-line-arguments", GH_ParamAccess.list);

            pManager[0].Optional=true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string sdLocation = "";
            DA.GetData("• SD Location", ref sdLocation);
            string userURL = "http://127.0.0.1:7860";
            DA.GetData("User URL", ref userURL);
            List<string> additionalArguments = new List<string>();
            DA.GetDataList("Additional Arguments", additionalArguments);

            var setting = SDWebUISetting.Instance;
            //setting.SetSDWebUISetting(null);
            if (setting.ServerURL != null)
            {
                
                if (refreshStatusPressed)
                {
                    setting.SetSDWebUISetting(userURL);
                    refreshStatusPressed = false;
                }
                this.Message = "WebUI is already initialised \n"+setting.ToString();
            }
            else
            {
                if(startWebUIPressed)
                {
                    setting.SetSDWebUISetting(userURL);
                    startWebUIPressed = false;
                    this.Message = "WebUI is already initialised \n" + setting.ToString();
                }
                
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
            get { return new Guid("EBDA4485-E8C3-4691-8625-C289677556F9"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        public override void CreateAttributes()
        {
            base.m_attributes = new SDInitialiser_ButtonAttributes(this);
        }
    }
    #endregion
    #region GH_ComponentAttributes interface

    public class SDInitialiser_ButtonAttributes : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        GH_Component thisowner = null;
        System.Drawing.RectangleF ButtonBounds2;

        public SDInitialiser_ButtonAttributes(GH_Component owner) : base(owner)
        {
            thisowner = owner;
        }

        protected override void Layout()
        {
            base.Layout();

            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(this.Bounds);
            rec0.Height += (22 * 2 + 8);
            System.Drawing.Rectangle rec1 = rec0;
            System.Drawing.Rectangle rec2 = rec0;
            rec1.Y = rec1.Bottom - 44 - 8;
            rec1.Height = 22;
            rec1.Inflate(-2, -2);
            Bounds = rec0;
            ButtonBounds = rec1;

            rec2.Y = rec2.Bottom - 22 - 8;
            rec2.Height = 22 +8;
            rec2.Inflate(-2, -2);
            ButtonBounds2 = rec2;

        }
        private System.Drawing.Rectangle ButtonBounds { get; set; }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.White, "Refresh Status", new int[] { 7, 7, 0, 7 }, 4);
                button.Render(graphics, Selected, Owner.Locked, false);
                GH_Capsule button2 = GH_Capsule.CreateTextCapsule(ButtonBounds2, ButtonBounds2, GH_Palette.Pink, "Initialise", new int[] { 7, 7, 0, 7 }, 4);
                button2.Render(graphics, Selected, Owner.Locked, false);

            }
        }
        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                System.Drawing.RectangleF rec = ButtonBounds;
                //rec.Inflate(-4, -4);
                System.Drawing.RectangleF rec2 = ButtonBounds2;
                var owner = this.Owner as SDInitialiser;
                if (rec.Contains(e.CanvasLocation))
                {
                    
                    if (owner != null)
                    {
                        owner.refreshStatusPressed = true;
                        owner.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;

                }
                else if (rec2.Contains(e.CanvasLocation))
                {
                        owner.startWebUIPressed = true;
                    if (owner != null)
                    {
                        owner.startWebUIPressed = true;
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