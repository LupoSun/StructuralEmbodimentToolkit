using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Linq;

namespace StructuralEmbodiment
{
    public class StructuralEmbodimentInfo : GH_AssemblyInfo
    {
        public override string Name => "StructuralEmbodiment";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resources.SE_Tab;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "A toolkit for form-finding, materialisation and visualisation via deep learning methods";

        public override Guid Id => new Guid("753ecf76-39dd-417e-8b19-952d1a673ded");

        //Return a string identifying you or your company.
        public override string AuthorName => "Tao Sun";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "tao.sun.lupo@gmail.com";
    }
    
    //Add the tab icon 
    public class SE_CategoryIcon : Grasshopper.Kernel.GH_AssemblyPriority
    {
        public override Grasshopper.Kernel.GH_LoadingInstruction PriorityLoad()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(n => n.EndsWith("SE_Tab.png"));
            var stream = assembly.GetManifestResourceStream(resourceName);
            Bitmap dasIcon = new Bitmap(stream);
            Grasshopper.Instances.ComponentServer.AddCategoryIcon("Structural Embodiment", dasIcon);
            Grasshopper.Instances.ComponentServer.AddCategorySymbolName("Structural Embodiment", 'S');
            return Grasshopper.Kernel.GH_LoadingInstruction.Proceed;

        }
    }
}