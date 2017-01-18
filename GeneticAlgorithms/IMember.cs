using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public interface IMember
    {
        int Id { get; }
        int Generation { get; }
        double Fitness { get; set; }
        void Initialize();
    }
}
