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
        /// <summary>
        /// Initializes a new instance of the RhinoVisualiser class.
        /// </summary>
        public RhinoVisualiser()
          : base("RhinoVisualiser", "RV",
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
            pManager.AddTextParameter("Material Name", "MN", "Name of the material", GH_ParamAccess.item);
            pManager.AddBooleanParameter("• Visualise", "• V", "Visualise the geometries", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Mapping Size", "MS", "Size of the mapping", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Clear", "C", "Clean up the visualisation layers from Rhino", GH_ParamAccess.item);

            pManager[1].Optional = true;
            //pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
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
            DA.GetDataList(0, geos);
            string materialName = null;
            DA.GetData(1, ref materialName);
            bool run = false;
            DA.GetData(2, ref run);
            bool clear = false;
            int mappingSize = 1;
            DA.GetData(3, ref mappingSize);
            DA.GetData(4, ref clear);
            
            

            // Access the Rhino document
            var doc = Rhino.RhinoDoc.ActiveDoc;
            string layerName = "SE_Display";
            Rhino.Render.RenderMaterial mat = null;

            // Check and clear layers if required
            if (clear)
            {
                var toDelete = new List<int>();
                foreach (var layer in doc.Layers)
                {

                    if (layer.HasName && layer.Name.StartsWith("SE_Display", StringComparison.OrdinalIgnoreCase))
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
                            layerName += "_"+materialName;
                            break;
                        }
                    }

                }

                // Check for the existence of the layer
                Layer layerFound = doc.Layers.FindName(layerName);
                int layerIndex;
                if (layerFound == null)
                {
                    Layer newLayer = new Layer();
                    newLayer.Name = layerName;
                    newLayer.Color = System.Drawing.Color.Blue; // You can set the layer color here

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
    }
}