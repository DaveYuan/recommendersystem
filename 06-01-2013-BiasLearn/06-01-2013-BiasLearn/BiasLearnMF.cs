using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using ProtoBuf;

namespace MultiRecommender
{
	class BiasLearnMF : Eval
	{													
		public static void biasLearnMF()
		{		
			int numTestEntries;
			double err;
			double rmse;
			double sigScore;
			double predictScore;
			double mappedPredictScore;					
			
			numTestEntries = testCheck();
			for (int itr = 1; itr <= numEpochs; itr++) {			
				for (int n = 0; n < numEntries; n++) {
					int user = trainUsersArray[n];
					int item = trainItemsArray[n];
					int rating = trainRatingsArray[n];	
				
					predictScore = globalAvg + PredictRating(user, item);	
					sigScore = g(predictScore);
					mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
					err = mappedPredictScore - rating;
					err = err * sigScore * (1 - sigScore) * (MAX_RATING - MIN_RATING);
					
					incBias(USER_INC, user, -lrate * (err + regBias * regUser * userBias[user]));
					incBias(ITEM_INC, item, -lrate * (err + regBias * regItem * itemBias[item]));
								
					for (int f = 0; f < numFeatures; f++) {
						double uF = userFeature[f, user];
						double iF = itemFeature[f, item];
						incFeature(USER_INC, user, f, -lrate * (err * iF + regUser * uF));
						incFeature(ITEM_INC, item, f, -lrate * (err * uF + regItem * iF));						
					}
				}
							
				rmse = errTestSet(numTestEntries);
				Console.WriteLine("\t\t- RMSE[{0}]: {1}", itr, rmse);
			}
		}				
	}
}