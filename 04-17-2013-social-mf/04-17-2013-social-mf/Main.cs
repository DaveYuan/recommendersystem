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
		public int numUsers {get; set;}
		public int numItems {get; set;}
		public int numFeatures {get; set;}
		public int numEpochs {get; set;}
		public double lrate {get; set;}
		public double lambdaU {get; set;}
		public double lambdaT {get; set;}
		public Dictionary<int, List<int>> trustUserDict {get; set;}
		public List<int> trainUsersList {get; set;}
        public List<int> trainItemsList {get; set;}
		public List<int> trainRatingsList {get; set;}	
		
		/*
		 * Constructor which initializes:
		 * 	-TrustDictionary
		 * 	-TrainUsersList
		 * 	-TrainItemsList
		 * 	-TrainRatingsList
		 */
		public MainClass(Trust trustObj, Rating ratingObj)
		{
			int indx = 0;		
			this.numFeatures = 40;
			this.numEpochs = 60;
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
		
		public static void writeToLognConsole(string mssg) 
		{
			Console.WriteLine("\t- " + mssg + " ...");
			File.AppendAllText("log.txt", "\t- " + mssg + " ...\n");
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
			
			writeToLognConsole("Mapping parameter to index in list");
			mainclass.learnMapping(mainclass.trainUsersList,
			                       mainclass.trainItemsList,			                       
			                       ref userToIndxInList, 
			                       ref itemToIndxInList);
									
			mainclass.numUsers = mainclass.trustUserDict.Count;
			mainclass.numItems = itemToIndxInList.Count;
			
			double[,] userFeature = new double[mainclass.numFeatures,mainclass.numUsers];
			double[,] itemFeature = new double[mainclass.numFeatures,mainclass.numItems];	
				
			mainclass.initFeatures(ref userFeature, 
			             		   ref itemFeature);					
			
			Console.WriteLine("\t #Users: {0}, #Items: {1}", mainclass.numUsers, mainclass.numItems);					
			Console.WriteLine ("Done!");			
		}
	}
}