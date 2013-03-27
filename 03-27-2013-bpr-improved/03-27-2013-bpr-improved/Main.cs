using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace bprimproved
{
	class MainClass
	{
		static void readTrainSet(String fileName, 
		                         ref Dictionary<string, List<int>> userIdMapping, 
		                         ref Dictionary<string, List<int>> itemIdMapping,
		                         ref Dictionary<string, List<string>> ratedItemsPerUser,
		                         ref List<string> trainUserIdList,
		                         ref List<string> trainItemIdList,		         
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
					rowIndexCounter++;
				}
			}							
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
		
		static double dotProduct(double[,] userFeature, 
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
			return Math.Sqrt(norm);
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
			
		static void recallAndPrecision(
			                          String fileName, 		                              
			                          int N,		   
		                              int epoch,		                              
			                          int numFeatures,
			                          double[,] userFeature,
			                          double[,] itemFeature,
			                          Dictionary<string, int> userIdHash,
			                          Dictionary<string, int> itemIdHash,
			                          ref Dictionary<int, double> recallData,
		                              ref Dictionary<int, double> precisionData)
			
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
						testUserItemProduct = dotProduct(userFeature, itemFeature, numFeatures, userIdHash[user], itemIdHash[s]);
						testPredictRating = testUserItemProduct;
						if (!itemRatingMapping.ContainsKey(s)) {
							itemRatingMapping.Add(s, testPredictRating);																												
						} 
					}
					rowIndexCounter++;
				}					
				hits += calcItemHitInSortedList(N, rankedItem, itemRatingMapping);							
			}	
							
			Console.WriteLine("\n#Test: {0}",T);
			Console.WriteLine("#Hits: {0}", hits);
			recall = (double)hits / (double)T;
			precision = (double)recall / (double)N;
			
			Console.WriteLine("Recall: {0}, Precision: {1}\n", recall, precision);	
			recallData.Add(N, recall);
			precisionData.Add(N, precision);
		}
			
		static void learnBPR(		               
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
			string randUser;
			string randRatedItem;
			string randNonRatedItem;
			
			Random r1 = new Random();
			Random r2 = new Random();
			Random r3 = new Random();											
			
			for (epoch = 1; epoch <= bprEpochs; epoch++) {
				bprOpt = 0.0;				
				for (int n = 0; n < numTrainingExamples; n++) {	
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
									
					xui = dotProduct(userFeature, itemFeature, numFeatures, userIndx, itemIIndex);
					xuj = dotProduct(userFeature, itemFeature, numFeatures, userIndx, itemJIndex);
					xuij = xui - xuj;
									
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
					
					bprOpt += Math.Log( sigmoid(xuij) );
				}
										
				Console.WriteLine("Epoch: {0}, Bpr-Opt: {1}", epoch, bprOpt);					
			}				
		}			
		
		static void writeToFile(int numLines,
		                        Dictionary<int, double> recallData,
		                        Dictionary<int, double> precisionData 
		                        )
		{
			int itr = 0;
			string[] recallPerN = new string[numLines];		
			string[] precisionPerN = new string[numLines];	
			string[] precisionPerRecall = new string[numLines];	
			
			foreach (KeyValuePair<int, double> recall in recallData)
            {
				string tmp = recall.Key + " " + recall.Value;
				recallPerN[itr] = tmp;
				tmp = recall.Key + " " + precisionData.Values.ElementAt(itr);
				precisionPerN[itr] = tmp;
				tmp = precisionData.Values.ElementAt(itr) + " " + recall.Value;
				precisionPerRecall[itr] = tmp;
				itr++;                
            }			
			
			System.IO.File.WriteAllLines("recall_per_n.txt", recallPerN);	
			System.IO.File.WriteAllLines("precision_per_n.txt", precisionPerN);
			System.IO.File.WriteAllLines("precision_per_recall.txt", precisionPerRecall);
			
		}
		
		public static void Main (string[] args)
		{							
			int minN = 8;
			int maxN = 35;
			int numLines = (maxN - minN) + 1;
			int numUsers;
			int numItems;											
			int bprEpochs = 25;
			int numTrainingExamples;
			int numFeatures = 50;
			double K = 0.01;
			double lrate = 0.01;
			
			List<string> userIdList = new List<string>();
			List<string> itemIdList = new List<string>();			
			List<string> uniqueItemList = new List<string>();			
			
			Dictionary<string, List<int>> userIdMapping = new Dictionary<string, List<int>>();
			Dictionary<string, List<int>> itemIdMapping = new Dictionary<string, List<int>>();	
			Dictionary<string, List<string>> ratedItemsPerUser = new Dictionary<string, List<string>>();
			Dictionary<string, int> userIdHash = new Dictionary<string, int>();
			Dictionary<string, int> itemIdHash = new Dictionary<string, int>();			
			Dictionary<int, double> recallData = new Dictionary<int, double>();	
			Dictionary<int, double> precisionData = new Dictionary<int, double>();
			
			readTrainSet("train.txt", 
			             ref userIdMapping, 
			             ref itemIdMapping, 
			             ref ratedItemsPerUser,
			             ref userIdList, 
			             ref itemIdList, 			            
			             ref uniqueItemList
			             );			
			
			numUsers = userIdMapping.Count;
			numItems = itemIdMapping.Count;
			numTrainingExamples = userIdList.Count;
			
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
			
			learnBPR(			         
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
				     userIdMapping,
				     itemIdMapping
		             );							
			
			for (int N = minN; N <= maxN; N++) {
				recallAndPrecision("test.txt", 		                  
						N, 			
				    	bprEpochs,					  
					    numFeatures,
					    userFeature, 
					    itemFeature, 
				    	userIdHash,
				    	itemIdHash, 
				    	ref recallData,
				        ref precisionData);
			}							
			
			writeToFile(numLines,
		                recallData,
		                precisionData 
		                );
		}		
	}
}