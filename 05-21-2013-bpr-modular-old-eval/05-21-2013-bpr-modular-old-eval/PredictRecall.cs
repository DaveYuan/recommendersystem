using System.Collections.Generic;

namespace bprmodularoldeval
{
	class PredictRecall : Init
	{		
		protected static double PredictRating(int userId, int itemId) 
		{
			return itemBias[itemId] + 
					userBias[userId] + 
					dotProduct(userId, itemId);
		}

		protected static double hitCount(int ratedItem, List<int> rankedList, int n)
		{
			int len;
			int itemId;
			
			len = rankedList.Count;
			for (int i = 0; i < len; i++) {
				itemId = rankedList[i];
				if (itemId != ratedItem) {
					continue;
				}
				if (i < n) {
					return 1;
				} else {
					break;
				}
			}
			return 0;
		}

		protected static Dictionary<string, double> predictAtN(int ratedItem, List<int> rankedList, int[] N)
		{
			var resultAtN = new Dictionary<string, double>();
			foreach (int n in N) {
				double hits = hitCount(ratedItem, rankedList, n);
				resultAtN["Prec["+n+"]"] =  (double) hits / (double) n;	
				resultAtN["Recl["+n+"]"] =  (double) hits;	
			}
			return resultAtN;
		}
	}
}
