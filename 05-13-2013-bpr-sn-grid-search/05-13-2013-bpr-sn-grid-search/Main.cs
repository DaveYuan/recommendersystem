using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace bprsngridsearch
{
	class MainClass
	{
		private double lrate;
		private double lambdaPostv;
		private double lambdaNegtv;
		private double alphaTarget;
		private double alphaAuxillary;
		private double minLambdaPostv;
		private double maxLambdaPostv;
		private int numEpochs;
		private int numUsers;
		private int numItems;
		private int numFeatures;
		private int[] trainUsersArray;
        private int[] trainItemsArray;
		private int[] uniqueUsersArray;
		private int[] uniqueItemsArray;
		private double[,] userFeature;
		private double[,] itemFeature;
		private string[] testUserItems;
		private string csvFileName;
		private string[] csvHeadLine;
		private Dictionary<int, int[]> frndsPerUser;
		private Dictionary<int, int[]> ratedItemsPerUser;						
		
		public static void writeToConsole(string mssg) 
		{
			Console.WriteLine("\t- " + mssg);
		}
							
		/// <summary>
		/// Constructor which initializes:
		///  	-trainUsersArray
		/// 	-trainItemsArray
		///  	-frndsPerUser : User1-> trustUser1, trustUser2, trustUser3 ....
		///			 		    User2-> trustUser1, trustUser2, trustUser3 ....		 
		/// </summary>
		/// <param name="trustObj">
		/// A <see cref="Trust"/>
		/// </param>
		/// <param name="trainObj">
		/// A <see cref="Train"/>
		/// </param>
		/// <param name="testObj">
		/// A <see cref="Test"/>
		/// </param>
		public MainClass(Trust trustObj, Train trainObj, Test testObj)
		{
			int indx = 0;		
			lrate = 0.09;			
			lambdaPostv = 0.0025;
			lambdaNegtv = -0.00025;
			alphaTarget = 1;
			alphaAuxillary = 1;
			minLambdaPostv = 0.0025;
			maxLambdaPostv = 0.02;
			numEpochs = 2;
			numFeatures = 2;
			csvFileName = "05-13-2013-BPRMF-SN.csv";
			csvHeadLine = new string[]{"#itr", "#feature", "lrate", "λ(+Rel)", "λ(-Rel)", "BPR-OPT", 
												"R(N=10)", "R(N=15)", "R(N=20)", "R(N=20)",
												"P(N=10)", "P(N=15)", "P(N=20)", "P(N=30)"};
			
			trainUsersArray = trainObj.usersList.ToArray();
			trainItemsArray = trainObj.itemsList.ToArray();
			testUserItems = testObj.testUserItem.ToArray();			
		
			double avgNumFrndsPerUser = 0.0;			
			frndsPerUser = new Dictionary<int, int[]>();
			int[] trustUserArray2 = trustObj.trustUserList2.ToArray();
			Dictionary<int, List<int>> frndsPerUserTmp = new Dictionary<int, List<int>>();
		
			foreach (int user1 in trustObj.trustUserList1) {
				if (frndsPerUserTmp.ContainsKey(user1)) {
					frndsPerUserTmp[user1].Add(trustUserArray2[indx]);					
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(trustUserArray2[indx]);
					frndsPerUserTmp.Add(user1, tmp);					
				}
				
				if (frndsPerUserTmp.ContainsKey(trustUserArray2[indx])) {
					frndsPerUserTmp[trustUserArray2[indx]].Add(user1);
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(user1);
					frndsPerUserTmp.Add(trustUserArray2[indx], tmp);
				}
				indx++;
			}
			
			//	Convert Dictionary<int, List<int>> frndsPerUserTmp => Dictionary<int, int[]> frndsPerUser						
			foreach (KeyValuePair<int, List<int>> trustRelation in frndsPerUserTmp) {
				frndsPerUser.Add(trustRelation.Key, trustRelation.Value.ToArray());
				avgNumFrndsPerUser += trustRelation.Value.Count;
			}			
			
			avgNumFrndsPerUser = avgNumFrndsPerUser / frndsPerUser.Count;
			Console.WriteLine("\t\t- Avg(friendsPerUser): {0}", avgNumFrndsPerUser);
							
			GC.Collect();
		}	
		
		/// <summary>
		/// Initialise the user and item feature vectors 
		/// </summary>		
		public void initFeatures()
		{			
			userFeature = new double[numFeatures,numUsers];
			itemFeature = new double[numFeatures,numItems];													
			
			for (int i = 0; i < this.numFeatures; i++) {
				for (int j = 0; j < numUsers; j++) {
					userFeature[i,j] = 0.1;
				}
			}
			
			for (int i = 0; i < this.numFeatures; i++) {
				for (int j = 0; j < numItems; j++) {
					itemFeature[i,j] = 0.1;
				}
			}
		}
		
		/// <summary>
		/// Calculates the dot product of user and item feature vectors 
		/// </summary>
		/// <param name="userId">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="itemId">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Double"/>
		/// </returns>
		public double dotProduct(int userId,
		                         int itemId)
		{
			double dotProduct = 0.0;			
			for (int i = 0; i < this.numFeatures; ++i) {
				dotProduct += userFeature[i,userId] * itemFeature[i,itemId];		
			}			
			return dotProduct;
		}		
		
		/// <summary>
		/// Finds the items being rated by a user
		/// -ratedItemsPerUser : User1-> Item1, Item2, Item3 ....
		///						User2-> Item1, Item2, Item3 .... 
		/// </summary>
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
		
		/// <summary>
		/// -Finds the uniqueUsers and uniqueItems from the dataset
		/// -numUsers and numItems 
		/// </summary>
		public void calUniqueUsersnItems() 
		{
			// Create a HashSet of uniqueItems and uniqueUsers
			var hashItems = new HashSet<int>(trainItemsArray);
			uniqueItemsArray = hashItems.ToArray();
			
			var hashUsers = new HashSet<int>(frndsPerUser.Keys);
			uniqueUsersArray = hashUsers.ToArray();
			
			numUsers = uniqueUsersArray.Length;
			numItems = uniqueItemsArray.Length;			
			
			Console.WriteLine("\t\t- Uniq(#Users): {0}, Uniq(#Items): {1}", numUsers, numItems);			
		}		
				
		/// <summary>
		/// Calculates the dot product of user-user feature vectors
		/// </summary>
		/// <param name="user1">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="user2">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Double"/>
		/// </returns>
		public double dotProductUser(int user1, int user2)
		{
			double dotProduct = 0.0;
			for (int i = 0; i < numFeatures; i++) {
				dotProduct += userFeature[i, user1] * userFeature[i, user2];
			}
			return dotProduct;
		}
			
		/// <summary>
		/// Calculates the sigmoid 
		/// </summary>
		/// <param name="x">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Double"/>
		/// </returns>
		public double sigmoid(double x)
		{
			return (1.0 / ( 1.0 + Math.Exp(-x)));
		}
		
		/// <summary>
		/// Calculates 1/(1+exp(x))
		/// </summary>
		/// <param name="x">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Double"/>
		/// </returns>
		public double oneByOnePlusExpX(double x) 
		{
			return (1.0 / (1.0 + Math.Exp(x)));
		}
		
		/// <summary>
		/// Multirelational Bayesian Personalised Ranking for Social Network Data 
		/// </summary>		
		public double bprSocialNetwork() 
		{
			int numRelation;
			int numUserTargetRel;
			int randUser;
			int randPostvRel;			
			int randNegtvRel;					
			int numEntries = trainItemsArray.Length;
			double bprOptUser = 0.0;
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
			
			Console.WriteLine("\t\t- #Entries: {0}", numEntries);
			Random r1 = new Random();
			Random r2 = new Random();
			Random r3 = new Random();
			
			for (int epoch = 1; epoch <= numEpochs; epoch++) {
				bprOptUser = 0.0;	
				bprOptItem = 0.0;
				maxUserFeature = Double.MinValue;
				maxItemFeature = Double.MinValue;
				
				// User-features
				for (int n = 0; n < numEntries; n++) {					
					randUser = frndsPerUser.Keys.ElementAt(r1.Next(0, numUsers));
					numRelation = frndsPerUser[randUser].Length;
					randPostvRel = frndsPerUser[randUser][r2.Next(0, numRelation)];
					randNegtvRel = uniqueUsersArray[r3.Next(0, numUsers)];	
					
					while (frndsPerUser[randUser].Contains(randNegtvRel)) {
						randNegtvRel = uniqueUsersArray[r3.Next(0, numUsers)];
					}					
					
					xuPostv = dotProductUser(randUser, randPostvRel);
					xuNegtv = dotProductUser(randUser, randNegtvRel);
					xuPostvNegtv = xuPostv - xuNegtv;
									
					for (int f = 0; f < numFeatures; f++) {					
						userValue = userFeature[f, randUser];
						dervxuPostvNegtv = userFeature[f, randPostvRel] - userFeature[f, randNegtvRel];
						userFeature[f, randUser] += lrate * (alphaAuxillary * oneByOnePlusExpX(xuPostvNegtv) * dervxuPostvNegtv -
						                                     lambdaPostv * userValue);
						
						if (userFeature[f, randUser] > maxUserFeature) {
							maxUserFeature = userFeature[f, randUser];
							userRelType = "randUsr";
						}
						
						dervxuPostvNegtv = userValue;
						userFeature[f, randPostvRel] += lrate * (alphaAuxillary * oneByOnePlusExpX(xuPostvNegtv) * dervxuPostvNegtv -
						                                              lambdaPostv * userFeature[f, randPostvRel]);
						
						if (userFeature[f, randPostvRel] > maxUserFeature) {
							maxUserFeature = userFeature[f, randPostvRel];
							userRelType = "+Rel";
						}
						
						dervxuPostvNegtv = -userValue;
						userFeature[f, randNegtvRel] += lrate * (alphaAuxillary * oneByOnePlusExpX(xuPostvNegtv) * dervxuPostvNegtv - 
						                                              lambdaNegtv * userFeature[f, randNegtvRel]);		
						if (userFeature[f, randNegtvRel] > maxUserFeature) {
							maxUserFeature = userFeature[f, randNegtvRel];
							userRelType = "-Rel";
						}
					}
				
					bprOptUser += Math.Log(sigmoid(xuPostvNegtv));
				
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
						// TODO: Check if this is needed or not
//						dervxuPostvNegtv = itemFeature[f, randPostvRel] - itemFeature[f, randNegtvRel];
//						userFeature[f, randUser] += lrate * (alphaTarget * oneByOnePlusExpX(xuPostvNegtv) * dervxuPostvNegtv -
//						                                     lambdaPostv * userValue);
						
						dervxuPostvNegtv = userValue;
						itemFeature[f, randPostvRel] += lrate * (alphaTarget * oneByOnePlusExpX(xuPostvNegtv) * dervxuPostvNegtv -
						                                              lambdaPostv * itemFeature[f, randPostvRel]);
						if (itemFeature[f, randPostvRel] > maxItemFeature) {
							maxItemFeature = itemFeature[f, randPostvRel];
							itemRelType = "+Rel";
						}
						
						dervxuPostvNegtv = -userValue;
						itemFeature[f, randNegtvRel] += lrate * (alphaTarget * (1.0 / (1.0 + Math.Exp(xuPostvNegtv))) * dervxuPostvNegtv -
						                                              lambdaNegtv * itemFeature[f, randNegtvRel]);						
						if (itemFeature[f, randNegtvRel] > maxItemFeature) {
							maxItemFeature = itemFeature[f, randNegtvRel];
							itemRelType = "-Rel";
						}
					}	
					bprOptItem += Math.Log(sigmoid(xuPostvNegtv));
				}										
				Console.WriteLine("\t\t- {0}: Opt(Usr+Itm): {1}, MaxUF({2}): {3}, MaxIF({4}): {5}", 
				                  epoch, bprOptUser+bprOptItem, userRelType, maxUserFeature, itemRelType, maxItemFeature);
			}	
			return bprOptUser+bprOptItem;
		}
		
		/// <summary>
		/// Calculates the number of hits 
		/// </summary>
		/// <param name="N">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="rankedItem">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="itemRatingMapping">
		/// A <see cref="Dictionary<System.Int32, System.Double>"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
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
		
		/// <summary>
		/// Calculate recall and precision from Testdata for N items
		/// </summary>
		/// <param name="N">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="recallData">
		/// A <see cref="Dictionary<System.Int32, System.Double>"/>
		/// </param>
		/// <param name="precisionData">
		/// A <see cref="Dictionary<System.Int32, System.Double>"/>
		/// </param>
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
				   
		public void writeToLog(string[] rowData)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Join(",", rowData));
			File.AppendAllText(csvFileName, builder.ToString());
		}
		
		public static void Main (string[] args)
		{	
			double bprOpt;			
			Trust trustObj;
			Train trainObj;
			Test testObj;					
			
			writeToConsole("Loading trust.bin");
			using (FileStream file = File.OpenRead("trust.bin"))
            {
                trustObj = Serializer.Deserialize<Trust>(file);
            }
				
			writeToConsole("Loading train.bin");
			using (FileStream file = File.OpenRead("train.bin"))
			{
				trainObj = Serializer.Deserialize<Train>(file);
			}
			
			writeToConsole("Loading test.bin");
			using (FileStream file = File.OpenRead("test.bin"))
			{
				testObj = Serializer.Deserialize<Test>(file);
			}
						
			writeToConsole("Constructor call");
			MainClass mainclass = new MainClass(trustObj, trainObj, testObj);
		
			writeToConsole("Create .csv log file");
			File.Open(mainclass.csvFileName, FileMode.Create).Close();
			
			writeToConsole("Find: RatedItemsPerUser");
			mainclass.calRatedItemsPerUser();
			
			writeToConsole("Find: uniqueUsers and uniqueItems");
			mainclass.calUniqueUsersnItems();
			
			writeToConsole("Initialize features");
			mainclass.initFeatures();
			
			writeToConsole("Bpr for social network data");
			mainclass.writeToLog(mainclass.csvHeadLine);
			for (double lambda = mainclass.minLambdaPostv; lambda <= mainclass.maxLambdaPostv; lambda=lambda+0.001) {				
				mainclass.lambdaPostv = lambda;
				mainclass.lambdaNegtv = mainclass.lambdaNegtv + 0.0005;
				Console.WriteLine("\t\t- λ(+Rel): {0}, λ(-Rel): {1}", mainclass.lambdaPostv, mainclass.lambdaNegtv);
				bprOpt = mainclass.bprSocialNetwork();			
				Console.WriteLine("\t\t- Bpr-Opt: {0}", bprOpt);
			
				Dictionary<int, double> recallData = new Dictionary<int, double>();	
				Dictionary<int, double> precisionData = new Dictionary<int, double>();
				for (int N = 10; N <= 25; N=N+5) {
					mainclass.recallPrecision(N,
				                          ref recallData,
				                          ref precisionData);
				}
				string[] rowData = new string[]{mainclass.numEpochs.ToString(), 
												mainclass.numFeatures.ToString(), 
												mainclass.lrate.ToString(),
												mainclass.lambdaPostv.ToString(),
												mainclass.lambdaNegtv.ToString(),
												bprOpt.ToString(),
												recallData[10].ToString(), recallData[15].ToString(), recallData[20].ToString(), recallData[25].ToString(),
												precisionData[10].ToString(), precisionData[15].ToString(), precisionData[20].ToString(), precisionData[25].ToString()};
				mainclass.writeToLog(rowData);											
			}			
			writeToConsole ("Done!");
		}
	}	
}

//			writeToConsole("Writing recall and precision values to file");
//			mainclass.writeRcllPrcsnToFile(recallData,
//			                      precisionData);
			
//	/// <summary>
//		/// Write the recall v/s n, precision v/s N, and precision v/s recall values to the file 
//		/// </summary>
//		/// <param name="recallData">
//		/// A <see cref="Dictionary<System.Int32, System.Double>"/>
//		/// </param>
//		/// <param name="precisionData">
//		/// A <see cref="Dictionary<System.Int32, System.Double>"/>
//		/// </param>
//		public void writeRcllPrcsnToFile(Dictionary<int, double> recallData,
//		                                 Dictionary<int, double> precisionData)
//		{
//			int itr = 0;
//			int numLines = maxNRcllPrcsn - minNRcllPrcsn + 1;
//			string[] recallPerN = new string[numLines];		
//			string[] precisionPerN = new string[numLines];	
//			string[] precisionPerRecall = new string[numLines];	
//			
//			foreach (KeyValuePair<int, double> recall in recallData)
//            {
//				string tmp = recall.Key + " " + recall.Value;
//				recallPerN[itr] = tmp;
//				tmp = recall.Key + " " + precisionData.Values.ElementAt(itr);
//				precisionPerN[itr] = tmp;
//				tmp = precisionData.Values.ElementAt(itr) + " " + recall.Value;
//				precisionPerRecall[itr] = tmp;
//				itr++;                
//            }			
//			
//			System.IO.File.WriteAllLines("recall_per_n.txt", recallPerN);	
//			System.IO.File.WriteAllLines("precision_per_n.txt", precisionPerN);
//			System.IO.File.WriteAllLines("precision_per_recall.txt", precisionPerRecall);		
//		}
//	