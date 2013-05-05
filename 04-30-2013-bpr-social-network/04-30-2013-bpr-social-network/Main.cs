using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace bprsocialnetwork
{
	class MainClass
	{
		private double lrate;
		private double lambdaU;
		private double lambdaI;
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
		private Dictionary<int, int[]> frndsPerUser;
		private Dictionary<int, int[]> ratedItemsPerUser;
		
		/*
		 * Write message to the Console and log.txt
		 */
		public static void writeToLognConsole(string mssg) 
		{
			Console.WriteLine("\t- " + mssg + " ...");
			File.AppendAllText("log.txt", "\t- " + mssg + " ...\n");
		}	
		
		/*
		 * Constructor which initializes:
		 * 	-trainUsersArray
		 * 	-trainItemsArray
		 * 	-frndsPerUser : User1-> trustUser1, trustUser2, trustUser3 ....
		 *		 		    User2-> trustUser1, trustUser2, trustUser3 ....		 
		 */
		public MainClass(Trust trustObj, Rating ratingObj)
		{
			int indx = 0;		
			this.lrate = 0.01;			
			this.lambdaU = 8;
			this.lambdaI = 8;
			this.numEpochs = 10;
			this.numFeatures = 50;
			this.trainUsersArray = ratingObj.usersList.ToArray();
			this.trainItemsArray = ratingObj.itemsList.ToArray();
		
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
				
		/*
		 * Finds the items being rated by a user
		 * -ratedItemsPerUser : User1-> Item1, Item2, Item3 ....
		 *						User2-> Item1, Item2, Item3 ....
		 */
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
		
		/*
		 * -Finds the uniqueUsers and uniqueItems from the dataset
		 * -numUsers and numItems
		 */
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
		
		/*
		 * Initialise the user and item feature vectors
		 */
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
		
		/* 
		 * Calculates the dot product of user and item feature vectors
		 */
		public double dotProduct(int userId,
		                         int itemId)
		{
			double dotProduct = 0.0;			
			for (int i = 0; i < this.numFeatures; ++i) {
				dotProduct += userFeature[i,userId] * itemFeature[i,itemId];		
			}			
			return dotProduct;
		}
		
		/*
		 * Calculates the dot product of user-user feature vectors
		 */
		public double dotProductUser(int user1, int user2)
		{
			double dotProduct = 0.0;
			for (int i = 0; i < numFeatures; i++) {
				dotProduct += userFeature[i, user1] * userFeature[i, user2];
			}
			return dotProduct;
		}
		
		/*
		 * Calculates the sigmoid 
		 */
		public double sigmoid(double x)
		{
			return ( 1.0 / ( 1.0 + Math.Exp(-x) ) );
		}
		
		/*
		 * Multirelational Bayesian Personalised Ranking for Social Network Data
		 */
		public void bprSocialNetwork() 
		{
			int numRelation;
			int numUserTargetRel;
			int randUser;
			int randPostvRel;			
			int randNegtvRel;					
			int numEntries = trainItemsArray.Length;
			double bprOpt;
			double userValue;
			double xuPostv;
			double xuNegtv;
			double xuPostvNegtv;			
			double dervxuPostvNegtv;
			
			Console.WriteLine("\t\t- #Entries: {0}", numEntries);
			Random r1 = new Random();
			Random r2 = new Random();
			Random r3 = new Random();
			
			for (int epoch = 1; epoch <= numEpochs; epoch++) {
				bprOpt = 0.0;					
				
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
						userFeature[f, randUser] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtv))) * dervxuPostvNegtv -
						                                     lambdaU * userValue);
						
						dervxuPostvNegtv = userValue;
						userFeature[f, randPostvRel] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtv))) * dervxuPostvNegtv -
						                                              lambdaU * userFeature[f, randPostvRel]);
						
						dervxuPostvNegtv = -userValue;
						userFeature[f, randNegtvRel] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtv))) * dervxuPostvNegtv - 
						                                              lambdaU * userFeature[f, randNegtvRel]);											
					}
					bprOpt += Math.Log(sigmoid(xuPostvNegtv));
//				}
//					
//				// Item-features
//				for (int n = 0; n < numEntries/100; n++) {					
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
						// Item-feature updation
						userValue = userFeature[f, randUser];
						dervxuPostvNegtv = itemFeature[f, randPostvRel] - itemFeature[f, randNegtvRel];
						userFeature[f, randUser] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtv))) * dervxuPostvNegtv -
						                                     lambdaU * userValue);
						
						dervxuPostvNegtv = userValue;
						itemFeature[f, randPostvRel] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtv))) * dervxuPostvNegtv -
						                                              lambdaI * itemFeature[f, randPostvRel]);
						
						dervxuPostvNegtv = -userValue;
						itemFeature[f, randNegtvRel] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtv))) * dervxuPostvNegtv -
						                                              lambdaI * itemFeature[f, randNegtvRel]);						
					}	
					bprOpt += Math.Log(sigmoid(xuPostvNegtv));
				}										
				Console.WriteLine("\t\t- Epoch: {0}, Bpr-Opt: {1}", epoch, bprOpt);
			}			
		}
		
		public static void Main (string[] args)
		{
			Trust trustObj;
			Rating ratingObj;
			File.Open("log.txt", FileMode.Create).Close();
			
			writeToLognConsole("Loading trust.bin");
			using (FileStream file = File.OpenRead("trust.bin"))
            {
                trustObj = Serializer.Deserialize<Trust>(file);
            }
				
			writeToLognConsole("Loading train.bin");
			using (FileStream file = File.OpenRead("train.bin"))
			{
				ratingObj = Serializer.Deserialize<Rating>(file);
			}
						
			writeToLognConsole("Constructor call");
			MainClass mainclass = new MainClass(trustObj, ratingObj);
		
			writeToLognConsole("Find: RatedItemsPerUser");
			mainclass.calRatedItemsPerUser();
			
			writeToLognConsole("Find: uniqueUsers and uniqueItems");
			mainclass.calUniqueUsersnItems();
			
			writeToLognConsole("Initialize features");
			mainclass.initFeatures();
			
			writeToLognConsole("Bpr for social network data");
			mainclass.bprSocialNetwork();
			
			Console.WriteLine ("Done!");
		}
	}
}