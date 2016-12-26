using System;
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
            foreach(var pass in myDict)
            {
                counter++;
                if(pass == "zzzzz")
                {
                    AreEqual(Math.Pow(26, 2)+Math.Pow(26, 3) + Math.Pow(26, 4) +Math.Pow(26, 5), counter);
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


            //test of max password's last letter is not the last letter of alphabet
            myDict = DictionaryOfPasswordsCreator.MakeDictionaryFromAlphabet(smallEnglishLetters, "aa", "ezzzb");

            AreEqual(Math.Pow(26, 2) + Math.Pow(26, 3) + 6* Math.Pow(26, 4) - 24, myDict.Count());

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

            AreEqual(Math.Pow(26, 2) + Math.Pow(26, 3) + Math.Pow(26, 4) + Math.Pow(26, 5) + numberOfCores - 1 , wholeNumberOfPasswords);


            myDict = DictionaryOfPasswordsCreator.MakeDictionariesForSomeThreads(smallEnglishLetters, 1, 1, numberOfCores);

            wholeNumberOfPasswords = myDict.Sum(a => a.Count());

            AreEqual(26 + numberOfCores - 1, wholeNumberOfPasswords);
        }

        [Test]
        public void MultiCoreBruteForceTest()
        {
            char[] smallEnglishLetters = new char[26].Select((letter, i) => (char)('a' + i)).ToArray();

            IList<IEnumerable<string>> myDict = DictionaryOfPasswordsCreator.MakeDictionariesForSomeThreads(smallEnglishLetters, 1, maxLength: 10, numberOfThreads: 1);

            IBruteForcer bf = new MultiThreadDictionaryBruteForcer(myDict);

            var rightValue = "zaswa";

            var foundValue = bf.BruteForceAsync(a => a, a => a == rightValue).Result;

            AreEqual(rightValue, foundValue);
        }
        
        [Test]
        public void MultiCoreTooMuchThreadsTest()
        {
            char[] smallEnglishLetters = new char[10].Select((letter, i) => (char)('a' + i)).ToArray();

            IList<IEnumerable<string>> myDict = DictionaryOfPasswordsCreator.MakeDictionariesForSomeThreads(smallEnglishLetters, 1, 3, 1500000);
            
            IBruteForcer bf = new MultiThreadDictionaryBruteForcer(myDict);

            var rightValue = "bc";

            var foundValue = bf.BruteForceAsync(a => a, a => a == rightValue).Result;

            AreEqual(rightValue, foundValue);
           
        }

        [Test]
        public void SiteAuthorizationTest()
        {
            char[] alphabet = DictionaryOfPasswordsCreator.Numbers;

            IList<IEnumerable<string>> myDict = DictionaryOfPasswordsCreator.MakeDictionariesForSomeThreads(alphabet, 1, 3, 3);

            string rightLogin = "";

            string rightPassword = "";

            HttpClient client = new HttpClient();


            MultiThreadDictionaryBruteForcer bf = new MultiThreadDictionaryBruteForcer(myDict);

            var result = bf.BruteForceAsync(login =>
            {
                MultiThreadDictionaryBruteForcer bfForPas = new MultiThreadDictionaryBruteForcer(myDict);

                var rightPass = bfForPas.BruteForceAsync(pas =>
                {
                    HttpRequestMessage message = new HttpRequestMessage()
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("http://localhost:49263/autharization"),
                        Content = new FormUrlEncodedContent(
             new List<KeyValuePair<string, string>>
             {
                            new KeyValuePair<string, string>("Login", login),
                            new KeyValuePair<string, string>("Password",pas)
             }
             )
                    };

                    var answerContent = client.SendAsync(message).Result.Content;

                    return answerContent;

                }, hasSuccess: cont => cont.ReadAsStringAsync().Result.Contains("You are welcome")
                ).Result;

                rightPassword = rightPass;

                rightLogin = login;

                return rightPass;

            }, pas =>
            {
                if (pas != null)
                {
                    return true;
                }
                return false;
            }, new CancellationToken()
            ).Result;


            AreEqual("13", rightLogin);
            AreEqual("45", rightPassword);
        }
        
    }
    
}
