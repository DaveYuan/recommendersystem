using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace itemrecommatrixfact2
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
	
		static double calcGlobalAverage(List<int> trainRatingList) 
		{
			int sum = 0;
			int size = trainRatingList.Count;
			double globalAverage;
			
			foreach (int rating in trainRatingList) {
				sum = sum + rating;
			}
			globalAverage = (double)sum/(double)size;
			Console.WriteLine("\tGlobal average: {0}\n", globalAverage);
			return globalAverage;
		}
		
		static void initFeatures(int numUsers, ref double[,] userFeature, 
			             		int numItems, ref double[,] itemFeature,
		                        int numFeatures)
		{
			int i;
			int j;
			
			for (i = 0; i < numFeatures; i++) {
				for (j = 0; j < numUsers; j++) {
					userFeature[i,j] = 0.1;
				}
			}
			
			for (i = 0; i < numFeatures; i++) {
				for (j = 0; j < numItems; j++) {
					itemFeature[i,j] = 0.1;
				}
			}
		}
		
		static double adjustingFactor(double[,] userFeature, 
		                         double[,] itemFeature, 
		                         int numFeatures, 
		                         int userId,
		                         int itemId)
		{
			double dotProduct = 0.0;
			for (int i = 0; i < numFeatures; ++i) {
				dotProduct += userFeature[i,userId] * itemFeature[i,itemId];				
			}
			return dotProduct;
		}
		
		static void userAvg(ref Dictionary<string, double> userIdAvgHash, 
		                    Dictionary<string, List<int>> trainUserIdMapping,
		                    int[] trainRatingArray)
		{
			double sum;
			int size;
			
			foreach (KeyValuePair<string, List<int>> userId in trainUserIdMapping) {
				size = userId.Value.Count;
				sum = 0.0;
				foreach (int indx in userId.Value) {
					sum += trainRatingArray[indx];
				}
				sum = sum / size;
				userIdAvgHash.Add(userId.Key, sum);
			}
		}
		
		static void itemAvg(ref Dictionary<string, double> itemIdAvgHash,
		                     Dictionary<string, List<int>> trainItemIdMapping,
		                     int[] trainRatingArray)
		{
			double sum;
			int size;
			
			foreach (KeyValuePair<string, List<int>> itemId in trainItemIdMapping) {
				size = itemId.Value.Count;
				sum = 0.0;
				foreach (int indx in itemId.Value) {
					sum += trainRatingArray[indx];
				}
				sum = sum / size;
				itemIdAvgHash.Add(itemId.Key, sum);
			}
		}
		
		static void train(double K,
		                  int epochs,
		                  int numUsers,
		                  int numItems,
		                  int numFeatures,
		                  double lrate,
		                  ref double globalAverage,
		                  ref double[,] userFeature,
		                  ref double[,] itemFeature,
		                  ref Dictionary<string, int> userIdHash,
		                  ref Dictionary<string, int> itemIdHash,
		                  List<string> trainUserIdList, 
		                  List<string> trainItemIdList,
		                  List<int> trainRatingList,
		                  Dictionary<string, List<int>> trainUserIdMapping,
		                  Dictionary<string, List<int>> trainItemIdMapping                                 
		                  )
		{	
			int uId;
			int mId;
			int itr;
			int trainNumEntries;	
			int userIdCounter;
			int itemIdCounter;						
			double uv;
			double trainErr;				
			double trainErrPerEpoch;					
			double trainPredictRating;				
			double trainUserItemProduct;
			
			string[] line = new string[epochs];
			Dictionary<int, double> trainErrList = new Dictionary<int, double>();
			Dictionary<string, double> userIdAvgHash = new Dictionary<string, double>();
			Dictionary<string, double> itemIdAvgHash = new Dictionary<string, double>();			
		
			trainNumEntries = trainRatingList.Count;
			
			int[] trainRatingArray = trainRatingList.ToArray();	
			
			globalAverage = calcGlobalAverage(trainRatingList);			
		
			initFeatures(trainUserIdMapping.Count, ref userFeature, 
			             trainItemIdMapping.Count, ref itemFeature,
			             numFeatures);			
			
			userIdCounter = itemIdCounter = 0;
			userAvg(ref userIdAvgHash, trainUserIdMapping, trainRatingArray);
			itemAvg(ref itemIdAvgHash, trainItemIdMapping, trainRatingArray);
			
			
			foreach (KeyValuePair<string, List<int>> userId in trainUserIdMapping)
            {
                userIdHash.Add(userId.Key, userIdCounter);				
				userIdCounter++;
            }
			
			foreach (KeyValuePair<string, List<int>> itemId in trainItemIdMapping)
            {
                itemIdHash.Add(itemId.Key, itemIdCounter);
				itemIdCounter++;
            }
			
			Stopwatch stopwatch = new Stopwatch();

			stopwatch.Start();		
			
			for (itr = 0; itr < epochs; ++itr) {			
				trainErrPerEpoch = 0.0;	

				for (int q = 0; q < trainNumEntries; ++q) {					
					trainUserItemProduct = adjustingFactor(userFeature, itemFeature, numFeatures, userIdHash[trainUserIdList[q]], itemIdHash[trainItemIdList[q]]);												
				
					trainPredictRating = globalAverage + trainUserItemProduct;								
					trainErr = trainRatingArray[q] - trainPredictRating;						 						
				
					uId = userIdHash[trainUserIdList[q]];
					mId = itemIdHash[trainItemIdList[q]];
					trainErrPerEpoch += trainErr * trainErr;			
					
					for (int j = 0; j < numFeatures; ++j) {																																																			
						uv = userFeature[j,uId];						
						userFeature[j,uId] += lrate * (trainErr * itemFeature[j,mId] - K * uv);
						itemFeature[j,mId] += lrate * (trainErr * uv - K * itemFeature[j,mId]);						
					}																
				}
			
				trainErrPerEpoch = Math.Sqrt(trainErrPerEpoch/trainNumEntries);				

				Console.WriteLine( "Epoch = {0}, trainErrPerEpoch = {1}", itr, trainErrPerEpoch);
			
				trainErrList.Add(itr, trainErrPerEpoch);
			}						
			
			stopwatch.Stop();
			Console.WriteLine("\n\tTrain Time: {0}\n", stopwatch.Elapsed);
			
			itr = 0;
			foreach (KeyValuePair<int, double> errId in trainErrList)
            {
				string tmp = errId.Key + " " + errId.Value;
				line[itr] = tmp;
				itr++;                
            }
			System.IO.File.WriteAllLines("train_error.txt", line);					
		}
		
//		
//		static void baselineAvgRating(List<string> trainItemIdList, 
//		                                      List<int> trainRatingList, 
//		                                      ref Dictionary<string, int> numRatings) 
//		{					
//			int n = trainRatingList.Count;
//			for (int itr = 0; itr < n; itr++) {
//				if (numRatings.ContainsKey( trainItemIdList[itr] )){
//					numRatings[trainItemIdList[itr]] = numRatings[trainItemIdList[itr]] + 1;
//				} else {
//					numRatings.Add(trainItemIdList[itr], 1);
//				}
//			}							
//		}
//					
		static int calcItemHitInSortedList(int N, string rankedItem, Dictionary<string, double> itemRatingMapping)
		{
			int count = 0;
			
			/*
			 * Sort Avg Rating List
			 */		
			
			var sortedItemRatingMapping = from pair in itemRatingMapping
								orderby pair.Value descending
								select pair;						
			
			foreach (KeyValuePair<string, double> pair in sortedItemRatingMapping)
			{
				count++;						
				if (count == N+1) break;
				if (pair.Key.Equals(rankedItem, StringComparison.Ordinal)) {
					return 1;
				}				
			}	
			
			return 0;
		}	
		
		static void readDataAndRecall(
			                          String fileName, 
			                          int N,
		                              double globalAverage,
			                          int numFeatures,
			                          double[,] userFeature,
			                          double[,] itemFeature,
			                          Dictionary<string, int> userIdHash,
			                          Dictionary<string, int> itemIdHash,
			                          ref Dictionary<int, double> recallData)
		{		
			int T = 0; 
			int hits = 0;			
			int rowIndexCounter;				
			double recall;
			double precision;		
			double testUserItemProduct;
			double testPredictRating;
			string[] lines = System.IO.File.ReadAllLines(fileName);					
			string[] stringSeparator = new string[] { "\t" };
			
			/*
			 * Read test set			 
			*/				 	
			
			foreach (string line in lines)
	        {	
				T++;				
				string rankedItem = null;
				string user = null;
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				Dictionary<string, double> itemRatingMapping = new Dictionary<string, double>();						
				
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{			
					if (rowIndexCounter == 0) {
						user = s;
					}
					
					if (rowIndexCounter == 1) {
						rankedItem = s;
						}
						
					if (rowIndexCounter >= 1) {
						testUserItemProduct = adjustingFactor(userFeature, itemFeature, numFeatures, userIdHash[user], itemIdHash[s]);
						testPredictRating = globalAverage + testUserItemProduct;
						if (!itemRatingMapping.ContainsKey(s)) {
							itemRatingMapping.Add(s, testPredictRating);																												
						} 
//						else {
//							Console.WriteLine("User: {0}, Repeated Item: {1}, Testpredict: {2}, Original: {3}", user, s, testPredictRating, itemRatingMapping[s]);
//						}
					}
					rowIndexCounter++;
				}					
				hits += calcItemHitInSortedList(N, rankedItem, itemRatingMapping);						
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
			int numUsers;
			int numItems;
			int epochs = 200;
			int numFeatures = 50;
			double K = 0.1;
			double lrate = 0.01;
			double globalAverage = 0;
				
			List<int> trainRatingList = new List<int>();			
			List<string> trainUserIdList = new List<string>();
			List<string> trainItemIdList = new List<string>();			
		
			Dictionary<string, int> userIdHash = new Dictionary<string, int>();
			Dictionary<string, int> itemIdHash = new Dictionary<string, int>();			
			Dictionary<string, List<int>> trainUserIdMapping = new Dictionary<string, List<int>>();
			Dictionary<string, List<int>> trainItemIdMapping = new Dictionary<string, List<int>>();						
			Dictionary<int, double> recallData = new Dictionary<int, double>();
						
			readTrainSet("train.txt", 
			             ref trainUserIdMapping, 
			             ref trainItemIdMapping, 
			             ref trainUserIdList, 
			             ref trainItemIdList, 
			             ref trainRatingList);						
			
			numUsers = trainUserIdMapping.Count;
			numItems = trainItemIdMapping.Count;
			
			double[,] userFeature = new double[numFeatures,numUsers];
			double[,] itemFeature = new double[numFeatures,numItems];						

			train(K,
			      epochs,
		          numUsers,
		          numItems,
		          numFeatures,
		          lrate,
			      ref globalAverage,
			      ref userFeature,
			      ref itemFeature,
			      ref userIdHash,
			      ref itemIdHash,
		          trainUserIdList, 
		          trainItemIdList,
		          trainRatingList,
		          trainUserIdMapping,
		          trainItemIdMapping			  
			      );	
			
			for (int N = minN; N <= maxN; N = N+1) {			
				readDataAndRecall("test.txt", 
				                  N, 
				                  globalAverage, 
				                  numFeatures,
				                  userFeature, 
				                  itemFeature, 
				                  userIdHash,
				                  itemIdHash, 
				                  ref recallData);			
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
			
	/*		
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
			*/					
		}					
	
	}
}