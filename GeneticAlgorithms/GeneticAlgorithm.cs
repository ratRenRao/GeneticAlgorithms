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

        public IEnumerable<T> CreateNewPopulation(int populationSize, Func<PropertyInfo, object> generationStrategy)
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

        public T GenerateRandomIndividualByProperty(Func<PropertyInfo, object> generationStrategy)
        {
            var props = typeof (T).GetProperties();
            var newMember = (T)Activator.CreateInstance(typeof(T));
            foreach (var attribute in props)
            {
                //var propertyType = attribute.PropertyType;
                //var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;
                //var propertyValue = Convert.ChangeType(generationStrategy.Invoke(attribute), targetType);
                var propertyValue = generationStrategy.Invoke(attribute);
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

        public T GenerateRandomIndividualByConstructor(Func<PropertyInfo, object> generationStrategy, ConstructorInfo constructorInfo = null)
        {
            if(constructorInfo == null)
                constructorInfo = typeof(T).GetConstructors().OrderByDescending(x => x.GetParameters().Length).Last();

            return (T)Activator.CreateInstance(typeof(T), typeof (T).GetProperties()
                .Where(x => constructorInfo.GetParameters()
                    .Select(y => y.ParameterType)
                    .Contains(x.PropertyType))
                .Select(generationStrategy.Invoke).ToArray());
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
                var firstParent = GetRandomIndividualWeightedByFitness(oldPopulation);
                var secontParent = GetRandomIndividualWeightedByFitness(oldPopulation, firstParent);
                population[i] = Reproduce(firstParent, secontParent, strategies.ReproductionStrategies);
            }

            return population;
        }

        public T Reproduce(T firstParent, T secondParent)
        {
            return Reproduce(firstParent, secondParent, Strategies.ReproductionStrategies);
        }

        public T Reproduce(T firstParent, T secondParent, ReproductionStrategies reproductionStrategies)
        {
            var objectProperties = typeof (T).GetProperties();
            var newProperties = new List<object>();
            foreach (var property in objectProperties)
            {
                var strategy = (Strategy<dynamic>) reproductionStrategies.GetReproductionStrategy(property.PropertyType); //ReproductionStrategy(property.PropertyType);
                newProperties.Add(GenerateChildAttribute(property, firstParent, secondParent, strategy));
            }

            var child = (T)Activator.CreateInstance(typeof(T), newProperties.ToArray());
            child.Initialize();
            return child;
        }

        private dynamic GenerateChildAttribute(PropertyInfo property, T firstParent, T secondParent, Strategy<dynamic> reproductionStrategy)
        {
            if (reproductionStrategy != null)
            {
                return reproductionStrategy.Method.Invoke(property.GetValue(firstParent), property.GetValue(secondParent));
            }
            else
            {
                var randomParent = new Random().Next(1) == 0 ? firstParent : secondParent;
                return property.GetValue(randomParent);
            }
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

        public T GetRandomIndividualWeightedByFitness(Population<T> population, T otherParent = null)
        {
            var individualArray = population.Members.ToList().Where(x => x.Id != otherParent?.Id).Select(x => _rand.Next(1, (int)x.Fitness)).ToArray();
            var dictionary = individualArray.Zip(population.Members, (k, v) => new {k, v}).ToDictionary(x => x.k, x => x.v);
            return dictionary.OrderByDescending(x => x.Key).First().Value; 
        }
    }
}
