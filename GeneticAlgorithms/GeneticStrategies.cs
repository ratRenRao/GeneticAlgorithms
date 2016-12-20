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
        public Func<PropertyInfo, float> GenerationStrategy { get; set; }
        public Func<T, float> FitnessStrategy { get; set; }
        public Func<float, float, float> ReproductionStrategy { get; set; }
        public Func<float, float> MutationStrategy { get; set; }

        public GeneticStrategies(Func<PropertyInfo, float> generationStrategy, Func<T, float> fitnessStrategy, Func<float, float, float> reproductionStrategy, Func<float, float> mutationStrategy)
        {
            GenerationStrategy = generationStrategy;
            FitnessStrategy = fitnessStrategy;
            ReproductionStrategy = reproductionStrategy;
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
