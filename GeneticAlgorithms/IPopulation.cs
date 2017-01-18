using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public interface IPopulation<T> where T : IMember
    {
        int GetCurrentIdIndex();
        void IncrementIdIndex();
        //void CreateNewPopulation(int size, Func<PropertyInfo, object> generationStrategy);
        void AddChildren(IEnumerable<T> children);
        void KillMember(int id);
        List<T> Members { get; }
        //void ProduceNextGeneration(int growthSize);
    }
}
