using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BruteForcer
{
    public interface IBruteForcer
    {
        string BruteForce<T>(Func<string, T> AttemptOfPassword, Predicate<T> HasSuccess);
    }    
}
