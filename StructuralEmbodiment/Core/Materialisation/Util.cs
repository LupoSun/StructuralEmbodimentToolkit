﻿using Rhino.Geometry;
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

        public static bool HaveSameDirection(Curve curve1, Curve curve2)
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
    }
}
