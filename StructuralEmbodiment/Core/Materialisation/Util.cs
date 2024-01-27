using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using StructuralEmbodiment.Core.Formfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructuralEmbodiment.Core.Materialisation
{
    internal static class Util
    {
        public static void RemoveClosestToCenter(List<Point3d> points)
        {
            if (points == null || points.Count == 0) return;
            if (points.Count % 2 != 0)
            {
                // Compute the centroid
                Point3d centroid = new Point3d(
                    points.Average(p => p.X),
                    points.Average(p => p.Y),
                    points.Average(p => p.Z));

                // Find the point closest to the centroid
                Point3d closestPoint = points.OrderBy(p => p.DistanceTo(centroid)).First();

                // Remove the closest point
                points.Remove(closestPoint);
            }
        }

        public static bool HaveSameDirectionVect(Curve curve1, Curve curve2)
        {
            // Get normalized tangent vectors at the start of the curves
            Vector3d tan1 = curve1.TangentAtStart;
            tan1.Unitize();

            Vector3d tan2 = curve2.TangentAtStart;
            tan2.Unitize();

            // Compute the dot product
            double dot = tan1 * tan2;

            // Check if dot product is close to 1 (same direction) or -1 (opposite direction)
            const double tolerance = 0.1; // Define a suitable tolerance
            return dot > 1 - tolerance;
        }

        public static bool HaveSameDirection(Curve curve1, Curve curve2)
        {
            var curve1Start = curve1.PointAtStart;
            var curve1End = curve1.PointAtEnd;
            var curve2Start = curve2.PointAtStart;

            return curve1End.DistanceTo(curve2Start) > curve1Start.DistanceTo(curve2Start);
        }

        public static bool IsPointConnectedToMember(Point3d point, Member member,double tolerance)
        {
            return member.EdgeAsPoints.Any(memberPoint => point.DistanceTo(memberPoint) < tolerance);
        }

        public static Vector3d ComputePolylineTangentAt(Polyline polyline, int index)
        {
            Vector3d tangent;
            if (index == 0)
            {
                // First point - use the first segment's tangent
                tangent = polyline[1] - polyline[0];
            }
            else if (index == polyline.Count - 1)
            {
                // Last point - use the last segment's tangent
                tangent = polyline[index] - polyline[index - 1];
            }
            else
            {
                // Middle points - average the tangents of adjacent segments
                Vector3d tangent1 = polyline[index] - polyline[index - 1];
                Vector3d tangent2 = polyline[index + 1] - polyline[index];
                tangent = (tangent1 + tangent2) * 0.5;
            }

            tangent.Unitize();
            return tangent;
        }

        public static double ValueRemap(double value, Interval from, Interval to)
        {
            if (from.Length == 0) return double.NaN; // Prevent division by zero

            // Calculate the proportion of 'value' in the original interval
            double proportion = (value - from.T0) / from.Length;

            // Apply the proportion to the new interval
            return to.T0 + (proportion * to.Length);
        }

        public static bool AreMembersConnected(Member member1, Member member2, double tolerance)
        {
            // Check if the members are the same instance or have the same identity
            if (ReferenceEquals(member1, member2))
            {
                return false; // The same member cannot be considered 'connected' to itself
            }

            // Check if any end point of member1 is close to any end point of member2
            return member1.EdgeAsPoints[0].EpsilonEquals(member2.EdgeAsPoints[0], tolerance) ||
                   member1.EdgeAsPoints[0].EpsilonEquals(member2.EdgeAsPoints[1], tolerance) ||
                   member1.EdgeAsPoints[1].EpsilonEquals(member2.EdgeAsPoints[0], tolerance) ||
                   member1.EdgeAsPoints[1].EpsilonEquals(member2.EdgeAsPoints[1], tolerance);
        }

        public static Point3d AveragePoint(List<Point3d> points)
        {
            if (points == null || !points.Any())
                return Point3d.Unset;

            Point3d sum = new Point3d(0,0,0);
            foreach (var pt in points)
            {
                sum += pt;
            }
            return sum / points.Count;
        }

        public static bool IsMemberConnectingOutlines(Member member, List<Curve> deckOutlines, double tolerance)
        {
            if (member.EdgeAsPoints.Count < 2) return false; // Check for at least 2 points

            // Assuming the member's start and end points are the first and last in EdgeAsPoints
            Point3d startPoint = member.EdgeAsPoints.First();
            Point3d endPoint = member.EdgeAsPoints.Last();

            bool startOnOutline1 = IsPointOnCurve(startPoint, deckOutlines.First(),tolerance);
            bool endOnOutline2 = IsPointOnCurve(endPoint, deckOutlines.Last(), tolerance);
            bool startOnOutline2 = IsPointOnCurve(startPoint, deckOutlines.Last(), tolerance);
            bool endOnOutline1 = IsPointOnCurve(endPoint, deckOutlines.First(),tolerance);

            return (startOnOutline1 && endOnOutline2) || (startOnOutline2 && endOnOutline1);
        }

        public static bool IsPointOnCurve(Point3d point, Curve curve, double tolerance)
        {
            
                PolylineCurve polylineCurve = (PolylineCurve)curve;
                Polyline polyline = polylineCurve.ToPolyline();
                var pts = polyline.ToArray();
                return pts.Any(pt => pt.DistanceTo(point) < tolerance);
            
            
        }

        
    }
}
