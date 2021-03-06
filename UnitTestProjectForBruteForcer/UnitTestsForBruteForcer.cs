﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using BruteForcer;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Threading;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace UnitTestProjectForBruteForcer
{
    [TestFixture]
    public class UnitTestsForBruteForcer
    {
        [Test]
        public void MakeDictionaryFromAlphabetTest()
        {
            char[] smallEnglishLetters = new char[26].Select((letter, i) => (char)('a' + i)).ToArray();

            IEnumerable<string> myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(smallEnglishLetters, 2, 5);

            long counter = 0;
            foreach (var pass in myDict)
            {
                counter++;
                if (pass == "zzzzz")
                {
                    AreEqual(Math.Pow(26, 2) + Math.Pow(26, 3) + Math.Pow(26, 4) + Math.Pow(26, 5), counter);
                }
            }
        }



        [Test]
        public void MakeDictionaryOfPasswordsFromAlphabetInRangeTest()
        {
            char[] smallEnglishLetters = new char[26].Select((letter, i) => (char)('a' + i)).ToArray();


            //test of equal length of min and max passwords
            IEnumerable<string> myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(smallEnglishLetters, "aa", "az");

            AreEqual(26, myDict.Count());


            //test of min password's last letter is not the first letter of alphabet
            myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(smallEnglishLetters, "ac", "az");

            AreEqual(24, myDict.Count());

            //test of min password's last letter is not the first letter of alphabet
            myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(smallEnglishLetters, "ac", "bn");

            AreEqual(38, myDict.Count());


            //test of max password's last letter is not the last letter of alphabet
            myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(smallEnglishLetters, "aa", "ezzzb");

            AreEqual(Math.Pow(26, 2) + Math.Pow(26, 3) + 6 * Math.Pow(26, 4) - 24, myDict.Count());

            //test of letter not from alphabet

            myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(smallEnglishLetters, "Aa", "e1zzb");

            try
            {
                myDict.Count();
            }
            catch (ArgumentException e)
            {
                IsTrue(e.Message.Contains("Letters not from alphabet where found"));
            }

        }

        [Test]
        public void SetDictionariesOfPasswordsTest()
        {
            char[] smallEnglishLetters = new char[26].Select((letter, i) => (char)('a' + i)).ToArray();

            int numberOfCores = 5;

            //test of right dividing entire dictionary into parts
            IList<IEnumerable<string>> myDict = DictionaryOfPasswordsCreator.MakeDictionariesForSomeThreads(smallEnglishLetters, 2, 5, numberOfCores);

            int wholeNumberOfPasswords = myDict.Sum(a => a.Count());

            AreEqual(Math.Pow(26, 2) + Math.Pow(26, 3) + Math.Pow(26, 4) + Math.Pow(26, 5) + numberOfCores - 1, wholeNumberOfPasswords);


            myDict = DictionaryOfPasswordsCreator.MakeDictionariesForSomeThreads(smallEnglishLetters, 1, 1, numberOfCores);

            wholeNumberOfPasswords = myDict.Sum(a => a.Count());

            AreEqual(26 + numberOfCores - 1, wholeNumberOfPasswords);
        }

        [Test]
        public void MultiCoreBruteForceTest()
        {
            char[] smallEnglishLetters = new char[26].Select((letter, i) => (char)('a' + i)).ToArray();

            IList<IEnumerable<string>> myDict = DictionaryOfPasswordsCreator.MakeDictionariesForSomeThreads(smallEnglishLetters, 1, maxLength: 10, numberOfThreads: 1);

            global::BruteForcer.BruteForcer bf = new global::BruteForcer.BruteForcer(myDict);

            var rightValue = "zaswa";

            var foundValue = bf.BruteForceAsync(a => a, a => a == rightValue).Result;

            AreEqual(rightValue, foundValue);
        }

        [Test]
        public void MultiCoreTooMuchThreadsTest()
        {
            char[] smallEnglishLetters = new char[10].Select((letter, i) => (char)('a' + i)).ToArray();

            IList<IEnumerable<string>> myDict = DictionaryOfPasswordsCreator.MakeDictionariesForSomeThreads(smallEnglishLetters, 1, 3, 15000);

            BruteForcer.BruteForcer bf = new BruteForcer.BruteForcer(myDict);

            var rightValue = "bc";

            var foundValue = bf.BruteForceAsync(a => a, a => a == rightValue).Result;

            AreEqual(rightValue, foundValue);

        }
        
        [Test]
        public void SiteAuthorizationTest()
        {
            char[] alphabet = DictionaryOfPasswordsCreator.Numbers;

            IList<IEnumerable<string>> myDict = DictionaryOfPasswordsCreator.MakeDictionariesForSomeThreads(alphabet, 1, 3, 3);

            string rightPassword = "";


            HttpClient client = new HttpClient();

            var requestUri = new Uri("http://localhost:81/Autharization/Login");

            BruteForcer.BruteForcer bf = new BruteForcer.BruteForcer(myDict);

            var rightLogin = bf.BruteForceAsync(login =>
            {
                BruteForcer.BruteForcer bfForPas = new BruteForcer.BruteForcer(myDict);

                var rightPass = bfForPas.BruteForceAsync(pas =>
                {
                    
                    HttpRequestMessage message = new HttpRequestMessage()
                    {
                        Method = HttpMethod.Post,
                        RequestUri = requestUri,
                        Content = new FormUrlEncodedContent(
             new List<KeyValuePair<string, string>>
             {
                            new KeyValuePair<string, string>("UserName", login),
                            new KeyValuePair<string, string>("Password",pas)
             }
             )
                    };

                    var answer = client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead).Result;
                                  

                    return answer.RequestMessage.RequestUri;

                }, hasSuccess: uri => uri != requestUri
                ).Result;

                return rightPass;

            }, pas =>
            {
                if (pas != null)
                {
                    rightPassword = pas;
                    return true;
                }
                return false;
            }, new CancellationToken()
            ).Result;


            AreEqual("123", rightLogin);
            AreEqual("456", rightPassword);
        }
        

        [Test]
        public void MakeDictionaryFromSomeAlphabets()
        {
            
            #region Validation Tests

            try
            {
                DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(null, 1, 1).Count();
            }
            catch(ArgumentNullException e)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(e.Message.Contains("alphabets"));
            }

            try
            {
                DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(new List<char[]>(), 1, 1).Count();
            }
            catch (ArgumentException e)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(e.Message.Contains("should contain at least one alphabet"));
            }

            
            try
            {
                var alphabets = new List<char[]>();
                alphabets.Add(null);
                
                DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, 1, 1).Count();
            }
            catch (NullReferenceException e)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(e.Message.Contains(" should not contain null references"));
            }


            try
            {
                var alphabets = new List<char[]>();
                alphabets.Add(new char[0]);

                DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, 1, 1).Count();
            }
            catch (ArgumentException e)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(e.Message.Contains(" should not contain empty alphabets"));
            }

            try
            {
                var alphabets = new List<char[]>();

                alphabets.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

                DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, -1, 1).Count();
            }
            catch(ArgumentException e)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(e.Message.Contains(" should be positive numbers"));
            }

            try
            {
                List<char[]> alphabets = new List<char[]>();

                alphabets.Add(DictionaryOfPasswordsCreator.Numbers);

                DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, 3, 1).Count();
            }
            catch (ArgumentException e)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(e.Message.Contains(" should not be greater"));
            }
            #endregion

            #region One alphabet test

            var alph = new List<char[]>();
            
            alph.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            var myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alph, 1, 3);

            AreEqual(Math.Pow(26,1) + Math.Pow(26, 2) + Math.Pow(26, 3), myDict.Count());

            #endregion

            #region Number of alphabets is equal to maxLength

            var alph1 = new List<char[]>();

            alph1.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            alph1.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            alph1.Add(DictionaryOfPasswordsCreator.Numbers);

            AreEqual(Math.Pow(26, 1) + Math.Pow(26, 2) + Math.Pow(26, 2)*10, DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alph1, 1, 3).Count());

            #endregion

            #region Number of alphabets less than maxLength

            var alph2 = new List<char[]>();

            alph2.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            alph2.Add(DictionaryOfPasswordsCreator.Numbers);

            AreEqual(Math.Pow(26, 1) + 26*10 + Math.Pow(10, 2) * 26, DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alph2, 1, 3).Count());


            AreEqual(Math.Pow(10, 3) * 26 + Math.Pow(10, 2) * 26, DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alph2, 3, 4).Count());

            #endregion

            #region Number of alphabets greater than maxLength

            var alph3 = new List<char[]>();

            alph3.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            alph3.Add(DictionaryOfPasswordsCreator.Numbers);

            alph3.Add(DictionaryOfPasswordsCreator.Numbers);

            alph3.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            alph3.Add(DictionaryOfPasswordsCreator.SmallEnglishLetters);

            AreEqual(Math.Pow(26, 1) + 26 * 10 + Math.Pow(10, 2) * 26, DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alph3, 1, 3).Count());

            #endregion
            /*
            var alphs = new List<char[]>();

            alphs.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            alphs.Add(DictionaryOfPasswordsCreator.Numbers);

            alphs.Add(DictionaryOfPasswordsCreator.SpecialSymbols);

            alphs.Add(DictionaryOfPasswordsCreator.SmallEnglishLetters);

            alphs.Add(DictionaryOfPasswordsCreator.SmallEnglishLetters);

            alphs.Add(DictionaryOfPasswordsCreator.Numbers);

            var myPasses = DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphs, 6, 6);

            AreEqual(26*10*26, myPasses.Count(a => a.Contains("aa3")));
            */
        }


        [Test]
        public void MakeDictionaryWithSubstringTest()
        {
            var alph = DictionaryOfPasswordsCreator.Numbers;

            #region Validation tests

            try
            {
                DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(null, 1, 3, "");
            }
            catch (ArgumentNullException e)
            {               
                IsTrue(e.Message.Contains("alphabet"));
            }

            try
            {
                DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(alph, 1, 3, null);
            }
            catch (ArgumentNullException e)
            {
                IsTrue(e.Message.Contains("subString"));
            }

            try
            {
                DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(alph, 1, 0, "");
            }
            catch (ArgumentException e)
            {
                IsTrue(e.Message.Contains("length of password should be greater then 0"));
            }

            try
            {
                DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(alph, 5, 3, "");
            }
            catch (ArgumentException e)
            {
                IsTrue(e.Message.Contains("minLength can't be greater than maxLength"));
            }

            try
            {
                DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(alph, 1, 3, "");
            }
            catch (ArgumentException e)
            {
                IsTrue(e.Message.Contains("subString should not be empty"));
            }
            
            #endregion


            var dict = DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(alph, 1, 7, "1234");

            AreEqual(1 + 320 + 1000 * 4, dict.Count());//111 11 x11 11x

            var dict1 = DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(alph, 1, 3, "1111");

            AreEqual(0, dict1.Count());
        }

        [Test]
        public void MakeDictionaryFromSomeAlphabetsWithSubstringTest()
        {
            IList<char[]> alphabets = new List<char[]>();

            var alph = DictionaryOfPasswordsCreator.Numbers;

            alphabets.Add(alph);

            var dict = DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, 1, 7, "1234");

            AreEqual(1 + 320 + 1000 * 4, dict.Count());

            dict = DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(alph, 1, 3, "1111");

            AreEqual(0, dict.Count());

            alphabets = new List<char[]>();

            alphabets.Add(DictionaryOfPasswordsCreator.Numbers);

            alphabets.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            alphabets.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            alphabets.Add(DictionaryOfPasswordsCreator.SmallEnglishLetters);

            alphabets.Add(DictionaryOfPasswordsCreator.Numbers);

            alphabets.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            dict = DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, 1, 7, "1ZZ");

            AreEqual(26 * 10 * 26 * 26 * 2 + 26 * 10 * 26 + 26 * 10 + 26 + 1, dict.Count());

            dict = DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, 3, 7, "1A");

            AreEqual(26 * 26 * 10 * 26 * 26 * 2 + 26 * 26 * 10 * 26 * 2 + 26 * 26 * 10 + 26 * 26 + 26, dict.Count());
        }

        [Test]
        public void MakeDictionatyFromSomeAlphabetsInRangeTest()
        {
            IList<char[]> alphabets = new List<char[]>();

            var alph = DictionaryOfPasswordsCreator.SmallEnglishLetters;

            alphabets.Add(alph);

            //test of equal length of min and max passwords
            IEnumerable<string> myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, "aa", "az");

            AreEqual(26, myDict.Count());


            //test of min password's last letter is not the first letter of alphabet
            myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, "ac", "az");

            AreEqual(24, myDict.Count());

            AreEqual("ae", myDict.ElementAt(2));


            //test of max password's last letter is not the last letter of alphabet
            myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, "aa", "ezzzb");

            AreEqual(Math.Pow(26, 2) + Math.Pow(26, 3) + 6 * Math.Pow(26, 4) - 24, myDict.Count());

            //test of letter not from alphabet

            myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, "Aa", "e1zzb");

            try
            {
                myDict.Count();
            }
            catch (ArgumentException e)
            {
                IsTrue(e.Message.Contains("can not be obtained"));
            }

            alphabets = new List<char[]>();

            alphabets.Add(DictionaryOfPasswordsCreator.BigEnglishLetters.Concat(DictionaryOfPasswordsCreator.SmallEnglishLetters).ToArray());

            alphabets.Add(DictionaryOfPasswordsCreator.SmallEnglishLetters.Concat(DictionaryOfPasswordsCreator.Numbers).ToArray());

            alphabets.Add(DictionaryOfPasswordsCreator.SmallEnglishLetters);
            
            myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, "Ca", "e1zzb");

            //       len 2     len 3      len 4      len 5  d9zzz       e0zzz         e1zzz      
            AreEqual(50*36 + 52*36*26 + 52*36*26*26 + 30*36*26*26*26 + 27*26*26*26 + 26*26*26 - 24, myDict.Count());

        }

        [Test]
        public void MakeDictionatiesForSomeThreadsFromSomeAlphabetsTest()
        {
            var alphabets = new List<char[]>();

            alphabets.Add(DictionaryOfPasswordsCreator.Numbers);

            alphabets.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            alphabets.Add(DictionaryOfPasswordsCreator.SmallEnglishLetters);

            alphabets.Add(DictionaryOfPasswordsCreator.Numbers);

            alphabets.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            var myDict = DictionaryOfPasswordsCreator.MakeDictionariesForSomeThreads(alphabets, 1, 6, 4);

            //AreEqual(10 + 10*26 + 10*26*26 + 10*26*26*10 + 10*26*26*10*26 + 10*26*26*10*26*26 + 3, myDict.Sum(a=>a.Count()));

            BruteForcer.BruteForcer bf = new BruteForcer.BruteForcer(myDict);

            var rightValue = "4Af3TR";

            var foundValue = bf.BruteForceAsync(a => a, a => a == rightValue).Result;

            AreEqual(rightValue, foundValue);
        }

        [Test]
        public void BruteForceWithSomeAlphabetsTest()
        {
            var alphabets = new List<char[]>();

            alphabets.Add(DictionaryOfPasswordsCreator.Numbers);

            alphabets.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            alphabets.Add(DictionaryOfPasswordsCreator.SmallEnglishLetters);

            alphabets.Add(DictionaryOfPasswordsCreator.Numbers);

            alphabets.Add(DictionaryOfPasswordsCreator.BigEnglishLetters);

            var myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromSomeAlphabets(alphabets, 1, 6);

            BruteForcer.BruteForcer bf = new BruteForcer.BruteForcer(myDict);

            var rightValue = "4Af3TR";

            var foundValue = bf.BruteForceAsync(a => a, a => a == rightValue).Result;

            AreEqual(rightValue, foundValue);
        }

        [Test]
        public void MakeDictionaryFromGenericAlphabetTest()
        {
            var alphabet = new int[16].Select((a,i)=>Convert.ToString(i, 16)).ToArray();

            var dictionary = DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(alphabet, 5, 5);

            AreEqual(Math.Pow(16,5), dictionary.Count());

            AreEqual(new []{"f","f","f","f","f"}, dictionary.Last());
        }
    }
}
