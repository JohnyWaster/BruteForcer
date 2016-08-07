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
        public void TestMethodForDictionaryBruteForcer()
        {
            IEnumerable<string> myPasswords = new string[3] { "1", "2", "3"};

            IBruteForcer bforcer = new DictionaryBruteForcer(myPasswords);

            var myPasswordsEnumerator = myPasswords.GetEnumerator();


            Stopwatch t = new Stopwatch();

            
            t.Start();
            while(bforcer.IsNotFinished == true)
            {
                var a = bforcer.GetNextValue();
            }
            t.Stop();
            
            var timeOfWhile = t.ElapsedTicks;

            t.Restart();
            foreach (var a in myPasswords)
            {
                var b = a;
            }
            t.Stop();
            var timeOfForeach = t.ElapsedTicks;
        }

        [TestMethod]
        public void MakeDictionaryFromAlphabetTest()
        {
            char[] smallEnglishLetters = new char[26].Select((letter, i) => (char)('a' + i)).ToArray();

            IEnumerable<string> myDict = DictionaryBruteForcer.MakeDictionaryOfPasswordsFromAlphabet(smallEnglishLetters, 2, 6);

            long counter = 0;
            foreach(var pass in myDict)
            {
                counter++;
                if(pass == "zzzzzz")
                {
                    Assert.AreEqual(Math.Pow(26, 2)+Math.Pow(26, 3) + Math.Pow(26, 4) +Math.Pow(26, 5) + Math.Pow(26,6), counter);
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

           // var foundValue = bf.BruteForce(a => a, a => a == rightValue);

            var foundValue = bf.BruteForceSingle(a => a, a => a == rightValue);

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
            myDict = DictionaryBruteForcer.MakeDictionaryOfPasswordsFromAlphabetInRange(smallEnglishLetters, "aa", "ezzzzb");

            Assert.AreEqual(71763460, myDict.Count());
        }
    }
}
