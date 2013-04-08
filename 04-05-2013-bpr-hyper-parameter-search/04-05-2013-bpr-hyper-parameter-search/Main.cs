using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace bprhyperparametersearch
{
	class MainClass
	{
		public int minN {get; private set;}
		public int maxN {get; private set;}
		public int numLines {get; private set;}
		public int numUsers {get; private set;}
		public int numItems {get; private set;}	
		public int bprMinEpochs {get; private set;}
		public int bprMaxEpochs {get; private set;}
		public int bprEpochs {get; private set;}
		public int numTrainingExamples {get; private set;}
		public int numFeatures {get; private set;}
		public double K {get; private set;}
		public double lrate {get; private set;}
		
		public MainClass(int _numUsers,
		                 int _numItems,		         
		                 int _numTrainingExamples)
		{	
			minN = 8;
			maxN = 8;
			numLines = (maxN - minN) + 1;
			numUsers = _numUsers;
			numItems = _numItems;
			bprMinEpochs = 5;	
			bprMaxEpochs = 25;
			numTrainingExamples = _numTrainingExamples;
			numFeatures = 40;			
			K = 0.0025;
			lrate = 0.1;
		}
		
		/*
		 * Read lines from a file
		 */
		static string[] readAllLines(String fileName)
		{
			return System.IO.File.ReadAllLines(fileName);
		}
		          		
		/*
		 * Read the training dataset
		 */
		static int readTrainSet(String fileName, 
		                         ref Dictionary<string, List<int>> userIdIndxInfo, 
		                         ref Dictionary<string, List<int>> itemIdIndxInfo,
		                         ref Dictionary<string, List<string>> ratedItemsPerUser,		                                
		                         ref List<string> uniqueItemList
		                         )
		{			
			int userIdIndex;
			int itemIdIndex;
			int rowIndexCounter;	
			
			string[] lines = readAllLines(fileName);
			
			userIdIndex = itemIdIndex = 0;
			
			foreach (string line in lines)
	        {	
				string[] stringSeparator = new string[] { " " };
				string user = null;
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{					
					if (rowIndexCounter == 0) {					
						if (userIdIndxInfo.ContainsKey(s))
						{
							userIdIndxInfo[s].Add(userIdIndex);				
							userIdIndex++;
						} else {
							List<int> tmpIndexList = new List<int>();
							tmpIndexList.Add(userIdIndex);
							userIdIndxInfo.Add(s,tmpIndexList);
							userIdIndex++;
						}
						user = s;
					} 
					
					if (rowIndexCounter == 1) {
						if (itemIdIndxInfo.ContainsKey(s))
						{
							itemIdIndxInfo[s].Add(itemIdIndex);				
							itemIdIndex++;
						} else {
							uniqueItemList.Add(s);
							List<int> tmpIndexList = new List<int>();
							tmpIndexList.Add(itemIdIndex);
							itemIdIndxInfo.Add(s,tmpIndexList);
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
			return lines.Length;
		}
		
		/*
		 * Initialise the user and item feature vectors
		 */
		public void initFeatures(ref double[,] userFeature, 
			             		 ref double[,] itemFeature)
		{
			int i;
			int j;
			int numUsers = this.numUsers;
			int numItems = this.numItems;		
			
			for (i = 0; i < this.numFeatures; i++) {
				for (j = 0; j < numUsers; j++) {
					userFeature[i,j] = 0.1;
				}
			}
			
			for (i = 0; i < this.numFeatures; i++) {
				for (j = 0; j < numItems; j++) {
					itemFeature[i,j] = 0.1;
				}
			}
		}
		
		/* 
		 * Calculates the dot product of user and item feature vectors
		 */
		public double dotProduct(double[,] userFeature, 
		                         double[,] itemFeature, 		                         
		                         int userId,
		                         int itemId)
		{
			double dotProduct = 0.0;
			
			for (int i = 0; i < this.numFeatures; ++i) {
				dotProduct += userFeature[i,userId] * itemFeature[i,itemId];		
			}
			
			return dotProduct;
		}			
		
		/* 
		 * Produces the userid and itemid (string->indx) hash
		 */
		public void learnHashID(ref Dictionary<string, int> userIdToIndx,
		                        ref Dictionary<string, int> itemIdToIndx,
		                        Dictionary<string, List<int>> userIdIndxInfo,
		                        Dictionary<string, List<int>> itemIdIndxInfo)
		{
			int userIdCounter;
			int itemIdCounter;	
			
			userIdCounter = itemIdCounter = 0;
			
			foreach (KeyValuePair<string, List<int>> userId in userIdIndxInfo)
            {
                userIdToIndx.Add(userId.Key, userIdCounter);				
				userIdCounter++;
            }
			
			foreach (KeyValuePair<string, List<int>> itemId in itemIdIndxInfo)
            {
                itemIdToIndx.Add(itemId.Key, itemIdCounter);
				itemIdCounter++;
            }			
		}
		
		/*
		 * Calculates ths sigmoid 
		 */
		public double sigmoid(double x)
		{
			return ( 1.0 / ( 1.0 + Math.Exp(-x) ) );
		}
		
		/*
		 * Calculates the norm of a vector
		 */
		public double calNorm(double[,] features, int indx) 
		{
			double norm = 0.0;
			
			for (int i = 0; i < this.numFeatures; i++) {
				norm += features[i,indx]*features[i, indx];
			}
			return Math.Sqrt(norm);
		}		
		
		/*
		 * Calculates the number of hits
		 */
		public int calcItemHitInSortedList(int N, string rankedItem, Dictionary<string, double> itemRatingMapping)
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
			
		/*
		 * Train bpr on matrix factorization model
		 */
		public void learnBPR(ref double[,] userFeature,
		                     ref double[,] itemFeature,
		                     Dictionary<string, int> userIdToIndx,
		                     Dictionary<string, int> itemIdToIndx,
		                     Dictionary<string, List<string>> ratedItemsPerUser, 
		                     List<string> uniqueItemList,		           
		                     Dictionary<string, List<int>> userIdIndxInfo,
		                     Dictionary<string, List<int>> itemIdIndxInfo) 
		{	
			Console.Write("Epoch: {0}, K: {1}, ", this.bprEpochs, this.K);
			string tmp = "Epoch: " + this.bprEpochs + ", K: " + this.K + ",";
			System.IO.File.AppendAllText("log.txt", tmp);
			int loopCount;
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
			
			if (this.bprEpochs > this.bprMinEpochs) {
				loopCount = 1;
			} else {
				loopCount = this.bprEpochs;
			}
			
			for (epoch = 1; epoch <= loopCount; epoch++) {
				bprOpt = 0.0;								

				for (int n = 0; n < this.numTrainingExamples; n++) {	
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
				
					userIndx = userIdToIndx[randUser];
					itemIIndex = itemIdToIndx[randRatedItem];
					itemJIndex = itemIdToIndx[randNonRatedItem];
									
					xui = dotProduct(userFeature, itemFeature, userIndx, itemIIndex);
					xuj = dotProduct(userFeature, itemFeature, userIndx, itemJIndex);									
					xuij = xui - xuj;					
									
					for(int f = 0; f < this.numFeatures; f++) {	
						uv = userFeature[f,userIndx];
						dervXuij = itemFeature[f,itemIIndex] - itemFeature[f,itemJIndex];						
						userFeature[f,userIndx] += this.lrate * 
												( ( ( (double)(Math.Exp(-xuij)) / (double)(1 + Math.Exp(-xuij)) ) * dervXuij ) - (this.K * uv));
																		
						dervXuij = uv;
						itemFeature[f, itemIIndex] += this.lrate * 
												( ( ( (double)(Math.Exp(-xuij)) / (double)(1.0 + Math.Exp(-xuij)) ) * dervXuij ) - (this.K * itemFeature[f, itemIIndex]) );
					
						dervXuij = -uv;

						itemFeature[f, itemJIndex] += this.lrate * 
												( ( ( (Math.Exp(-xuij)) / (1 + Math.Exp(-xuij)) ) * dervXuij ) - (this.K * itemFeature[f, itemJIndex]));
					}
					
					bprOpt += Math.Log( sigmoid(xuij) );
				}
										
//				if (loopCount == 1) {
//					Console.WriteLine("Epoch: {0}, Bpr-Opt: {1}", this.bprEpochs, bprOpt);
//				} else {
//					Console.WriteLine("Epoch: {0}, Bpr-Opt: {1}", epoch, bprOpt);					
//				}
//				Console.WriteLine("[{0},{1}] = {2}, ReglUV: {3}, ReglIV: {4}, ItemType: {5}", maxUser, maxItem, maxRating, maxUserReg, maxItemReg, itemTypeForMaxRating);				
			}								
		}					
		
		/*
		 * Calculates recall and precision
		 */
		public void recallAndPrecision(string[] lines,
		                              int N,
			                          double[,] userFeature,
			                          double[,] itemFeature,
			                          Dictionary<string, int> userIdToIndx,
			                          Dictionary<string, int> itemIdToIndx
			                         )			
		{		
			int T = 0; 
			int hits = 0;			
			int rowIndexCounter;				
			double recall;
			double precision;		
			double testUserItemProduct;
			double testPredictRating;							
			string[] stringSeparator = new string[] { " " };
			
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
						testUserItemProduct = this.dotProduct(userFeature, itemFeature, userIdToIndx[user], itemIdToIndx[s]);
						testPredictRating = testUserItemProduct;
						if (!itemRatingMapping.ContainsKey(s)) {
							itemRatingMapping.Add(s, testPredictRating);																												
						} 
					}
					rowIndexCounter++;
				}					
				hits += calcItemHitInSortedList(N, rankedItem, itemRatingMapping);							
			}	
				
			//Console.WriteLine("\n#N: {0}",N);			
			Console.Write("#Test: {0}, #Hits: {1}, ",T, hits);		
			string tmp = "#Test: " + T + ", #Hits: " + hits + ",";
			System.IO.File.AppendAllText("log.txt", tmp);
			recall = (double)hits / (double)T;
			precision = (double)recall / (double)N;
			
			Console.WriteLine("Rl: {0}, Pr: {1}", recall, precision);	
			tmp = "Rl: " + recall + ", Pr: " + precision + "\n";
			System.IO.File.AppendAllText("log.txt", tmp);
			tmp = this.bprEpochs + " " + this.K + " " + recall + "\n";
			System.IO.File.AppendAllText("recall.txt", tmp);
			tmp = this.bprEpochs + " " + this.K + " " + precision + "\n";
			System.IO.File.AppendAllText("precision.txt", tmp);						
		}
			
		public static void Main (string[] args)
		{			
			int numTrainingExamples;
			string[] testData = readAllLines("ml-100k.test.ht");
			List<string> uniqueItemList = new List<string>();			
			
			Dictionary<string, List<int>> userIdIndxInfo = new Dictionary<string, List<int>>();
			Dictionary<string, List<int>> itemIdIndxInfo = new Dictionary<string, List<int>>();	
			Dictionary<string, List<string>> ratedItemsPerUser = new Dictionary<string, List<string>>();
			Dictionary<string, int> userIdToIndx = new Dictionary<string, int>();
			Dictionary<string, int> itemIdToIndx = new Dictionary<string, int>();			
			
			numTrainingExamples = readTrainSet("ml-100k.train.hcr", 
			             						ref userIdIndxInfo, 
			             						ref itemIdIndxInfo,
			             						ref ratedItemsPerUser,			            			             		           
			             						ref uniqueItemList);			
			/*
			 * minN 
			 * maxN 
			 * numLines 
			 * numUsers 
			 * numItems 						
			 * bprEpochs 
			 * numTrainingExamples 
			 * numFeatures
			 * K 
			 * lrate
			 */
						
			MainClass mainclass = new MainClass(userIdIndxInfo.Count,
			          							itemIdIndxInfo.Count,			          							
			          							numTrainingExamples);					
						
			mainclass.learnHashID(ref userIdToIndx,
			                      ref itemIdToIndx,
			                      userIdIndxInfo,
			                      itemIdIndxInfo);				
			
			for (double k = 0.0025; k <= 0.25; k=k+0.004) {
				mainclass.K = k;
				double[,] userFeature = new double[mainclass.numFeatures,mainclass.numUsers];
				double[,] itemFeature = new double[mainclass.numFeatures,mainclass.numItems];	
				
				mainclass.initFeatures(ref userFeature, 
			    	         		   ref itemFeature);												
								
				for (int epoch = mainclass.bprMinEpochs; epoch <= mainclass.bprMaxEpochs; epoch++) {
					mainclass.bprEpochs = epoch;					
					mainclass.learnBPR(ref userFeature,
				    	               ref itemFeature,
				        	           userIdToIndx,
				            	       itemIdToIndx,
				                	   ratedItemsPerUser,
					                   uniqueItemList,		
					                   userIdIndxInfo,
					                   itemIdIndxInfo);										
				
					for (int N = mainclass.minN; N <= mainclass.maxN; N++) {					
						mainclass.recallAndPrecision(testData,
						                             N,						                            						                         
						                             userFeature,
						                             itemFeature,
						                             userIdToIndx,
						                             itemIdToIndx
						                            );
					}	
				}
			}						
		}		
	}
}