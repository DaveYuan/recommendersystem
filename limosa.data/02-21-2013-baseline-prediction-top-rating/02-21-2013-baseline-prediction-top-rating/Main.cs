using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace baselinepredictiontoprating
{
	class MainClass
	{
		static void readTrainSet(String fileName, 
		                         ref Dictionary<string, List<int>> trainUserIdMapping, 
		                         ref Dictionary<string, List<int>> trainMovieIdMapping,
		                         ref List<string> trainUserIdList,
		                         ref List<string> trainMovieIdList,
		                         ref List<int> trainRatingList)
		{
			int userIdIndex;
			int movieIdIndex;
			int rowIndexCounter;	
			
			string[] lines = System.IO.File.ReadAllLines(fileName);
			//displayTrainSet(lines);
			
			userIdIndex = movieIdIndex = 0;
			
			foreach (string line in lines)
	        {	
				string[] stringSeparator = new string[] { "\t" };
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{
					if (rowIndexCounter == 0) {
						trainUserIdList.Add(s);
						if (trainUserIdMapping.ContainsKey(s))
						{
							trainUserIdMapping[s].Add(userIdIndex);				
							userIdIndex++;
						} else {
							List<int> tmpIndexList = new List<int>();
							tmpIndexList.Add(userIdIndex);
							trainUserIdMapping.Add(s,tmpIndexList);
							userIdIndex++;
						}
					} 
					
					if (rowIndexCounter == 1) {
						trainMovieIdList.Add(s);
						if (trainMovieIdMapping.ContainsKey(s))
						{
							trainMovieIdMapping[s].Add(movieIdIndex);				
							movieIdIndex++;
						} else {
							List<int> tmpIndexList = new List<int>();
							tmpIndexList.Add(movieIdIndex);
							trainMovieIdMapping.Add(s,tmpIndexList);
							movieIdIndex++;
						}
					}
					
					if(rowIndexCounter == 2) {
						trainRatingList.Add(Convert.ToInt32(s));
					}
					
					rowIndexCounter++;
				}
			}	
		}	
		
		static void readTestSet(String fileName,
			            ref List<string> testUserIdList, 
			            ref List<string> testMovieIdList, 
			            ref List<int> testRatingList)
		{
			int rowIndexCounter;	
			
			string[] lines = System.IO.File.ReadAllLines(fileName);					
			
			foreach (string line in lines)
	        {	
				string[] stringSeparator = new string[] { "\t" };
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{
					if (rowIndexCounter == 0) {
						testUserIdList.Add(s);
					} 
					
					if (rowIndexCounter == 1) {
						testMovieIdList.Add(s);
					}
					
					if(rowIndexCounter == 2) {
						testRatingList.Add(Convert.ToInt32(s));
					}
					
					rowIndexCounter++;
				}
			}	
			
		}
		
		static Dictionary<string, double> sortDictionaryValue(Dictionary<string, double> avgRating)
		{
			List<KeyValuePair<string, double>> tempList = new List<KeyValuePair<string, double>>(avgRating);

    		tempList.Sort(delegate(KeyValuePair<string, double> firstPair, KeyValuePair<string, double> secondPair)
            {
            	return firstPair.Value.CompareTo(secondPair.Value);
            }
            );

    		Dictionary<string, double> mySortedDictionary = new Dictionary<string, double>();
    		foreach(KeyValuePair<string, double> pair in tempList)
    		{
        		mySortedDictionary.Add(pair.Key, pair.Value);
    		}

    		return mySortedDictionary;
		}
		
		static List<string> topNRatings(int N, Dictionary<string, int> numRatings) 
		{
			List<string> topNList = new List<string>();
			int count = 0;
			
			var sortedNumRatings = from pair in numRatings
								orderby pair.Value descending
								select pair;
			
			
			foreach (KeyValuePair<string, int> pair in sortedNumRatings)
			{
				count++;						
				if(count == N+1) break;
				topNList.Add(pair.Key);	    					
			}	
			
			return topNList;			
			
		}
		
		static List<string> baselineTopPopular(int N, List<string> trainMovieIdList, List<int> trainRatingList) 
		{
			List<string> topNList;
			Dictionary<string, int> numRatings = new Dictionary<string, int>();
			
			for (int itr = 0; itr < trainRatingList.Count; itr++) {
				if (numRatings.ContainsKey( trainMovieIdList[itr] )){
					numRatings[trainMovieIdList[itr]] = numRatings[trainMovieIdList[itr]] + 1;
				} else {
					numRatings.Add(trainMovieIdList[itr], 1);
				}
			}
			
			return topNRatings(N, numRatings);					
		}
		
		static void recall_prediction(int N, List<string> topNList, List<string> testMovieIdList, List<int> testRatingList)
		{
			int hit = 0;
			int T = testRatingList.Count;
			double recall;
			double precision;
			
			for (int i = 0; i < T; i++) {
				if(topNList.Contains(testMovieIdList[i],StringComparer.OrdinalIgnoreCase)) {
					hit = hit+1;
				}				
			}
			
			Console.WriteLine("#Test: {0}",T);
			Console.WriteLine("#Hits: {0}", hit);
			recall = (double)hit / (double)T;
			precision = (double)recall / (double)N;
			
			Console.WriteLine("Recall: {0}, Precision: {1}", recall, precision);
		}
		
		public static void Main (string[] args)
		{			
			int N = 10;
			List<string> topNList = new List<string>();
			List<int> trainRatingList = new List<int>();			
			List<string> trainUserIdList = new List<string>();
			List<string> trainMovieIdList = new List<string>();			
			List<int> testRatingList = new List<int>();			
			List<string> testUserIdList = new List<string>();
			List<string> testMovieIdList = new List<string>();	
			
			Dictionary<string, List<int>> trainUserIdMapping = new Dictionary<string, List<int>>();
			Dictionary<string, List<int>> trainMovieIdMapping = new Dictionary<string, List<int>>();
			
			readTrainSet("train.txt.mtx", 
			             ref trainUserIdMapping, 
			             ref trainMovieIdMapping, 
			             ref trainUserIdList, 
			             ref trainMovieIdList, 
			             ref trainRatingList);			
			
			topNList = baselineTopPopular(N, trainMovieIdList,trainRatingList);
			
			readTestSet("test.txt.mtx",
			            ref testUserIdList, 
			            ref testMovieIdList, 
			            ref testRatingList);
	
			recall_prediction(N, topNList, testMovieIdList, testRatingList);						
		}					
	}
}

