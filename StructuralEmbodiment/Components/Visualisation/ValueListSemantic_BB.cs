using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Plugin;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using GH_IO.Serialization;
using Grasshopper.Kernel.Special;
using System.Drawing.Drawing2D;

namespace StructuralEmbodiment.Components.Visualisation
{
    #region CustomValueListCompunent
    public class ValueListSemantic_BB : Grasshopper.Kernel.Special.GH_ValueList
    {
        public new List<GH_ValueListItem> ListItems;

        public GH_ValueListAttributes instanceAttributes;

        private readonly Guid ID = new Guid("{9AE62FC7-2715-4C0F-B762-BB07D7D5817F}");

        public ValueListSemantic_BB() : base()
        {

            this.ListMode = Grasshopper.Kernel.Special.GH_ValueListMode.DropDown;
            this.Description = "The BEST Value List";
            this.Name = "The BBEST Value List";
            this.Category = "Testing";
            // NickName is the name seen on the Grasshopper canvas 
            this.NickName = "Testing sfddsfs";

            base.ListItems.Clear();

            this.ListItems = new List<GH_ValueListItem>();

            //base.ListItems.Add(new GH_ValueListItem("21", "21"));
            //base.ListItems.Add(new GH_ValueListItem("22", "22"));
            //base.ListItems.Add(new GH_ValueListItem("23", "23"));
            base.ListItems.Add(new GH_ValueListItem("24", "24"));
            base.ListItems.Add(new GH_ValueListItem("25", "25"));
            base.ListItems.Add(new GH_ValueListItem("26", "26"));
            base.ListItems.Add(new GH_ValueListItem("27", "27"));
        }

        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            this.Menu_AppendObjectName(menu);

            this.Menu_AppendEnableItem(menu);

            this.AppendAdditionalMenuItems(menu);

            this.Menu_AppendObjectHelp(menu);

            return true;
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
        }

        public override Guid ComponentGuid
        {
            get
            {
                return ID;
            }
        }

        public new List<Grasshopper.Kernel.Special.GH_ValueListItem> SelectedItems
        {// Allow user to select multiple items unlike the default GH_ValueList
            get
            {
                List<GH_ValueListItem> list = new List<GH_ValueListItem>();

                if (this.ListItems.Count != 0)
                {
                    foreach (Grasshopper.Kernel.Special.GH_ValueListItem item in this.ListItems)
                    {
                        if (item.Selected)
                        {
                            list.Add(item);
                        }
                    }
                    return list;
                }
                return list;
            }
        }

        public override void CreateAttributes()
        {
            ValueListSemanticAttributes instanceAttributes = new ValueListSemanticAttributes(this);

            foreach (GH_ValueListItem item in base.ListItems)
            {
                ToolStripMenuItem ToolStripItem = new ToolStripMenuItem(item.Name);
                ToolStripItem.Click += new EventHandler(this.ValueMenuItem_Click);
                ToolStripItem.MouseLeave += new EventHandler(MouseLeave);
                ToolStripItem.MouseEnter += new EventHandler(MouseEnter);

                instanceAttributes.collectionToolStripMenuItems.Add(ToolStripItem);
            }

            this.instanceAttributes = instanceAttributes;

            base.m_attributes = this.instanceAttributes;
        }

        private void ValueMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item2 = (ToolStripMenuItem)sender;
            if (!item2.Checked)
            {
                item2.Checked = true;
                // Add to Design Space
                ToolStripDropDownMenu menu = (ToolStripDropDownMenu)item2.Owner;
                menu.Show();
                this.ExpireSolution(true);
                return;
            }
            else
            {
                item2.Checked = false;
                // Remove from Design Space
                ToolStripDropDownMenu menu = (ToolStripDropDownMenu)item2.Owner;
                menu.Show();
                this.ExpireSolution(true);
                return;
            }
        }

        private void MouseLeave(object sender, EventArgs e)
        {
            ToolStripMenuItem item2 = (ToolStripMenuItem)sender;
            ToolStripDropDownMenu menu = (ToolStripDropDownMenu)item2.Owner;
            menu.Hide();
        }

        private void MouseEnter(object sender, EventArgs e)
        {
            ToolStripMenuItem item2 = (ToolStripMenuItem)sender;
            ToolStripDropDownMenu menu = (ToolStripDropDownMenu)item2.Owner;
            menu.Show();
        }
    }
    #endregion

    #region CustomValueListAttributes
    public class ValueListSemanticAttributes : GH_ValueListAttributes
    {
        private RectangleF _NameBounds;

        private RectangleF _ItemBounds;

        public ValueListSemantic_BB ownerOfThisAttribute;

        public List<ToolStripMenuItem> collectionToolStripMenuItems;

        public ValueListSemanticAttributes(ValueListSemantic_BB owner) : base(owner)
        {
            this.ownerOfThisAttribute = owner;

            this.collectionToolStripMenuItems = new List<ToolStripMenuItem>();
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule capsule = GH_Capsule.CreateCapsule(this.Bounds, GH_Palette.Normal);
                capsule.AddOutputGrip(this.OutputGrip.Y);
                capsule.Render(canvas.Graphics, this.Selected, this.Owner.Locked, this.Owner.Hidden);
                capsule.Dispose();
                int zoomFadeLow = GH_Canvas.ZoomFadeLow;
                if (zoomFadeLow > 0)
                {
                    canvas.SetSmartTextRenderingHint();
                    GH_PaletteStyle impliedStyle = GH_CapsuleRenderEngine.GetImpliedStyle(GH_Palette.Normal, this);
                    Color color = Color.FromArgb(zoomFadeLow, impliedStyle.Text);

                    if (this.NameBounds.Width > 0f)
                    {
                        SolidBrush brush = new SolidBrush(color);
                        graphics.DrawString(this.ownerOfThisAttribute.NickName, GH_FontServer.Standard, brush, this.NameBounds, GH_TextRenderingConstants.CenterCenter);
                        brush.Dispose();
                        int x = Convert.ToInt32(this.NameBounds.Right);
                        int num3 = Convert.ToInt32(this.NameBounds.Top);
                        int num4 = Convert.ToInt32(this.NameBounds.Bottom);
                        GH_GraphicsUtil.EtchFadingVertical(graphics, num3, num4, x, Convert.ToInt32((double)(0.8 * zoomFadeLow)), Convert.ToInt32((double)(0.3 * zoomFadeLow)));
                    }

                    this.RenderDropDown(canvas, graphics, color);

                }
            }
        }

        protected override void Layout()
        {
            this.LayoutDropDown();

            this.ItemBounds = this.Bounds;
            RectangleF bounds = this.Bounds;
            RectangleF ef5 = new RectangleF(bounds.X, this.Bounds.Y, 0f, this.Bounds.Height);
            this.NameBounds = ef5;
            if (this.Owner.DisplayName != null)
            {
                int num = GH_FontServer.StringWidth(this.Owner.DisplayName, GH_FontServer.Standard) + 10;
                bounds = new RectangleF(this.Bounds.X - num, this.Bounds.Y, (float)num, this.Bounds.Height);
                this.NameBounds = bounds;
                this.Bounds = RectangleF.Union(this.NameBounds, this.ItemBounds);
            }
        }


        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            return GH_ObjectResponse.Ignore;
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Grasshopper.Kernel.Special.GH_ValueListItem firstSelectedItem = this.Owner.FirstSelectedItem;
                if (firstSelectedItem.BoxRight.Contains(e.CanvasLocation))
                {
                    ToolStripDropDownMenu menu = new ToolStripDropDownMenu();
                    menu.AutoClose = true;

                    foreach (ToolStripMenuItem toolStripItem in this.collectionToolStripMenuItems)
                    {
                        menu.Items.Add(toolStripItem);
                    }

                    menu.Show(sender, e.ControlLocation);

                    return GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseDown(sender, e);
        }

        private RectangleF NameBounds
        {
            get { return this._NameBounds; }
            set { this._NameBounds = value; }
        }

        private RectangleF ItemBounds
        {
            get { return this._ItemBounds; }
            set { this._ItemBounds = value; }
        }

        private void RenderDropDown(GH_Canvas canvas, Graphics graphics, Color color)
        {
            GH_ValueListItem firstSelectedItem = this.Owner.FirstSelectedItem;
            if (firstSelectedItem != null)
            {
                graphics.DrawString("30", GH_FontServer.Standard, Brushes.Black, firstSelectedItem.BoxName, GH_TextRenderingConstants.CenterCenter);
                RenderDownArrow(canvas, graphics, firstSelectedItem.BoxRight, color);
            }
        }

        private static void RenderDownArrow(GH_Canvas canvas, Graphics graphics, RectangleF bounds, Color color)
        {
            int num = Convert.ToInt32((float)(bounds.X + (0.5f * bounds.Width)));
            int num2 = Convert.ToInt32((float)(bounds.Y + (0.5f * bounds.Height)));
            PointF[] tfArray2 = new PointF[3];
            PointF tf = new PointF((float)num, (float)(num2 + 6));
            tfArray2[0] = tf;
            PointF tf2 = new PointF((float)(num + 6), (float)(num2 - 6));
            tfArray2[1] = tf2;
            PointF tf3 = new PointF((float)(num - 6), (float)(num2 - 6));
            tfArray2[2] = tf3;
            PointF[] points = tfArray2;
            RenderShape(canvas, graphics, points, color);
        }

        private static void RenderShape(GH_Canvas canvas, Graphics graphics, PointF[] points, Color color)
        {
            int zoomFadeMedium = GH_Canvas.ZoomFadeMedium;
            float x = points[0].X;
            float num3 = x;
            float y = points[0].Y;
            float num5 = y;
            int num7 = points.Length - 1;
            for (int i = 1; i <= num7; i++)
            {
                x = Math.Min(x, points[i].X);
                num3 = Math.Max(num3, points[i].X);
                y = Math.Min(y, points[i].Y);
                num5 = Math.Max(num5, points[i].Y);
            }
            RectangleF rect = RectangleF.FromLTRB(x, y, num3, num5);
            rect.Inflate(1f, 1f);
            LinearGradientBrush brush = new LinearGradientBrush(rect, color, GH_GraphicsUtil.OffsetColour(color, 50), LinearGradientMode.Vertical)
            {
                WrapMode = WrapMode.TileFlipXY
            };
            graphics.FillPolygon(brush, points);
            brush.Dispose();
            if (zoomFadeMedium > 0)
            {
                Color color2 = Color.FromArgb(Convert.ToInt32((double)(0.5 * zoomFadeMedium)), Color.White);
                Color color3 = Color.FromArgb(0, Color.White);
                LinearGradientBrush brush2 = new LinearGradientBrush(rect, color2, color3, LinearGradientMode.Vertical)
                {
                    WrapMode = WrapMode.TileFlipXY
                };
                Pen pen2 = new Pen(brush2, 3f)
                {
                    LineJoin = LineJoin.Round,
                    CompoundArray = new float[] {
                    0f,
                    0.5f
                }
                };
                graphics.DrawPolygon(pen2, points);
                brush2.Dispose();
                pen2.Dispose();
            }
            Pen pen = new Pen(color, 1f)
            {
                LineJoin = LineJoin.Round
            };
            graphics.DrawPolygon(pen, points);
        }

        private int ItemMaximumWidth()
        {
            int num2 = 20;
            foreach (GH_ValueListItem item in this.Owner.ListItems)
            {
                int num3 = GH_FontServer.StringWidth(item.Name, GH_FontServer.Standard);
                num2 = Math.Max(num2, num3);
            }
            return (num2 + 10);
        }

        private void LayoutDropDown()
        {
            List<GH_ValueListItem>.Enumerator enumerator;
            int num2 = this.ItemMaximumWidth() + 0x16;
            int num = 0x16;
            this.Pivot = (PointF)GH_Convert.ToPoint(this.Pivot);
            RectangleF ef2 = new RectangleF(this.Pivot.X, this.Pivot.Y, (float)num2, (float)num);
            this.Bounds = ef2;
            GH_ValueListItem firstSelectedItem = this.Owner.FirstSelectedItem;

            enumerator = this.Owner.ListItems.GetEnumerator();

            try
            {
                while (enumerator.MoveNext())
                {
                    // Tried to write a cast but you can't write user defined casts for derivd classes! so this will do.
                    GH_ValueListItem current = enumerator.Current;

                    if (current == firstSelectedItem)
                    {
                        SetDropdownBounds(current, this.Bounds);
                    }
                    else
                    {
                        SetEmptyBounds(current, this.Bounds);
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private void SetDropdownBounds(GH_ValueListItem item, RectangleF bounds)
        {
            RectangleF rectangleF = new RectangleF(bounds.X, bounds.Y, 0f, bounds.Height);
            item.BoxLeft = rectangleF;
            rectangleF = new RectangleF(bounds.X, bounds.Y, bounds.Width - 22f, bounds.Height);
            item.BoxName = rectangleF;
            rectangleF = new RectangleF(bounds.Right - 22f, bounds.Y, 22f, bounds.Height);
            item.BoxRight = rectangleF;
        }

        private void SetEmptyBounds(GH_ValueListItem item, RectangleF bounds)
        {
            RectangleF rectangleF = new RectangleF(bounds.X, bounds.Y, 0f, 0f);
            item.BoxLeft = rectangleF;
            rectangleF = new RectangleF(bounds.X, bounds.Y, 0f, 0f);
            item.BoxName = rectangleF;
            rectangleF = new RectangleF(bounds.X, bounds.Y, 0f, 0f);
            item.BoxRight = rectangleF;
        }
    }
    #endregion
}