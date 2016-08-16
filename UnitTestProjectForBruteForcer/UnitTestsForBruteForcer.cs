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

            IEnumerable<string> myDict = DictionaryBruteForcer.MakeDictionaryOfPasswordsFromAlphabet(smallEnglishLetters, 1, 4);

            long counter = 0;
            foreach(var pass in myDict)
            {
                counter++;
                if(pass == "zzzz")
                {
                    Assert.AreEqual(26 + Math.Pow(26, 2) + Math.Pow(26, 3) + Math.Pow(26, 4), counter);
                }
            }
                   
        }
    }
}
