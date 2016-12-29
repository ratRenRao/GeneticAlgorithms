using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeneticAlgorithms
{
    public class GeneticAlgorithm<T> where T : Member
    {
        private readonly Random _rand;
        public GeneticStrategies<T> Strategies { get; set; }

        public GeneticAlgorithm(GeneticStrategies<T> strategies)
        {
            Strategies = strategies;
            _rand = new Random();
        }

        public IEnumerable<T> CreateNewPopulation(int populationSize)
        {
            return CreateNewPopulation(populationSize, Strategies.GenerationStrategy);
        }

        public IEnumerable<T> CreateNewPopulation(int populationSize, Func<PropertyInfo, float> generationStrategy)
        {
            var population = new T[populationSize];

            for (var i = 0; i < populationSize; i++)
            {
                population[i] = GenerateRandomIndividualByProperty(generationStrategy);
            }

            return population;
        }

        public T GenerateRandomIndividualByProperty()
        {
            return GenerateRandomIndividualByProperty(Strategies.GenerationStrategy);
        }

        public T GenerateRandomIndividualByProperty(Func<PropertyInfo, float> generationStrategy)
        {
            var props = typeof (T).GetProperties();
            var newMember = (T)Activator.CreateInstance(typeof(T));
            foreach (var attribute in props)
            {
                var propertyType = attribute.PropertyType;
                var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;
                var propertyValue = Convert.ChangeType(generationStrategy.Invoke(attribute), targetType);
                attribute.SetValue(newMember, propertyValue, null);
            }

            return newMember;
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public T GenerateRandomIndividualByConstructor(ConstructorInfo constructorInfo = null)
        {
            return GenerateRandomIndividualByConstructor(Strategies.GenerationStrategy, constructorInfo);
        }

        public T GenerateRandomIndividualByConstructor(Func<PropertyInfo, float> generationStrategy, ConstructorInfo constructorInfo = null)
        {
            if(constructorInfo == null)
                constructorInfo = typeof(T).GetConstructors().OrderByDescending(x => x.GetParameters().Length).Last();

            return (T)Activator.CreateInstance(typeof(T), typeof (T).GetProperties()
                .Where(x => constructorInfo.GetParameters()
                    .Select(y => y.ParameterType)
                    .Contains(x.PropertyType))
                .Select(generationStrategy.Invoke)
                .Cast<object>().ToArray());
        }

        public float CalculateFitness(T individual)
        {
            return CalculateFitness(individual, Strategies.FitnessStrategy);
        }

        public float CalculateFitness(T individual, Func<T, float> fitnessStrategy)
        {
            return fitnessStrategy.Invoke(individual);
        }

        public IEnumerable<T> NextGeneration(Population<T> oldPopulation, int populationSize)
        {
            return NextGeneration(oldPopulation, populationSize, Strategies);
        }

        public IEnumerable<T> NextGeneration(Population<T> oldPopulation, int populationSize, GeneticStrategies<T> strategies)
        {
            var population = new T[populationSize];

            foreach (var individual in oldPopulation.Members)
            {
                individual.Fitness = CalculateFitness(individual, strategies.FitnessStrategy);
            }

            for (var i = 0; i < populationSize; i++)
            {
                var firstParent = (T) GetRandomIndividualWeightedByFitness(oldPopulation);
                var secontParent = (T) GetRandomIndividualWeightedByFitness(oldPopulation, firstParent);
                population[i] = Reproduce(firstParent, secontParent, strategies.ReproductionStrategies);
            }

            return population;
        }

        public T Reproduce(T firstParent, T secondParent)
        {
            return Reproduce(firstParent, secondParent, Strategies.ReproductionStrategies);
        }

        public T Reproduce(T firstParent, T secondParent, Dictionary<Type, Func<float, float, float>> reproductionStrategies)
        {
            var objectProperties = typeof (T).GetProperties();
            var newProperties = new List<object>();
            foreach (var attribute in objectProperties)
            {
                var strategy = reproductionStrategies.SingleOrDefault(prop => prop.Key == attribute.PropertyType);
                if (strategy.Key != null)
                {
                    newProperties.Add(strategy.Value.Invoke((float) attribute.GetValue(firstParent), (float) attribute.GetValue(secondParent)));
                }
                else
                {
                    newProperties.Add(new Random().Next(1) == 0 ? firstParent : secondParent);
                }
            }

            var child = (T)Activator.CreateInstance(typeof(T), newProperties.ToArray());
            child.Initialize();
            return child;
        }

        public T MutateIndividual(T individual)
        {
            return MutateIndividual(individual, Strategies.MutationStrategy);
        }

        public T MutateIndividual(T individual, Func<float, float> mutationStrategy)
        {
            var props = typeof(T).GetProperties();
            foreach (var attribute in props)
            {
                typeof(T).GetProperty(attribute.Name).SetValue(individual, mutationStrategy.Invoke((float)attribute.GetValue(individual)));
            }

            return individual;
        }

        public IEnumerable<T> GetRandomIndividualWeightedByFitness(Population<T> population, T otherParent = null)
        {
            var individualArray = population.Members.ToList().Where(x => x.Id != otherParent?.Id).Select(x => _rand.Next(1, (int)x.Fitness)).ToArray();
            for(var i = 0; i < population.Members.Count() - 1; i++)
            {
                yield return individualArray[i] > individualArray[1 + 1] ? population.Members.ToList()[i] : population.Members.ToList()[i + 1];
            }
        }
    }
}
