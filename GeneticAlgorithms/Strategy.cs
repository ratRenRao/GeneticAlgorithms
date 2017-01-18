using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public interface IStrategy
    {
        string PropertyName { get; }
        Type PropertyType { get; }
        Type ContainerType { get; }
        object Func { get; }
    }

    public interface IReproductionStrategy<T, V> : IStrategy
    {
        new Func<V, V, V> Func { get; }
    }

    public interface IGenerationStrategy<T, V> : IStrategy
    {
        new Func<PropertyInfo, V> Func { get; }
    }

    public interface IMutationStrategy<T, V> : IStrategy
    {
        new Func<V, V> Func { get; }
    }

    public interface IFitnessStrategy<T, V> : IStrategy
    {
        new Func<T, T> Func { get; }
    }

    public abstract class Strategy<T, V>
    {
        public string PropertyName { get; set; }
        public Type PropertyType => typeof(V);
        public Type ContainerType => typeof (T);
    }

    public class ReproductionStrategy<T, V> : Strategy<T, V>, IReproductionStrategy<T, V>
    {
        public ReproductionStrategy(Func<V, V, V> func, string propertyName = null)
        {
            PropertyName = propertyName; 
            Func = func;
        }

        public Func<V, V, V> Func { get; }
        object IStrategy.Func => Func;
        string IStrategy.PropertyName => PropertyName;
    }

    public class GenerationStrategy<T, V> : Strategy<T, V>, IGenerationStrategy<T, V>
    {
        public GenerationStrategy(Func<PropertyInfo, V> func, string propertyName = null)
        {
            PropertyName = propertyName;
            Func = func;
        }

        public Func<PropertyInfo, V> Func { get; }
        object IStrategy.Func => Func;
        string IStrategy.PropertyName => PropertyName;
    }

    public class MutationStrategy<T, V> : Strategy<T, V>, IMutationStrategy<V, V>
    {
        public MutationStrategy(Func<V, V> func, string propertyName = null)
        {
            PropertyName = propertyName;
            Func = func;
        }

        public Func<V, V> Func { get; }
        object IStrategy.Func => Func;
        string IStrategy.PropertyName => PropertyName;
    }

    public class FitnessStrategy<T, V> : Strategy<T, V>, IFitnessStrategy<T, V>
    {
        public FitnessStrategy(Func<T, T> func, string propertyName = null)
        {
            PropertyName = propertyName;
            Func = func;
        }

        public Func<T, T> Func { get; }
        object IStrategy.Func => Func;
        string IStrategy.PropertyName => PropertyName;
    }

    public enum StrategyType
    {
        Generation,
        Fitness,
        Reproduction,
        Mutation
    }
}
