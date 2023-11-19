using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StructuralEmbodiment.Core.Materialisation
{
    public abstract class Structure
    {
        public List<Member> Members { get; set; }
        public List<Point3d> Supports { get; set; }
        public Interval ForceRange { get; set; }
        public Interval ForceRangeUnsigned { get; set; }
        public List<LineCurve> TrailEdges { get; set; }
        public List<LineCurve> DeviationEdges { get; set; }

        public Structure(List<Member> members, List<Point3d> supports)
        {
            this.Members = members;
            this.Supports = supports;
            ComputeForceRange(this.Members);
            ComputeCEMEdges();
        }

        private void ComputeForceRange(List<Member> members)
        {
            if (members == null || members.Count == 0)
            {
                this.ForceRange = new Interval(0.0, 0.0);
                //this.ForceRange = new List<double> { 0.0, 0.0 }; // Or handle empty list appropriately
            }

            double minForce = double.MaxValue;
            double maxForce = double.MinValue;

            foreach (Member member in members)
            {
                minForce = Math.Min(minForce, member.Force);
                maxForce = Math.Max(maxForce, member.Force);
            }

            this.ForceRange = new Interval(minForce, maxForce);
            this.ForceRangeUnsigned = new Interval(Math.Abs(minForce), Math.Abs(maxForce));
            this.ForceRangeUnsigned.MakeIncreasing();
        }
        private void ComputeCEMEdges()
        {
            this.TrailEdges = Members.Where(m => m.EdgeType == EdgeType.TrailEdge).Select(m => new LineCurve(m.EdgeAsPoints[0], m.EdgeAsPoints[1])).ToList();
            this.DeviationEdges = Members.Where(m => m.EdgeType == EdgeType.DeviationEdge).Select(m => new LineCurve(m.EdgeAsPoints[0], m.EdgeAsPoints[1])).ToList();
        }


    }

    public class Member
    {
        public int CEMid { get; set; }
        public double Force { get; set; }
        public List<Point3d> EdgeAsPoints { get; set; }
        public MemberType MemberType { get; set; }
        public EdgeType EdgeType { get; set; }

        public Member(int cemid, double force, List<Point3d> edgeAsPoints, MemberType memberType, EdgeType edgeType)
        {
            this.CEMid = cemid;
            this.Force = force;
            this.EdgeAsPoints = edgeAsPoints;
            this.MemberType = memberType;
            this.EdgeType = edgeType;
        }
    }
}
