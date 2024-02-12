using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using StructuralEmbodiment.Core.Visualisation;
using System;
using System.Collections.Generic;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class ValueListCNModel : Grasshopper.Kernel.Special.GH_ValueList, ISE_ValueList
    {
        public new List<GH_ValueListItem> ListItems;

        /// <summary>
        /// Initializes a new instance of the ValueListSemantic class.
        /// </summary>
        public ValueListCNModel()
          : base()
        {
            this.ListMode = GH_ValueListMode.DropDown;
            this.Description = "List of the available ControlNet models";
            this.Name = "ControlNet Models";
            this.NickName = "CNM";
            this.Category = "Structural Embodiment";
            this.SubCategory = "Visualisation";

            var sDWebUISetting = SDWebUISetting.Instance;
            if (sDWebUISetting.isInitialised)
            {

                base.ListItems.Clear();
                foreach (var model in sDWebUISetting.CNModels)
                {
                    var item = new GH_ValueListItem(model, "\"" + model + "\"");
                    //item.Selected = colour.StartsWith("sky");
                    base.ListItems.Add(item);
                }
            }
            else
            {
                base.ListItems.Clear();
                base.ListItems.Add(new GH_ValueListItem("Not Initialised, Use AI Initialiser Component", "0"));
                base.ListItems.Add(new GH_ValueListItem("Select to Refresh", "1"));
            }
        }
        
        public void Refresh()
        {

            var sDWebUISetting = SDWebUISetting.Instance;
            if (sDWebUISetting.isInitialised)
            {

                base.ListItems.Clear();
                foreach (var model in sDWebUISetting.CNModels)
                {
                    var item = new GH_ValueListItem(model, "\"" + model + "\"");
                    //item.Selected = colour.StartsWith("sky");
                    base.ListItems.Add(item);
                }
            }
            base.ExpireSolution(true);


        }
        

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Refresh List", (s, e) =>
            {
                Refresh();
            });
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
                return Properties.Resources.VIS_CNModels;
            }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quarternary; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("047B3C7F-6E16-466E-B98A-FE2C2EA87FB0"); }
        }
    }
}