using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino.Geometry;
using StructuralEmbodiment.Properties;
using System;
using System.Collections.Generic;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class RhinoVisualiser : GH_Component
    {
        Dictionary<string, (int, int, int)> sematicColours = new Dictionary<string, (int, int, int)>
        {
            {"wall", (120, 120, 120)},
            {"building", (180, 120, 120)},
            { "sky", (6, 230, 230)},
            { "floor", (80, 50, 50)},
            { "tree", (4, 200, 3)},
            { "ceiling", (120, 120, 80)},
            { "road", (140, 140, 140)},
            { "bed", (204, 5, 255)},
            { "windowpane", (230, 230, 230)},
            { "grass", (4, 250, 7)},
            { "cabinet", (224, 5, 255)},
            { "sidewalk", (235, 255, 7)},
            { "person", (150, 5, 61)},
            { "earth", (120, 120, 70)},
            { "door", (8, 255, 51)},
            { "table", (255, 6, 82)},
            { "mountain", (143, 255, 140)},
            { "plant", (204, 255, 4)},
            { "curtain", (255, 51, 7)},
            { "chair", (204, 70, 3)},
            { "car", (0, 102, 200)},
            { "water", (61, 230, 250)},
            { "painting", (255, 6, 51)},
            { "sofa", (11, 102, 255)},
            { "shelf", (255, 7, 71)},
            { "house", (255, 9, 224)},
            { "sea", (9, 7, 230)},
            { "mirror", (220, 220, 220)},
            { "rug", (255, 9, 92)},
            { "field", (112, 9, 255)},
            { "armchair", (8, 255, 214)},
            { "seat", (7, 255, 224)},
            { "fence", (255, 184, 6)},
            { "desk", (10, 255, 71)},
            { "rock", (255, 41, 10)},
            { "wardrobe", (7, 255, 255)},
            { "lamp", (224, 255, 8)},
            { "bathtub", (102, 8, 255)},
            { "railing", (255, 61, 6)},
            { "cushion", (255, 194, 7)},
            { "base", (255, 122, 8)},
            { "box", (0, 255, 20)},
            { "column", (255, 8, 41)},
            { "signboard", (255, 5, 153)},
            { "chest", (6, 51, 255)},
            { "counter", (235, 12, 255)},
            { "sand", (160, 150, 20)},
            { "sink", (0, 163, 255)},
            { "skyscraper", (140, 140, 140)},
            { "fireplace", (0250, 10, 15)},
            { "refrigerator", (20, 255, 0)},
            { "grandstand", (31, 255, 0)},
            { "path", (255, 31, 0)},
            { "stairs", (255, 224, 0)},
            { "runway", (153, 255, 0)},
            { "case", (0, 0, 255)},
            { "pool", (255, 71, 0)},
            { "pillow", (0, 235, 255)},
            { "screen", (0, 173, 255)},
            { "stairway", (31, 0, 255)},
            { "river", (11, 200, 200)},
            { "bridge", (255, 82, 0)},
            { "bookcase", (0, 255, 245)},
            { "blind", (0, 61, 255)},
            { "coffee", (0, 255, 112)},
            { "toilet", (0, 255, 133)},
            { "flower", (255, 0, 0)},
            { "book", (255, 163, 0)},
            { "hill", (255, 102, 0)},
            { "bench", (194, 255, 0)},
            { "countertop", (0, 143, 255)},
            { "stove", (51, 255, 0)},
            { "palm", (0, 82, 255)},
            { "kitchen", (0, 255, 41)},
            { "computer", (0, 255, 173)},
            { "swivel", (10, 0, 255)},
            { "boat", (173, 255, 0)},
            { "bar", (0, 255, 153)},
            { "arcade", (255, 92, 0)},
            { "hovel", (255, 0, 255)},
            { "bus", (255, 0, 245)},
            { "towel", (255, 0, 102)},
            { "light", (255, 173, 0)},
            { "truck", (255, 0, 20)},
            { "tower", (255, 184, 184)},
            { "chandelier", (0, 31, 255)},
            { "awning", (0, 255, 61)},
            { "streetlight", (0, 71, 255)},
            { "booth", (255, 0, 204)},
            { "television", (0, 255, 194)},
            { "airplane", (0, 255, 82)},
            { "dirt", (0, 10, 255)},
            { "apparel", (0, 112, 255)},
            { "pole", (51, 0, 255)},
            { "land", (0, 194, 255)},
            { "bannister", (0, 122, 255)},
            { "escalator", (0, 255, 163)},
            { "ottoman", (255, 153, 0)},
            { "bottle", (0, 255, 10)},
            { "buffet", (255, 112, 0)},
            { "poster", (143, 255, 0)},
            { "stage", (82, 0, 255)},
            { "van", (163, 255, 0)},
            { "ship", (255, 235, 0)},
            { "fountain", (8, 184, 170)},
            { "conveyer", (133, 0, 255)},
            { "canopy", (0, 255, 92)},
            { "washer", (184, 0, 255)},
            { "plaything", (255, 0, 31)},
            { "swimming", (0, 184, 255)},
            { "stool", (0, 214, 255)},
            { "barrel", (255, 0, 112)},
            { "basket", (92, 255, 0)},
            { "waterfall", (0, 224, 255)},
            { "tent", (112, 224, 255)},
            { "bag", (70, 184, 160)},
            { "minibike", (163, 0, 255)},
            { "cradle", (153, 0, 255)},
            { "oven", (71, 255, 0)},
            { "ball", (255, 0, 163)},
            { "food", (255, 204, 0)},
            { "step", (255, 0, 143)},
            { "tank", (0, 255, 235)},
            { "trade", (133, 255, 0)},
            { "microwave", (255, 0, 235)},
            { "pot", (245, 0, 255)},
            { "animal", (255, 0, 122)},
            { "bicycle", (255, 245, 0)},
            { "lake", (10, 190, 212)},
            { "dishwasher", (214, 255, 0)},
            { "silver", (0, 204, 255)},
            { "blanket", (20, 0, 255)},
            { "sculpture", (255, 255, 0)},
            { "hood", (0, 153, 255)},
            { "sconce", (0, 41, 255)},
            { "vase", (0, 255, 204)},
            { "traffic", (41, 0, 255)},
            { "tray", (41, 255, 0)},
            { "ashcan", (173, 0, 255)},
            { "fan", (0, 245, 255)},
            { "pier", (71, 0, 255)},
            { "crt", (122, 0, 255)},
            { "plate", (0, 255, 184)},
            { "monitor", (0, 92, 255)},
            { "bulletin", (184, 255, 0)},
            { "shower", (0, 133, 255)},
            { "radiator", (255, 214, 0)},
            { "glass", (25, 194, 194)},
            { "clock", (102, 255, 0)},
            { "flag", (92, 0, 255)}
        };


/// <summary>
/// Initializes a new instance of the RhinoVisualiser class.
/// </summary>
public RhinoVisualiser()
          : base("Rhino Visualiser", "RV",
              "Visualise Geometry in Rhion via live baking",
              "Structural Embodiment", "Visualisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("• Geometry", "• G", "Geometry to be visualised", GH_ParamAccess.list);
            pManager.AddTextParameter("• Segment Name", "• SN", "Name for semantic segmentation map, could be seem as the layer name", GH_ParamAccess.item);
            pManager.AddTextParameter("Material Name", "MN", "Name of the material", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Mapping Size", "MS", "Size of the mapping", GH_ParamAccess.item);
            pManager.AddBooleanParameter("• Visualise", "V", "Visualise the geometries", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Clear", "C", "Clean up the visualisation layers from Rhino", GH_ParamAccess.item);

            //pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            //pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Layer Name", "LN", "Name of the layer", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GeometryBase> geos = new List<GeometryBase>();
            DA.GetDataList("• Geometry", geos);
            string materialName = null;
            DA.GetData("Material Name", ref materialName);
            string segmentName = null;
            DA.GetData("• Segment Name", ref segmentName);
            bool run = false;
            DA.GetData("• Visualise", ref run);
            bool clear = false;
            int mappingSize = 1;
            DA.GetData("Mapping Size", ref mappingSize);
            DA.GetData("Clear", ref clear);

            string message = "";
            
            // Check if the segment name is provided and set the colour
            var colour = System.Drawing.Color.Black;
            if (segmentName != null)
            {
                if (sematicColours.ContainsKey(segmentName))
                {
                    var c = sematicColours[segmentName];
                    colour = System.Drawing.Color.FromArgb(c.Item1, c.Item2, c.Item3);
                    message = "Seg Colour set ✓";
                } else { message = "Seg Colour not found, set to black"; }
            }

            // Access the Rhino document
            var doc = Rhino.RhinoDoc.ActiveDoc;
            string layerName = "SE_VIS";
            Rhino.Render.RenderMaterial mat = null;

            // Check and clear layers if required
            if (clear)
            {
                var toDelete = new List<int>();
                foreach (var layer in doc.Layers)
                {

                    if (layer.HasName && layer.Name.StartsWith("SE_VIS", StringComparison.OrdinalIgnoreCase))
                    {
                        toDelete.Add(layer.Index);
                        //doc.Layers.Delete(layer.Id, true);
                        doc.Layers.Purge(layer.Id, true);
                    }

                }

            }

            if (run && !clear)
            {
                // Check if material name is provided and search for the material
                if (!string.IsNullOrEmpty(materialName))
                {

                    foreach (var m in doc.RenderMaterials)
                    {

                        if (m.Name.Equals(materialName, StringComparison.OrdinalIgnoreCase))
                        {
                            mat = m;
                            message += ("\n" +"Material found and mapped ✓");
                            break;
                        }
                    }
                }
                if (mat == null)
                {
                    message += ("\n" + "Material not found");
                }

                layerName += "_" + segmentName;


                // Check for the existence of the layer
                Layer layerFound = doc.Layers.FindName(layerName);
                int layerIndex;
                if (layerFound == null)
                {
                    Layer newLayer = new Layer();
                    newLayer.Name = layerName;
                    newLayer.Color = colour; // You can set the layer color here

                    // Set layer material if found
                    if (mat != null)
                    {
                        newLayer.RenderMaterial = mat;
                    }
                    layerFound = newLayer;
                    layerIndex = doc.Layers.Add(layerFound);

                }
                else
                {
                    layerIndex = layerFound.Index;
                }

                // Get the layer
                var layer = doc.Layers[layerIndex];
                layer.Color = colour;

                // Delete all objects in the layer
                var objsToDelete = doc.Objects.FindByLayer(layer);
                foreach (var obj in objsToDelete)
                {
                    doc.Objects.Delete(obj);
                }

                // Bake the breps into the layer
                foreach (var geo in geos)
                {
                    if (geo != null)
                    {
                        // Create a new object attributes holder
                        var attributes = new Rhino.DocObjects.ObjectAttributes
                        {
                            LayerIndex = layerIndex
                        };



                        Guid brepId = Guid.Empty;
                        if (geo is Brep) { brepId = doc.Objects.AddBrep((Brep)geo, attributes); }
                        else if (geo is Mesh)
                        {
                            brepId = doc.Objects.AddMesh((Mesh)geo, attributes);
                        }
                        else if (geo is Curve)
                        {
                            brepId = doc.Objects.AddCurve((Curve)geo, attributes);
                        }
                        else if (geo is NurbsCurve)
                        {
                            brepId = doc.Objects.AddCurve((NurbsCurve)geo, attributes);
                        }
                        else if (geo is Surface)
                        {
                            brepId = doc.Objects.AddSurface((Surface)geo, attributes);
                        }
                        else if (geo is Extrusion)
                        {
                            brepId = doc.Objects.AddExtrusion((Extrusion)geo, attributes);
                        }
                        else if (geo is SubD)
                        {
                            brepId = doc.Objects.AddSubD((SubD)geo, attributes);
                        }

                        if (brepId != Guid.Empty)
                        {
                            // Create and set the mapping to box mapping
                            var boxMapping = Rhino.Render.TextureMapping.CreateBoxMapping(
                                Rhino.Geometry.Plane.WorldXY,
                                new Rhino.Geometry.Interval(0, mappingSize),
                                new Rhino.Geometry.Interval(0, mappingSize),
                                new Rhino.Geometry.Interval(0, mappingSize),
                                true
                                ); 
                            doc.Objects.ModifyTextureMapping(brepId, 1, boxMapping);
                        }
                    }
                }
                this.Message = message;
                DA.SetData("Layer Name", layerName);
                // Redraw the view to update changes
                doc.Views.Redraw();

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
                return Resources.VIS_RhinoVisualiser;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("268382FC-C7A2-48C9-8DCE-E655693859C9"); }
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }
    }
}