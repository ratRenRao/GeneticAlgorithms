using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class GeneticAlgorithm<T>
    {
        private readonly Random _rand;
        public Func<PropertyInfo, float> GenerationStrategy { get; set; }
        public Func<T, float> FitnessStrategy { get; set; }
        public Func<float, float, float> ReproductionStrategy { get; set; }
        public Func<float, float> MutationStrategy { get; set; }

        public GeneticAlgorithm(Func<PropertyInfo, float> generationStrategy, Func<T, float> fitnessStrategy, Func<float, float, float> reproductionStrategy, Func<float, float> mutationStrategy)
        {
            GenerationStrategy = generationStrategy;
            FitnessStrategy = fitnessStrategy;
            ReproductionStrategy = reproductionStrategy;
            MutationStrategy = mutationStrategy;
            _rand = new Random();
        }

        public IEnumerable<T> InitializePopulation(int populationSize, Func<PropertyInfo, float> generationStrategy)
        {
            T[] population = new T[populationSize];

            for (int i = 0; i < populationSize; i++)
            {
                population[i] = GenerateRandomIndividualByProperty(generationStrategy);
            }

            return population;
        }

        public T GenerateRandomIndividualByProperty(Func<PropertyInfo, float> generationStrategy)
        {
            var props = typeof (T).GetProperties();
            var newIndividual = (T)Activator.CreateInstance(typeof(T));
            foreach (var attribute in props) //typeof(T).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public))
            {
                typeof(T).GetProperty(attribute.Name).SetValue(newIndividual, generationStrategy.Invoke(attribute));
            }

            return newIndividual;
        }

        // Feature not yet implemented
        public T GenerateRandomIndividualByConstructor(Func<ParameterInfo, float> generationStrategy)
        {
            var constructor = typeof(T).GetConstructors().OrderByDescending(x => x.GetParameters().Length).First();
            List<object> newParameters = new List<object>();
            foreach (var attribute in constructor.GetParameters())
            {
                newParameters.Add(generationStrategy.Invoke(attribute));
            }

            return (T)Activator.CreateInstance(typeof(T), newParameters.ToArray());
        }

        public float CalculateFitness(T individual)
        {
            return CalculateFitness(individual, FitnessStrategy);
        }

        public float CalculateFitness(T individual, Func<T, float> fitnessStrategy)
        {
            return fitnessStrategy.Invoke(individual);
        }

        public IEnumerable<T> NextGeneration(T[] initialSet, int populationSize, Func<float, float, float> reproductionStrategy)
        {
            T[] population = new T[populationSize];

            for (int i = 0; i < populationSize; i++)
            {
                population[i] = Reproduce(initialSet[_rand.Next(initialSet.Length)], initialSet[_rand.Next(initialSet.Length)], reproductionStrategy);
            }

            return population;
        }

        public T Reproduce(T firstParent, T secondParent, Func<float, float, float> reproductionStrategy)
        {
            var objectProperties = typeof (T).GetProperties();
            List<object> newProperties = new List<object>();
            foreach (var attribute in objectProperties)
            {
                newProperties.Add(reproductionStrategy.Invoke((float)attribute.GetValue(firstParent), (float)attribute.GetValue(secondParent)));
            }

            return (T)Activator.CreateInstance(typeof(T), newProperties.ToArray());
        }

        public T MutateIndividual(T individual)
        {
            return MutateIndividual(individual, MutationStrategy);
        }

        public T MutateIndividual(T individual, Func<float, float> mutationStrategy)
        {
            var props = typeof(T).GetProperties();
            foreach (var attribute in props) //typeof(T).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public))
            {
                typeof(T).GetProperty(attribute.Name).SetValue(individual, mutationStrategy.Invoke((float)attribute.GetValue(individual)));
            }

            return individual;
        }
    }
}
