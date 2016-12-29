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
        public Func<PropertyInfo, float> GenerationStrategy { get; set; } = info => 0;
        public Func<T, float> FitnessStrategy { get; set; } = arg => 0;
        public Dictionary<Type, Func<float, float, float>> ReproductionStrategies = new Dictionary<Type, Func<float, float, float>>();
        public Func<float, float> MutationStrategy { get; set; } = f => 0;

        public GeneticStrategies(Func<PropertyInfo, float> generationStrategy, Func<T, float> fitnessStrategy, Func<float, float> mutationStrategy)
        {
            GenerationStrategy = generationStrategy;
            FitnessStrategy = fitnessStrategy;
            MutationStrategy = mutationStrategy;
        }
    }

    public enum StrategyType
    {
        Generation,
        Fitness,
        Reproduction,
        Mutation
    }
}
