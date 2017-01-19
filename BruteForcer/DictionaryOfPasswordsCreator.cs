using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BruteForcer
{
    /// <summary>
    /// Provides methods for generation all possible combinations of letters from given
    /// alphabets with different conditions. It looks like picking over all numbers
    /// in n-ary number system.
    /// </summary>
    public static class DictionaryOfPasswordsCreator
    {
        public static char[] SmallEnglishLetters
        {
            get { return new char[26].Select((letter, i) => (char) ('a' + i)).ToArray(); }
        }

        public static char[] BigEnglishLetters
        {
            get { return new char[26].Select((letter, i) => (char) ('A' + i)).ToArray(); }
        }

        public static char[] Numbers
        {
            get { return new char[10].Select((letter, i) => (char) ('0' + i)).ToArray(); }
        }

        public static char[] SpecialSymbols
        {
            get
            {
                var special1 = new char[16].Select((letter, i) => (char) (' ' + i));

                var special2 = new char[6].Select((letter, i) => (char) ('[' + i));

                var special3 = new char[4].Select((letter, i) => (char) ('{' + i));

                var special = special1.Concat(special2).Concat(special3);

                return special.ToArray();
            }
        }

        /// <summary>
        /// Makes dictionaries of passwords for each thread from alphabet
        /// </summary>
        /// <param name="alphabet">set of characters, which can be included in password</param>
        /// <param name="minLength">min length of password</param>
        /// <param name="maxLength">max length of password</param>
        /// <param name="numberOfThreads">number of dictionaries for dividing entire dictionary of passwords, each dictionary for each core</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IList<IEnumerable<string>> MakeDictionariesForSomeThreads(char[] alphabet, int minLength,
            int maxLength, int numberOfThreads)
        {
            if (numberOfThreads < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfThreads));
            }
            if (minLength < 0 || maxLength < 0)
            {
                throw new ArgumentException("length of password should be nonnegative");
            }
            //in this case we don't have anything to return
            if (minLength == 0 && maxLength == 0)
            {
                return new List<IEnumerable<string>>();
            }
            if (minLength == 0)
            {
                minLength = 1;
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
                string maxVal = new string(alphabet[alphabet.Length/numberOfThreads], maxLength);

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
                        maxVal = new string(alphabet[(i + 1)*alphabet.Length/numberOfThreads], maxLength);
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
        /// Makes dictionaries of passwords for each thread from different alphabets, where letters 
        /// at each position can be from corresponding alphabets.
        /// It means, than 1-st alphabet is used for letter at 1-st position, 2-nd alphabet for 2-nd position of password, etc...
        /// If number of alphabets less than maxLength, last alphabet is used for the rest of letters.
        /// If number of alphabets greater than maxLength, rest of alphabets are not used.
        /// </summary>
        /// <param name="alphabets">Set of alphabets, each alphabet is using for corresponding position in password</param>
        /// <param name="minLength">Min length of password.</param>
        /// <param name="maxLength">Max length of password.</param>
        /// <param name="numberOfThreads">number of dictionaries for dividing entire dictionary of passwords, each dictionary for each thread</param>
        /// <returns>List of sets of passwords, obtained from given alphabets. Each set for each thread.</returns>
        public static IList<IEnumerable<string>> MakeDictionariesForSomeThreads(IList<char[]> alphabets, int minLength,
            int maxLength, int numberOfThreads)
        {
            #region Validation of input parameters

            if (numberOfThreads < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfThreads));
            }
            if (minLength < 0 || maxLength < 0)
            {
                throw new ArgumentException("length of password should be nonnegative");
            }
            //in this case we don't have anything to return
            if (minLength == 0 && maxLength == 0)
            {
                return new List<IEnumerable<string>>();
            }
            if (minLength == 0)
            {
                minLength = 1;
            }
            if (minLength > maxLength)
            {
                throw new ArgumentException("minLength can't be greater than maxLength");
            }
            if (alphabets == null)
            {
                throw new ArgumentNullException();
            }

            if (!alphabets.Any())
            {
                throw new ArgumentException(nameof(alphabets) + " should contain at least one alphabet.");
            }

            foreach (var alph in alphabets)
            {
                if (alph == null)
                {
                    throw new NullReferenceException(nameof(alphabets) + " should not contain null references.");
                }
                if (!alph.Any())
                {
                    throw new ArgumentException(nameof(alphabets) + " contains empty alphabet.");
                }
            }

            #endregion


            var dictionariesForDifferentCores = new List<IEnumerable<string>>();

            if (numberOfThreads == 1)
            {
                dictionariesForDifferentCores.Add(MakeDictionaryFromSomeAlphabets(alphabets, minLength, maxLength));
            }

            else
            {
                #region Remember min and max elements of alphabets and complete alphabets with last alphabet, if required.
                //remember min and max elements of each alphabet
                List<char> minElements = new List<char>();

                List<char> maxElements = new List<char>();

                foreach (var alph in alphabets)
                {
                    minElements.Add(alph.First());
                    maxElements.Add(alph.Last());
                }

                //if number of alphabets less than maxLength
                if (maxElements.Count < maxLength)
                {
                    var lastMaxElem = maxElements.Last();
                    var lastMinElem = minElements.Last();

                    //in this case use last alphabet for the rest letters
                    while (maxElements.Count != maxLength)
                    {
                        maxElements.Add(lastMaxElem);
                        minElements.Add(lastMinElem);

                        alphabets.Add(alphabets.Last());
                    }
                }

                #endregion

                string minVal = new string(minElements.Take(minLength).ToArray());

                //max value for first thread should begin from this value
                var maxValList = new List<char>()
                {
                    alphabets[0][alphabets[0].Length / numberOfThreads]
                };

                string maxVal = new string(maxValList.Concat(maxElements.Skip(1)).ToArray());

                for (int i = 1; i <= numberOfThreads; ++i)
                {
                    try
                    {
                        dictionariesForDifferentCores.Add(MakeDictionaryFromSomeAlphabets(alphabets, minVal, maxVal));
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
                        //max value for first thread should begin from this value
                        maxValList = new List<char>()
                        {
                            alphabets[0][(i+1)*alphabets[0].Length / numberOfThreads]
                        };

                        maxVal = new string(maxValList.Concat(maxElements.Skip(1)).ToArray());
                    }
                    else
                    {
                        maxVal = new string(maxElements.ToArray());
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
            #region Validation of input parameters

            if (minLength < 0 || maxLength < 0)
            {
                throw new ArgumentException("length of password should not be negative");
            }
            if (minLength > maxLength)
            {
                throw new ArgumentException("minLength can't be greater than maxLength");
            }
            //in this case we don't have anything to return
            if (minLength == 0 && maxLength == 0)
            {
                yield break;
            }
            if (minLength == 0)
            {
                minLength = 1;
            }

            if (alphabet == null)
            {
                throw new ArgumentNullException();
            }
            if (alphabet.Length == 0)
            {
                throw new ArgumentException(nameof(alphabet) + " should contain at least one element");
            }

            #endregion

            //if alphabet has duplicates remove them
            alphabet = alphabet.Distinct().ToArray();

            int len = alphabet.Length;

            //we will work with indexes of alphabet, only at finish we transform them into letters
            char maxElem = (char) (len - 1);

            char minElem = (char) 0;


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
        public static IEnumerable<string> MakeDictionaryFromAlphabet(char[] alphabet, string minPassword,
            string maxPassword)
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
                throw new ArgumentException("Letters not from alphabet where found",
                    notMatchedLetters.First(c => true).ToString());
            }


            if (String.Compare(maxPassword, minPassword, StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("maxPassword should be greater than minPassword");
            }

            int len = alphabet.Length;

            //we will work with indexes of alphabet, only at finish we transform them into letters
            char maxElem = (char) (len - 1);

            char minElem = (char) 0;

            //go throw all possible lengthes of password
            for (int currentLength = minPassword.Length; currentLength <= maxPassword.Length; ++currentLength)
            {
                int lastPosition = currentLength - 1;

                //finish when attain max value
                char[] maxCharPassword = new string(maxElem, currentLength).ToCharArray();

                if (currentLength == maxPassword.Length)
                {
                    maxCharPassword =
                        maxPassword.ToCharArray().Select(a => (char) new string(alphabet).IndexOf(a)).ToArray();
                }


                //begin from the smallest value
                char[] minCharPassword = new string(minElem, currentLength).ToCharArray();

                if (currentLength == minPassword.Length)
                {
                    minCharPassword =
                        minPassword.ToCharArray().Select(a => (char) new string(alphabet).IndexOf(a)).ToArray();
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
        /// It means, than 1-st alphabet is used for letter at 1-st position, 2-nd alphabet for 2-nd position of password, etc...
        /// If number of alphabets less than maxLength, last alphabet is used for the rest of letters.
        /// If number of alphabets greater than maxLength, rest of alphabets are not used.
        /// If alphabets contain only one alphabet, method is equal to MakeDictionaryFromAlphabet.
        /// </summary>
        /// <param name="alphabets">Set of alphabets, each alphabet is using for corresponding position in password</param>
        /// <param name="minLength">Minimal length of password</param>
        /// <param name="maxLength">Maximum length of password.</param>
        /// <returns></returns>
        public static IEnumerable<string> MakeDictionaryFromSomeAlphabets(IList<char[]> alphabets, int minLength,
            int maxLength)
        {
            #region Validation of input parameters
            if (minLength < 0 || maxLength < 0)
            {
                throw new ArgumentException(nameof(minLength) + " and " + nameof(maxLength) +
                                            " should be positive numbers");
            }
            if (minLength > maxLength)
            {
                throw new ArgumentException(nameof(minLength) + " should not be greater than " + nameof(maxLength));
            }
            //in this case we don't have anything to return
            if (minLength == 0 && maxLength == 0)
            {
                yield break;
            }
            if (minLength == 0)
            {
                minLength = 1;
            }

            if (alphabets == null)
            {
                throw new ArgumentNullException(nameof(alphabets));
            }
            if (!alphabets.Any())
            {
                throw new ArgumentException(nameof(alphabets) + " should contain at least one alphabet");
            }
            foreach (var alph in alphabets)
            {
                if (alph == null)
                {
                    throw new NullReferenceException(nameof(alphabets) + " should not contain null references");
                }
                if (alph.Length == 0)
                {
                    throw new ArgumentException(nameof(alphabets) + " should not contain empty alphabets");
                }
            }           
            #endregion

            #region Remember min and max elems for each position and complete alphabets, if required 

            //we will work with indexes of alphabet, only at finish we transform them into letters

            //remember max elem of each alphabet
            List<char> maxElements = new List<char>();

            foreach (var alph in alphabets)
            {
                maxElements.Add((char) (alph.Length - 1));
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
            var minElem = (char) 0;


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
                            charPass.Select((a, i) => alphabets[i][a]).ToArray()
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

        /// <summary>
        /// generate all possible values of password of required length, which contain substring
        /// </summary>
        /// <param name="alphabet">set of characters, which can be included in password</param>
        /// <param name="minLength">min length of a password</param>
        /// <param name="maxLength">max length of a password</param>
        /// <param name="subString">substring, which should be included in password</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>dictionary of all appropriate words, number of them equals alphabet.Length**minLength+...+alphabet.Length**maxLength</returns>
        public static IEnumerable<string> MakeDictionaryFromAlphabet(char[] alphabet, int minLength, int maxLength,
            string subString)
        {
            #region Validation input parameters

            if (minLength < 0 || maxLength < 0)
            {
                throw new ArgumentException("length of password should be greater then 0");
            }
            if (minLength > maxLength)
            {
                throw new ArgumentException("minLength can't be greater than maxLength");
            }
            //in this case we don't have anything to return
            if (minLength == 0 && maxLength == 0)
            {
                yield break;
            }
            if (minLength == 0)
            {
                minLength = 1;
            }
            if (alphabet == null)
            {
                throw new ArgumentNullException();
            }
            if (alphabet.Length == 0)
            {
                throw new ArgumentException(nameof(alphabet) + " should contain at least one element");
            }

            if (subString == null)
            {
                throw new ArgumentNullException(nameof(subString));
            }
            if (subString == "")
            {
                throw new ArgumentException(nameof(subString) + " shoud not be empty");
            }

            //if substring length is greater then minlength, minlength of password will be at least substring length
            minLength = Math.Max(minLength, subString.Length);

            //if substring length is greater then maxlength, we don't have anything to return
            if (subString.Length > maxLength)
            {
                yield break;
            }

            //find letters not from alphabet in substring
            var notMatchedLetters = from letter in subString.ToCharArray()
                where !alphabet.Contains(letter)
                select letter;

            if (notMatchedLetters.Any())
            {
                yield break;
            }

            #endregion

            //go throw all possible lengthes of password
            for (int currentLength = minLength; currentLength <= maxLength; ++currentLength)
            {
                var restOfPassword = currentLength - subString.Length;

                //push substring throw password with current length
                for (int subStringStart = 0; subStringStart < restOfPassword + 1; subStringStart++)
                {

                    //if we don't have left part
                    if (subStringStart == 0)
                    {
                        //if password equals to substrinig
                        if (currentLength == subString.Length)
                        {
                            yield return subString;
                        }

                        //pick over part of password to the right from substring
                        foreach (
                            var rightPart in
                                MakeDictionaryFromAlphabet(alphabet, restOfPassword - subStringStart,
                                    restOfPassword - subStringStart))
                        {
                            yield return subString + rightPart;
                        }
                    }

                    //pick over part of password to the left from substring
                    foreach (var leftPart in MakeDictionaryFromAlphabet(alphabet, subStringStart, subStringStart))
                    {
                        //pick over part of password to the right from substring
                        foreach (
                            var rightPart in
                                MakeDictionaryFromAlphabet(alphabet, restOfPassword - subStringStart,
                                    restOfPassword - subStringStart))
                        {
                            yield return leftPart + subString + rightPart;
                        }
                    }

                    //if we don't have right part
                    if (subStringStart == restOfPassword)
                    {
                        //pick over part of password to the left from substring
                        foreach (var leftPart in MakeDictionaryFromAlphabet(alphabet, subStringStart, subStringStart))
                        {
                            yield return leftPart + subString;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate all possible values of password, where letters at each position can be from corresponding alphabets, which contain given substring.
        /// It means, than 1-st alphabet is used for letter at 1-st position, 2-nd alphabet for 2-nd position of password, etc...
        /// If number of alphabets less than maxLength, last alphabet is used for the rest of letters.
        /// If number of alphabets greater than maxLength, rest of alphabets are not used.
        /// If alphabets contain only one alphabet, method is equal to MakeDictionaryFromAlphabet.
        /// </summary>
        /// <param name="alphabets">Set of alphabets, each alphabet is using for corresponding position in password</param>
        /// <param name="minLength">Minimal length of password</param>
        /// <param name="maxLength">Maximum length of password.</param>
        /// <param name="subString">substring, which should be included in password</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <returns></returns>
        public static IEnumerable<string> MakeDictionaryFromSomeAlphabets(IList<char[]> alphabets, int minLength,
            int maxLength, string subString)
        {
            #region Validation of input parameters

            if (alphabets == null)
            {
                throw new ArgumentNullException(nameof(alphabets));
            }
            if (!alphabets.Any())
            {
                throw new ArgumentException(nameof(alphabets) + " should contain at least one alphabet");
            }
            foreach (var alph in alphabets)
            {
                if (alph == null)
                {
                    throw new NullReferenceException(nameof(alphabets) + " should not contain null references");
                }
                if (alph.Length == 0)
                {
                    throw new ArgumentException(nameof(alphabets) + " should not contain empty alphabets");
                }
            }

            if (minLength < 0 || maxLength < 0)
            {
                throw new ArgumentException(nameof(minLength) + " and " + nameof(maxLength) +
                                            " should be positive numbers");
            }
            if (minLength > maxLength)
            {
                throw new ArgumentException(nameof(minLength) + " should not be greater than " + nameof(maxLength));
            }
            //in this case we don't have anything to return
            if (minLength == 0 && maxLength == 0)
            {
                yield break;
            }
            if (minLength == 0)
            {
                minLength = 1;
            }
            if (subString == null)
            {
                throw new ArgumentNullException(nameof(subString));
            }
            if (subString == "")
            {
                throw new ArgumentException(nameof(subString) + " should not be empty");
            }

            //if substring length is greater then minlength, minlength of password will be at least substring length
            minLength = Math.Max(minLength, subString.Length);

            //if substring length is greater then maxlength, we don't have anything to return
            if (subString.Length > maxLength)
            {
                yield break;
            }

            #endregion

            #region Remember min and max elems for each position and complete alphabets, if required.

            //remember max elem of each alphabet
            List<char> maxElements = new List<char>();

            foreach (var alph in alphabets)
            {
                maxElements.Add((char) (alph.Length - 1));
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


            #endregion

            #region  Remember, where substring can start 

            var tailLength = maxLength - subString.Length;

            //list to remember all positions, where subString can start
            var startPositionsOfSubstring = new List<int>();

            var lettersOfSubstring = subString.ToCharArray();

            //push substring throw password with current length
            for (int subStringStart = 0; subStringStart < tailLength + 1; subStringStart++)
            {
                bool canStartAtThisPosition = true;

                //go throw all letters of substring
                for (int i = 0; i < subString.Length; ++i)
                {
                    //if letter is included in corresponding alphabet
                    if (!alphabets[subStringStart + i].Contains(lettersOfSubstring[i]))
                    {
                        canStartAtThisPosition = false;
                        break;
                    }
                }

                if (canStartAtThisPosition == true)
                {
                    startPositionsOfSubstring.Add(subStringStart);
                }
            }

            //if start positions of substring were not found
            if (!startPositionsOfSubstring.Any())
            {
                yield break;
            }

            #endregion

            
            //go throw all possible lengthes of password
            for (int currentLength = minLength; currentLength <= maxLength; ++currentLength)
            {
                var restOfPassword = currentLength - subString.Length;

                //push substring throw password with current length
                foreach (var startPosition in startPositionsOfSubstring)
                {
                    //if substring will not get in currentLength
                    if (startPosition + subString.Length > currentLength)
                    {
                        break;
                    }

                    //if we don't have left part
                    if (startPosition == 0)
                    {
                        //if password equals to substrinig
                        if (currentLength == subString.Length)
                        {
                            yield return subString;
                        }

                        //pick over part of password to the right from substring
                        foreach (var rightPart in MakeDictionaryFromSomeAlphabets(alphabets.Skip(subString.Length).ToList(), restOfPassword - startPosition, restOfPassword - startPosition))
                        {
                            yield return subString + rightPart;
                        }
                    }

                    //pick over part of password to the left from substring
                    foreach (var leftPart in MakeDictionaryFromSomeAlphabets(alphabets.Take(startPosition).ToList(), startPosition, startPosition))
                    {
                        //pick over part of password to the right from substring
                        foreach (var rightPart in MakeDictionaryFromSomeAlphabets(alphabets.Skip(startPosition + subString.Length).ToList(), restOfPassword - startPosition, restOfPassword - startPosition))
                        {
                            yield return leftPart + subString + rightPart;
                        }
                    }

                    //if we don't have right part
                    if (startPosition == restOfPassword)
                    {
                        //pick over part of password to the left from substring
                        foreach (var leftPart in MakeDictionaryFromSomeAlphabets(alphabets.Take(startPosition).ToList(), startPosition, startPosition))
                        {
                            yield return leftPart + subString;
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// Generate all possible values of password in range from minPassword to maxPassword, where letters at each position
        /// can be from corresponding alphabets.
        /// It means, than 1-st alphabet is used for letter at 1-st position, 2-nd alphabet for 2-nd position of password, etc...
        /// If number of alphabets less than maxLength, last alphabet is used for the rest of letters.
        /// If number of alphabets greater than maxLength, rest of alphabets are not used.
        /// If alphabets contain only one alphabet, method is equal to MakeDictionaryFromAlphabet.
        /// </summary>
        /// <param name="alphabets">Set of alphabets, each alphabet is using for corresponding position in password</param>
        /// <param name="minPassword">min value of password</param>
        /// <param name="maxPassword">max value of password</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static IEnumerable<string> MakeDictionaryFromSomeAlphabets(IList<char[]> alphabets, string minPassword,
            string maxPassword)
        {
            #region Validation of input parameters

            if (alphabets == null || minPassword == null || maxPassword == null)
            {
                throw new ArgumentNullException();
            }
            if (!alphabets.Any())
            {
                throw new ArgumentException(nameof(alphabets) + " should contain at least one alphabet");
            }
            foreach (var alph in alphabets)
            {
                if (alph == null)
                {
                    throw new NullReferenceException(nameof(alphabets) + " should not contain null references");
                }
                if (alph.Length == 0)
                {
                    throw new ArgumentException(nameof(alphabets) + " should not contain empty alphabets");
                }
            }


            if (String.Compare(maxPassword, minPassword, StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("maxPassword should be greater than minPassword");
            }

            #endregion

            #region Remember min and max elems for each position and complete alphabets, if required.

            var minElem = (char) 0;

            //remember max elem of each alphabet
            List<char> maxElements = new List<char>();

            foreach (var alph in alphabets)
            {
                maxElements.Add((char) (alph.Length - 1));
            }

            //if number of alphabets less than maxLength
            if (maxElements.Count < maxPassword.Length)
            {
                var lastMaxElem = maxElements.Last();

                //in this case use last alphabet for the rest letters
                while (maxElements.Count != maxPassword.Length)
                {
                    maxElements.Add(lastMaxElem);

                    alphabets.Add(alphabets.Last());
                }
            }

            #endregion

            #region  Additional validation of minPassword and maxPassword 

            if (minPassword.Cast<char>().Where((t, i) => !alphabets[i].Contains(t)).Any())
            {
                throw new ArgumentException(nameof(minPassword) + " can not be obtained from this set of alphabets.");
            }

            if (maxPassword.Cast<char>().Where((t, i) => !alphabets[i].Contains(t)).Any())
            {
                throw new ArgumentException(nameof(maxPassword) + " can not be obtained from this set of alphabets.");
            }

            #endregion

            //go throw all possible lengthes of password
            for (int currentLength = minPassword.Length; currentLength <= maxPassword.Length; ++currentLength)
            {
                int lastPosition = currentLength - 1;

                //finish when attain max value
                char[] maxCharPassword = maxElements.Take(currentLength).ToArray();

                if (currentLength == maxPassword.Length)
                {
                    maxCharPassword =
                        maxPassword.ToCharArray().Select((a,i) => (char) new string(alphabets[i]).IndexOf(a)).ToArray();
                }


                //begin from the smallest value
                char[] minCharPassword = new string(minElem, currentLength).ToCharArray();

                if (currentLength == minPassword.Length)
                {
                    minCharPassword =
                        minPassword.ToCharArray().Select((a,i) => (char) new string(alphabets[i]).IndexOf(a)).ToArray();
                }

                //current password 
                char[] charPass = minCharPassword;

                //variable for distinguish minPassword, used only once
                char minElemForCurrentPosition = minCharPassword[lastPosition];
                
                //go throw all possible values of charPass, while attain max value
                while (!charPass.SequenceEqual(maxCharPassword))
                {
                    //go throw all possible characters at last position
                    for (char k = minElemForCurrentPosition; k <= maxElements[lastPosition]; ++k)
                    {
                        charPass[lastPosition] = k;

                        //make words from indexes
                        yield return new string(
                            charPass.Select((a,i) => alphabets[i][a]).ToArray()
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
                    while (charPass[lastPosition - j] == maxElements[lastPosition - j])
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
                            charPass.Select((a,i) => alphabets[i][a]).ToArray()
                            );
                    }
                }
            }
        }

        /// <summary>
        /// Generate all posiible combinations from alphabet.
        /// </summary>
        /// <typeparam name="T">type of letter in password</typeparam>
        /// <param name="alphabet">set of different letters of type T</param>
        /// <param name="minLength">minimal length of password</param>
        /// <param name="maxLength">maximum length of password</param>
        /// <returns></returns>
        public static IEnumerable<IList<T>> MakeDictionaryFromAlphabet<T>(T[] alphabet, int minLength, int maxLength)
        {
            #region Validation of input parameters
            
            if (minLength < 0 || maxLength < 0)
            {
                throw new ArgumentException("length of password should not be negative");
            }
            if (minLength > maxLength)
            {
                throw new ArgumentException("minLength can't be greater than maxLength");
            }
            //in this case we don't have anything to return
            if (minLength == 0 && maxLength == 0)
            {
                yield break;
            }
            if (minLength == 0)
            {
                minLength = 1;
            }

            if (alphabet == null)
            {
                throw new ArgumentNullException();
            }
            if (alphabet.Length == 0)
            {
                throw new ArgumentException(nameof(alphabet) + " should contain at least one element");
            }

            if (alphabet.Length >= Math.Pow(2,16))
            {
                throw new ArgumentException(nameof(alphabet) + " is too large");
            }

            #endregion

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
                        yield return charPass.Select(a => alphabet[a]).ToList();
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
    }
}
