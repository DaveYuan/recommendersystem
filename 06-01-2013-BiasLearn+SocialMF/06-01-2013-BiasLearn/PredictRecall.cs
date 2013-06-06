using System.Collections.Generic;

namespace MultiRecommender
{
	public class PredictRecall : Init
	{				
		public static double dotProduct(int userId,
					  			 int itemId)
		{
			double dotProduct = 0.0;
			for (int i = 0; i < numFeatures; ++i) {
				dotProduct += userFeature[i,userId] * itemFeature[i,itemId];
			}
			return dotProduct;
		}
		
		public static double PredictRating(int userId, int itemId) 
		{
			return globalAvg +
				   itemBias[itemId] + 
				   userBias[userId] +
				   dotProduct(userId, itemId);
		}		
	}
}
