using System.Collections.Generic;

namespace socialMFv1
{
	class PredictRecall : Init
	{				
		protected static double dotProduct(int userId,
					  			 int itemId)
		{
			double dotProduct = 0.0;
			for (int i = 0; i < numFeatures; ++i) {
				dotProduct += userFeature[i,userId] * itemFeature[i,itemId];
			}
			return dotProduct;
		}
		
		protected static double PredictRating(int userId, int itemId) 
		{
			return itemBias[itemId] + 
					userBias[userId] + 
					dotProduct(userId, itemId);
		}		
	}
}
