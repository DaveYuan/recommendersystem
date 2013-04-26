using System;
using System.IO;
using System.Linq; /* Add System.Core in References to use this */
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;
using ProtoBuf;

// TODO: Uniq(#User) in Trust > Uniq(#User) in UserList

namespace socialmf
{
	class MainClass
	{		
		private int numEpochs;
		private double lrate;
		private double lambdaU;
		private double lambdaV;
		private double lambdaT;		
		private int numUsers;
		private int numItems;
		private int numFeatures;
		private int[] trainUsersArray;
        private int[] trainItemsArray;
		private double[] trainRatingsArray;
		private double[,] userFeature;
		private double[,] itemFeature;
		private Dictionary<int, int[]> trustUserDict;
		private Dictionary<int, int> trustNumNghbr;
	
		/*
		 * Constructor which initializes:
		 * 	-TrainUsersList
		 * 	-TrainItemsList
		 * 	-TrainRatingsList
		 * 	-TrustDictionary : User1-> trustUser1, trustUser2, trustUser3 ....
		 *		 			   User2-> trustUser1, trustUser2, trustUser3 ....		 
		 */
		public MainClass(Trust trustObj, Rating ratingObj)
		{
			int indx = 0;		
			this.lrate = 0.01;
			this.lambdaU = 0.0025;
			this.lambdaV = 0.0025;
			this.lambdaT = 0.0025;			
			this.numEpochs = 10;
			this.numFeatures = 10;
			this.trainUsersArray = ratingObj.usersList.ToArray();
			this.trainItemsArray = ratingObj.itemsList.ToArray();
		//	this.trainRatingsArray = ratingObj.ratingsList.ToArray();	
			
			Dictionary<int, List<int>> trustUserDictTmp = new Dictionary<int, List<int>>();
			trustUserDict = new Dictionary<int, int[]>();
			int[] trustUserArray2 = trustObj.trustUserList2.ToArray();
			
			foreach (int user1 in trustObj.trustUserList1) {
				if (trustUserDictTmp.ContainsKey(user1)) {
					trustUserDictTmp[user1].Add(trustUserArray2[indx]);					
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(trustUserArray2[indx]);
					trustUserDictTmp.Add(user1, tmp);					
				}
				
				if (trustUserDictTmp.ContainsKey(trustUserArray2[indx])) {
					trustUserDictTmp[trustUserArray2[indx]].Add(user1);
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(user1);
					trustUserDictTmp.Add(trustUserArray2[indx], tmp);
				}
				indx++;
			}
			
			// Convert Dictionary<int, List<int>> => Dictionary<int, int[]>			
			foreach (KeyValuePair<int, List<int>> trustRelation in trustUserDictTmp) {
				trustUserDict.Add(trustRelation.Key, trustRelation.Value.ToArray());
			}			 
			
			// Count Uniq(numUsers) and Uniq(numItems)
			numUsers = trustUserDict.Count;
			numItems = trainItemsArray.Distinct().ToArray().Length;
			userFeature = new double[numFeatures,numUsers];
			itemFeature = new double[numFeatures,numItems];				
			GC.Collect();
		}		
				
		/*
		 * Write message to the Console and log.txt
		 */
		public static void writeToLognConsole(string mssg) 
		{
			Console.WriteLine("\t- " + mssg + " ...");
			File.AppendAllText("log.txt", "\t- " + mssg + " ...\n");
		}									
		
		/*
		 * Initialise the user and item feature vectors
		 */
		public void initFeatures()
		{
			int i;
			int j;
			
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
		 * Logistic sigmoid function
		 * g(x) = 1 / (1 + e^(-x))
		 */
		public double g(double x) 
		{
			double sigmoid = 1.0 / (double)(1.0 + Math.Exp(-x));
			return sigmoid;			
		}
		
		/*
		 * Derivative of sigmoid 
		 * g'(x) = e^(-x)/((1 + e^(-x))^2)
		 */
		public double gDerv(double x)
		{
			double sigmoidDerv = Math.Exp(-x) / (double)(Math.Pow( (1.0 + Math.Exp(-x)), 2.0));
			return sigmoidDerv;
		}			
		
		/* 
		 * Maps R(0-5) => R(0-1)
		 */
		public void mapDownRatings(Rating ratingObj) 
		{			
			int[] trainRatingsArrayTmp = ratingObj.ratingsList.ToArray();
			int arrayL = trainRatingsArrayTmp.Length;
			trainRatingsArray = new double[arrayL];
			
			for (int i = 0; i < arrayL; i++) {
				trainRatingsArray[i] = g(trainRatingsArrayTmp[i]);
			}
			GC.Collect();
		}		
		
		/*
		 * Count trust neighbours per user
		 */
		public void countNghbr() 
		{
			trustNumNghbr = new Dictionary<int, int>();
			
			foreach (KeyValuePair<int, int[]> userTrust in trustUserDict) {				
				trustNumNghbr.Add(userTrust.Key, userTrust.Value.Length);	
			}		
		}
		
		/* 
		 * Calculates the gradient of loss function wrt userfeature		 
		 */
		public double gradientUser(int userId,
		                           int itemId,
		                           int feature,
		                           double rating)
		{
			int numNghbrU;
			int numNghbrV;			
			double t1;
			double t2;
			double t3;
			double t4;
			double trustuv;
			double trustvw;
			double usrItmProduct;
			double sumTrustFeatureProductUV;
			double sumTrustFeatureProductVW;
						
			t1 = t2 = t3 = t4 = 0.0;
			
			usrItmProduct = dotProduct(userId, itemId);
			t1 = itemFeature[feature,itemId] * gDerv(usrItmProduct) * (g(usrItmProduct) - rating);
			t2 = lambdaU * userFeature[feature, userId];
			
			numNghbrU = trustNumNghbr[userId];
			trustuv = 1.0 / (double)numNghbrU;
			sumTrustFeatureProductUV = 0.0;
				
			foreach (int userV in trustUserDict[userId]) {
				sumTrustFeatureProductUV += trustuv * userFeature[feature,userV];
				
				numNghbrV = trustNumNghbr[userV]; 
				trustvw = 1.0 / (double)numNghbrV;
				sumTrustFeatureProductVW = 0.0;
				
				foreach (int userW in trustUserDict[userV]) {
					sumTrustFeatureProductVW += trustvw * userFeature[feature,userW];
				}
				
				t4 += trustvw * (userFeature[feature,userV] - sumTrustFeatureProductVW);								
			}
			
			t3 = lambdaT * (userFeature[feature,userId] - sumTrustFeatureProductUV);
			t4 = -1 * lambdaT * t4;
			
			return (t1 + t2 + t3 + t4);													
		}
		
		/* 
		 * Calculate the gradient of objective function wrt itemfeature
		 */
		public double gradientItem(int userId,
		                           int itemId,
		                           int feature,		                           
		                           double rating)
		{
			double t1;
			double t2;
			double usrItmProduct;			
			
			t1 = t2 = 0.0;
			usrItmProduct = dotProduct(userId, itemId);
			t1 = userFeature[feature,userId] * gDerv(usrItmProduct) * (g(usrItmProduct) - rating);
			t2 = lambdaV * itemFeature[feature,itemId];
			
			return (t1 + t2);			
		}
				
		public void socialmf() 
		{
			int userId;
			int itemId;
			int numEntries;	
			double gU;
			double gI;
			double err;
		//	double ratingScaledDown;
			double errPerEpoch;
			double usrItmProduct;			
			
			numEntries = trainRatingsArray.Length;
			Console.WriteLine("\t- numEntries: {0}", numEntries);
			
			for (int itr = 0; itr < this.numEpochs; itr++) {
				Console.WriteLine("\t- Epoch: {0}", itr);
				err = 0.0;
				errPerEpoch = 0.0;
				
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				for (int q = 0; q < numEntries; q++) {  				
					if (q % 1000 == 0) {
						Console.WriteLine("\t\t- Time: {0}, #Epoch: {1}, #Tuple: {2}", stopwatch.Elapsed, itr, q);
					}
					
					userId = trainUsersArray[q];
					itemId = trainItemsArray[q];		
					usrItmProduct = dotProduct(userId, itemId);
					//ratingScaledDown = mapDownRating(trainRatingsArray[q]);
					err = trainRatingsArray[q] - usrItmProduct;
					errPerEpoch += err * err;	
					
					for (int f = 0; f < numFeatures; f++) {	
						gU = gradientUser(userId, itemId, f, trainRatingsArray[q]);
						gI = gradientItem(userId, itemId, f, trainRatingsArray[q]);
						userFeature[f, userId] += lrate * gU;
						itemFeature[f, itemId] += lrate * gI;						
					}										
				}
				errPerEpoch = Math.Sqrt(errPerEpoch/numEntries);
				stopwatch.Stop();
				Console.WriteLine("\t- Time: {0}, Epoch: {1}, Err: {2}", stopwatch.Elapsed, itr, errPerEpoch);
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
					
			writeToLognConsole("Count neighbour per user");
			mainclass.countNghbr();	
			
			writeToLognConsole("Map down ratings R(1-5) -> R(0-1)");
			mainclass.mapDownRatings(ratingObj);
			
			Console.WriteLine("\t- Uniq(#Users): {0}, Uniq(#Items): {1}", mainclass.numUsers, mainclass.numItems);
				
			writeToLognConsole("Initialize feature vectors");						
			mainclass.initFeatures();					
						
			writeToLognConsole("Initiate social mF");			
			mainclass.socialmf();						
										
			Console.WriteLine ("Done!");			
		}
	}
}

// Since trust is normalized, so neighbour effect calculated directly
// neighbrUsrItmProduct = trustNormUserDict[userId].ElementAt(0) * usrItmProduct * trustNormUserDict[userId].Count;					
//	predictRating = g( (trustEffect * usrItmProduct) + ( (1 - trustEffect) * neighbrUsrItmProduct) );
//			writeToLognConsole("Hash calculation");			
//		
//			string s ="286 56";
//			string str = "286 56";
//			int hash1 = s.GetHashCode();
//			int hash2 = str.GetHashCode();
//			if (hash1 == hash2) {
//				Console.WriteLine("Hash are same");
//			} else {
//				Console.WriteLine("Hash different! Hash1: {0}, \nHash2: {1}", hash1, hash2);
//			}