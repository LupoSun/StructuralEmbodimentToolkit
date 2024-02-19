using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Rhino.Geometry;
using StructuralEmbodimentToolkit.Core.GrasshopperAsyncComponent;
using StructuralEmbodimentToolkit.Core.Visualisation;
using StructuralEmbodimentToolkit.Properties;

namespace StructuralEmbodimentToolkit.Components.Visualisation
{
    public class AIVisualiser : GH_AsyncComponent
    {
        private List<Bitmap> ImagePersistent;
        public bool generatePressed = false;
        public bool clearCachePressed = false;
        /// <summary>
        /// Initializes a new instance of the Visualiser class.
        /// </summary>
        public AIVisualiser()
          : base("AI Visualiser", "Vis",
              "AI Visualisation Generator by StableDiffusion",
              "Structural Embodiment", "Visualisation")
        {
            ImagePersistent = new List<Bitmap>();
            BaseWorker = new VisualiserWorker(this,SDWebUISetting.Instance);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("• Settings", "• S", "Settings for image Generation", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Images", "I", "Generated Images",GH_ParamAccess.list);
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
                return Resources.VIS_SDVisualiser;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E88FEE74-326A-442C-BF93-C8396E1479C1"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        public override void CreateAttributes()
        {
            base.m_attributes = new SDVisualiser_ButtonAttributes(this);
        }

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Cancel Current Generation", (s, e) =>
            {
                //TODO: Add cancellation logic for the server
                RequestCancellation();
            });

        }

        private class VisualiserWorker: WorkerInstance
        {
            public GH_Component Owner;
            public ImageGenerationSetting ImgGenSettings { get; set; }
            public ImageRequest ImageRequest { get; set; }
            public SDWebUISetting SDWebUISetting { get; set; }
            

            public VisualiserWorker(GH_Component owner,SDWebUISetting sDWebUISetting): base(null) {

                this.Owner = owner;
                this.SDWebUISetting = sDWebUISetting;
            }

            public override WorkerInstance Duplicate() => new VisualiserWorker(Owner, SDWebUISetting);

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                var owner = this.Owner as AIVisualiser;
                // Check if the operation has been cancelled
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                if (owner.generatePressed)
                {
                    ImageRequest = new ImageRequest(SDWebUISetting.ServerURL, ImgGenSettings.Settings, SDWebUISetting.Client);
                    Task imageRequestTask = ImageRequest.GenerateImage(GenerationMode.text2img);
                    imageRequestTask.Wait();
                    //ImageRequest.Images.Reverse();
                    owner.ImagePersistent.InsertRange(0,ImageRequest.Images);

                    owner.generatePressed = false;
                    Done();
                }
                else if (owner.clearCachePressed)
                {
                    owner.ImagePersistent.Clear();
                    owner.clearCachePressed = false;
                    Done();
                }
                
            }
            public override void SetData(IGH_DataAccess DA)
            {
                var owner = this.Owner as AIVisualiser;
                DA.SetDataList("Images", owner.ImagePersistent);
            }
            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                ImageGenerationSetting _imgGenSettings = null;
                DA.GetData("• Settings", ref _imgGenSettings);

                ImgGenSettings = _imgGenSettings;
            }

        }
    }

    #region GH_ComponentAttributes interface

    public class SDVisualiser_ButtonAttributes : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        GH_Component thisowner = null;
        System.Drawing.RectangleF ButtonBounds2;

        public SDVisualiser_ButtonAttributes(GH_Component owner) : base(owner)
        {
            thisowner = owner;
        }

        protected override void Layout()
        {
            base.Layout();
            var buttonWidth = 120;
            var bezel = 5;
            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(this.Bounds);
            rec0.Height += (22 * 2 + 8 + bezel);
            System.Drawing.Rectangle rec1 = rec0;
            System.Drawing.Rectangle rec2 = rec0;
            rec1.Y = rec1.Bottom - 44 - 8 - bezel;
            rec1.Height = 22;
            rec1.Width = buttonWidth;
            var x = rec0.Right - rec0.Width / 2 - rec1.Width / 2;
            rec1.X = x;
            rec1.Inflate(-2, -2);
            Bounds = rec0;
            ButtonBounds = rec1;

            rec2.Y = rec2.Bottom - 22 - 8 - bezel;
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
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.White, "Clear Cache", new int[] { 7, 7, 0, 7 }, 4);
                button.Render(graphics, Selected, Owner.Locked, false);
                GH_Capsule button2 = GH_Capsule.CreateTextCapsule(ButtonBounds2, ButtonBounds2, GH_Palette.Pink, "Generate !", new int[] { 7, 7, 0, 7 }, 4);
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
                var owner = this.Owner as AIVisualiser;
                if (rec.Contains(e.CanvasLocation))
                {
                    if (owner != null)
                    {
                        owner.clearCachePressed = true;
                        owner.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;

                }
                else if (rec2.Contains(e.CanvasLocation))
                {
                    if (owner != null)
                    {
                        owner.generatePressed = true;
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