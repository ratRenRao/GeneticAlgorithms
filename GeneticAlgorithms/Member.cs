using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class Member
    {
        private static int _idCount;
        public int Id = _idCount++;
        public float Fitness;
    }
}
