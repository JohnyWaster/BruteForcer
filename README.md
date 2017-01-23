There are two classes in the namespace BruteForcer: DictionaryOfPasswordsCreator and BruteForcer.

	  DictionaryOfPasswordsCreator class helps to generate all combinations in given alphabet with different conditions.
	  This class provides 3 methods:
    
		1. MakeDictionaryFromAlphabet has 4 overloads:
			a) Generate all possible combinations of letters from given alphabet with required length.
			b) Generate all possible combinations of letters from given alphabet from minimal word to maximum word (in alphabetical order).
			c) Generate all possible combinations of letters from given alphabet with required length, which contain required substring.
			d) Generic version of point a).
      
		2. MakeDictionaryFromSomeAlphabets has 3 overloads:
			a) Generate all possible combinations of letters from given alphabets with required length. Each letter of the word is chosen from corresponding alphabet.
			b) Generate all possible combinations of letters from given alphabets from minimal word to maximum word (in alphabetical order). Each letter of the word is chosen from corresponding alphabet.
			c) Generate all possible combinations of letters from given alphabets with required length, which contain required substring. Each letter of the word is chosen from corresponding alphabet.
		
    3. MakeDictionariesForSomeThreads has 2 overloads:
			a) Generate all possible combinations of letters from given alphabet with required length and divide it into some parts (for example for multithreading usage).
			b) Generate all possible combinations of letters from given alphabets with required length and divide it into some parts (for example for multithreading usage). Each letter of the word is chosen from corresponding alphabet.	  
	  
    BruteForcer class helps to pick over all possible passwords in asynchronous way. It can be very useful for test covering with all possible values. 
	  It is handy to generate dictionary of passwords with DictionaryOfPasswordsCreator and than use this class to pick over this dictionary.

You can find examples of usage in Example.cs.txt file.

Nuget package is available here: https://www.nuget.org/packages/BruteForcer/1.0.0
