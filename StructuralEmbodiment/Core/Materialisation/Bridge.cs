using Rhino;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace StructuralEmbodiment.Core.Materialisation
{
    public class Bridge : Structure
    {
        public List<Curve> DeckOutlines { get; set; }
        public List<Point3d> DeckSupports { get; set; }
        public List<Point3d> NonDeckSupports { get; set; }
        public List<Point3d> TowerSupports { get; set; }

        public Bridge(List<Member> members, List<Point3d> supports, List<Point3d> deckSupport, List<Point3d> nonDeckSupport) : base(members, supports)
        {
            this.Members = members;
            this.Supports = supports;

            this.DeckSupports = deckSupport;
            this.NonDeckSupports = nonDeckSupport;

            if (supports.Count - deckSupport.Count - nonDeckSupport.Count == 1)
            {
                TowerSupports = (List<Point3d>)supports.Except(deckSupport).Except(nonDeckSupport);
            }
            ComputeDeckOutlines();

        }

        public void ComputeDeckOutlines()
        {
            var filteredMembers = this.Members.Where(m => m.MemberType == MemberType.Deck && m.EdgeType == EdgeType.TrailEdge).ToList();
            var lines = new List<LineCurve>();
            //Connect the individual members to form continuous outlines
            foreach (Member member in filteredMembers)
            {
                LineCurve line = new LineCurve(member.EdgeAsPoints[0], member.EdgeAsPoints[1]);
                lines.Add(line);
            }
            List<Curve> outlines = Curve.JoinCurves(lines, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 10).ToList();
            this.DeckOutlines = outlines;

            if (this.DeckOutlines.Count == 2)
            {
                if (!Util.HaveSameDirection(this.DeckOutlines[0], this.DeckOutlines[1]))
                {
                    this.DeckOutlines[1].Reverse();
                }
            } else throw new System.Exception("Deck Outlines are not 2, try to increase the tolerance");


        }

        public Dictionary<Point3d, List<Member>> FindConncectedTrails()
        {

            var connectedMembersDict = new Dictionary<Point3d, List<Member>>();

            // Initialize dictionary with support points
            foreach (Point3d support in this.Supports)
            {
                var connectedMembers = new List<Member>();
                FindConnectedMembersRecursive(support,null, ref connectedMembers);
                connectedMembersDict[support] = connectedMembers;
            }

            return connectedMembersDict; 
        }

        private void FindConnectedMembersRecursive(Point3d currentPoint, Member previousMember, ref List<Member> connectedMembers)
        {
            foreach (var member in this.Members)
            {
                if (member.EdgeType == EdgeType.DeviationEdge) continue;
                if (member == previousMember) continue;

                if (Util.IsPointConnectedToMember(currentPoint, member))
                {
                    connectedMembers.Add(member);
                    var nextPoint = member.EdgeAsPoints[0].EpsilonEquals(currentPoint, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance) ? member.EdgeAsPoints[1] : member.EdgeAsPoints[0];
                    FindConnectedMembersRecursive(nextPoint, member, ref connectedMembers);
                    break; // Assuming one member doesn't connect to multiple members at the same point
                }
            }
        }

        public List<Plane> ComputePerpendicularPlanesAtPoints(List<Member> connectedMembers)
        {
            var points = new List<Point3d>();
            foreach(Member member in connectedMembers)
            {
                points.Add(member.EdgeAsPoints[0]);
                points.Add(member.EdgeAsPoints[1]);
            }
            points = Point3d.SortAndCullPointList(points, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToList();

            var polyline = new Polyline(points);

            var planes = new List<Plane>();

            for (int i = 0; i < polyline.Count; i++)
            {
                Vector3d tangent = Util.ComputePolylineTangentAt(polyline, i);

                // The Y-axis vector
                Vector3d yAxis = Vector3d.YAxis;

                // The cross product gives a vector perpendicular to both tangent and Y-axis,
                // and it will be parallel to the XZ plane
                Vector3d xAxis = Vector3d.CrossProduct(tangent, yAxis);
                xAxis.Unitize();

                planes.Add(new Plane(polyline[i], xAxis,yAxis));
            }

            return planes;

        }




        public override string ToString()
        {
          
        return this.GetType().ToString();
        }

    }
}
