﻿using System;
using System.Linq;
using System.Reflection;
using GeneticAlgorithms.Tests.TestingObjects;
using Xunit;
using Shouldly;

namespace GeneticAlgorithms.Tests
{
    public class AlgorithmTests
    {
        private readonly GeneticAlgorithm<TestCoordinates> _algorithm;

        public AlgorithmTests()
        {
            _algorithm = new GeneticAlgorithm<TestCoordinates>(info => 0f, coordinates => 0f, (f, f1) => 0f, f => 0f);
        }

        [Fact]
        public void generate_individual_returns_randomized_child()
        {
            Random rand = new Random();
            var vals = new[] {rand.Next(0, 10), rand.Next(10, 20)};
            Func<PropertyInfo, float> generationStrategy = info => info.Name == "X" ? vals[0] : vals[1];
            var child = _algorithm.GenerateRandomIndividualByProperty(generationStrategy);
            child.X.ShouldBe(vals[0]);
            child.Y.ShouldBe(vals[1]);
        }

        [Fact]
        public void calculate_fitness_returns_correct_value()
        {
            Random rand = new Random();
            var testCoordinates = new TestCoordinates(rand.Next(0, 10), rand.Next(10, 20));
            Func<TestCoordinates, float> fitnessStrategy = coords => coords.X + 1f;
            var fitness = _algorithm.CalculateFitness(testCoordinates, fitnessStrategy);
            fitness.ShouldBe(testCoordinates.X + 1);
        }

        [Fact]
        public void reproduce_method_returns_correct_child()
        {
            Random rand = new Random();
            var parent1 = new TestCoordinates(rand.Next(1, 1), rand.Next(2, 2));
            var parent2 = new TestCoordinates(rand.Next(3, 3), rand.Next(4, 4));
            Func<float, float, float> fitnessStrategy = (f1, f2) => (f1+f2)/2;
            var child = _algorithm.Reproduce(parent1, parent2, fitnessStrategy);
            child.X.ShouldBe(2);
            child.Y.ShouldBe(3);
        }

        [Fact]
        public void initialize_population_returns_random_set_of_children()
        {
            Random rand = new Random();
            Func<PropertyInfo, float> generationStrategy = x =>
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
            var population = _algorithm.InitializePopulation(10, generationStrategy).ToList();
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

        //Does it make sense to implement this???
        [Fact]
        public void reproduce_works_with_non_numeric_types()
        {
            
        }
    }
} 