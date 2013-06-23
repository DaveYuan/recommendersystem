using System;

namespace HPlearnSocialBPRMF
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
				
				predictScore = globalAvg + PredictRecall.PredictRating(user, item);
				sigScore = g(predictScore);
				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
				
				err = mappedPredictScore - rating;				
				rmse += err * err;				
			}
			rmse = Math.Sqrt(rmse/numTestEntries);			
			return rmse;
		}
	}
}