using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace baselineitemrecomnitemtoprating2
{
	class MainClass
	{		
		static void readTrainSet(String fileName, 
		                         ref Dictionary<string, List<int>> trainUserIdMapping, 
		                         ref Dictionary<string, List<int>> trainItemIdMapping,
		                         ref List<string> trainUserIdList,
		                         ref List<string> trainItemIdList,
		                         ref List<int> trainRatingList)
		{
			int userIdIndex;
			int itemIdIndex;
			int rowIndexCounter;				
			string[] lines = System.IO.File.ReadAllLines(fileName);			
			
			userIdIndex = itemIdIndex = 0;
			
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
						trainItemIdList.Add(s);
						if (trainItemIdMapping.ContainsKey(s))
						{
							trainItemIdMapping[s].Add(itemIdIndex);				
							itemIdIndex++;
						} else {
							List<int> tmpIndexList = new List<int>();
							tmpIndexList.Add(itemIdIndex);
							trainItemIdMapping.Add(s,tmpIndexList);
							itemIdIndex++;
						}
					}
					
					if(rowIndexCounter == 2) {
						trainRatingList.Add(Convert.ToInt32(s));
					}
					
					rowIndexCounter++;
				}
			}	
		}	
	
		static void baselineAvgRating(List<string> trainItemIdList, 
		                                      List<int> trainRatingList, 
		                                      ref Dictionary<string, int> numRatings) 
		{					
			int n = trainRatingList.Count;
			for (int itr = 0; itr < n; itr++) {
				if (numRatings.ContainsKey( trainItemIdList[itr] )){
					numRatings[trainItemIdList[itr]] = numRatings[trainItemIdList[itr]] + 1;
				} else {
					numRatings.Add(trainItemIdList[itr], 1);
				}
			}							
		}
					
		static int calcItemHitInSortedList(int N, string rankedItem, Dictionary<string,double> itemAvgList)
		{
			int count = 0;
			
			/*
			 * Sort Avg Rating List
			 */		
			
			var sortedAvgRating = from pair in itemAvgList
								orderby pair.Value descending
								select pair;
			
			foreach (KeyValuePair<string, double> pair in sortedAvgRating)
			{
				count++;						
				if (count == N+1) break;
				if (pair.Key.Equals(rankedItem, StringComparison.Ordinal)) {
					return 1;
				}				
			}	
			
			return 0;
		}	
		
		static void readDataAndRecall(String fileName, int N, Dictionary<string, int> numRatings,
		                              ref Dictionary<int, double> recallData)
		{
			int T = 0; 
			int hits = 0;			
			int rowIndexCounter;				
			double recall;
			double precision;			
			string[] lines = System.IO.File.ReadAllLines(fileName);					
			string[] stringSeparator = new string[] { "\t" };
			
			/*
			 * Read test set			 
			*/				 	
			
			foreach (string line in lines)
	        {	
				T++;				
				string rankedItem = null;					
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				Dictionary<string,double> itemAvgList = new Dictionary<string,double>();	
				
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{																				
					if (rowIndexCounter == 1) {
						rankedItem = s;
					}
						
					/*						 
					 * Look into this
					 */
						
					if (rowIndexCounter >= 1) {
						if (!itemAvgList.ContainsKey(s)) {
							if (numRatings.ContainsKey(s)) {
								itemAvgList.Add(s, numRatings[s]);
							} 
//								else {
//								Console.WriteLine("Key not present in train: {0}", s);
//								itemAvgList.Add(s,0.0);
//							}
						}					
					}
					rowIndexCounter++;
				}					
				hits += calcItemHitInSortedList(N, rankedItem, itemAvgList);						
			}	
							
			Console.WriteLine("#Test: {0}",T);
			Console.WriteLine("#Hits: {0}", hits);
			recall = (double)hits / (double)T;
			precision = (double)recall / (double)N;
			
			Console.WriteLine("Recall: {0}, Precision: {1}", recall, precision);	
			recallData.Add(N, recall);
		}
				
		public static void Main (string[] args)
		{		
			int itr;
			int minN = 8;
			int maxN = 50;		
			int numLines = (maxN - minN) + 1;
				
			List<int> trainRatingList = new List<int>();			
			List<string> trainUserIdList = new List<string>();
			List<string> trainItemIdList = new List<string>();			
		
			Dictionary<string, List<int>> trainUserIdMapping = new Dictionary<string, List<int>>();
			Dictionary<string, List<int>> trainItemIdMapping = new Dictionary<string, List<int>>();
			Dictionary<string, int> numRatings = new Dictionary<string, int>();
			Dictionary<int, double> recallData = new Dictionary<int, double>();
						
			readTrainSet("train.txt", 
			             ref trainUserIdMapping, 
			             ref trainItemIdMapping, 
			             ref trainUserIdList, 
			             ref trainItemIdList, 
			             ref trainRatingList);						
			
			baselineAvgRating(trainItemIdList,trainRatingList, ref numRatings);				
			
			for (int N = minN; N <= maxN; N = N+1) {			
				readDataAndRecall("test.txt", N, numRatings, ref recallData);			
			}
		
			itr = 0;
			string[] line = new string[numLines];
			foreach (KeyValuePair<int, double> recall in recallData)
            {
				string tmp = recall.Key + " " + recall.Value;
				line[itr] = tmp;
				itr++;                
            }
			System.IO.File.WriteAllLines("recall.txt", line);						
		}					
	}
}