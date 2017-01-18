using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public abstract class Member : IMember
    {
        private static int _id;
        protected Member(int generation)
        {
            Id = _id++;
            Generation = generation;
        }

        public int Id { get; }
        public double Fitness { get; set; }
        public int Generation { get; }

        public virtual void Initialize() { }
       
    }
}
