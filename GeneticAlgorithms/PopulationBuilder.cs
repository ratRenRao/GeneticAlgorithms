using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class PopulationBuilder<T> where T : IMember
    {
        private static int _generation = 1;
        public IPopulation<T> Population { get; set; }
        public GeneticStrategies<T> Strategies { get; private set; }
        public GeneticAlgorithmExecutor AlgorithmExecutor;

        public PopulationBuilder(GeneticStrategies<T> strategies)
        {
            Strategies = strategies;
            AlgorithmExecutor = new GeneticAlgorithmExecutor();
        }

        public void InitializePopulationMembers(int populationSize)
        {
            Population = new Population<T>(CreateNewPopulationMembers(populationSize, Strategies.GenerationStrategies));
        }

        public List<T> CreateNewPopulationMembers(int size, StrategyStore generationStrategies)
        {
            return AlgorithmExecutor.CreateNewPopulationMembers<T>(size, generationStrategies, _generation++);
        }

        public void ProduceNextGeneration(int growthSize)
        {
            var nextGeneration = AlgorithmExecutor.NextGeneration<T>(Population.Members, growthSize, Strategies.ReproductionStrategies, _generation++);
            Population.AddChildren(nextGeneration);
        }

        public void KillOffLowFitnessMembers(double bottomPercentileToKill)
        {
            var bottomPercentileCount = (int) (bottomPercentileToKill / (100d / Population.Members.Count()));
            foreach (var unfitMember in Population.Members.Where(x => x.Fitness != 0).OrderByDescending(x => -x.Fitness).Take(bottomPercentileCount))
            {
                Population.KillMember(unfitMember.Id);
            }
        }

        public void SetFitness(T member)
        {
            member.Fitness = AlgorithmExecutor.CalculateFitness(member, Strategies.FitnessStrategy);
        }
    }
}
