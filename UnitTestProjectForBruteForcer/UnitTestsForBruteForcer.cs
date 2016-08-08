using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using BruteForcer;
using System.Diagnostics;
using System.Linq;

namespace UnitTestProjectForBruteForcer
{
    [TestClass]
    public class UnitTestsForBruteForcer
    {
        [TestMethod]
        public void MakeDictionaryFromAlphabetTest()
        {
            char[] smallEnglishLetters = new char[26].Select((letter, i) => (char)('a' + i)).ToArray();

            IEnumerable<string> myDict = DictionaryBruteForcer.MakeDictionaryOfPasswordsFromAlphabet(smallEnglishLetters, 2, 5);

            long counter = 0;
            foreach(var pass in myDict)
            {
                counter++;
                if(pass == "zzzzz")
                {
                    Assert.AreEqual(Math.Pow(26, 2)+Math.Pow(26, 3) + Math.Pow(26, 4) +Math.Pow(26, 5), counter);
                }
            }                   
        }

        [TestMethod]
        public void BruteForce()
        {
            char[] smallEnglishLetters = new char[26].Select((letter, i) => (char)('a' + i)).ToArray();

            IEnumerable<string> myDict = DictionaryBruteForcer.MakeDictionaryOfPasswordsFromAlphabet(smallEnglishLetters, 1, 10);

            DictionaryBruteForcer bf = new DictionaryBruteForcer(myDict);

            var rightValue = "xzasw";

            var foundValue = bf.BruteForce(a => a, a => a == rightValue);

            Assert.AreEqual(foundValue, rightValue);
        }

        [TestMethod]
        public void MakeDictionaryOfPasswordsFromAlphabetInRangeTest()
        {
            char[] smallEnglishLetters = new char[26].Select((letter, i) => (char)('a' + i)).ToArray();
           
            
            //test of equal length of min and max passwords
            IEnumerable<string> myDict = DictionaryBruteForcer.MakeDictionaryOfPasswordsFromAlphabetInRange(smallEnglishLetters, "aa", "az");

            Assert.AreEqual(26, myDict.Count());


            //test of min password's last letter is not the first letter of alphabet
            myDict = DictionaryBruteForcer.MakeDictionaryOfPasswordsFromAlphabetInRange(smallEnglishLetters, "ac", "az");

            Assert.AreEqual(24, myDict.Count());


            //test of max password's last letter is not the last letter of alphabet
            myDict = DictionaryBruteForcer.MakeDictionaryOfPasswordsFromAlphabetInRange(smallEnglishLetters, "aa", "ezzzb");

            Assert.AreEqual(Math.Pow(26, 2) + Math.Pow(26, 3) + 6* Math.Pow(26, 4) - 24, myDict.Count());
        }

        [TestMethod]
        public void SetDictionariesOfPasswordsTest()
        {
            char[] smallEnglishLetters = new char[26].Select((letter, i) => (char)('a' + i)).ToArray();

            int numberOfCores = 5;

            //test of right dividing entire dictionary into parts
            IList<IEnumerable<string>> myDict = MultiCoreDictionaryBruteForcer.MakeDictionariesOfPasswordsFromAlphabetForSomeCores(smallEnglishLetters, 2, 5, numberOfCores);

            int wholeNumberOfPasswords = myDict.Sum(a => a.Count());

            Assert.AreEqual(Math.Pow(26, 2) + Math.Pow(26, 3) + Math.Pow(26, 4) + Math.Pow(26, 5) + numberOfCores - 1 , wholeNumberOfPasswords);
        }
    }
}
