using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeneticAlgorithms.Tests.TestingObjects;
using Xunit;
using Moq;
using Shouldly;

namespace GeneticAlgorithms.Tests
{
    public class AlgorithmTests
    {
        private readonly PopulationBuilder<TestCoordinates> _populationBuilder;

        public AlgorithmTests()
        {
            _populationBuilder = new PopulationBuilder<TestCoordinates>(new GeneticStrategies<TestCoordinates>( coordinates => 0d));
            _populationBuilder.Strategies.GenerationStrategies.Add(new GenerationStrategy<TestCoordinates, int>(x => 1));
            _populationBuilder.Strategies.ReproductionStrategies.Add(new ReproductionStrategy<TestCoordinates, int>((x, y) => x + y / 2));
            _populationBuilder.Strategies.GenerationStrategies.Add(new GenerationStrategy<NestedAttribute, int>(x => 1));
        }

        [Fact]
        public void generate_individual_returns_randomized_child()
        {
            Random rand = new Random();
            var vals = new[] {rand.Next(0, 10), rand.Next(10, 20)};
            var algorithms = new GeneticAlgorithmExecutor();
            _populationBuilder.Strategies.GenerationStrategies.Add(new GenerationStrategy<TestCoordinates, double>(info => info.Name == "X" ? vals[0] : vals[1], "X"));
            var child = algorithms.GenerateRandomIndividualByProperties<TestCoordinates>(_populationBuilder.Strategies.GenerationStrategies, 1);
            child.X.ShouldBe(vals[0]);
            child.Y.ShouldBe(vals[1]);
        }

        [Fact]
        public void calculate_fitness_returns_correct_value()
        {
            Random rand = new Random();
            var testCoordinates = new TestCoordinates(rand.Next(0, 10), rand.Next(10, 20), 1);
            _populationBuilder.Strategies.FitnessStrategy = coords => coords.X + 1d;
            var fitness = _populationBuilder.AlgorithmExecutor.CalculateFitness<TestCoordinates>(testCoordinates, _populationBuilder.Strategies.FitnessStrategy);
            fitness.ShouldBe(testCoordinates.X + 1);
        }

        [Fact]
        public void reproduce_method_returns_correct_child()
        {
            Random rand = new Random();
            var parent1 = new TestCoordinates(1, 2, 1)
            {
                Z = new NestedAttribute(3)
            };
            var parent2 = new TestCoordinates(3, 4, 1)
            {
                Z = new NestedAttribute(5)
            };
            var algorithms = new GeneticAlgorithmExecutor();
            _populationBuilder.Strategies.ReproductionStrategies.Add(new ReproductionStrategy<TestCoordinates, double>((f1, f2) => (f1 + f2) / 2, "X"));
            _populationBuilder.Strategies.ReproductionStrategies.Add(new ReproductionStrategy<TestCoordinates, double>((f1, f2) => (f1 + f2) / 2, "Y"));
            _populationBuilder.Strategies.ReproductionStrategies.Add(new ReproductionStrategy<NestedAttribute, int>((f1, f2) => (f1 + f2) / 2, "Z"));
            var child = algorithms.Reproduce(parent1, parent2, _populationBuilder.Strategies.ReproductionStrategies, 1);
            child.X.ShouldBe(2);
            child.Y.ShouldBe(3);
            child.Z.NestedVal.ShouldBe(4);
        }

        [Fact]
        public void initialize_population_returns_random_set_of_children()
        {
            Random rand = new Random();
            _populationBuilder.Strategies.GenerationStrategies.Add(new GenerationStrategy<TestCoordinates, double>( x =>
            {
                switch (x.Name)
                {
                    case "X":
                        return rand.Next(0, 10);
                    case "Y":
                        return rand.Next(10, 20);
                    default:
                        return 0f;
                }
            }));
            var population = _populationBuilder.AlgorithmExecutor.CreateNewPopulationMembers<TestCoordinates>(10, _populationBuilder.Strategies.GenerationStrategies, 1).ToList();
            foreach (var individual in population)
            {
                individual.X.ShouldBeGreaterThanOrEqualTo(0);
                individual.X.ShouldBeLessThan(10);
                individual.Y.ShouldBeGreaterThanOrEqualTo(10);
                individual.Y.ShouldBeLessThan(20);
            }
            population.Max(x => x.X).ShouldBeGreaterThan(population.Min(x => x.X));
            population.Max(x => x.Y).ShouldBeGreaterThan(population.Min(x => x.Y));
        }

        [Fact]
        public void population_add_children_stores_new_individuals()
        {
            Random rand = new Random();
            _populationBuilder.Strategies.GenerationStrategies.Add(new GenerationStrategy<TestCoordinates, double>(x =>
            {
                switch (x.Name)
                {
                    case "X":
                        return rand.Next(0, 10);
                    case "Y":
                        return rand.Next(10, 20);
                    default:
                        return 0d;
                }
            }));
            _populationBuilder.InitializePopulationMembers(10);
            _populationBuilder.Population.AddChildren(_populationBuilder.AlgorithmExecutor.CreateNewPopulationMembers<TestCoordinates>(4, _populationBuilder.Strategies.GenerationStrategies, 1).ToList());
            _populationBuilder.Population.Members.Count().ShouldBe(14);
        }

        [Fact]
        public void kill_individual_removes_object_from_population()
        {
            _populationBuilder.Strategies.GenerationStrategies.Add(new GenerationStrategy<TestCoordinates, double>(x => 0));
            _populationBuilder.InitializePopulationMembers(10);
            var id = _populationBuilder.Population.Members.First().Id;
            _populationBuilder.Population.KillMember(id);
            _populationBuilder.Population.Members.SingleOrDefault(x => x.Id == id).ShouldBeNull();
        }

        [Fact]
        public void generate_by_constructor_creates_valid_individual()
        {
            _populationBuilder.Strategies.GenerationStrategies.Add(new GenerationStrategy<TestCoordinates, double>(x => 1));

            var constructor = typeof(TestCoordinates).GetConstructors().SingleOrDefault(x => x.GetParameters().Length == 3);
            var individual = _populationBuilder.AlgorithmExecutor
                .GenerateRandomIndividualByConstructor<TestCoordinates>(_populationBuilder.Strategies.GenerationStrategies, 1, constructor);
            individual.X.ShouldBe(1);
            individual.Y.ShouldBe(1);
        }

        [Fact]
        public void kill_unfit_members_removes_lowest_fitness_members()
        {
            Random rand = new Random();
            _populationBuilder.Strategies.GenerationStrategies.Add(new GenerationStrategy<TestCoordinates, double>(x =>
            {
                switch (x.Name)
                {
                    case "X":
                        return rand.Next(0, 10);
                    case "Y":
                        return rand.Next(10, 20);
                    default:
                        return 0d;
                }
            }));
            _populationBuilder.InitializePopulationMembers(10);
            var unfitIds = _populationBuilder.Population.Members.Where(x => x.Fitness != 0).OrderByDescending(x => -x.Fitness).Take(2).Select(x => x.Id).ToArray();
            _populationBuilder.KillOffLowFitnessMembers(.20);
            _populationBuilder.Population.Members.SingleOrDefault(x => unfitIds.Contains(x.Id)).ShouldBeNull();
        }

        [Fact]
        public void get_strategy_by_type_returns_correct_strategy_type()
        {
            var testCoord = new TestCoordinates(1.0, 1.0, 1);
            _populationBuilder.Strategies.ReproductionStrategies.Add(new ReproductionStrategy<TestCoordinates, double>((f1, f2) => f1, "X"));
            var strategy = _populationBuilder.Strategies.ReproductionStrategies.GetStrategy<TestCoordinates, double>();
            strategy.ShouldBeOfType<ReproductionStrategy<TestCoordinates, double>>();
        }
    }
} 