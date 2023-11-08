using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace StructuralEmbodiment
{
    public class StructuralEmbodimentInfo : GH_AssemblyInfo
    {
        public override string Name => "StructuralEmbodiment";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("753ecf76-39dd-417e-8b19-952d1a673ded");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}