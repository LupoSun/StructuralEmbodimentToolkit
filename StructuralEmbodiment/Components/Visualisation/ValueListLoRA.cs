using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using StructuralEmbodimentToolkit.Core.Visualisation;
using System;
using System.Collections.Generic;

namespace StructuralEmbodimentToolkit.Components.Visualisation
{
    public class ValueListLoRA : Grasshopper.Kernel.Special.GH_ValueList, ISE_ValueList
    {
        public new List<GH_ValueListItem> ListItems;
        /// <summary>
        /// Initializes a new instance of the ValueListSemantic class.
        /// </summary>
        public ValueListLoRA()
          : base()
        {
            this.ListMode = GH_ValueListMode.DropDown;
            this.Description = "List of the available LoRA models";
            this.Name = "LoRA Models";
            this.NickName = "LM";
            this.Category = "Structural Embodiment";
            this.SubCategory = "Visualisation";

            var sDWebUISetting = SDWebUISetting.Instance;
            if (sDWebUISetting.isInitialised)
            {

                base.ListItems.Clear();
                foreach (var lora in sDWebUISetting.LoRAs)
                {
                    var item = new GH_ValueListItem(lora, "\"" + lora + "\"");
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
        
        public void Refresh() {
            var sDWebUISetting = SDWebUISetting.Instance;
            if (sDWebUISetting.isInitialised)
            {

                base.ListItems.Clear();
                foreach (var lora in sDWebUISetting.LoRAs)
                {
                    var item = new GH_ValueListItem(lora, "\"" + lora + "\"");
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
                return Properties.Resources.VIS_LoRAs;
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
            get { return new Guid("C2F83FC0-A591-4C9C-9C72-DBAAB9EE9211"); }
        }
    }
}