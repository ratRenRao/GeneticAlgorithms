using System.Security.Principal;

namespace GeneticAlgorithms.Tests.TestingObjects
{
    public class TestCoordinates : Member
    {
        public double X { get; set; }
        public double  Y { get; set; }
        public NestedAttribute Z { get; set; }

        public TestCoordinates(int generation) : base(generation)
        {
        }

        public TestCoordinates(double x, double y, int generation) : base(generation)
        {
            X = x;
            Y = y;
        }

        public void Initialize()
        {
        }
    }

    public class NestedAttribute
    {
        public int NestedVal { get; set; }

        public NestedAttribute(int n)
        {
            NestedVal = n;
        }
    }
}
