using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BruteForcer
{
    /// <summary>
    /// Helps to pick over dictionary of passwords asynchronously.
    /// </summary>
    public class BruteForcer
    {
        private IList<IEnumerable<string>> _dictionariesForDifferentThreads;

        public IList<IEnumerable<string>> DictionariesForDifferentThreads
        {
            get { return _dictionariesForDifferentThreads; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(DictionariesForDifferentThreads));
                }

                _dictionariesForDifferentThreads = value;
            }
        }


        /// <summary>
        /// Constructor, which sets dictionaries for look through.
        /// </summary>
        /// <param name="listOfDictionaries">list of dictionaries, where password will be seeked</param>
        public BruteForcer(IList<IEnumerable<string>> listOfDictionaries)
        {
            if (listOfDictionaries == null)
            {
                throw new ArgumentNullException(nameof(listOfDictionaries));
            }

            foreach(var dict in listOfDictionaries)
            {
                if(dict == null)
                {
                    throw new ArgumentNullException(nameof(listOfDictionaries), "one of dictionaries is null");
                }
            }

            DictionariesForDifferentThreads = listOfDictionaries;
        }


        /// <summary>
        /// Constructor, which sets dictionary for look through for one thread.
        /// </summary>
        /// <param name="dictionary">dictionary, where password will be seeked.</param>
        public BruteForcer(IEnumerable<string> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            DictionariesForDifferentThreads = new List<IEnumerable<string>>();

            DictionariesForDifferentThreads.Add(dictionary);
        }

        


        /// <summary>
        /// Pick over all possible passwords in asynchronous way
        /// </summary>
        /// <typeparam name="T">type of value, which is returned by function, which makes one effort of password, and consumed by predicate, which determines is effort successfull</typeparam>
        /// <param name="attemptOfPassword">function, which makes one effort of password</param>
        /// <param name="hasSuccess">predicate, which determines, if password is right</param>
        /// <returns>right password or null, if all dictionary was picked over and right password was not found</returns>
        public async Task<string> BruteForceAsync<T>(Func<string, T> attemptOfPassword, Predicate<T> hasSuccess)
        {
            if(attemptOfPassword == null)
            {
                throw new ArgumentNullException(nameof(attemptOfPassword));
            }
            if(hasSuccess == null)
            {
                throw new ArgumentNullException(nameof(hasSuccess));
            }

            //list for storing tasks for each thread
            List<Task<string>> tasksForParticularThreads = new List<Task<string>>(DictionariesForDifferentThreads.Count);

            CancellationTokenSource cts = new CancellationTokenSource();

            //run picking over each dictionary in different threads
            foreach (var dict in DictionariesForDifferentThreads)
            {
                tasksForParticularThreads.Add(Task.Run(() =>
                {
                    string rightPassword = null;
                    
                    foreach (var password in dict)
                    {
                        try
                        {
                            if(cts.Token.IsCancellationRequested)
                            {
                                break;
                            }

                            T result = attemptOfPassword(password);

                            if (hasSuccess(result) == true)
                            {
                                rightPassword = password;
                                break;
                            }
                        }
                        catch(Exception e)
                        {
                            throw new Exception("error was occured during test of password " + password,e);
                        }
                    }

                    return rightPassword;
                }, cts.Token
                ));
            }

            //task to return
            Task<string> str = null;

            //wait for finishing tasks
            while (tasksForParticularThreads.Count > 0)
            {
                str = await Task.WhenAny(tasksForParticularThreads);

                tasksForParticularThreads.Remove(str);

                //if right password was found
                if(str.Result != null)
                {
                    //cancel another tasks
                    cts.Cancel();      
                                                   
                    return str.Result;
                }
            }

            return str.Result;
        }

        /// <summary>
        /// Pick over all possible passwords in asynchronous way
        /// </summary>
        /// <typeparam name="T">type of value, which is returned by function, which makes one effort of password, and consumed by predicate, which determines is effort successfull</typeparam>
        /// <param name="attemptOfPassword">function, which makes one effort of password</param>
        /// <param name="hasSuccess">predicate, which determines, if password is right</param>
        /// <param name="ct">token for cancellation</param>
        /// <returns>right password or null, if all dictionary was picked over and right password was not found</returns>
        public async Task<string> BruteForceAsync<T>(Func<string, T> attemptOfPassword, Predicate<T> hasSuccess, CancellationToken ct)
        {
            if (attemptOfPassword == null)
            {
                throw new ArgumentNullException(nameof(attemptOfPassword));
            }
            if (hasSuccess == null)
            {
                throw new ArgumentNullException(nameof(hasSuccess));
            }

            //list for storing tasks for each thread
            List<Task<string>> tasksForParticularThreads = new List<Task<string>>(DictionariesForDifferentThreads.Count);

            CancellationTokenSource cts = new CancellationTokenSource();

            //run picking over each dictionary in different threads
            foreach (var dict in DictionariesForDifferentThreads)
            {
                tasksForParticularThreads.Add(Task.Run(() =>
                {
                    string rightPassword = null;

                    foreach (var password in dict)
                    {
                        try
                        {
                            if (cts.Token.IsCancellationRequested || ct.IsCancellationRequested)
                            {
                                break;
                            }

                            T result = attemptOfPassword(password);

                            if (hasSuccess(result) == true)
                            {
                                rightPassword = password;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            cts.Cancel();
                            throw new Exception("error was occured during test of password " + password, e);
                        }
                    }

                    return rightPassword;
                }, cts.Token
                ));
            }

            //task to return
            Task<string> str = null;

            //wait for finishing tasks
            while (tasksForParticularThreads.Count > 0)
            {
                str = await Task.WhenAny(tasksForParticularThreads);

                tasksForParticularThreads.Remove(str);

                //if right password was found
                if (str.Result != null)
                {
                    //cancel another tasks
                    cts.Cancel();

                    return str.Result;
                }
            }

            return str.Result;
        }
    }
}