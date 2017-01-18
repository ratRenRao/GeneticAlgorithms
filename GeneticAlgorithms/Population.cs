using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class Population<T> : IPopulation<T> where T : IMember
    {
        private int _idIndex;
        
        //public GeneticAlgorithmExecutor<T> AlgorithmExecutor { get; set; }
        public List<T> Members { get; private set; }

        public Population(List<T> members)
        {
            //Strategies = strategies;
            //AlgorithmExecutor = new GeneticAlgorithmExecutor<T>(Strategies);
            Members = members;
        }

        //public void CreateNewPopulation(int size, Func<PropertyInfo, object> generationStrategy)
        //{
        //    Members = AlgorithmExecutor.CreateNewPopulation(size, generationStrategy);
        //}

        public void AddChild(T child)
        {
            Members.Add(child);
        }

        public void AddChildren(IEnumerable<T> children)
        {
            children.ToList().ForEach(x => Members.Add(x));
        }

        public void KillMember(int id)
        {
            var member = Members.SingleOrDefault(x => x.Id == id);
            Members.Remove(member);
        }

        public List<T> GetMembers() { return Members; }
        public int GetCurrentIdIndex() { return _idIndex; }
        public void IncrementIdIndex() { _idIndex++; }
    }
}
