using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeneticAlgorithms
{
    public class GeneticAlgorithmExecutor
    {
        private readonly Random _rand;

        public GeneticAlgorithmExecutor()
        {
            _rand = new Random();
        }

        public List<T> CreateNewPopulationMembers<T>(int populationSize, StrategyStore generationStrategy, int generation) where T : IMember
        {
            var population = new T[populationSize];

            for (var i = 0; i < populationSize; i++)
            {
                population[i] = GenerateRandomIndividualByProperties<T>(generationStrategy, generation);
            }

            return population.ToList();
        }

        public T GenerateRandomIndividualByProperties<T>(StrategyStore generationStrategies, int generation)
        {
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanWrite);
            var newMember = (T)Activator.CreateInstance(typeof(T), generation);
            foreach (var attribute in props)
            {
                var method = typeof(GeneticAlgorithmExecutor).GetMethod("GenerateProperty")
                    .MakeGenericMethod(typeof(T), attribute.PropertyType);
                var result = method.Invoke(this, new object[] {attribute, generationStrategies});
                attribute.SetValue(newMember, result, null);
            }

            return newMember;
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public T GenerateRandomIndividualByConstructor<T>(StrategyStore generationStrategies, int generation, ConstructorInfo constructorInfo = null) where T : IMember
        {
            if(constructorInfo == null)
                constructorInfo = typeof(T).GetConstructors().OrderByDescending(x => x.GetParameters().Length).First();

            if (!constructorInfo.ContainsGenericParameters)
                return GenerateRandomIndividualByProperties<T>(generationStrategies, generation);
            var tmp1 = constructorInfo.GetGenericArguments();
            var paramaterPropertyInfos = tmp1.ToDictionary(
                x => x.FullName, 
                x => x.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList());

            var parameterValues = new List<object>();
            foreach(var kvp in paramaterPropertyInfos)
            {
                var parameterType = kvp.Value.GetType();
                if (parameterType.IsPrimitive)
                {
                    var method = CreateGenericMethod<T>("GenerateProperty", parameterType);
                    var result = method.Invoke(this, new object[] { kvp.Value.First(), generationStrategies });
                    parameterValues.Add(result);
                }
                else
                {
                    var method = CreateGenericMethod<T>("GenerateRandomIndividualByConstructor", parameterType);
                    parameterValues.Add(method.Invoke(this, new object[] { generationStrategies, }));
                }
            }

            return (T) Activator.CreateInstance(typeof(T), parameterValues);
        }

        public object[] GenerateProperties<T>(StrategyStore generationStrategies)
        {
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanWrite);
            var properties = new object[props.Count()];
            var i = 0;
            foreach (var attribute in props)
            {
                if (attribute.PropertyType.Namespace == "System")
                {
                    var method = typeof(GeneticAlgorithmExecutor).GetMethod("GenerateProperty")
                                    .MakeGenericMethod(typeof(T), attribute.PropertyType);
                    var result = method.Invoke(this, new object[] { attribute, generationStrategies });
                    properties[i++] = result;
                }
                else
                {
                    var method = typeof(GeneticAlgorithmExecutor).GetMethod("GenerateProperties")
                                    .MakeGenericMethod(attribute.PropertyType);
                    var result = method.Invoke(this, new object[] { generationStrategies });
                    properties[i++] = result;
                }
            }

            return properties;
        }

        public V GenerateProperty<T, V>(PropertyInfo propertyInfo, StrategyStore generationStrategies)
        {
            if (propertyInfo.PropertyType.Namespace != "System")
            {
                var method = typeof(GeneticAlgorithmExecutor).GetMethod("GenerateRandomIndividualByProperties")
                    .MakeGenericMethod(typeof(V));
                return (V) method.Invoke(this, new object[] {generationStrategies, 0});
            }
            else
            {
                var strategy = (IGenerationStrategy<T, V>) generationStrategies
                    .GetStrategy<T, V>(propertyInfo.Name);
                return strategy.Func.Invoke(propertyInfo);
            }
        }

        public T CreateInstance<T>(object[] parameters)
        {
            return (T) CreateInstance(typeof(T), parameters);
        }

        public object CreateInstance(Type type, object[] parameters)
        {
            return Activator.CreateInstance(type, parameters);
        }

        public double CalculateFitness<T>(T individual, Func<T, double> fitnessStrategy)
        {
            return fitnessStrategy.Invoke(individual);
        }

        public IEnumerable<T> NextGeneration<T>(List<T> oldMembers, int populationSize, StrategyStore reproductionStrategy, int generation) where T : IMember
        {
            var population = new T[populationSize];

            for (var i = 0; i < populationSize; i++)
            {
                var firstParent = GetRandomIndividualWeightedByFitness(oldMembers);
                oldMembers.Remove(firstParent);
                var secontParent = GetRandomIndividualWeightedByFitness(oldMembers);
                population[i] = Reproduce(firstParent, secontParent, reproductionStrategy, generation);
            }

            return population.ToList();
        }

        public T Reproduce<T>(T firstParent, T secondParent, StrategyStore reproductionStrategies, int generation)
        {
            var childProperties = typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanWrite);
            var child = (T) Activator.CreateInstance(typeof(T), generation);
            foreach (var property in childProperties)
            {
                var method = CreateGenericMethod<T>("GenerateChildAttribute", property.PropertyType);
                var result = method.Invoke(this, new object[] { property, firstParent, secondParent, reproductionStrategies });
                property.SetValue(child, result);
            }

            return child;
        }

         public V GenerateChildAttribute<T, V>(PropertyInfo property, T firstParent, T secondParent, StrategyStore reproductionStrategies)
        {
            if (property.PropertyType.Namespace != "System")
            {
                var method = typeof(GeneticAlgorithmExecutor).GetMethod("Reproduce").MakeGenericMethod(typeof(V));
                var firstProperty = property.GetValue(firstParent);
                var secondProperty = typeof(T).GetProperty(property.Name).GetValue(secondParent);
                return (V)method.Invoke(this, new object[] { firstProperty, secondProperty, reproductionStrategies, 0});
            }

            var strategy = (IReproductionStrategy<T, V>)reproductionStrategies
                .GetStrategy<T, V>(property.Name);
            if (strategy != null)
            {
                var result = strategy.Func.Invoke((V) property.GetValue(firstParent), (V) property.GetValue(secondParent));
                return result;
            }
            else
            {
                var randomParent = new Random().Next(1) == 0 ? firstParent : secondParent;
                return (V) property.GetValue(randomParent);
            }
        }

        private MethodInfo CreateGenericMethod<T>(string methodName, Type genericType)
        {
            var method = typeof(GeneticAlgorithmExecutor).GetMethod(methodName);
            return method.MakeGenericMethod(typeof(T), genericType);
        }

        public T MutateIndividual<T>(T individual)
        {
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanWrite);
            foreach (var property in props)
            {
                var method = CreateGenericMethod<T>("MutatePropertyValue", property.PropertyType);
                var result = method.Invoke(null, new[] { property, property.GetValue(individual) });
                property.SetValue(individual, result);
            }

            return individual;
        }

        private V MutatePropertyValue<T, V>(PropertyInfo propertyInfo, V currentValue, StrategyStore strategies)
        {
            var strategy = (IMutationStrategy<T, V>)strategies
                .GetStrategy<T, V>(propertyInfo.Name);
            if (strategy != null)
            {
                var result = strategy.Func.Invoke(currentValue);
                return result;
            }
            else
            {
                return currentValue;
            }
        }

        public T GetRandomIndividualWeightedByFitness<T>(List<T> members) where T : IMember
        {
            var individualArray = members.Select(x => _rand.Next(1, (int)x.Fitness)).ToArray();
            var dictionary = members.Zip(individualArray, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v); //individualArray.Zip(members.Members, (k, v) => new {k, v}).ToDictionary(x => x.k, x => x.v);
            return dictionary.OrderByDescending(x => x.Value).First().Key;
        }
    }
}
