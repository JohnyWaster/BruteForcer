using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BruteForcer
{
    public interface IBruteForcer
    {
        Task<string> BruteForceAsync<T>(Func<string, T> attemptOfPassword, Predicate<T> hasSuccess);
    }    
}
