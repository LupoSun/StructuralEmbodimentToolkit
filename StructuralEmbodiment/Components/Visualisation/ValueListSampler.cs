using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using StructuralEmbodiment.Core.Visualisation;
using System;
using System.Collections.Generic;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class ValueListSampler : Grasshopper.Kernel.Special.GH_ValueList
    {
        public new List<GH_ValueListItem> ListItems;
        /// <summary>
        /// Initializes a new instance of the ValueListSemantic class.
        /// </summary>
        public ValueListSampler()
          : base()
        {
            this.ListMode = GH_ValueListMode.DropDown;
            this.Description = "List of the available samplers";
            this.Name = "Samplers";
            this.NickName = "SP";
            this.Category = "Structural Embodiment";
            this.SubCategory = "Visualisation";

            var sDWebUISetting = SDWebUISetting.Instance;
            if (sDWebUISetting.isInitialised)
            {

                base.ListItems.Clear();
                foreach (var sampler in sDWebUISetting.Samplers)
                {
                    var item = new GH_ValueListItem(sampler, "\"" + sampler + "\"");
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

        public override void ExpireSolution(bool recompute)
        {
            var sDWebUISetting = SDWebUISetting.Instance;
            if (sDWebUISetting.isInitialised)
            {

                base.ListItems.Clear();
                foreach (var sampler in sDWebUISetting.Samplers)
                {
                    var item = new GH_ValueListItem(sampler, "\"" + sampler + "\"");
                    //item.Selected = colour.StartsWith("sky");
                    base.ListItems.Add(item);
                }
            }
            base.ExpireSolution(recompute);
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
                return Properties.Resources.VIS_Samplers;
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
            get { return new Guid("429BB50F-0FF8-4962-8F6A-B398E47A1F2A"); }
        }
    }
}