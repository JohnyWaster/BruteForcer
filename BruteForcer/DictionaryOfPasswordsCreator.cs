using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BruteForcer
{
    public static class DictionaryOfPasswordsCreator
    {
        public static char[] SmallEnglishLetters
        {
            get { return new char[26].Select((letter, i) => (char)('a' + i)).ToArray(); }
        }

        public static char[] BigEnglishLetters
        {
            get { return new char[26].Select((letter, i) => (char)('A' + i)).ToArray(); }
        }

        public static char[] Numbers
        {
            get { return new char[10].Select((letter, i) => (char)('0' + i)).ToArray(); }
        }

        public static char[] SpecialSymbols
        {
            get
            {
                var special1 = new char[16].Select((letter, i) => (char)(' ' + i));

                var special2 = new char[6].Select((letter, i) => (char)('[' + i));

                var special3 = new char[4].Select((letter, i) => (char)('{' + i));

                var special = special1.Concat(special2).Concat(special3);

                return special.ToArray();
            }
        }

        /// <summary>
        /// makes dictionaries of passwords for each core from alphabet
        /// </summary>
        /// <param name="alphabet">set of characters, which can be included in password</param>
        /// <param name="minLength">min length of password</param>
        /// <param name="maxLength">max length of password</param>
        /// <param name="numberOfThreads">number of dictionaries for dividing entire dictionary of passwords, each dictionary for each core</param>
        public static IList<IEnumerable<string>> MakeDictionariesForSomeThreads(char[] alphabet, int minLength, int maxLength, int numberOfThreads)
        {
            if (numberOfThreads < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfThreads));
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

            if (numberOfThreads == 1)
            {
                dictionariesForDifferentCores.Add(MakeDictionaryFromAlphabet(alphabet, minLength, maxLength));
            }

            else
            {
                string minVal = new string(alphabet[0], minLength);
                string maxVal = new string(alphabet[alphabet.Length / numberOfThreads], maxLength);

                for (int i = 1; i <= numberOfThreads; ++i)
                {
                    try
                    {
                        dictionariesForDifferentCores.Add(MakeDictionaryFromAlphabet(alphabet, minVal, maxVal));
                    }
                    catch (ArgumentException)
                    {
                        throw new ArgumentException("Too much", nameof(numberOfThreads));
                    }

                    minVal = maxVal;
                    if (i == numberOfThreads)
                    {
                        break;
                    }
                    if (i != numberOfThreads - 1)
                    {
                        maxVal = new string(alphabet[(i + 1) * alphabet.Length / numberOfThreads], maxLength);
                    }
                    else
                    {
                        maxVal = new string(alphabet[alphabet.Length - 1], maxLength);
                    }
                }
            }

            return dictionariesForDifferentCores;
        }

        /// <summary>
        /// generate all possible values of password of required length
        /// </summary>
        /// <param name="alphabet">set of characters, which can be included in password</param>
        /// <param name="minLength">min length of a password</param>
        /// <param name="maxLength">max length of a password</param>
        /// <returns>dictionary of all appropriate words, number of them equals alphabet.Length**minLength+...+alphabet.Length**maxLength</returns>
        public static IEnumerable<string> MakeDictionaryFromAlphabet(char[] alphabet, int minLength, int maxLength)
        {
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
            if(alphabet.Length == 0)
            {
                throw new ArgumentException(nameof(alphabet) + " should contain at least one element");
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
                            charPass.Select(a => alphabet[a]).ToArray()
                            );
                    }
                    //when all values of last position were picked over

                    //for the last value
                    if (charPass.SequenceEqual(maxPassword))
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
        /// Generate all possible values of password from alphabet in range between minPassword and maxPassword
        /// </summary>
        /// <param name="alphabet">set of characters, which can be included in password</param>
        /// <param name="minPassword">min value of password</param>
        /// <param name="maxPassword">max value of password</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static IEnumerable<string> MakeDictionaryFromAlphabet(char[] alphabet, string minPassword, string maxPassword)
        {
            if (minPassword == null || maxPassword == null || alphabet == null)
            {
                throw new ArgumentNullException();
            }
            if (alphabet.Length == 0)
            {
                throw new ArgumentException(nameof(alphabet) + " should contain at least one element");
            }

            //find letters not from alphabet in minPassword and maxPassword
            var notMatchedLetters = from letter in minPassword.ToCharArray().Concat(maxPassword.ToCharArray())
                where !alphabet.Contains(letter)
                select letter;

            if (notMatchedLetters.Any())
            {
                throw new ArgumentException("Letters not from alphabet where found", notMatchedLetters.First(c => true).ToString());
            }


            if (String.Compare(maxPassword, minPassword, StringComparison.Ordinal) < 0)
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
                    maxCharPassword = maxPassword.ToCharArray().Select(a => (char)new string(alphabet).IndexOf(a)).ToArray();
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

        /// <summary>
        /// Generate all possible values of password, where letters at each position can be from corresponding alphabets.
        /// If number of alphabets less than maxLength, last alphabet is used for the rest of letters.
        /// If number of alphabets greater than maxLength, rest of alphabets are not used.
        /// If alphabets contain only one alphabet, method is equal to MakeDictionaryFromAlphabet.
        /// </summary>
        /// <param name="alphabets">Set of alphabets, each alphabet is using for corresponding position in password</param>
        /// <param name="minLength">Minimal length of password</param>
        /// <param name="maxLength">Maximum length of password.</param>
        /// <returns></returns>
        public static IEnumerable<string> MakeDictionaryFromSomeAlphabets(IList<char[]> alphabets, int minLength, int maxLength)
        {
            #region Validation of input parameters
            if (alphabets ==  null)
            {
                throw new ArgumentNullException(nameof(alphabets));
            }
            if(alphabets.Count() == 0)
            {
                throw new ArgumentException(nameof(alphabets) + " shoud contain at least one alphabet");
            }
            foreach(var alph in alphabets)
            {
                if(alph == null)
                {
                    throw new NullReferenceException(nameof(alphabets) + " shoud not contain null references");
                }
                if(alph.Length == 0)
                {
                    throw new ArgumentException(nameof(alphabets) + " shoud not contain empty alphabets");
                }
            }

            if(minLength <= 0 || maxLength <= 0)
            {
                throw new ArgumentException(nameof(minLength) + " and " + nameof(maxLength) + " should be positive numbers");
            }
            if(minLength > maxLength)
            {
                throw new ArgumentException(nameof(minLength) + " should not be greater than " + nameof(maxLength));
            }
            #endregion

            #region Remember min and max elems for each position and complete alphabets, if required 
            //we will work with indexes of alphabet, only at finish we transform them into letters

            //remember max elem of each alphabet
            List<char> maxElements = new List<char>();

            foreach (var alph in alphabets)
            {
                maxElements.Add((char)(alph.Length - 1));
            }

            //if number of alphabets less than maxLength
            if (maxElements.Count < maxLength)
            {
                var lastMaxElem = maxElements.Last();

                //in this case use last alphabet for the rest letters
                while (maxElements.Count != maxLength)
                {
                    maxElements.Add(lastMaxElem);

                    alphabets.Add(alphabets.Last());
                }
            }

            //min elem of each alphabet is 0 (because we work with indexes)
            var minElem = (char)0;


            #endregion


            //go throw all possible lengthes of password
            for (int currentLength = minLength; currentLength <= maxLength; ++currentLength)
            {
                int lastPosition = currentLength - 1;

                //finish when attain max value
                char[] maxPassword = maxElements.Take(currentLength).ToArray();

                //begin from the smallest value
                char[] minPassword = new string(minElem, currentLength).ToCharArray();

                //current password 
                char[] charPass = minPassword;


                //go throw all possible values of charPass, while attain max value
                while (!charPass.SequenceEqual(maxPassword))
                {
                    //go throw all possible characters at last position
                    for (char k = minElem; k <= maxPassword[lastPosition]; ++k)
                    {
                        charPass[lastPosition] = k;

                        //make words from indexes
                        yield return new string(
                            charPass.Select((a,i) => alphabets[i][a]).ToArray()
                            );
                    }
                    //when all values of last position were picked over

                    //for the last value
                    if (charPass.SequenceEqual(maxPassword))
                    {
                        break;
                    }

                    //counter for going throw all positions
                    int j = 0;

                    //go throw all positions, check if overflow
                    while (charPass[lastPosition - j] == maxElements[lastPosition - j])
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
    }
}
