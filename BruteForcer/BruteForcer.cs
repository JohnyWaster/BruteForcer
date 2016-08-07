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

        private IList<IEnumerable<string>> _dictionariesForDifferentCores;

        /// <summary>
        /// Constructor, which sets  dictionary for look throw. But it does not allow to parallel work between some cores
        /// </summary>
        /// <param name="dictWithPasswords">dictionary, where password will be seeked</param>
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


        public void SetDictionariesOfPasswords(char[] alphabet, int minLength, int maxLength, int numberOfCores = 1)
        {
            if(numberOfCores < 1)
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

            if (numberOfCores == 1)
            {
                _dictionariesForDifferentCores.Add(MakeDictionaryOfPasswordsFromAlphabet(alphabet, minLength, maxLength));
            }

            else
            {
                for(int i = 0; i < numberOfCores; ++i)
                {

                }
            }
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

        /// <summary>
        /// go throw all values of dictionary and try each of them, using asynchronous attempt function
        /// </summary>
        /// <typeparam name="T">type of object, which is returned by attempt function and concieved by success predicate</typeparam>
        /// <param name="AttemptOfPasswordAsync">func, which gets a word from dictionary and makes one attempt assynchronously, returns result of attemption of type T</param>
        /// <param name="HasSuccess">predicate, which determines, if attempt was successful</param>
        /// <returns>returns right password or null, if password is not found</returns>
        public async Task<string> BruteForceAsync<T>(Func<string, Task<T>> AttemptOfPasswordAsync, Predicate<T> HasSuccess)
        {
            string rightPassword = null;

            Parallel.ForEach(_dictionaryWithPasswords, async (password, state) =>
                {
                    T result = await AttemptOfPasswordAsync(password);

                    if (HasSuccess(result) == true)
                    {
                        rightPassword = password;
                        state.Break();
                    }
                }
            );

            return rightPassword;
        }


        public string BruteForceSingle<T>(Func<string, T> AttemptOfPassword, Predicate<T> HasSuccess)
        {
            string rightPassword = null;

            foreach(var password in _dictionaryWithPasswords)
            {
                T result = AttemptOfPassword(password);

                if (HasSuccess(result) == true)
                {
                    rightPassword = password;
                    break;                
                }
            }

            return rightPassword;
        }

        /// <summary>
        /// go throw all values of dictionary and try each of them, using attempt function
        /// </summary>
        /// <typeparam name="T">type of object, which is returned by attempt function and concieved by success predicate</typeparam>
        /// <param name="AttemptOfPassword">func, which gets a word from dictionary and makes one attempt, returns result of attemption of type T</param>
        /// <param name="HasSuccess">predicate, which determines, if attempt was successful</param>
        /// <returns>returns right password or null, if password is not found</returns>
        public string BruteForce<T>(Func<string, T> AttemptOfPassword, Predicate<T> HasSuccess)
        {
            string rightPassword = null;

            Parallel.ForEach(_dictionaryWithPasswords, (password, state) =>
            {
                T result = AttemptOfPassword(password);

                if (HasSuccess(result) == true)
                {
                    rightPassword = password;
                    state.Break();                
                }
            }
            );

            return rightPassword;
        }


       

        /// <summary>
        /// generate all possible values of password of required length
        /// </summary>
        /// <param name="alphabet">set of characters, which can be included in password</param>
        /// <param name="minLength">min length of a password</param>
        /// <param name="maxLength">max length of a password</param>
        /// <returns>dictionary of all appropriate words, number of them equals alphabet.Length**minLength+...+alphabet.Length**maxLength</returns>
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
                throw new ArgumentNullException();
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

                        //make words from indexes
                        yield return new string(
                            charPass.Select(a=>alphabet[a]).ToArray()
                            );
                    }
                    //when all values of last position were picked over

                    //for the last value
                    if(charPass.SequenceEqual(maxPassword))
                    {
                        break;
                    }

                    //counter for going throw all positions
                    int j = 0;

                    //go throw all positions, check if overflow
                    while (charPass[lastPosition - j] == maxElem)
                    {
                        //if maxElem at some position, make it minElem
                        charPass[lastPosition - j] = minElem;
                        j++;                       
                    }

                    //if not maxElem increment it
                    charPass[lastPosition - j]++;
                }
            }
        }

        /// <summary>
        /// generate all possible values of password from alphabet in range between minPassword and maxPassword
        /// </summary>
        /// <param name="alphabet">set of characters, which can be included in password</param>
        /// <param name="minPassword">min value of password</param>
        /// <param name="maxPassword">max value of password</param>
        /// <returns></returns>
        public static IEnumerable<string> MakeDictionaryOfPasswordsFromAlphabetInRange(char[] alphabet, string minPassword, string maxPassword)
        {
            if(minPassword == null || maxPassword == null || alphabet == null)
            {
                throw new ArgumentNullException();
            }
            if(maxPassword.CompareTo(minPassword) < 0)
            {
                throw new ArgumentException("maxPassword should be greater than minPassword");
            }

            int len = alphabet.Length;

            //we will work with indexes of alphabet, only at finish we transform them into letters
            char maxElem = (char)(len - 1);

            char minElem = (char)0;

            //go throw all possible lengthes of password
            for (int currentLength = minPassword.Length; currentLength <= maxPassword.Length; ++currentLength)
            {
                int lastPosition = currentLength - 1;

                //finish when attain max value
                char[] maxCharPassword = new string(maxElem, currentLength).ToCharArray();

                if (currentLength == maxPassword.Length)
                {
                    maxCharPassword = maxPassword.ToCharArray().Select(a=>(char)new string(alphabet).IndexOf(a)).ToArray();
                }


                //begin from the smallest value
                char[] minCharPassword = new string(minElem, currentLength).ToCharArray();

                if (currentLength == minPassword.Length)
                {
                    minCharPassword = minPassword.ToCharArray().Select(a => (char)new string(alphabet).IndexOf(a)).ToArray();
                }

                //current password 
                char[] charPass = minCharPassword;

                //variable for distinguish minPassword, used only once
                char minElemForCurrentPosition = minCharPassword[lastPosition];

                //go throw all possible values of charPass, while attain max value
                while (!charPass.SequenceEqual(maxCharPassword))
                {
                    //go throw all possible characters at last position
                    for (char k = minElemForCurrentPosition; k <= maxElem; ++k)
                    {
                        charPass[lastPosition] = k;

                        //make words from indexes
                        yield return new string(
                            charPass.Select(a => alphabet[a]).ToArray()
                            );

                        //for the last value
                        if (charPass[0] == maxCharPassword[0] && charPass.SequenceEqual(maxCharPassword))
                        {
                            break;
                        }
                    }
                    //when all values of last position were picked over
                    minElemForCurrentPosition = minElem;
                    
                    //for the last value
                    if (charPass[0] == maxCharPassword[0] && charPass.SequenceEqual(maxCharPassword))
                    {
                        break;
                    }
                    
                    //counter for going throw all positions
                    int j = 0;

                    //go throw all positions, check if overflow
                    while (charPass[lastPosition - j] == maxElem)
                    {
                        //if maxElem at some position, make it minElem
                        charPass[lastPosition - j] = minElem;
                        j++;
                    }

                    //if not maxElem increment it
                    charPass[lastPosition - j]++;

                    //for the last value
                    if (charPass[0] == maxCharPassword[0] && charPass.SequenceEqual(maxCharPassword))
                    {
                        yield return new string(
                            charPass.Select(a => alphabet[a]).ToArray()
                            );
                    }
                }
            }
        }     
    }
}
