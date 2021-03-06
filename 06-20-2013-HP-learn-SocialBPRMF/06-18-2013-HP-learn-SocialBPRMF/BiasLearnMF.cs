using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace HPlearnSocialBPRMF
{
	public class BiasLearnMF : Eval
	{		
		public BiasLearnMF()
		{			
			Console.Write("\t\t- #epochs: {0}, #features: {1}, lrate: {2}, regUser: {3}, regItem: {4}, regBias: {5}",
			                  numEpochs, numFeatures, lrate, regUser, regItem, regBias);					
		}
		
		public double predictionErrorLearn()
		{			
			double err;
			double rmse;
			double sigScore;
			double predictScore;
			double mappedPredictScore;				
			
			rmse = 0;
			for (int n = 0; n < numTrainEntries; n++) {
				int user = trainUsersArray[n];
				int item = trainItemsArray[n];
				int rating = trainRatingsArray[n];	
				
				predictScore = PredictRating(user, item);	
				sigScore = g(predictScore);
				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
				err = mappedPredictScore - rating;
				err = err * sigScore * (1 - sigScore) * (MAX_RATING - MIN_RATING);
					
				globalAvg = globalAvg - lrate*err  - regGlbAvg*globalAvg;
				decBias(USER, user, lrate * (err));
				decBias(ITEM, item, lrate * (err));
							
				for (int f = 0; f < numFeatures; f++) {
					double uF = userFeature[f, user];
					double iF = itemFeature[f, item];
					decFeature(USER, user, f, lrate * (err * iF));
					decFeature(ITEM, item, f, lrate * (err * uF));						
				}
				rmse += err * err;				
			}
			rmse = Math.Sqrt(rmse/(double)numTrainEntries);
			return rmse;
		}
		
		public void regularization() 
		{
			//globalAvg = globalAvg
			
			for (int u = 0; u <= MAX_USER_ID; u++) {
				decBias(USER, u, lrate * userBias[u] * regUser * regBias);
				for (int f = 0; f < numFeatures; f++) {
					decFeature(USER, u, f, lrate * userFeature[f,u] * regUser);
				}
			}
			
			for (int i = 0; i <= MAX_ITEM_ID; i++) {
				decBias(ITEM, i, lrate * itemBias[i] * regItem * regBias);
				for (int f = 0; f < numFeatures; f++) {
					decFeature(ITEM, i, f, lrate * itemFeature[f,i] * regItem);
				}
			}			
		}
		
		public void biasLearnMFTrain()
		{		
			int numTestEntries;
			
			double rmse_X;
			double rmse_XY;									
			numTestEntries = testCheck();
			
			for (int itr = 1; itr <= numEpochs; itr++) {
				regularization();					
				rmse_X = predictionErrorLearn();	
				
				rmse_XY = errTestSet(numTestEntries);
				Console.WriteLine("\t\t- {0}: RMSE_X: {1}, RMSE_XY: {2}", itr, rmse_X, rmse_XY);
			}								
		}				
	}
}
