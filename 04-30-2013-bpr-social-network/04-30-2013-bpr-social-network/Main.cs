using System;
using System.IO;
using System.Linq; /* Add System.Core in References to use this */
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
			this.lambdaU = 0.0025;
			this.lambdaI = 0.0025;
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
		
		public void bprSocialNetwork() 
		{
			int numRelation;
			int numUserTargetRel;
			int randUserUF;
			int randPostvRelUF;			
			int randNegtvRelUF;
			int randUserIF;
			int randPostvRelIF;
			int randNegtvRelIF;			
			int numEntries = trainItemsArray.Length;
			double bprOpt;
			double userValue;
			double xuPostvUF;
			double xuNegtvUF;
			double xuPostvNegtvUF;
			double xuPostvIF;
			double xuNegtvIF;
			double xuPostvNegtvIF;
			double dervxuPostvNegtv;
			
			Console.WriteLine("\t\t- #Entries: {0}", numEntries);
			Random r1 = new Random();
			Random r2 = new Random();
			Random r3 = new Random();
			
			for (int epoch = 1; epoch <= numEpochs; epoch++) {
				bprOpt = 0.0;	
				xuPostvNegtvUF = xuPostvNegtvIF = 0.0;
				for (int n = 0; n < numEntries/100; n++) {
					// Init for User-features
					randUserUF = frndsPerUser.Keys.ElementAt(r1.Next(0, numUsers));
					numRelation = frndsPerUser[randUserUF].Length;
					randPostvRelUF = frndsPerUser[randUserUF][r2.Next(0, numRelation)];
					randNegtvRelUF = uniqueUsersArray[r3.Next(0, numUsers)];							
					while (frndsPerUser[randUserUF].Contains(randNegtvRelUF)) {
						randNegtvRelUF = uniqueUsersArray[r3.Next(0, numUsers)];
					}					
					xuPostvUF = dotProductUser(randUserUF, randPostvRelUF);
					xuNegtvUF = dotProductUser(randUserUF, randNegtvRelUF);
					xuPostvNegtvUF = xuPostvUF - xuNegtvUF;
									
					for (int f = 0; f < numFeatures; f++) {
						// User-feature updation
						userValue = userFeature[f, randUserUF];
						dervxuPostvNegtv = userFeature[f, randPostvRelUF] - userFeature[f, randNegtvRelUF];
						userFeature[f, randUserUF] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtvUF))) * dervxuPostvNegtv - 
						                                     lambdaU * userValue);
						
						dervxuPostvNegtv = userValue;
						userFeature[f, randPostvRelUF] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtvUF))) * dervxuPostvNegtv - 
						                                              lambdaU * userFeature[f, randPostvRelUF]);
						
						dervxuPostvNegtv = -userValue;
						userFeature[f, randNegtvRelUF] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtvUF))) * dervxuPostvNegtv - 
						                                              lambdaU * userFeature[f, randNegtvRelUF]);											
					}
					bprOpt += Math.Log(sigmoid(xuPostvNegtvUF));
				}
					
				for (int n = 0; n < numEntries/100; n++) {
					// Init for Item-features
					numUserTargetRel = ratedItemsPerUser.Keys.Count;
					randUserIF = ratedItemsPerUser.Keys.ElementAt(r1.Next(0, numUserTargetRel));
					numRelation = ratedItemsPerUser[randUserIF].Length;
					randPostvRelIF = ratedItemsPerUser[randUserIF][r2.Next(0, numRelation)];
					randNegtvRelIF = uniqueItemsArray[r3.Next(0, numItems)];					
					while (ratedItemsPerUser[randUserIF].Contains(randNegtvRelIF)) {
						randNegtvRelIF = uniqueItemsArray[r3.Next(0, numItems)];
					}
					xuPostvIF = dotProduct(randUserIF, randPostvRelIF);
					xuNegtvIF = dotProduct(randUserIF, randNegtvRelIF);
					xuPostvNegtvIF = xuPostvIF - xuNegtvIF;		
					
					for (int f = 0; f < numFeatures; f++) {						
						// Item-feature updation
						userValue = userFeature[f, randUserIF];
						dervxuPostvNegtv = itemFeature[f, randPostvRelIF] - itemFeature[f, randNegtvRelIF];
						userFeature[f, randUserIF] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtvIF))) * dervxuPostvNegtv - 
						                                     lambdaU * userValue);
						
						dervxuPostvNegtv = userValue;
						itemFeature[f, randPostvRelIF] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtvIF))) * dervxuPostvNegtv - 
						                                              lambdaI * itemFeature[f, randPostvRelIF]);
						
						dervxuPostvNegtv = -userValue;
						itemFeature[f, randNegtvRelIF] += lrate * ((1.0 / (1.0 + Math.Exp(-xuPostvNegtvIF))) * dervxuPostvNegtv - 
						                                              lambdaI * itemFeature[f, randNegtvRelIF]);						
					}	
					bprOpt += Math.Log(sigmoid(xuPostvNegtvIF));
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
