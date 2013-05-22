using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Collections.Generic;

namespace bprmodularMML
{
	class Init {
		static protected double regUser;
		static protected double regPostv;
		static protected double regNegtv;
		static protected double regBias;
		static protected double lrate;
		static protected int numEpochs;
		static protected int numUsers;
		static protected int numItems;
		static protected int numEntries;
		static protected int numFeatures;
		static protected int MAX_USER_ID;
		static protected int MAX_ITEM_ID;
		static protected int[] trainUsersArray;
		static protected int[] trainItemsArray;
		static protected int[] testUsersArray;
		static protected int[] testItemsArray;
		static protected int[] uniqueItemsArray;
		static protected double[,] userFeature;
		static protected double[,] itemFeature;
		static protected double[] userBias;
		static protected double[] itemBias;	
		static protected Random random;
		static protected Dictionary<int, int[]> trainRatedItems;
		static protected Dictionary<int, int[]> testRatedItems;
		
		protected static void writeToConsole(string mssg) 
		{
			Console.WriteLine("\t- " + mssg);
		}
		
		protected static void init()
		{
			userFeature = new double[numFeatures,MAX_USER_ID+1];
			itemFeature = new double[numFeatures,MAX_ITEM_ID+1];
			userBias = new double[MAX_USER_ID+1];
			itemBias = new double[MAX_ITEM_ID+1];
			Random rd = new Random();
			
			for (int i = 0; i < numFeatures; i++) {
				for (int j = 0; j <= MAX_USER_ID; j++) {
					userFeature[i,j] = (float) rd.NextDouble();
				}
			}
			
			rd = new Random();
			for (int i = 0; i < numFeatures; i++) {
				for (int j = 0; j <= MAX_ITEM_ID; j++) {
					itemFeature[i,j] = (float) rd.NextDouble();
				}
			}
		}
		
		protected static Dictionary<string, double> initResult() 
		{
			Dictionary<string, double> result = new Dictionary<string, double>();
			result.Add("Prec[5]", 0.0);
			result.Add("Prec[10]", 0.0);
			result.Add("Prec[15]", 0.0);
			result.Add("Recl[5]", 0.0);
			result.Add("Recl[10]", 0.0);
			result.Add("Recl[15]", 0.0);
			return result;
		}
		
		protected static double dotProduct(int userId,
					  			 int itemId)
		{
			double dotProduct = 0.0;
			for (int i = 0; i < numFeatures; ++i) {
				dotProduct += userFeature[i,userId] * itemFeature[i,itemId];
			}
			return dotProduct;
		}
		
		protected static double sigmoid(double x)
		{
			return (1.0 / ( 1.0 + Math.Exp(-x)));
		}
		
		protected static void trainItemsRatedByUser()
		{
			int user;
			int size = trainItemsArray.Length;
			double avgItemsRatedPerUser = 0.0;
			trainRatedItems = new Dictionary<int, int[]>();
			Dictionary<int, List<int>> trainRatedItemsTmp = new Dictionary<int, List<int>>();
			
			for (int i = 0; i < size; i++) {
				user = trainUsersArray[i];
				if (trainRatedItemsTmp.ContainsKey(user)) {
					trainRatedItemsTmp[user].Add(trainItemsArray[i]);
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(trainItemsArray[i]);
					trainRatedItemsTmp.Add(user, tmp);
				}
			}
			
			foreach (KeyValuePair<int, List<int>> ratedItem in trainRatedItemsTmp) {
				trainRatedItems.Add(ratedItem.Key, ratedItem.Value.ToArray());
				avgItemsRatedPerUser += ratedItem.Value.Count;
				
			}
			
			avgItemsRatedPerUser = avgItemsRatedPerUser / trainRatedItems.Count;
			Console.WriteLine("\t\t- Avg(TrainItemRatedPerUser): {0}", avgItemsRatedPerUser);
			GC.Collect();
		}

		protected static void testItemsRatedByUser()
		{
			int user;
			int size = testItemsArray.Length;
			double avgItemsRatedPerUser = 0.0;
			testRatedItems = new Dictionary<int, int[]>();
			Dictionary<int, List<int>> testRatedItemsTmp = new Dictionary<int, List<int>>();
			
			for (int i = 0; i < size; i++) {
				user = testUsersArray[i];
				if (testRatedItemsTmp.ContainsKey(user)) {
					testRatedItemsTmp[user].Add(testItemsArray[i]);
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(testItemsArray[i]);
					testRatedItemsTmp.Add(user, tmp);
				}
			}
			
			foreach (KeyValuePair<int, List<int>> ratedItem in testRatedItemsTmp) {
				testRatedItems.Add(ratedItem.Key, ratedItem.Value.ToArray());
				avgItemsRatedPerUser += ratedItem.Value.Count;
				
			}
			
			avgItemsRatedPerUser = avgItemsRatedPerUser / testRatedItems.Count;
			Console.WriteLine("\t\t- Avg(TestItemRatedPerUser): {0}", avgItemsRatedPerUser);
			GC.Collect();

		}
		
		protected static void calUniqueUsersnItems() 
		{
			var hashItems = new HashSet<int>(trainItemsArray);
			uniqueItemsArray = hashItems.ToArray();
			
			var hashUsers = new HashSet<int>(trainUsersArray);
			
			numUsers = hashUsers.Count;
			numItems = uniqueItemsArray.Length;
		}
		
		protected static void displayResult(Dictionary<string, double> result)
		{
			foreach (var pair in result) {
				Console.WriteLine("\t\t- " + pair.Key + ": " + pair.Value);
			}
		}

	}
}