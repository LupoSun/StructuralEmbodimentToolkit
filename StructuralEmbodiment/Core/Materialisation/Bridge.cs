﻿using Rhino;
using Rhino.Geometry;
using System;
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
                TowerSupports = (List<Point3d>)supports.Except(deckSupport).ToList().Except(nonDeckSupport).ToList();
                RegisterTowerMembers();
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
            }
            else throw new System.Exception("Deck Outlines are not 2, try to increase the tolerance");

        }
        public void RegisterTowerMembers()
        {
            var towerMembers = new List<Member>();
            FindConnectedMembersRecursive(
                this.TowerSupports.First(),
                this.Members.Where(m => m.EdgeType == EdgeType.TrailEdge).ToList(),
                null,
                ref towerMembers);
            foreach (Member member in towerMembers)
            {
                member.MemberType = MemberType.Tower;
            }
        }

        public Dictionary<Point3d, List<Member>> FindConncectedTrails()
        {

            var connectedMembersDict = new Dictionary<Point3d, List<Member>>();

            // Initialize dictionary with support points
            var supports = new List<Point3d>(this.Supports);
            Util.RemoveClosestToCenter(supports);
            foreach (Point3d support in supports)
            {
                var connectedMembers = new List<Member>();
                FindConnectedMembersRecursive(support, this.Members, null, ref connectedMembers);
                connectedMembersDict[support] = connectedMembers;
            }

            return connectedMembersDict;
        }

        private void FindConnectedMembersRecursive(Point3d currentPoint, List<Member> membersPool, Member previousMember, ref List<Member> connectedMembers)
        {
            foreach (var member in membersPool)
            {
                if (member.EdgeType == EdgeType.DeviationEdge) continue;
                if (member == previousMember) continue;

                if (Util.IsPointConnectedToMember(currentPoint, member))
                {
                    connectedMembers.Add(member);
                    var nextPoint = member.EdgeAsPoints[0].EpsilonEquals(currentPoint, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance) ? member.EdgeAsPoints[1] : member.EdgeAsPoints[0];
                    FindConnectedMembersRecursive(nextPoint, membersPool, member, ref connectedMembers);
                    break; // Assuming one member doesn't connect to multiple members at the same point
                }
            }
        }

        public List<Plane> ComputePerpendicularPlanesAtPoints(List<Member> connectedMembers)
        {
            var points = new List<Point3d>();
            foreach (Member member in connectedMembers)
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

                planes.Add(new Plane(polyline[i], xAxis, yAxis));
            }

            return planes;

        }

        public List<Brep> LoftTrails(Dictionary<Point3d, List<Member>> connnectedMembersDict, int crossSection, double multiplier, Interval range, bool IncludeDeckTrails)
        {
            var breps = new List<Brep>();
            foreach (KeyValuePair<Point3d, List<Member>> kvp in connnectedMembersDict)
            {
                var crossSectinos = new List<Curve>();
                var connectedMembers = kvp.Value;
                if (!IncludeDeckTrails && connectedMembers.First().MemberType == MemberType.Deck)
                {
                    continue;
                }
                var planes = ComputePerpendicularPlanesAtPoints(connectedMembers);
                var forces = ComputeConnectedMemberCrosssection(connectedMembers);
                for (int i = 0; i < planes.Count; i++)
                {
                    var plane = planes[i];
                    var force = forces[i];
                    //Remap the unsigned force from the unsigned force range to the new range
                    double crossSectionSize = Util.ValueRemap(Math.Abs(force), this.ForceRangeUnsigned, range) * multiplier;
                    if (crossSection == 0)
                    {
                        crossSectinos.Add(new Circle(plane, crossSectionSize / 2).ToNurbsCurve());
                    }
                    else if (crossSection == 1)
                    {
                        crossSectinos.Add(new Rectangle3d(plane, new Interval(-crossSectionSize / 2, crossSectionSize / 2), new Interval(-crossSectionSize / 2, crossSectionSize / 2)).ToNurbsCurve());
                    }
                }

                var brep = Brep.CreateFromLoft(crossSectinos, Point3d.Unset, Point3d.Unset, LoftType.Tight, false)[0];
                breps.Add(brep.CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance));


            }
            return breps;
        }
        public List<Brep> LoftDeviations(int crossSection, double multiplier, Interval range)
        {
            var breps = new List<Brep>();
            double cableThicknessCoeff = 0.1;
            double compressionMemberCoeff = 3 * cableThicknessCoeff;
            foreach (Member m in this.Members)
            {
                if (m.EdgeType == EdgeType.DeviationEdge)
                {
                    //If a deviation edge is in tension
                    double crossSectionSize;
                    if (m.Force > 0)
                    {
                        m.MemberType = MemberType.Cabel;
                        crossSectionSize = Util.ValueRemap(this.ForceRange.T1, this.ForceRangeUnsigned, range);
                        crossSectionSize *= (multiplier * cableThicknessCoeff);

                        //If a deviation edge is in compression
                    }
                    else
                    {
                        //If the compresion members have smaller forces than the tension members
                        if (Math.Abs(m.Force) < Math.Abs(this.ForceRange.T1))
                        {
                            crossSectionSize = Util.ValueRemap(Math.Abs(this.ForceRange.T1), this.ForceRangeUnsigned, range);
                        }
                        else crossSectionSize = Util.ValueRemap(Math.Abs(m.Force), this.ForceRangeUnsigned, range);

                        crossSectionSize *= (multiplier * compressionMemberCoeff);

                    }
                    breps.Add(Brep.CreatePipe(
                            new LineCurve(m.EdgeAsPoints[0], m.EdgeAsPoints[1]),
                            crossSectionSize / 2,
                            false,
                            PipeCapMode.Round,
                            true,
                            RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                            RhinoDoc.ActiveDoc.ModelAngleToleranceRadians)[0]);

                }
            }
            return breps;
        }

        public List<Brep> LoftDeck(double multiplier, Interval range)
        {
            //Compute the cross section value
            double deckThicknessCoeff = 0.5;
            double absMaxForce = this.Members
                .Where(m => m.MemberType == MemberType.Deck)
                .Select(m => Math.Abs(m.Force))
                .DefaultIfEmpty(0) // This ensures a default value if the collection is empty
                .Max();
            double crossSectionSize = Util.ValueRemap(absMaxForce, this.ForceRangeUnsigned, range);
            crossSectionSize *= (multiplier * deckThicknessCoeff);

            //Move and loft the deck
            Transform move = Transform.Translation(0, 0, (crossSectionSize / 2));
            foreach (Curve outline in this.DeckOutlines)
            {
                outline.Transform(move);
            }
            List<Brep> brepVolumes = new List<Brep>();
            // Convert polylines to curves
            var loftCurves = this.DeckOutlines.Select(polyline => polyline.ToNurbsCurve()).ToList();

            // Create the lofted surface
            var loftedSurface = Brep.CreateFromLoft(loftCurves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);

            // Check if loft was successful
            if (loftedSurface == null || loftedSurface.Length == 0)
                return brepVolumes;

            // Extrude the loft surface
            var extrusionPath = new LineCurve(new Point3d(0, 0, 0), new Point3d(0, 0, (-crossSectionSize)));
            foreach (var face in loftedSurface[0].Faces)
            {
                Brep faceBrep = face.DuplicateFace(false);
                Brep extrudedBrep = faceBrep.Faces[0].CreateExtrusion(extrusionPath,true);
                brepVolumes.Add(extrudedBrep);
            }

            return brepVolumes;
        }

        /*
         * <summary>
         * [Needs to go private]Computes the cross-sectional forces for each member in the connected members list.
         * </summary>
         * <param name="connectedMembers">The list of connected members.</param>
         * <returns>A list of cross-sectional forces.</returns>
         */
        public List<double> ComputeConnectedMemberCrosssection(List<Member> connectedMembers)
        {
            var crossSectionalForces = new List<double>();

            if (connectedMembers == null || !connectedMembers.Any())
            {
                return crossSectionalForces; // Return empty list if there are no members
            }

            // Add the force of the first member
            crossSectionalForces.Add(connectedMembers.First().Force);

            // Handle intermediate members
            for (int i = 0; i < connectedMembers.Count - 1; i++)
            {
                double currentForce = connectedMembers[i].Force;
                double nextForce = connectedMembers[i + 1].Force;
                double maxForce = Math.Max(currentForce, nextForce);

                crossSectionalForces.Add(maxForce);
            }

            // Add the force of the last member
            crossSectionalForces.Add(connectedMembers.Last().Force);

            return crossSectionalForces;
        }




        public override string ToString()
        {

            return this.GetType().ToString();
        }

    }
}
