using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeneticAlgorithms
{
    public class GeneticStrategies<T>
    {
        public Func<T, double> FitnessStrategy;
        public StrategyStore ReproductionStrategies = new StrategyStore();
        public StrategyStore MutationStrategies = new StrategyStore();
        public StrategyStore GenerationStrategies = new StrategyStore();

        public GeneticStrategies(Func<T, double> fitnessStrategy)
        {
            FitnessStrategy = fitnessStrategy;
        }
    }

    public class StrategyStore : List<IStrategy>
    {
        public IStrategy GetStrategy<T, V>(string propName = null)
        {
            var result = this.Where(x => 
                            x.ContainerType.FullName == typeof(T).FullName
                            && x.PropertyType.FullName == typeof(V).FullName).ToArray();

            return result.SingleOrDefault(x => result.Count() == 1 || x.PropertyName == propName);
        }
    }
}
