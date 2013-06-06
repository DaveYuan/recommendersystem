using System.Collections.Generic;

namespace JointFactBPR
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
		
		public double dotProductUser(int user1, int user2)
		{
			double dotProduct = 0.0;
			for (int i = 0; i < numFeatures; i++) {
				dotProduct += userFeature[i, user1] * userFeature[i, user2];
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
		
		public double predictRatingSocialRel(int userId, int userIdAssoc)
		{
			return userBias[userId] + userBias[userIdAssoc] + dotProductUser(userId, userIdAssoc);
		}
	}
}
