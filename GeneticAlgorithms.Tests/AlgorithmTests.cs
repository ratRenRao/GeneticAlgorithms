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
        private readonly GeneticStrategies<TestCoordinates> _strategies;
        private readonly Population<TestCoordinates> _population;

        public AlgorithmTests()
        {
            _strategies = new GeneticStrategies<TestCoordinates>(info => 0f, coordinates => 0f, f => 0f);
            _population = new Population<TestCoordinates>(_strategies, 2);
        }

        [Fact]
        public void generate_individual_returns_randomized_child()
        {
            Random rand = new Random();
            var vals = new[] {rand.Next(0, 10), rand.Next(10, 20)};
            _strategies.GenerationStrategy = info => info.Name == "X" ? vals[0] : vals[1];
            var child = _population.Algorithm.GenerateRandomIndividualByProperty(_strategies.GenerationStrategy);
            child.X.ShouldBe(vals[0]);
            child.Y.ShouldBe(vals[1]);
        }

        [Fact]
        public void calculate_fitness_returns_correct_value()
        {
            Random rand = new Random();
            var testCoordinates = new TestCoordinates(rand.Next(0, 10), rand.Next(10, 20));
            _strategies.FitnessStrategy = coords => coords.X + 1f;
            var fitness = _population.Algorithm.CalculateFitness(testCoordinates, _strategies.FitnessStrategy);
            fitness.ShouldBe(testCoordinates.X + 1);
        }

        [Fact]
        public void reproduce_method_returns_correct_child()
        {
            Random rand = new Random();
            var parent1 = new TestCoordinates(rand.Next(1, 1), rand.Next(2, 2));
            var parent2 = new TestCoordinates(rand.Next(3, 3), rand.Next(4, 4));
            _strategies.ReproductionStrategies.Add(new Strategy<dynamic>
            {
                Method = (f1, f2) => (f1+f2)/2
            });
            _strategies.ReproductionStrategies.Add(new Strategy<dynamic>
            {
                Method = (f1, f2) => (f1 + f2)/2
            });
            var child = _population.Algorithm.Reproduce(parent1, parent2, _strategies.ReproductionStrategies);
            child.X.ShouldBe(2);
            child.Y.ShouldBe(3);
        }

        [Fact]
        public void initialize_population_returns_random_set_of_children()
        {
            Random rand = new Random();
            _strategies.GenerationStrategy = x =>
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
            };
            var population = _population.Algorithm.CreateNewPopulation(10, _strategies.GenerationStrategy).ToList();
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
            _strategies.GenerationStrategy = x =>
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
            };
            _population.ReinitializePopulation(10, _strategies.GenerationStrategy);
            _population.AddChildren(_population.Algorithm.CreateNewPopulation(4, _strategies.GenerationStrategy).ToArray());
            _population.Members.Count().ShouldBe(14);
        }

        [Fact]
        public void kill_individual_removes_object_from_population()
        {
            _population.ReinitializePopulation(10, _strategies.GenerationStrategy);
            var id = _population.Members.First().Id;
            _population.KillMember(id);
            _population.Members.SingleOrDefault(x => x.Id == id).ShouldBeNull();
        }

        [Fact]
        public void generate_by_constructor_creates_valid_individual()
        {
            _strategies.GenerationStrategy = x => 1;

            var individual = _population.Algorithm
                .GenerateRandomIndividualByConstructor(_strategies.GenerationStrategy, typeof(TestCoordinates).GetConstructors().SingleOrDefault(x => x.GetParameters().Length == 2));
            individual.X.ShouldBe(1);
            individual.Y.ShouldBe(1);
        }

        [Fact]
        public void kill_unfit_members_removes_lowest_fitness_members()
        {
            Random rand = new Random();
            _strategies.GenerationStrategy = x =>
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
            };
            _population.ReinitializePopulation(10, _strategies.GenerationStrategy);
            var unfitIds = _population.Members.Where(x => x.Fitness != 0).OrderByDescending(x => -x.Fitness).Take(2).Select(x => x.Id).ToArray();
            _population.KillOffLowFitnessMembers(.20);
            _population.Members.SingleOrDefault(x => unfitIds.Contains(x.Id)).ShouldBeNull();
        }

        //Does it make sense to implement this???
        [Fact]
        public void reproduce_works_with_non_numeric_types()
        {
            
        }
    }
} 