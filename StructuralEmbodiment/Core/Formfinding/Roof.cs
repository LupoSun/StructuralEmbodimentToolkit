using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace StructuralEmbodiment.Core.Formfinding
{
    public class Roof: Structure
    {
        public List<List<Member>> SortedMembers { get; set; }
        public List<List<Point3d>> SortedSupports { get; set; }
        public List<Point3d> NakedSupports { get; set; }
        public List<LineCurve> Boundary { get; set; }
        public List<LineCurve> Borders { get; set; }
        public Mesh SurfaceMesh { get; set; }

        public Roof(List<Member> members, List<Point3d> supports,Mesh surfaceMesh, List<List<Member>> sortedMembers, List<List<Point3d>> sortedSupports, List<Point3d> nakedSupports, List<LineCurve> boundary, List<LineCurve> borders) : base(members, supports)
        {
            SurfaceMesh = surfaceMesh;
            SortedMembers = sortedMembers;
            SortedSupports = sortedSupports;
            NakedSupports = nakedSupports;
            Boundary = boundary;
            Borders = borders;
        }
    }
}
