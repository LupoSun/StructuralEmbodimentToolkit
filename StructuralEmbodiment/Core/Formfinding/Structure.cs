using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StructuralEmbodimentToolkit.Core.Formfinding
{
    public abstract class Structure
    {
        public List<Member> Members { get; set; }
        public List<Point3d> Supports { get; set; }
        public Interval ForceRange { get; set; }
        public Interval ForceRangeUnsigned { get; set; }

        public Structure(List<Member> members, List<Point3d> supports)
        {
            Members = members;
            Supports = supports;
            ComputeForceRange(Members);
        }
        

        private void ComputeForceRange(List<Member> members)
        {
            if (members == null || members.Count == 0)
            {
                ForceRange = new Interval(0.0, 0.0);
                //this.ForceRange = new List<double> { 0.0, 0.0 }; // Or handle empty list appropriately
            }

            double minForce = double.MaxValue;
            double maxForce = double.MinValue;

            foreach (Member member in members)
            {
                minForce = Math.Min(minForce, member.Force);
                maxForce = Math.Max(maxForce, member.Force);
            }

            ForceRange = new Interval(minForce, maxForce);
            ForceRangeUnsigned = new Interval(Math.Abs(minForce), Math.Abs(maxForce));
            ForceRangeUnsigned.MakeIncreasing();
        }
        


    }

    public class Member
    {
        public int id { get; set; }
        public double Force { get; set; }
        public List<Point3d> EdgeAsPoints { get; set; }
        public MemberType MemberType { get; set; }
        public EdgeType EdgeType { get; set; }

        public Member(int id, double force, List<Point3d> edgeAsPoints, MemberType memberType, EdgeType edgeType)
        {
            this.id = id;
            Force = force;
            EdgeAsPoints = edgeAsPoints;
            MemberType = memberType;
            EdgeType = edgeType;
        }
        public override string ToString()
        {
            return "ID: "+id.ToString()+"\nMember Type: "+MemberType.ToString() +"\nEdge Type: " +EdgeType.ToString()+"\nForce: "+ Force.ToString();   
        }
    }
}
