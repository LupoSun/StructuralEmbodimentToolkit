using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace StructuralEmbodiment.Components.Visualisation
{
    public class InputRecorder : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public InputRecorder()
          : base("Input Recorder", "IR",
              "Record all inputs on the canvas with a user defined tag as suffix",
              "Structural Embodiment", "Visualisation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Tag", "T", "Tag the inputs with a customised string (_X) to be recorded, by default _S", GH_ParamAccess.item);
            pManager.AddGenericParameter("Refresh", "R", "Refresh the data", GH_ParamAccess.tree);
            pManager.AddTextParameter("File Path", "P", "Path to the folder to save the data", GH_ParamAccess.item);
            pManager.AddTextParameter("File Name", "FN", "Name of the file to save the data", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Index", "I", "Index of the data", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write", "W", "Write to file", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Header", "H", "Header of the gethered data", GH_ParamAccess.list);
            pManager.AddGenericParameter("Values", "R", "The gethered data", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Initialise the variables
            string tag = "_S";
            string fileName = "data";
            string filePath = null;
            bool write = false;
            int index = 0;

            DA.GetData("Tag", ref tag);
            DA.GetData("File Name", ref fileName);
            DA.GetData("File Path", ref filePath);
            DA.GetData("Index", ref index);
            if (!string.IsNullOrEmpty(filePath))
            {
                filePath = Path.Combine(filePath, fileName);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                fileName += ("_" + index.ToString() + ".csv");
                filePath = Path.Combine(filePath, fileName);
            }
            
            DA.GetData("Write", ref write);


            var ghDoc = Grasshopper.Instances.ActiveCanvas.Document; // Get the active Grasshopper document
            var inputDataDict = new Dictionary<string, string>(); // Dictionary to store input data
            List<string> test = new List<string>();

            if (ghDoc != null)
            {
                foreach (IGH_DocumentObject docObj in ghDoc.Objects) // Iterate over all objects in the document
                {
                    if (docObj is IGH_Component component) // Check if the object is a component
                    {
                        string componentName = component.Name;

                        foreach (IGH_Param param in component.Params.Input) // Iterate over all input parameters of the component
                        {
                            // Check if the parameter name ends with "_S"
                            if (param.NickName.EndsWith(tag))
                            {
                                if (param.SourceCount > 0) // Check if the parameter has sources (inputs)
                                {
                                    // Construct key without "_S"
                                    string key = componentName + "//" + param.NickName.Replace(tag, "");

                                    for (int i = 0; i < param.SourceCount; i++)
                                    {
                                        var source = param.Sources[i];
                                        if (source is IGH_Param sourceParam) // Check if the source is a parameter
                                        {
                                            var dataTree = sourceParam.VolatileData; // Get the data tree from the source parameter
                                            if (dataTree != null)
                                            {
                                                foreach (var path in dataTree.Paths) // Iterate over all paths in the data tree
                                                {
                                                    var branch = dataTree.get_Branch(path); // Get the branch at the current path
                                                    foreach (var item in branch) // Iterate over all items in the branch
                                                    {
                                                        /*
                                                        if (!inputDataDict.ContainsKey(key))
                                                        {
                                                            inputDataDict[key] = null;
                                                        }
                                                        */
                                                        test.Add(item.GetType().ToString());
                                                        if (item is GH_Number)  inputDataDict[key] = ((GH_Number)item).ToString();
                                                        else if (item is GH_Integer) inputDataDict[key] = ((GH_Integer)item).ToString();
                                                        else if (item is GH_String) inputDataDict[key] = ((GH_String)item).Value;
                                                        else if (item is GH_Boolean) inputDataDict[key] = ((GH_Boolean)item).ToString();
                                                        else if (item is GH_Point) inputDataDict[key] = ((GH_Point)item).ToString();
                                                        else if (item is GH_Vector) inputDataDict[key] = ((GH_Vector)item).ToString();
                                                         // Add the item to the list under the key
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var sortedInputData = inputDataDict.OrderBy(entry => entry.Key);

            //Handle the writing to file
            if (write && !string.IsNullOrEmpty(filePath))
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        
                        foreach (var entry in sortedInputData)
                        {
                            string key = entry.Key;
                            string value = entry.Value.Contains(",") ? $"\"{entry.Value}\"" : entry.Value;
                            writer.WriteLine($"{key},{value}");
                        }
                    }

                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            List<string> keys = new List<string>();
            List<string> values = new List<string>();
            foreach (var entry in sortedInputData)
            {
                string key = entry.Key;
                string value = entry.Value;
                keys.Add(key);
                values.Add(value);
            }
            DA.SetDataList("Header", keys);
            DA.SetDataList("Values", values);
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
            get { return new Guid("F15DA333-B5B1-41F7-AB3B-7F91593A8CAF"); }
        }
    }
}