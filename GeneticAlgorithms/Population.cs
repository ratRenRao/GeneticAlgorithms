using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class Population<T> where T : Member
    {
        public GeneticStrategies<T> Strategies { get; set; }
        public GeneticAlgorithm<T> Algorithm { get; set; }
        public IEnumerable<T> Members { get; private set; }

        public Population(GeneticStrategies<T> strategies, int populationSize)
        {
            Strategies = strategies;
            Algorithm = new GeneticAlgorithm<T>(Strategies);
            Members = Algorithm.CreateNewPopulation(populationSize, Strategies.GenerationStrategy);
        }

        public void ReinitializePopulation(int size, Func<PropertyInfo, object> generationStrategy)
        {
            Members = Algorithm.CreateNewPopulation(size, generationStrategy);
        }

        public void AddChildren(IEnumerable<T> children)
        {
            Members = Members.Concat(children);// = new T[Members.Count() + children.Length]
        }

        public void KillMember(int id)
        {
            Members = Members.Where(x => x.Id != id);
        }

        public void CreateNextGeneration(int growthSize)
        {
            AddChildren(Algorithm.NextGeneration(this, growthSize, Strategies));
        }

        public void KillOffLowFitnessMembers(double bottomPercentileToKill)
        {
            var bottomPercentileCount = (int)(bottomPercentileToKill/(100d/Members.Count()));
            foreach(var unfitMember in Members.Where(x => x.Fitness != 0).OrderByDescending(x => -x.Fitness).Take(bottomPercentileCount))
            {
                KillMember(unfitMember.Id);
            }
        }
    }
}
