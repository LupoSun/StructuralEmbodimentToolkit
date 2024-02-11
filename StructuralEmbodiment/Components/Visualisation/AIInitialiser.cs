using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using StructuralEmbodiment.Core.GrasshopperAsyncComponent;
using StructuralEmbodiment.Core.Visualisation;
using System;
using System.Collections.Generic;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class AIInitialiser : GH_AsyncComponent
    {
        public bool startWebUIPressed = false;
        public bool refreshStatusPressed = false;

        #region Methods of GH_Component interface
        /// <summary>
        /// Initializes a new instance of the SDInitialiser class.
        /// </summary>
        public AIInitialiser()
          : base("AI Initialiser", "AII",
              "Initialise the StableDiffusion WebUI by AUTOMATIC1111 with api enabled",
              "Structural Embodiment", "Visualisation")
        {
            base.BaseWorker = new SDInitialiserWorker(this, SDWebUISetting.Instance);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("• SD Location", "• SD", "Path to StableDiffusion WebUI folder", GH_ParamAccess.item);
            pManager.AddTextParameter("User URL", "CA", "Customised Adress to access the api. By default http://127.0.0.1:7860", GH_ParamAccess.item);
            pManager.AddTextParameter("Additional Arguments", "AA", "Additional Arguments for the WebUI, refer to https://github.com/AUTOMATIC1111/stable-diffusion-webui/wiki/Command-Line-Arguments-and-Settings#all-command-line-arguments", GH_ParamAccess.list);

            pManager[0].Optional = true;
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
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.VIS_SDInitialiser;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5F0ED92F-1F07-403D-AC62-C001ED9FB763"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        public override void CreateAttributes()
        {
            base.m_attributes = new SDInitialiser_ButtonAttributes(this);
        }

        private class SDInitialiserWorker : WorkerInstance
        {
            SDWebUISetting SDWebUISetting;
            GH_Component Owner;
            public string SDLocation { get; set; }

            public string UserURL { get; set; }
            public List<string> AdditionalArguments { get; set; }


            public SDInitialiserWorker(GH_Component owner, SDWebUISetting sDWebUISetting) : base(null)
            {
                this.Owner = owner;
                this.SDWebUISetting = sDWebUISetting;
            }

            public override WorkerInstance Duplicate() => new SDInitialiserWorker(Owner, SDWebUISetting);

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                var owner = this.Owner as AIInitialiser;

                // Check if the operation has been cancelled
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (owner.startWebUIPressed)
                {
                    SDWebUISetting.SetSDWebUISetting(UserURL);
                    SDWebUISetting.Initialise();
                    owner.startWebUIPressed = false;
                    Done();
                }
                else if (owner.refreshStatusPressed)
                {
                    SDWebUISetting.Refresh();
                    SDWebUISetting.ReloadValueLists();
                    owner.refreshStatusPressed = false;
                    Done();
                }


                owner.Message = "Not Initialised";
            }
            public override void SetData(IGH_DataAccess DA)
            {
                //DA.SetDataList("Images", this.Images);
            }
            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                string _sdLocation = "";
                DA.GetData("• SD Location", ref _sdLocation);
                string _userURL = "http://127.0.0.1:7860";
                DA.GetData("User URL", ref _userURL);
                List<string> _additionalArguments = new List<string>();
                DA.GetDataList("Additional Arguments", _additionalArguments);

                this.SDLocation = _sdLocation;
                this.UserURL = _userURL;
                this.AdditionalArguments = _additionalArguments;

            }
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
            var buttonWidth = 120;
            var bezel = 5;
            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(this.Bounds);
            rec0.Height += (22 * 2 + 8+bezel);
            System.Drawing.Rectangle rec1 = rec0;
            System.Drawing.Rectangle rec2 = rec0;
            rec1.Y = rec1.Bottom - 44 - 8-bezel;
            rec1.Height = 22;
            rec1.Width = buttonWidth;
            var x = rec0.Right - rec0.Width / 2 - rec1.Width / 2;
            rec1.X = x;
            rec1.Inflate(-2, -2);
            Bounds = rec0;
            ButtonBounds = rec1;

            rec2.Y = rec2.Bottom - 22 - 8-bezel;
            rec2.Height = 22 + 8;
            rec2.Width = buttonWidth;
            rec2.X = x;
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
                var owner = this.Owner as AIInitialiser;
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