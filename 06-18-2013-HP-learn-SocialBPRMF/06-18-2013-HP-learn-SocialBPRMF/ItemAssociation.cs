using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace HPlearnSocialBPRMF
{
	public class ItemAssociation : BiasLearnMF
	{
		public static Dictionary<int, int[]> trainRatedItems;
		public static Dictionary<int, int[]> testRatedItems;
				
		public void trainItemsRatedByUser()
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

		public void testItemsRatedByUser()
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
	}
}
		
