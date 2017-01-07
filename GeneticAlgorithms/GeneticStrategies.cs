using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class GeneticStrategies<T>
    {
        public Func<PropertyInfo, object> GenerationStrategy { get; set; } = info => 0;
        public Func<T, float> FitnessStrategy { get; set; } = arg => 0;
        public Func<float, float> MutationStrategy { get; set; } = f => 0;

        public ReproductionStrategies ReproductionStrategies = new ReproductionStrategies();

        public GeneticStrategies(Func<PropertyInfo, object> generationStrategy, Func<T, float> fitnessStrategy, Func<float, float> mutationStrategy)
        {
            GenerationStrategy = generationStrategy;
            FitnessStrategy = fitnessStrategy;
            MutationStrategy = mutationStrategy;
        }

        //public Strategy<dynamic> ReproductionStrategy<TV>() => ReproductionStrategies.SingleOrDefault(x => x.PropertyInfo.PropertyType == typeof(TV));
    }

    public class ReproductionStrategies : List<Strategy<object>>
    {
        public Strategy<object> GetReproductionStrategy(Type reprodutionType)
        {
            return this.SingleOrDefault(x => x.PropertyInfo.PropertyType == reprodutionType);
        }
    }

    public class Strategy<TV>
    {
        public PropertyInfo PropertyInfo { get; set; }
        public Func<TV, TV, TV> Method { get; set; }
        public StrategyType StrategyType { get; set; }
    }

    public enum StrategyType
    {
        Generation,
        Fitness,
        Reproduction,
        Mutation
    }
}
