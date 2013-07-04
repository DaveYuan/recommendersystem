using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MultiRecommender.Evaluation
{
	public class Eval : PredictRecall
	{
		public static int testCheck() 
		{
			int numTestEntries = testItemsArray.Length;	
			if (numTestEntries == 0) {
				Console.WriteLine("Exit: Test-set empty");
				Environment.Exit(0);
			} else {
				Console.WriteLine("\n\t\t- #test: {0}", numTestEntries);
			}
			return numTestEntries;
		}	

		public static double errNoBiasTestSet(int numTestEntries) 
		{
			double err;
			double rmse;
			double sigScore;
			double predictScore;
			double mappedPredictScore;

			rmse = 0.0;			

			for (int i = 0; i < numTestEntries; i++) {
				int user = testUsersArray[i];
				int item = testItemsArray[i];
				double rating = testRatingsArray[i];
							
				predictScore = globalAvg + dotProduct(user, item);
				sigScore = g(predictScore);
				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);

				err = mappedPredictScore - rating;				

				rmse += err * err;				
			}
			rmse = Math.Sqrt(rmse/numTestEntries);			
			return rmse;
		}
				
		public static double errTestSet(int numTestEntries) 
		{
			double err;
			double rmse;
			double sigScore;
			double predictScore;
			double mappedPredictScore;
			
			rmse = 0.0;			
			
			for (int i = 0; i < numTestEntries; i++) {
				int user = testUsersArray[i];
				int item = testItemsArray[i];
				double rating = testRatingsArray[i];

				//TODO: remove globalAvg
				predictScore = globalAvg + PredictRecall.PredictRating(user, item);
				sigScore = g(globalAvg + predictScore);
				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);				
				err = mappedPredictScore - rating;								
//				err = predictScore - rating;
				rmse += err * err;				
			}
			rmse = Math.Sqrt(rmse/numTestEntries);			
			return rmse;
		}
		
		public static double hitCount(List<int> ratedItems, List<int> rankedItems, int n)
		{
			int len;
			int hits = 0;
			int itemId;

			len = rankedItems.Count;
			for (int i = 0; i < len; i++) {
				itemId = rankedItems[i];
				if (!ratedItems.Contains(itemId)) {
					continue;
				}
				
				if (i < n) {
					hits++;
				} else {
					break;
				}
			}
			return hits;
		}

		public static Dictionary<string, double> predictAtN(List<int> ratedItems, List<int> rankedItems, int[] N)
		{
			var resultAtN = new Dictionary<string, double>();
			foreach (int n in N) {
				double hits = hitCount(ratedItems, rankedItems, n);
				resultAtN["Prec["+n+"]"] =  (double) hits / (double) n;	
				resultAtN["Recl["+n+"]"] =  (double) hits / (double) ratedItems.Count;	
			}
			return resultAtN;
		}
		
		public static Dictionary<int, double> removeRatedItems(int userId, 
					      			List<int> removeItems)
		{
			int itemId;
			int uniqueItemsCnt = uniqueItemsArray.Length;
			Dictionary<int, double> predictItemRating = new Dictionary<int, double>();

			for (int i = 0; i < uniqueItemsCnt; i++) {
				itemId = uniqueItemsArray[i];
				if (!removeItems.Contains(itemId)) {
					predictItemRating.Add(itemId, PredictRating(userId, itemId));
				}
			}

			return predictItemRating;
				
			/*Dictionary<int, double> predictItemRating = new Dictionary<int, double>();

			for (int itemId = 0; itemId < MAX_ITEM_ID+1; itemId++) {
				if (!removeItems.Contains(itemId)) {
					predictItemRating.Add(itemId, PredictRating(userId, itemId));
				}
			}

			return predictItemRating;*/
		}
				
		public static Dictionary<string, double> bprEval()
		{
			int numUniqueTestUsers;
			HashSet<int> testUniqueUsers = new HashSet<int>(testUsersArray);
			HashSet<int> testUniqueItems = new HashSet<int>(testItemsArray);
			int[] N = new int[] {5, 10};

			Dictionary<string, double> result = initResult();
			List<string> keys = new List<string>(result.Keys);

			numUniqueTestUsers = testUniqueUsers.Count;
//			Console.WriteLine("\t\t- TestEntries: {0}, Uniq(#Users): {1}, Uniq(#Items): {2}", 
//					testUsersArray.Length, testUniqueUsers.Count, testUniqueItems.Count);

			List<int> candidateItems = testItemsArray.Union(trainItemsArray).ToList();
			Parallel.ForEach(testUniqueUsers, userId => {		
				try {
					var ratedItems = new HashSet<int>(testRatedItems[userId]);
					ratedItems.IntersectWith(candidateItems);
					if(ratedItems.Count == 0) return; 
					HashSet<int> itemsToRemove = new HashSet<int>();
					if (trainRatedItems.ContainsKey(userId)) {
						itemsToRemove = new HashSet<int>(trainRatedItems[userId]);					
						itemsToRemove.IntersectWith(candidateItems);
					} 

					var predictItemRating = removeRatedItems(userId, itemsToRemove.ToList());
					var rankedItems = from pair in predictItemRating
									orderby pair.Value descending
									select pair.Key;
					var resultAtN = predictAtN(ratedItems.ToList(), rankedItems.ToList(), N);			
					lock (result) 
					{
						foreach (string k in keys) {
							result[k] += resultAtN[k];
						}
//						Console.WriteLine("\t\t- UserId: {0}, #Rnkd: {1}, #Rtd: {2}, #Rmv: {3}, #CndItm: {4}",
//						userId, rankedItems.ToList().Count, ratedItems.ToList().Count, itemsToRemove.ToList().Count,candidateItems.Count);
					}
				} catch (Exception e) {
					Console.Error.WriteLine("Error: " + e.Message + e.StackTrace);
					throw;
				}
			});

			foreach (string k in keys) {
				result[k] /= (double)numUniqueTestUsers;
			}
				
			return result;
		}
	}
}
