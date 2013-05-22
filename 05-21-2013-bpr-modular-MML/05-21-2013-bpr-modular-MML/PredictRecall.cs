using System.Collections.Generic;

namespace bprmodularMML
{
	class PredictRecall : Init
	{		
		protected static double PredictRating(int userId, int itemId) 
		{
			return itemBias[itemId] + 
					userBias[userId] + 
					dotProduct(userId, itemId);
		}
		
		protected static Dictionary<int, double> removeRatedItems(int userId, 
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
		}

		protected static double hitCount(List<int> ratedItems, List<int> rankedItems, int n)
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

		protected static Dictionary<string, double> predictAtN(List<int> ratedItems, List<int> rankedItems, int[] N)
		{
			var resultAtN = new Dictionary<string, double>();
			foreach (int n in N) {
				double hits = hitCount(ratedItems, rankedItems, n);
				resultAtN["Prec["+n+"]"] =  (double) hits / (double) n;	
				resultAtN["Recl["+n+"]"] =  (double) hits / (double) ratedItems.Count;	
			}
			return resultAtN;
		}
	}
}
