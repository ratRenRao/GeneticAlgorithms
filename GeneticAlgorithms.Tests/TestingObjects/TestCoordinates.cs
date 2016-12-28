namespace GeneticAlgorithms.Tests.TestingObjects
{
    public class TestCoordinates : Member
    {
        private static int _idCount = 0;
        public float X { get; set; }
        public float Y { get; set; }


        public TestCoordinates()
        {
            Id = _idCount++;
        }

        public TestCoordinates(float x, float y)
        {
            X = x;
            Y = y;
            Id = _idCount++;
        }
    }
}
