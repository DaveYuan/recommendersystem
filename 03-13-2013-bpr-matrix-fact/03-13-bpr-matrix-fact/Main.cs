using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace bprmatrixfact
{
	class MainClass
	{
		static void readTrainSet(String fileName, 
		                         ref Dictionary<string, List<int>> userIdMapping, 
		                         ref Dictionary<string, List<int>> itemIdMapping,
		                         ref Dictionary<string, List<string>> ratedItemsPerUser,
		                         ref List<string> trainUserIdList,
		                         ref List<string> trainItemIdList,
		                         ref List<int> trainRatingList,
		                         ref List<string> uniqueItemList
		                         )
		{
			int userIdIndex;
			int itemIdIndex;
			int rowIndexCounter;	
			
			string[] lines = System.IO.File.ReadAllLines(fileName);
			
			userIdIndex = itemIdIndex = 0;
			
			foreach (string line in lines)
	        {	
				string[] stringSeparator = new string[] { "\t" };
				string user = null;
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{					
					if (rowIndexCounter == 0) {
						trainUserIdList.Add(s);
						if (userIdMapping.ContainsKey(s))
						{
							userIdMapping[s].Add(userIdIndex);				
							userIdIndex++;
						} else {
							List<int> tmpIndexList = new List<int>();
							tmpIndexList.Add(userIdIndex);
							userIdMapping.Add(s,tmpIndexList);
							userIdIndex++;
						}
						user = s;
					} 
					
					if (rowIndexCounter == 1) {
						trainItemIdList.Add(s);
						if (itemIdMapping.ContainsKey(s))
						{
							itemIdMapping[s].Add(itemIdIndex);				
							itemIdIndex++;
						} else {
							uniqueItemList.Add(s);
							List<int> tmpIndexList = new List<int>();
							tmpIndexList.Add(itemIdIndex);
							itemIdMapping.Add(s,tmpIndexList);
							itemIdIndex++;
						}
						if (ratedItemsPerUser.ContainsKey(user))
						{
							ratedItemsPerUser[user].Add(s);
						} else {
							List<string> tmpIndexList = new List<string>();
							tmpIndexList.Add(s);
							ratedItemsPerUser.Add(user, tmpIndexList);
						}
					}
					
					if(rowIndexCounter == 2) {
						trainRatingList.Add(Convert.ToInt32(s));
					}					
					rowIndexCounter++;
				}
			}							
		}
		
		static double calcGlobalAverage(List<int> ratingList) 
		{
			int sum = 0;
			int size = ratingList.Count;
			double globalAverage;
			
			foreach (int rating in ratingList) {
				sum = sum + rating;
			}
			globalAverage = (double)sum/(double)size;
			Console.WriteLine("Global average {0}", globalAverage);
			return globalAverage;
		}
		
		static void initFeatures(int numUsers, ref double[,] userFeature, 
			             		int numItem, ref double[,] itemFeature,
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
				for (j = 0; j < numItem; j++) {
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
				//Console.WriteLine("{0} ", userFeature[i,userId]);
			}
			return dotProduct;
		}			
				
		static void learnHashID(ref Dictionary<string, int> userIdHash,
		                        ref Dictionary<string, int> itemIdHash,
		                        Dictionary<string, List<int>> userIdMapping,
		                        Dictionary<string, List<int>> itemIdMapping
		                        )
		{
			int userIdCounter;
			int itemIdCounter;	
			
			userIdCounter = itemIdCounter = 0;
			
			foreach (KeyValuePair<string, List<int>> userId in userIdMapping)
            {
                userIdHash.Add(userId.Key, userIdCounter);				
				userIdCounter++;
            }
			
			foreach (KeyValuePair<string, List<int>> itemId in itemIdMapping)
            {
                itemIdHash.Add(itemId.Key, itemIdCounter);
				itemIdCounter++;
            }			
		}
		
		static double sigmoid(double x)
		{
			return ( 1.0 / ( 1.0 + Math.Exp(-x) ) );
		}
		
		static double calNorm(double[,] features, int indx, int numFeatures) 
		{
			double norm = 0.0;
			
			for (int i = 0; i < numFeatures; i++) {
				norm += features[i,indx]*features[i, indx];
			}
			return norm;
		}
		
		
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
		                              int epoch,
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
						
					if (rowIndexCounter > 1) {
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
			
			Console.WriteLine("Recall: {0}, Precision: {1}\n", recall, precision);	
			recallData.Add(N, recall);
			//recallData.Add(epoch, recall);
		}
		

		
		static void learnBPR(
		                     double globalAverage,
		                     int bprEpochs,
		                     int numFeatures,
		                     int numTrainingExamples,
		                     double lrate,
		                     double K,
		                     ref double[,] userFeature,
		                     ref double[,] itemFeature,
		                     Dictionary<string, int> userIdHash,
		                     Dictionary<string, int> itemIdHash,
		                     Dictionary<string, List<string>> ratedItemsPerUser, 
		                     List<string> uniqueItemList,
		                     List<int> ratingList,
		                     Dictionary<string, List<int>> userIdMapping,
		                     Dictionary<string, List<int>> itemIdMapping
		                     ) 
		{
			int userIndx;
			int itemIIndex;
			int itemJIndex;
			int epoch;
			int randUserIndx;
			int randRatedItemIndx;			
			int randNonRatedItemIndx;
			int numRatedItems;
			int numUniqueUsers = ratedItemsPerUser.Keys.Count;
			int numUniqueItems = uniqueItemList.Count;						
			double uv;
			double xuij;
			double xui;
			double xuj;
			double bprOpt;
			double dervXuij;
			double err1;
			double err2;
			double errPerEpoch;
			string randUser;
			string randRatedItem;
			string randNonRatedItem;
			
			Random r1 = new Random();
			Random r2 = new Random();
			Random r3 = new Random();	
		
					
		//	Console.WriteLine("BPR Learning");		
		//	Console.WriteLine("numUniqueUsers: {0}, numUniqueItems: {1}", numUniqueUsers, numUniqueItems);
			
			int[] ratingArray = ratingList.ToArray();
			
			for (epoch = 1; epoch <= bprEpochs; epoch++) {
				bprOpt = 0.0;
				errPerEpoch = 0.0;
				
				for (int n = 0; n < numTrainingExamples; n++) {
					err1 = 0.0;
					err2 = 0.0;
					randUserIndx = r1.Next(0, numUniqueUsers);
					numRatedItems = ratedItemsPerUser[ratedItemsPerUser.Keys.ElementAt(randUserIndx)].Count;
					randRatedItemIndx = r2.Next(0, numRatedItems);
					randNonRatedItemIndx = r3.Next(0, numUniqueItems);
				
					randUser = ratedItemsPerUser.Keys.ElementAt(randUserIndx);
					randRatedItem = ratedItemsPerUser[randUser][randRatedItemIndx];
					randNonRatedItem = uniqueItemList[randNonRatedItemIndx];
				
					while (ratedItemsPerUser[randUser].Contains(randNonRatedItem)) {
						randNonRatedItemIndx = r3.Next(0, numUniqueItems);
						randNonRatedItem = uniqueItemList[randNonRatedItemIndx];
					}
				
					userIndx = userIdHash[randUser];
					itemIIndex = itemIdHash[randRatedItem];
					itemJIndex = itemIdHash[randNonRatedItem];
					
//					if (userIndx >= userIdMapping.Count) {Console.WriteLine("Exceeding User index: {0}", userIndx);}
//					if (itemIIndex >= itemIdMapping.Count) {Console.WriteLine("Exceeding Item_I index: {0}", itemIIndex);}
//					if (itemJIndex >= itemIdMapping.Count) {Console.WriteLine("Exceeding Item_J index: {0}", itemJIndex);}
//					
					xui = adjustingFactor(userFeature, itemFeature, numFeatures, userIndx, itemIIndex);
					xuj = adjustingFactor(userFeature, itemFeature, numFeatures, userIndx, itemJIndex);
					xuij = xui - xuj;
				
				//	if (xuij > 5)
				//	Console.WriteLine("xui: {0}, xuj: {1}, XUIJ: {2}", xui, xuj, xuij);
					
					for(int j = 0; j < numFeatures; j++) {	
						uv = userFeature[j,userIndx];
						dervXuij = itemFeature[j,itemIIndex] - itemFeature[j,itemJIndex];						
						userFeature[j,userIndx] += lrate * 
												( ( ( (Math.Exp(-xuij)) / (1 + Math.Exp(-xuij)) ) * dervXuij ) + K * uv );
					
						dervXuij = uv;
						itemFeature[j, itemIIndex] += lrate * 
												( ( ( (Math.Exp(-xuij)) / (1 + Math.Exp(-xuij)) ) * dervXuij ) + K * itemFeature[j, itemIIndex] );
					
						dervXuij = -uv;
						itemFeature[j, itemJIndex] += lrate * 
												( ( ( (Math.Exp(-xuij)) / (1 + Math.Exp(-xuij)) ) * dervXuij ) + K * itemFeature[j, itemJIndex] );															                                    					
					}
					
//					int ratingIndx;
//					List<int> userMappingIndx = userIdMapping[randUser];
//					List<int> itemIMappingIndx = itemIdMapping[randRatedItem];
//					List<int> itemJMappingIndx = itemIdMapping[randNonRatedItem];
//					
////					foreach (int i in userMappingIndx) {
//						if (itemIMappingIndx.Contains(i)) {	
//							if (i < ratingArray.Length) {								
//								err1 = ratingArray[i] - (globalAverage + xui);
//						//		err1 = err1 * err1;
//								break;
//							}
//						}
//					}					
//					
//					if (epoch == 1) {
//			//			Console.WriteLine("#Training: {0}, Err1: {1}, Product: {2}", n+1, err1, xui);
//					}
//					
//					errPerEpoch += err1 * err1;
					
					bprOpt += Math.Log( sigmoid(xuij) ) -
								(K * calNorm(userFeature, userIndx, numFeatures)) -
								(K * calNorm(itemFeature, itemIIndex, numFeatures)) -
								(K * calNorm(itemFeature, itemJIndex, numFeatures));				
				}
							
			//	Console.Write("{0}, {1} ", numTrainingExamples, errPerEpoch);
					errPerEpoch = (errPerEpoch / numTrainingExamples);
				Console.WriteLine("Epoch: {0}, Err: {1}, Bpr-Opt: {2}", epoch, errPerEpoch, bprOpt);	
				
//				int N = 8;
//					Dictionary<int, double> recallData = new Dictionary<int, double>();
//				readDataAndRecall("test.txt", 				                  
//						N, 			
//				    	epoch,
//					    globalAverage,
//					    numFeatures,
//					    userFeature, 
//					    itemFeature, 
//				    	userIdHash,
//				    	itemIdHash, 
//				    	ref recallData);				
					//Console.WriteLine( "Epoch: {0}, u: {1}, i: {2}, j: {3}", epoch, randUser, randRatedItem, randNonRatedItem);
			}				
		}			
		
		public static void Main (string[] args)
		{		
			int itr;
		//	int N = 8;
			int minN = 8;
			int maxN = 50;
			int numLines = (maxN - minN) + 1;
			int numUsers;
			int numItems;											
			int bprEpochs = 10;
			int numTrainingExamples;
			int numFeatures = 10;
			double K = 0.001;
			double lrate = 0.3;
			
			List<string> userIdList = new List<string>();
			List<string> itemIdList = new List<string>();
			List<int> ratingList = new List<int>();
			List<string> uniqueItemList = new List<string>();			
			
			Dictionary<string, List<int>> userIdMapping = new Dictionary<string, List<int>>();
			Dictionary<string, List<int>> itemIdMapping = new Dictionary<string, List<int>>();	
			Dictionary<string, List<string>> ratedItemsPerUser = new Dictionary<string, List<string>>();
			Dictionary<string, int> userIdHash = new Dictionary<string, int>();
			Dictionary<string, int> itemIdHash = new Dictionary<string, int>();			
			Dictionary<int, double> recallData = new Dictionary<int, double>();			
			
			readTrainSet("train.txt", 
			             ref userIdMapping, 
			             ref itemIdMapping, 
			             ref ratedItemsPerUser,
			             ref userIdList, 
			             ref itemIdList, 
			             ref ratingList,
			             ref uniqueItemList
			             );			
			
			numUsers = userIdMapping.Count;
			numItems = itemIdMapping.Count;
			numTrainingExamples = ratingList.Count;
			
			double globalAverage = calcGlobalAverage(ratingList);	
			double[,] userFeature = new double[numFeatures,numUsers];
			double[,] itemFeature = new double[numFeatures,numItems];								
				
			initFeatures(userIdMapping.Count, ref userFeature, 
			             itemIdMapping.Count, ref itemFeature,
			             numFeatures);												
								
			learnHashID(ref userIdHash,
		                ref itemIdHash,
		                userIdMapping,
		                itemIdMapping
		                );				
			
		//	Console.WriteLine("numUsers: {0}, usersList: {1}, numItems: {2}, itemsList: {3}", numUsers, userIdHash.Count, numItems, itemIdHash.Count);
			
			learnBPR(
			         globalAverage,
				     bprEpochs,
			         numFeatures,
				     numTrainingExamples,
					 lrate,
				     K,
		             ref userFeature,
		             ref itemFeature,
			         userIdHash,
		             itemIdHash,
		             ratedItemsPerUser, 
		             uniqueItemList,
				     ratingList,
				     userIdMapping,
				     itemIdMapping
		             );							
			
//			for (int N = minN; N <= maxN; N++) {
//				readDataAndRecall("test.txt", 				                  
//						N, 			
//				    	30,
//					    globalAverage,
//					    numFeatures,
//					    userFeature, 
//					    itemFeature, 
//				    	userIdHash,
//				    	itemIdHash, 
//				    	ref recallData);
//			}
			
			itr = 0;
			string[] line = new string[50];
			
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