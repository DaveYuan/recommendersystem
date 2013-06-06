using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using ProtoBuf;

namespace JointFactBPR
{
	public class BiasLearnMF : Eval
	{		
		public BiasLearnMF()
		{
			Console.WriteLine("\t\t- MAX_USER_ID: {0}, MAX_ITEM_ID: {1}", MAX_USER_ID, MAX_ITEM_ID);	
			Console.Write("\t\t- #epochs: {0}, #features: {1}, lrate: {2}, regUser: {3}, regItem: {4}, regBias: {5}",
			                  numEpochs, numFeatures, lrate, regUser, regItem, regBias);					
		}
		
		public void predictionErrorLearn()
		{			
			double err;
			double sigScore;
			double predictScore;
			double mappedPredictScore;				
			
			for (int n = 0; n < numEntries; n++) {
				int user = trainUsersArray[n];
				int item = trainItemsArray[n];
				int rating = trainRatingsArray[n];	
				
				predictScore = PredictRating(user, item);	
				sigScore = g(predictScore);
				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
				err = mappedPredictScore - rating;
				err = err * sigScore * (1 - sigScore) * (MAX_RATING - MIN_RATING);
					
				decBias(USER_DEC, user, lrate * (err));
				decBias(ITEM_DEC, item, lrate * (err));
							
				for (int f = 0; f < numFeatures; f++) {
					double uF = userFeature[f, user];
					double iF = itemFeature[f, item];
					decFeature(USER_DEC, user, f, lrate * (err * iF));
					decFeature(ITEM_DEC, item, f, lrate * (err * uF));						
				}				
			}
		}
		
		public void regularization() 
		{
			for (int u = 0; u <= MAX_USER_ID; u++) {
				decBias(USER_DEC, u, lrate * userBias[u] * regUser * regBias);
				for (int f = 0; f < numFeatures; f++) {
					decFeature(USER_DEC, u, f, lrate * userFeature[f,u] * regUser);
				}
			}
			
			for (int i = 0; i <= MAX_ITEM_ID; i++) {
				decBias(ITEM_DEC, i, lrate * itemBias[i] * regItem * regBias);
				for (int f = 0; f < numFeatures; f++) {
					decFeature(ITEM_DEC, i, f, lrate * itemFeature[f,i] * regItem);
				}
			}			
		}
		
		public void biasLearnMFTrain()
		{		
			int numTestEntries;
			
			double rmse;									
			numTestEntries = testCheck();
			
			for (int itr = 1; itr <= numEpochs; itr++) {
				predictionErrorLearn();	
				regularization();					
				
				rmse = errTestSet(numTestEntries);
				Console.WriteLine("\t\t- RMSE[{0}]: {1}", itr, rmse);
			}								
		}				
	}
}