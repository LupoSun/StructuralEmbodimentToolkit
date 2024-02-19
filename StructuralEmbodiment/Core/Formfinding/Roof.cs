using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rhino.Geometry;

namespace StructuralEmbodimentToolkit.Core.Formfinding
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

        // Loft to materialize the Roof Structure, index0 for the bordary beams, index1 for the inner beams
        public List<List<Brep>> LoftRoof(int crossSection, double multiplier, Interval range, double minimalThickness, double tolerance,out List<Curve> test)
        {
            var boundaryBeams = new List<Brep>();
            var innerBeams = new List<Brep>();
            test = new List<Curve>();
            //Make sure normals are computed
            this.SurfaceMesh.Normals.ComputeNormals();
            var crossSections = new List<List<Curve>>();
            var crossSectionsBoundary = new List<List<Curve>>();

            double genericWidth = 1;
            double genericHeight = 1.5;

            foreach (var memberStripe in SortedMembers)
            {
                var isBoundaryBeam = false;
                var crossSectionsBranch = new List<Curve>();
                for (int i=0; i < memberStripe.Count-1; i++)
                {
                    var member1 = memberStripe[i];
                    var member2 = memberStripe[i + 1];
                    Point3d node = member1.EdgeAsPoints[1];
                    double force1 = Math.Abs(member1.Force);
                    double force2 = Math.Abs(member2.Force);
                    double crossSectionSize = Util.ValueRemap((force1+force2) / 2, ForceRange, range) * multiplier;
                    if (crossSectionSize < minimalThickness) crossSectionSize = minimalThickness;
                    double width = genericWidth * crossSectionSize;
                    double height = genericHeight * crossSectionSize;

                    var meshPt = SurfaceMesh.ClosestMeshPoint(node, tolerance*10);
                    var normal = SurfaceMesh.NormalAt(meshPt);

                    var zAxis1 = new Vector3d(member1.EdgeAsPoints[1] - member1.EdgeAsPoints[0]);
                    var zAxis2 = new Vector3d(member2.EdgeAsPoints[1] - member2.EdgeAsPoints[0]);
                    var zAverage = (zAxis1 + zAxis2) / 2;
                    var xAxis = Vector3d.CrossProduct(normal, zAverage);
                    var plane = new Plane(node, xAxis, normal);

                    if (Core.Util.IsPointInCloud(NakedSupports, memberStripe[0].EdgeAsPoints[0], tolerance) &&
                        Core.Util.IsPointInCloud(NakedSupports, memberStripe[0].EdgeAsPoints[1], tolerance)
                        )
                    {
                        xAxis = Vector3d.CrossProduct(-Vector3d.ZAxis, zAverage);
                        plane = new Plane(node, xAxis, -Vector3d.ZAxis);
                        isBoundaryBeam = true;

                    } 

                    //In case of the start of the stripe
                    if (i == 0)
                    {
                        var zAxis0 = Core.Util.ClosestWorldAxis(zAxis1);
                        var xAxis0 = Vector3d.CrossProduct(-Vector3d.ZAxis, zAxis0);
                        var plane0 = new Plane(member1.EdgeAsPoints[0], xAxis, -Vector3d.ZAxis);
                        if (crossSection == 0)
                        {
                            crossSectionsBranch.Add(new Circle(plane0, crossSectionSize / 2).ToNurbsCurve());
                        }
                        else if (crossSection == 1)
                        {
                            crossSectionsBranch.Add(new Rectangle3d(plane0, new Interval(-width / 2, width / 2), new Interval(-height / 2, height / 2)).ToNurbsCurve());
                        }
                    }

                    //Check if the member connects two sorted supports
                    if (member1.MemberType == MemberType.Generic)
                    {
                        crossSections.Add(crossSectionsBranch);
                        crossSectionsBranch = new List<Curve>();
                    }

                    if (crossSection == 0) { 
                        
                        crossSectionsBranch.Add(new Circle(plane, crossSectionSize/2).ToNurbsCurve());

                    } else if(crossSection==1) {
                        crossSectionsBranch.Add(new Rectangle3d(plane, new Interval(-width / 2, width / 2), new Interval(-height / 2, height / 2)).ToNurbsCurve());
                    }


                    //In case of the end of the stripe
                    if (i == memberStripe.Count - 2)
                    {
                        var zAxis0 = Core.Util.ClosestWorldAxis(zAxis2);
                        var xAxis0 = Vector3d.CrossProduct(-Vector3d.ZAxis, zAxis0);
                        var plane0 = new Plane(member2.EdgeAsPoints[1], xAxis, -Vector3d.ZAxis);
                        if (crossSection == 0)
                        {
                            crossSectionsBranch.Add(new Circle(plane0, crossSectionSize / 2).ToNurbsCurve());
                        }
                        else if (crossSection == 1)
                        {
                            crossSectionsBranch.Add(new Rectangle3d(plane0, new Interval(-width / 2, width / 2), new Interval(-height / 2, height / 2)).ToNurbsCurve());
                        }
                    }

                } 
                if(isBoundaryBeam) crossSectionsBoundary.Add(crossSectionsBranch);
                else crossSections.Add(crossSectionsBranch);
                test.AddRange(crossSectionsBranch);
            }

            test = crossSections[0];

            foreach (var crossSectionsBranch in crossSections)
            {
                foreach (var brep in Brep.CreateFromLoft(crossSectionsBranch, Point3d.Unset, Point3d.Unset, LoftType.Straight, false)) {
                    innerBeams.Add(brep.CapPlanarHoles(tolerance));
                }
                
            }
            foreach(var crossSectionsBranch in crossSectionsBoundary)
            {
                foreach (var brep in Brep.CreateFromLoft(crossSectionsBranch, Point3d.Unset, Point3d.Unset, LoftType.Straight, false))
                {
                    boundaryBeams.Add(brep.CapPlanarHoles(tolerance));
                }
            }
            
            return new List<List<Brep>> { boundaryBeams, innerBeams,};
        }
    }
}
