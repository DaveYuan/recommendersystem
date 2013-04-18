using System;
using System.IO;
using System.Linq; /* Add System.Core in References to use this */
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace socialmf
{
	class MainClass
	{		
		public int numEpochs {get; set;}
		public double lrate {get; set;}
		public double lambdaU {get; set;}
		public double lambdaV {get; set;}
		public double lambdaT {get; set;}
		public double trustEffect {get; set;}
		public int numUsers {get; set;}
		public int numItems {get; set;}
		public int numFeatures {get; set;}
		public List<int> trainUsersList {get; set;}
        public List<int> trainItemsList {get; set;}
		public List<int> trainRatingsList {get; set;}	
		public Dictionary<int, List<int>> trustUserDict {get; set;}
		public Dictionary<int, List<double>> trustNormUserDict {get; set;}
		
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
			this.trustEffect = 0.0025;
			this.numEpochs = 1;
			this.numFeatures = 1;
			this.trainUsersList = ratingObj.usersList;
			this.trainItemsList = ratingObj.itemsList;
			this.trainRatingsList = ratingObj.ratingsList;	
			
			this.trustUserDict = new Dictionary<int, List<int>>();
			int[] trustUserArray2 = trustObj.trustUserList2.ToArray();
			
			foreach (int user1 in trustObj.trustUserList1) {
				if (trustUserDict.ContainsKey(user1)) {
					trustUserDict[user1].Add(trustUserArray2[indx]);					
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(trustUserArray2[indx]);
					trustUserDict.Add(user1, tmp);					
				}
				
				if (trustUserDict.ContainsKey(trustUserArray2[indx])) {
					trustUserDict[trustUserArray2[indx]].Add(user1);
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(user1);
					trustUserDict.Add(trustUserArray2[indx], tmp);
				}
				indx++;
			}			
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
		 * Normalize the trust values per user
		 * Dictionary: User1-> trustValUser1, trustValUser2, trustValUser3 ....
		 * 			   User2-> trustValUser1, trustValUser2, trustValUser3 ....
		 */
		public void normalizeTrust() 
		{
			int listSize;
			double trust;
			this.trustNormUserDict = new Dictionary<int, List<double>>();
			
			foreach (KeyValuePair<int, List<int>> userTrust in this.trustUserDict) {
				listSize = userTrust.Value.Count;
				trust = 1.0/(double)listSize;
				List<double> tmp = new List<double>();
				for (int i = 0; i < listSize; i++) {
					tmp.Add(trust);
				}
				this.trustNormUserDict.Add(userTrust.Key, tmp);
			}
		}
				
		/*
		 * Map each parameter to the index in list where its info is present
		 * parameter: user, item
		 */
		public void learnMapping(List<int> trainUsersList, 
		                         List<int> trainItemsList,		                       		                         		                   
		                         ref Dictionary<int, List<int>> userToIndxInList,
		                         ref Dictionary<int, List<int>> itemToIndxInList)
		{				
			int indx = 0;
			int[] trainItemsArray = trainItemsList.ToArray();			
			
			foreach (int user in trainUsersList) {
				if (userToIndxInList.ContainsKey(user)) {
					userToIndxInList[user].Add(indx);
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(indx);
					userToIndxInList.Add(user, tmp);
				}
				
				if (itemToIndxInList.ContainsKey(trainItemsArray[indx])) {
					itemToIndxInList[trainItemsArray[indx]].Add(indx);
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(indx);
					itemToIndxInList.Add(trainItemsArray[indx], tmp);
				}
				indx++;
			}
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
		 * Calculates the gradient of objective function wrt userfeature
		 * Assume binary rating(u,i)
		 */
		public double gradientUser(double[,] userFeature,
		                           double[,] itemFeature,
		                           int feature,
		                           int userId)
		{
			int numNghbr;
			double trustuv;
			double trustvw;
			double v1;
			double v2;
			double v3;
			double usrItmProduct;
			double gradient;
			
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			
			gradient = v1 = 0.0;
			for (int i = 0; i < numItems; i++) {
				usrItmProduct = dotProduct(userFeature, itemFeature, userId, i);
				v1 += itemFeature[feature, i] * gDerv(usrItmProduct) * g(usrItmProduct);
			}
			
			stopwatch.Stop();
			Console.WriteLine("Term1 time: {0}", stopwatch.Elapsed);
			
			stopwatch = new Stopwatch();
			stopwatch.Start();
			usrItmProduct = lambdaU * userFeature[feature, userId];
			stopwatch.Stop();
			Console.WriteLine("Term2 time: {0}", stopwatch.Elapsed);
			
			gradient += v1 - 1.0 + usrItmProduct;
			
			v1 = 0.0;
			v2 = 0.0;
			
			stopwatch = new Stopwatch();
			stopwatch.Start();
			numNghbr = trustNormUserDict[userId].Count;
			trustuv = trustNormUserDict[userId].ElementAt(0);
			
			foreach (int v in trustUserDict[userId]) {
				v1 += trustuv * userFeature[feature, v];
				trustvw = trustNormUserDict[v].ElementAt(0);				
				v3 = 0.0;
				foreach (int w in trustUserDict[v]) {
						v3 += trustvw * userFeature[feature, w];
				}
				v2 += trustuv * (userFeature[feature, v] - v3);
			}
			stopwatch.Stop();
			Console.WriteLine("Term3 n term4 time: {0}", stopwatch.Elapsed);
			
			gradient += (lambdaT * (userFeature[feature, userId] - v1)) - (lambdaT * v2);
			
			return gradient;
		}
		
		/* 
		 * Calculate the gradient of objective function wrt itemfeature
		 */
		public double gradientItem(double[,] userFeature,
		                           double[,] itemFeature,
		                           int feature,
		                           int itemId)
		{
			double v1;
			double gradient;
			double usrItmProduct;			
			
			gradient = v1 = 0.0;
			for (int u = 0; u < numUsers; u++) {
				usrItmProduct = dotProduct(userFeature, itemFeature, u, itemId);				
				v1 += userFeature[feature, u] * gDerv(usrItmProduct) * (g(usrItmProduct) - 1.0);
			}
			gradient += v1 + (lambdaV * itemFeature[feature, itemId]);
			
			return gradient;
		}
		
		public void socialmf(ref double[,] userFeature,
		                     ref double[,] itemFeature,
		                     Dictionary<int, List<int>> userToIndxInList,
		                     Dictionary<int, List<int>> itemToIndxInList) 
		{
			int userId;
			int itemId;
			int numEntries;	
			double uv;
			double err;
			double errPerEpoch;
			double usrItmProduct;			
			
			numEntries = trainRatingsList.Count;
			Console.WriteLine("\t- numEntries: {0}", numEntries);
			
			for (int itr = 0; itr < this.numEpochs; itr++) {
				Console.WriteLine("\t- Epoch: {0}", itr);
				err = 0.0;
				errPerEpoch = 0.0;
				//for (int q = 0; q < numEntries/1000; q++) {
				for (int q = 0; q < 5; q++) {
					if (q % 10 == 0) {
						Console.WriteLine("\t\t- #Entry: {0}", q);
					}
					userId = trainUsersList[q];
					itemId = trainItemsList[q];
					usrItmProduct = dotProduct(userFeature, itemFeature, userId, itemId);
					err = 1.0 - usrItmProduct;
					errPerEpoch += err * err;
					for (int f = 0; f < numFeatures; f++) {
						uv = userFeature[f, userId];
						userFeature[f, userId] += lrate * gradientUser(userFeature, itemFeature, f, userId);
						itemFeature[f, itemId] += lrate * gradientItem(userFeature, itemFeature, f, itemId);
					}										
				}
				errPerEpoch = Math.Sqrt(errPerEpoch/numEntries);
				Console.WriteLine("\t- Epoch: {0}, Err: {1}", itr, errPerEpoch);
			}			
		}			
		
		public static void Main (string[] args)
		{						
			Trust trustObj;
			Rating ratingObj;
			Dictionary<int, List<int>> userToIndxInList = new Dictionary<int, List<int>>();
			Dictionary<int, List<int>> itemToIndxInList = new Dictionary<int, List<int>>();
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
			writeToLognConsole("Normalizing trust data");
			mainclass.normalizeTrust();
			
			writeToLognConsole("Mapping user and item to index in list");
			mainclass.learnMapping(mainclass.trainUsersList,
			                       mainclass.trainItemsList,			                       
			                       ref userToIndxInList, 
			                       ref itemToIndxInList);
									
			mainclass.numUsers = mainclass.trustUserDict.Count;
			mainclass.numItems = itemToIndxInList.Count;
			Console.WriteLine("\t- #Users: {0}, #Items: {1}", mainclass.numUsers, mainclass.numItems);

			
			double[,] userFeature = new double[mainclass.numFeatures,mainclass.numUsers];
			double[,] itemFeature = new double[mainclass.numFeatures,mainclass.numItems];	
				
			writeToLognConsole("Initialize feature vectors");						
			mainclass.initFeatures(ref userFeature, 
			             		   ref itemFeature);					
						
			writeToLognConsole("Social MF");			
			mainclass.socialmf(ref userFeature,
		             ref itemFeature,
		             userToIndxInList,
		             itemToIndxInList);
			
			Console.WriteLine ("Done!");			
		}
	}
}




// Since trust is normalized, so neighbour effect calculated directly
// neighbrUsrItmProduct = trustNormUserDict[userId].ElementAt(0) * usrItmProduct * trustNormUserDict[userId].Count;					
//	predictRating = g( (trustEffect * usrItmProduct) + ( (1 - trustEffect) * neighbrUsrItmProduct) );