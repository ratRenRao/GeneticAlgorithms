using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class Population<T>
    {
        public GeneticStrategies<T> Strategies { get; set; }
        public GeneticAlgorithm<T> Algorithm { get; set; }
        public IEnumerable<T> Individuals { get; private set; }

        public Population(GeneticStrategies<T> strategies, int populationSize)
        {
            Strategies = strategies;
            Algorithm = new GeneticAlgorithm<T>(Strategies);
            Individuals = Algorithm.InitializePopulation(populationSize, Strategies.GenerationStrategy);
        }
    }
}
