using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BruteForcer
{
    public interface IBruteForcer
    {
        string GetNextValue();

        bool IsNotFinished { get; }
    }

    public class DictionaryBruteForcer : IBruteForcer
    {
        private IEnumerable<string> _dictionaryWithPasswords;

        private IEnumerator<string> _passEnumerator;

        private bool _isNotFinished;

        private string current;

        public DictionaryBruteForcer(IEnumerable<string> dictWithPasswords)
        {
            if(dictWithPasswords == null)
            {
                throw new ArgumentNullException("dictWithPasswords");
            }

            _dictionaryWithPasswords = dictWithPasswords;
 
            _passEnumerator = _dictionaryWithPasswords.GetEnumerator();

            _isNotFinished = _passEnumerator.MoveNext();
        }

        public bool IsNotFinished
        {
            get
            {
                return _isNotFinished;
            }
        }

        public string GetNextValue()
        {
            current = _passEnumerator.Current;
            _isNotFinished = _passEnumerator.MoveNext();

            return current;
        }

        public async Task<string> BruteForceAsync<T>(Func<string, Task<T>> AttemptOfPassword, Predicate<T> HasSuccess)
        {
            foreach(var password in _dictionaryWithPasswords)
            {
                T result = await AttemptOfPassword(password);

                if(HasSuccess(result) == true)
                {
                    return password;
                }
            }

            return null;
        }

        /// <summary>
        /// generate all possible values of password of required length
        /// </summary>
        /// <param name="alphabet">set of characters, which can be included in password</param>
        /// <param name="minLength">min length of a password</param>
        /// <param name="maxLength">max length of a password</param>
        /// <returns></returns>
        public static IEnumerable<string> MakeDictionaryOfPasswordsFromAlphabet(char[] alphabet, int minLength, int maxLength)
        {        
            if(minLength <= 0 || maxLength <= 0)
            {
                throw new ArgumentException("length of password should be greater then 0");
            }
            if(minLength > maxLength)
            {
                throw new ArgumentException("minLength can't be greater than maxLength");
            }   
            if(alphabet == null)
            {
                throw new NullReferenceException();
            }
            //if alphabet has duplicates remove them
            alphabet = alphabet.Distinct().ToArray();

            int len = alphabet.Length;

            //we will work with indexes of alphabet, only at finish we transform them into letters
            char maxElem = (char)(len - 1);

            char minElem = (char)0;


            //go throw all possible lengthes of password
            for (int currentLength = minLength; currentLength <= maxLength; ++currentLength)
            {
                int lastPosition = currentLength - 1;

                //finish when attain max value
                char[] maxPassword = new string(maxElem, currentLength).ToCharArray();

                //begin from the smallest value
                char[] minPassword = new string(minElem, currentLength).ToCharArray();

                //current password 
                char[] charPass = minPassword;


                //go throw all possible values of charPass, while attain max value
                while (!charPass.SequenceEqual(maxPassword))
                {
                    //go throw all possible characters at last position
                    for (char k = minElem; k <= maxElem; ++k)
                    {                    
                        charPass[lastPosition] = k;

                        yield return new string(
                            charPass.Select(a=>alphabet[a]).ToArray()
                            );
                    }
                    //when all values of last position were picked over

                    //counter for going throw all positions
                    int j = 0;

                    //go throw all positions, check if overflow
                    while (charPass[lastPosition - j] == maxElem)
                    {
                        //if maxElem at some position, make it minElem
                        charPass[lastPosition - j] = minElem;
                        j++;

                        //for the last value
                        if(lastPosition - j == -1)
                        {
                            break;
                        }
                       
                    }

                    //for the last value
                    if(lastPosition - j == -1)
                    {
                        break;
                    }

                    //if not maxElem increment it
                    charPass[lastPosition - j]++;
                }
            }
        }        
    }
}
