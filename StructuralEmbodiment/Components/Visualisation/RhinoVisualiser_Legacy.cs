using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.DocObjects;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class RhinoVisualiser_Legacy : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RhinoVisualiser class.
        /// </summary>
        public RhinoVisualiser_Legacy()
          : base("RhinoVisualiser_Legacy", "RV_L",
              "Visualise Geometry in Rhion via live baking",
              "Structural Embodiment", "Visualisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("• Geometry", "G", "Geometry to be visualised", GH_ParamAccess.list);
            pManager.AddTextParameter("Material Name", "MN", "Name of the material", GH_ParamAccess.item);
            pManager.AddBooleanParameter("• Visualise", "V", "Visualise the geometries", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Clear", "C", "Clean up the visualisation layers from Rhino", GH_ParamAccess.item);

            pManager[1].Optional = true;
            //pManager[2].Optional = true;
            pManager[3].Optional = true;
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
            List<GeometryBase> geos = new List<GeometryBase>();
            DA.GetDataList(0, geos);
            string materialName = null;
            DA.GetData(1, ref materialName);
            bool run = false;
            DA.GetData(2, ref run);
            bool clear = false;
            DA.GetData(3, ref clear);

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
                var layerIndex = doc.Layers.Find(layerName, true);
                if (layerIndex == -1)
                {
                    Layer newLayer = new Layer();
                    newLayer.Name = layerName;
                    newLayer.Color = System.Drawing.Color.Blue; // You can set the layer color here

                    // Set layer material if found
                    if (mat != null)
                    {
                        newLayer.RenderMaterial = mat;
                    }

                    layerIndex = doc.Layers.Add(newLayer);
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
                            var attributes = new Rhino.DocObjects.ObjectAttributes
                            {
                                LayerIndex = layerIndex
                            };

                            if (geo is Brep) doc.Objects.AddBrep((Brep)geo, attributes);
                            else if (geo is Mesh) doc.Objects.AddMesh((Mesh)geo, attributes);
                            else if (geo is Curve) doc.Objects.AddCurve((Curve)geo, attributes);
                            else if (geo is NurbsCurve) doc.Objects.AddCurve((NurbsCurve)geo, attributes);
                            else if (geo is Surface) doc.Objects.AddSurface((Surface)geo, attributes);
                            else if (geo is Extrusion) doc.Objects.AddExtrusion((Extrusion)geo, attributes);
                            else if (geo is SubD) doc.Objects.AddSubD((SubD)geo, attributes);

                        }
                    }

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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("AD605FD9-D9E1-4BC7-8263-455FF84307E0"); }
        }
    }
}