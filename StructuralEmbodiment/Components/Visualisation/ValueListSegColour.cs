using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using StructuralEmbodiment.Core.Visualisation;
using System;
using System.Collections.Generic;


namespace StructuralEmbodiment.Components.Visualisation
{
    public class ValueListSegColour : Grasshopper.Kernel.Special.GH_ValueList, ISE_ValueList
    {
        public new List<GH_ValueListItem> ListItems;
        public string[] colours = new string[]{
    "wall",
            "building",
            "sky",
            "floor",
            "tree",
            "ceiling",
            "road",
            "bed",
            "windowpane",
            "grass",
            "cabinet",
            "sidewalk",
            "person",
            "earth",
            "door",
            "table",
            "mountain",
            "plant",
            "curtain",
            "chair",
            "car",
            "water",
            "painting",
            "sofa",
            "shelf",
            "house",
            "sea",
            "mirror",
            "rug",
            "field",
            "armchair",
            "seat",
            "fence",
            "desk",
            "rock",
            "wardrobe",
            "lamp",
            "bathtub",
            "railing",
            "cushion",
            "base",
            "box",
            "column",
            "signboard",
            "chest",
            "counter",
            "sand",
            "sink",
            "skyscraper",
            "fireplace",
            "refrigerator",
            "grandstand",
            "path",
            "stairs",
            "runway",
            "case",
            "pool",
            "pillow",
            "screen",
            "stairway",
            "river",
            "bridge",
            "bookcase",
            "blind",
            "coffee",
            "toilet",
            "flower",
            "book",
            "hill",
            "bench",
            "countertop",
            "stove",
            "palm",
            "kitchen",
            "computer",
            "swivel",
            "boat",
            "bar",
            "arcade",
            "hovel",
            "bus",
            "towel",
            "light",
            "truck",
            "tower",
            "chandelier",
            "awning",
            "streetlight",
            "booth",
            "television",
            "airplane",
            "dirt",
            "apparel",
            "pole",
            "land",
            "bannister",
            "escalator",
            "ottoman",
            "bottle",
            "buffet",
            "poster",
            "stage",
            "van",
            "ship",
            "fountain",
            "conveyer",
            "canopy",
            "washer",
            "plaything",
            "swimming",
            "stool",
            "barrel",
            "basket",
            "waterfall",
            "tent",
            "bag",
            "minibike",
            "cradle",
            "oven",
            "ball",
            "food",
            "step",
            "tank",
            "trade",
            "microwave",
            "pot",
            "animal",
            "bicycle",
            "lake",
            "dishwasher",
            "silver",
            "blanket",
            "sculpture",
            "hood",
            "sconce",
            "vase",
            "traffic",
            "tray",
            "ashcan",
            "fan",
            "pier",
            "crt",
            "plate",
            "monitor",
            "bulletin",
            "shower",
            "radiator",
            "glass",
            "clock",
            "flag"
};
        /// <summary>
        /// Initializes a new instance of the ValueListSemantic class.
        /// </summary>
        public ValueListSegColour()
          : base()
        {
            this.ListMode = GH_ValueListMode.DropDown;
            this.Description = "Values for semantic assignment in the images";
            this.Name = "Segmentation Colours";
            this.NickName = "SC";
            this.Category = "Structural Embodiment";
            this.SubCategory = "Visualisation";

            base.ListItems.Clear();
            foreach (var colour in this.colours)
            {
                var item = new GH_ValueListItem(colour, "\""+colour+ "\"");
                item.Selected = colour.StartsWith("sky");
                base.ListItems.Add(item);
            }
            
        }

        
        public void Refresh()
        {
            base.ListItems.Clear();
            foreach (var colour in this.colours)
            {
                var item = new GH_ValueListItem(colour, "\"" + colour + "\"");
                item.Selected = colour.StartsWith("sky");
                base.ListItems.Add(item);
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
                return Properties.Resources.VIS_SegmentationColours;
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
            get { return new Guid("2E9D7031-3A08-4F29-8188-900D5A8DC15C"); }
        }
    }
}