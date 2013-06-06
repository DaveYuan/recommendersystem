using System;
using System.Linq;
using System.Collections.Generic;

namespace MultiRecommender
{
	class SocialMF : BiasLearnMF
	{
		public static double regSocial;
		public static SparseMatrix userAssociations;
		
		public static void socialMFInit ()
		{			
			regSocial = 10;														
		}
		
		public static void predictionErrorLearn()
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
		
		public static void regularization() 
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
		
		public static void socialRelLearn()
		{
			int MAX_USER_ID_ASSOCIATION_DATA = userAssociations.sparseMatrix.Count - 1;
			var userReverseAssociations = userAssociations.transpose();
			
			for (int u = 0; u < MAX_USER_ID_ASSOCIATION_DATA; u++) {
				double sumUserUAssociationBias = 0.0;
				double[] sumUserUAssociationFeatures = new double[numFeatures];	
				// User has no social relation, but present in train
				if (u > MAX_USER_ID_ASSOCIATION_DATA) {				
					continue;					
				}
				int numUAssociations = userAssociations.sparseMatrix[u].Count;
				
				foreach (int v in userAssociations.sparseMatrix[u]) {
					sumUserUAssociationBias += userBias[v];
					for (int f = 0; f < numFeatures; f++) {
						sumUserUAssociationFeatures[f]+= userFeature[f,v];
					}
				}
				
				if (numUAssociations != 0) {
					decBias(USER_DEC, u, lrate *(regSocial * (userBias[u] - sumUserUAssociationBias/numUAssociations)));
					for (int f = 0; f < numFeatures; f++) {
						decFeature(USER_DEC, u, f, lrate * (regSocial * (userFeature[f,u] - sumUserUAssociationFeatures[f]/numUAssociations)));
					}
				}								
				
				foreach (int v in userReverseAssociations[u]) {					
					int numVAssociations = userAssociations.sparseMatrix[v].Count;
					double sumUserWAssociationBias = 0.0;
					double[] sumUserWAssociationFeatures = new double[numFeatures];									
					
					foreach (int w in userAssociations.sparseMatrix[v]) {
						sumUserWAssociationBias += userBias[w];
						for (int f = 0; f < numFeatures; f++) {
							sumUserWAssociationFeatures[f] += userFeature[f, w];
						}
					}
					
					if (numVAssociations != 0) {
						double biasInc = -userBias[v] + sumUserWAssociationBias/(double)numVAssociations;
						decBias(USER_DEC, u, lrate * (regSocial * biasInc / (double)numVAssociations));
						for (int f = 0; f < numFeatures; f++) {
							double dec = -userFeature[f,v] + sumUserWAssociationFeatures[f]/(double)numVAssociations;
							decFeature(USER_DEC, u, f, lrate * (regSocial * dec / (double)numVAssociations));
						}
					}
				}				
			}
		}	
		
		public static void socialMF()
		{		
			double rmse;
			int numTestEntries; 
			
			numTestEntries = testCheck();
			
			for (int itr = 1; itr <= numEpochs; itr++) {
				predictionErrorLearn();	
				regularization();					
				socialRelLearn();
				
				rmse = errTestSet(numTestEntries);
				Console.WriteLine("\t\t- RMSE[{0}]: {1}", itr, rmse);
			}
		}
	}
}

