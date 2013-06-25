using System.Collections.Generic;

namespace MultiRecommender.Evaluation
{
	//TODO: Change name of class PredictRecall to Predict
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
			
		public double dotProduct(int user, int item, double err)
		{
			double product = 0.0;
			for (int f = 0; f < numFeatures; f++) {
				product += (userFeature[f, user] - lrate*(err*itemFeature[f, item] + regUser*userFeature[f, user])) *
				           (itemFeature[f, item] - lrate*(err*userFeature[f, user] + regItem*itemFeature[f, item]));
			}
			return product;
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
			return globalAvg + 
			       userBias[userId] +
			       userBias[userIdAssoc] +
			       dotProductUser(userId, userIdAssoc);
		}
			
		public double predictAtT1(int user, int item, double err)
		{
			double predictRating;
			predictRating = (globalAvg - lrate*(err + regGlbAvg*globalAvg)) +
							(itemBias[item] - lrate*(err + regItem*itemBias[item])) +
				 			(userBias[user] - lrate*(err + regUser*userBias[user])) +
				  			dotProduct(user, item, err);				
			return predictRating;
		}
		
	}
}
