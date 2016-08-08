using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BruteForcer
{
    public class MultiCoreDictionaryBruteForcer : IBruteForcer
    {
        private IList<IEnumerable<string>> _dictionariesForDifferentCores;

        /// <summary>
        /// Constructor, which sets  dictionaries for look throw.
        /// </summary>
        /// <param name="dictWithPasswords">list of dictionaries, where password will be seeked</param>
        public MultiCoreDictionaryBruteForcer(IList<IEnumerable<string>> listOfDictionaries)
        {
            if (listOfDictionaries == null)
            {
                throw new ArgumentNullException("listOfDictionaries");
            }

            foreach(var dict in listOfDictionaries)
            {
                if(dict == null)
                {
                    throw new ArgumentNullException("listOfDictionaries", "one of dictionaries is null");
                }
            }

            _dictionariesForDifferentCores = listOfDictionaries;
        }

        /// <summary>
        /// makes dictionaries of passwords for each core from alphabet
        /// </summary>
        /// <param name="alphabet">set of characters, which can be included in password</param>
        /// <param name="minLength">min length of password</param>
        /// <param name="maxLength">max length of password</param>
        /// <param name="numberOfCores">number of dictionaries for dividing entire dictionary of passwords, each dictionary for each core</param>
        public static IList<IEnumerable<string>> MakeDictionariesOfPasswordsFromAlphabetForSomeCores(char[] alphabet, int minLength, int maxLength, int numberOfCores = 1)
        {
            if (numberOfCores < 1)
            {
                throw new ArgumentOutOfRangeException("numberOfCores");
            }
            if (minLength <= 0 || maxLength <= 0)
            {
                throw new ArgumentException("length of password should be greater then 0");
            }
            if (minLength > maxLength)
            {
                throw new ArgumentException("minLength can't be greater than maxLength");
            }
            if (alphabet == null)
            {
                throw new ArgumentNullException();
            }

            var dictionariesForDifferentCores = new List<IEnumerable<string>>();

            if (numberOfCores == 1)
            {
                dictionariesForDifferentCores.Add(DictionaryBruteForcer.MakeDictionaryOfPasswordsFromAlphabet(alphabet, minLength, maxLength));
            }

            else
            {
                string minVal = new string(alphabet[0], minLength);
                string maxVal = new string(alphabet[alphabet.Length / numberOfCores], maxLength);

                for (int i = 1; i <= numberOfCores; ++i)
                {
                    dictionariesForDifferentCores.Add(DictionaryBruteForcer.MakeDictionaryOfPasswordsFromAlphabetInRange(alphabet, minVal, maxVal));

                    minVal = maxVal;
                    if (i == numberOfCores)
                    {
                        break;
                    }
                    if (i != numberOfCores - 1)
                    {
                        maxVal = new string(alphabet[(i + 1) * alphabet.Length / numberOfCores], maxLength);
                    }
                    else
                    {
                        maxVal = new string(alphabet[alphabet.Length - 1], maxLength);
                    }
                }
            }

            return dictionariesForDifferentCores;
        }

        public string BruteForce<T>(Func<string, T> AttemptOfPassword, Predicate<T> HasSuccess)
        {
            throw new NotImplementedException();
        }
    }
}
