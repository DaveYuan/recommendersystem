using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;
//using MathNet.Numerics.Distributions;

namespace bprbininput
{
	class MainClass
	{
		private double regUser;
		private double regPostv;
		private double regNegtv;
		private double lrate;
//		private double minRegPostv;
//		private double maxRegPostv;
//		private double initMean;
//		private double initStDev;
		private int numEpochs;
		private int numUsers;
		private int numItems;
		private int numEntries;
		private int numFeatures;
		private int[] trainUsersArray;
  		private int[] trainItemsArray;
		private int[] uniqueItemsArray;
		private double[,] userFeature;
		private double[,] itemFeature;
		private string[] testUserItems;
		private Dictionary<int, int[]> ratedItemsPerUser;						
		
		public static void writeToConsole(string mssg) 
		{
			Console.WriteLine("\t- " + mssg);
		}
		
		public MainClass(Train trainObj, Test testObj)
		{				
			lrate = 0.05;
			regUser = 0.0025;
			regPostv = 0.0025;
			regNegtv = 0.00025;
//			initMean = 0;
//			initStDev = 0.1;
			numEpochs = 30;								
			numFeatures = 10;
			
			trainUsersArray = trainObj.usersList.ToArray();
			trainItemsArray = trainObj.itemsList.ToArray();
			testUserItems = testObj.testUserItem.ToArray();
		}		
						
		public void initFeatures()
		{			
			userFeature = new double[numFeatures,numUsers];
			itemFeature = new double[numFeatures,numItems];																
			
//			var nd = new Normal(initMean, initStDev);
  //          nd.RandomSource = new Random();
            Random rd = new Random();

            for (int i = 0; i < numFeatures; i++) {
				for (int j = 0; j < numFeatures; j++) {
//					userFeature[i,j] = (float) nd.Sample();
					userFeature[i,j] = (float) rd.NextDouble();
				}
			}
			
//			nd.RandomSource = new Random();
            rd = new Random();
			for (int i = 0; i < numFeatures; i++) {
				for (int j = 0; j < numItems; j++) {
	//				itemFeature[i,j] = (float) nd.Sample();
					itemFeature[i,j] = (float) rd.NextDouble();
				}
			}
		}
		
		public double dotProduct(int userId,
		                         int itemId)
		{
			double dotProduct = 0.0;			
			for (int i = 0; i < numFeatures; ++i) {
				dotProduct += userFeature[i,userId] * itemFeature[i,itemId];		
			}			
			return dotProduct;
		}
		
		public void calRatedItemsPerUser()
		{
			int user;
			int lenDataset = trainItemsArray.Length;	
			double avgItemsRatedPerUser = 0.0;
			ratedItemsPerUser = new Dictionary<int, int[]>();			
			Dictionary<int, List<int>> ratedItemsPerUserTmp = new Dictionary<int, List<int>>();
			
			for (int i = 0; i < lenDataset; i++) {
				user = trainUsersArray[i];
				if (ratedItemsPerUserTmp.ContainsKey(user)) {
					ratedItemsPerUserTmp[user].Add(trainItemsArray[i]);
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(trainItemsArray[i]);
					ratedItemsPerUserTmp.Add(user, tmp);
				}		
			}			
			
			// Convert Dictionary<int, List<int>> ratedItemsPerUserTmp => Dictionary<int, int[]> ratedItemsPerUser					
			foreach (KeyValuePair<int, List<int>> ratedItem in ratedItemsPerUserTmp) {
				ratedItemsPerUser.Add(ratedItem.Key, ratedItem.Value.ToArray());
				avgItemsRatedPerUser += ratedItem.Value.Count;
				
			}
			
			avgItemsRatedPerUser = avgItemsRatedPerUser / ratedItemsPerUser.Count;
			Console.WriteLine("\t\t- Avg(itemRatedPerUser): {0}", avgItemsRatedPerUser);				
			GC.Collect();
		}
		
		public void calUniqueUsersnItems() 
		{
			// Create a HashSet of uniqueItems and uniqueUsers
			var hashItems = new HashSet<int>(trainItemsArray);
			uniqueItemsArray = hashItems.ToArray();
			
			var hashUsers = new HashSet<int>(trainUsersArray);
			
			numUsers = hashUsers.Count;
			numItems = uniqueItemsArray.Length;			
			
			Console.WriteLine("\t\t- Uniq(#Users): {0}, Uniq(#Items): {1}", numUsers, numItems);			
		}
		
		public double sigmoid(double x)
		{
			return (1.0 / ( 1.0 + Math.Exp(-x)));
		}
		
		public double oneByOnePlusExpX(double x) 
		{
			return (1.0 / (1.0 + Math.Exp(x)));
		}
		
		public double bpr()
		{
			int numRelation;
			int numUserTargetRel;
			int randUser;
			int randPostvRel;			
			int randNegtvRel;					
			double bprOptItem = 0.0;
			double userValue;
			double xuPostv;
			double xuNegtv;
			double xuPostvNegtv;			
			double dervxuPostvNegtv;
			
			// temp var
			double maxUserFeature;
			double maxItemFeature;
			string userRelType = null;
			string itemRelType = null;
			
			numEntries = trainItemsArray.Length;
			Console.WriteLine("\t\t- #Entries: {0}", numEntries);
			Random r1 = new Random();
			Random r2 = new Random();
			Random r3 = new Random();
			
			for (int epoch = 1; epoch <= numEpochs; epoch++) {
				bprOptItem = 0.0;
				maxUserFeature = Double.MinValue;
				maxItemFeature = Double.MinValue;
				
				// User-features
				for (int n = 0; n < numEntries; n++) {										
					numUserTargetRel = ratedItemsPerUser.Keys.Count;
					randUser = ratedItemsPerUser.Keys.ElementAt(r1.Next(0, numUserTargetRel));
					numRelation = ratedItemsPerUser[randUser].Length;
					randPostvRel = ratedItemsPerUser[randUser][r2.Next(0, numRelation)];
					randNegtvRel = uniqueItemsArray[r3.Next(0, numItems)];					
					
					while (ratedItemsPerUser[randUser].Contains(randNegtvRel)) {
						randNegtvRel = uniqueItemsArray[r3.Next(0, numItems)];
					}
					
					xuPostv = dotProduct(randUser, randPostvRel);
					xuNegtv = dotProduct(randUser, randNegtvRel);
					xuPostvNegtv = xuPostv - xuNegtv;		
					
					for (int f = 0; f < numFeatures; f++) {											
						userValue = userFeature[f, randUser];					
						dervxuPostvNegtv = itemFeature[f, randPostvRel] - itemFeature[f, randNegtvRel];
						userFeature[f, randUser] += lrate * (oneByOnePlusExpX(xuPostvNegtv) * dervxuPostvNegtv -
						                                     regUser * userValue);
						
						if (userFeature[f, randUser] > maxUserFeature) {
							maxUserFeature = userFeature[f, randUser];
							userRelType = "rndUsr";
						}
						
						dervxuPostvNegtv = userValue;
						itemFeature[f, randPostvRel] += lrate * (oneByOnePlusExpX(xuPostvNegtv) * dervxuPostvNegtv -
						                                              regPostv * itemFeature[f, randPostvRel]);
						if (itemFeature[f, randPostvRel] > maxItemFeature) {
							maxItemFeature = itemFeature[f, randPostvRel];
							itemRelType = "+Rel";
						}
						
						dervxuPostvNegtv = -userValue;
						itemFeature[f, randNegtvRel] += lrate * (oneByOnePlusExpX(xuPostvNegtv) * dervxuPostvNegtv -
						                                              regNegtv * itemFeature[f, randNegtvRel]);						
						if (itemFeature[f, randNegtvRel] > maxItemFeature) {
							maxItemFeature = itemFeature[f, randNegtvRel];
							itemRelType = "-Rel";
						}
					}	
					bprOptItem += Math.Log(sigmoid(xuPostvNegtv));
				}										
				Console.WriteLine("\t\t- {0}: Opt(Usr+Itm): {1}, MaxUF({2}): {3}, MaxIF({4}): {5}", 
				                  epoch, bprOptItem, userRelType, maxUserFeature, itemRelType, maxItemFeature);
			}	
			return bprOptItem;
		}
		
		public int calcItemHitInSortedList(int N, int rankedItem, Dictionary<int, double> itemRatingMapping)
		{
			int count = 0;
			
			/*
			 * Sort Avg Rating List
			 */														
			var sortedItemRatingMapping = from pair in itemRatingMapping
								orderby pair.Value descending
								select pair;						
			
			foreach (KeyValuePair<int, double> pair in sortedItemRatingMapping)
			{
				count++;						
				if (count == N+1) break;
				if (pair.Key == rankedItem) {
					return 1;
				}				
			}	
			
			return 0;
		}	
		
		public void recallPrecision(int N,
		                            ref Dictionary<int, double> recallData,
		                            ref Dictionary<int, double> precisionData)
		{
			int T = 0; 
			int hits = 0;
			int user = -1;			
			int intVal;
			int rankedItem = -1;			
			int rowIndexCounter;		
			double recall;
			double precision;
			double testPredictRating;							
			
			string[] stringSeparator = new string[] { "\t" };
			
			foreach (string line in testUserItems) {
				T++;
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				Dictionary<int, double> itemRatingMapping = new Dictionary<int, double>();						
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{	
					intVal = Convert.ToInt32(s);
					if (rowIndexCounter == 0) {
						user = intVal;
					}					
					if (rowIndexCounter == 1) {
						rankedItem = intVal;
						
					}
						
					if (rowIndexCounter >= 1) {
						testPredictRating = dotProduct(user, intVal);												
						if (!itemRatingMapping.ContainsKey(intVal)) {
							itemRatingMapping.Add(intVal, testPredictRating);																												
						} 
					}
					rowIndexCounter++;
				}					
				hits += calcItemHitInSortedList(N, rankedItem, itemRatingMapping);											
			}
			
			
			Console.WriteLine("\n\t\t\t- N: {0}", N);
			Console.WriteLine("\t\t\t- #Hits/#Tests: {0}/{1}", hits, T);
			recall = (double)hits / (double)T;
			precision = (double)recall / (double)N;
			
			Console.WriteLine("\t\t\t- Recall: {0}, Precision: {1}\n", recall, precision);	
			recallData.Add(N, recall);
			precisionData.Add(N, precision);
		}
		
		public static void Main (string[] args)
		{			
			Train trainObj;
			Test testObj;			
//			double bprOpt;
			
			writeToConsole("Loading u1.base.bin");
			using (FileStream file = File.OpenRead("u1.base.bin"))
			{
				trainObj = Serializer.Deserialize<Train>(file);
			}
			
			writeToConsole("Loading u1.test.bin");
			using (FileStream file = File.OpenRead("u1.test.bin"))
			{
				testObj = Serializer.Deserialize<Test>(file);
			}
			
			writeToConsole("Constructor call");
			MainClass mainclass = new MainClass(trainObj, testObj);
			
			writeToConsole("Find: RatedItemsPerUser");
			mainclass.calRatedItemsPerUser();
			
			writeToConsole("Find: uniqueUsers and uniqueItems");
			mainclass.calUniqueUsersnItems();
			
			writeToConsole("Initialize features");
			mainclass.initFeatures();
			
			writeToConsole("Bpr for social network data");
			mainclass.bpr();
			
			Dictionary<int, double> recallData = new Dictionary<int, double>();	
			Dictionary<int, double> precisionData = new Dictionary<int, double>();
			for (int N = 5; N <= 15; N=N+5) {
				mainclass.recallPrecision(N,
				                          ref recallData,
				                          ref precisionData);
			}
			writeToConsole("Done!");
		}		
	}
}
