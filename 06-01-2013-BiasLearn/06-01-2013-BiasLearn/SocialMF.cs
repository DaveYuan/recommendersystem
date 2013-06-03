using System;
using System.Linq;
using System.Collections.Generic;

namespace MultiRecommender
{
	class SocialMF : BiasLearnMF
	{
		public static double regSocial;
		public static double[] userBiasGradient;
		public static double[] itemBiasGradient;
		public static double[,] userFeatureGradient;
		public static double[,] itemFeatureGradient;
		public static SparseMatrix userAssociations;
		
		public static void socialMFInit ()
		{			
			regSocial = 0.3;
			userBiasGradient = new double[MAX_USER_ID+1];
			itemBiasGradient = new double[MAX_ITEM_ID+1];
			userFeatureGradient = new double[numFeatures, MAX_USER_ID+1];
			itemFeatureGradient = new double[numFeatures, MAX_ITEM_ID+1];															
		}
		
		public static void incBiasGradient(int incRel, int indx, double inc) 
		{
			if (incRel == USER_INC) {
				userBiasGradient[indx] += inc;
			} else if (incRel == ITEM_INC) {
				itemBiasGradient[indx] += inc;
			}
		}
		
		public static void incFeatureGradient(int incRel, int indx, int feature, double inc)
		{
			if (incRel == USER_INC) {
				userFeatureGradient[feature, indx] += inc;
			} else if (incRel == ITEM_INC) {
				itemFeatureGradient[feature, indx] += inc;
			}
		}
		
		public static void predictionErrorGradientLearn()
		{			
			double err;
			double sigScore;
			double predictScore;
			double mappedPredictScore;				
			
			for (int n = 0; n < numEntries; n++) {
				int user = trainUsersArray[n];
				int item = trainItemsArray[n];
				int rating = trainRatingsArray[n];																
				
				// TODO: globalBias
				predictScore = globalAvg + PredictRating(user, item);			
				sigScore = g(predictScore);
				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
				err = mappedPredictScore - rating;
				
				err = err * sigScore * (1 - sigScore) * (MAX_RATING - MIN_RATING);
					
				incBiasGradient(USER_INC, user, err);
				incBiasGradient(ITEM_INC, item, err);
							
				for (int f = 0; f < numFeatures; f++) {
					incFeatureGradient(USER_INC, user, f, err*itemFeature[f, item]);
					incFeatureGradient(ITEM_INC, item, f, err*userFeature[f, user]);
				}
			}
		}
		
		public static void regularizeGradient() 
		{
			for (int u = 0; u <= MAX_USER_ID; u++) {
				incBiasGradient(USER_INC, u, userBias[u] * regUser * regBias);
				for (int f = 0; f < numFeatures; f++) {
					incFeatureGradient(USER_INC, u, f, userFeature[f,u] * regUser);
				}
			}
			
			for (int i = 0; i <= MAX_ITEM_ID; i++) {
				incBiasGradient(ITEM_INC, i, itemBias[i] * regItem * regBias);
				for (int f = 0; f < numFeatures; f++) {
					incFeatureGradient(ITEM_INC, i, f, itemFeature[f,i] * regItem);
				}
			}			
		}
		
		public static void socialRelGradientLearn()
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
					incBiasGradient(USER_INC, u, regSocial * (userBias[u] - sumUserUAssociationBias/numUAssociations));
					for (int f = 0; f < numFeatures; f++) {
						incFeatureGradient(USER_INC, u, f, regSocial * (userFeature[f,u] - sumUserUAssociationFeatures[f]/numUAssociations));
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
						incBiasGradient(USER_INC, u, regSocial * biasInc / (double)numVAssociations);
						for (int f = 0; f < numFeatures; f++) {
							double inc = -userFeature[f,v] + sumUserWAssociationFeatures[f]/(double)numVAssociations;
							incFeatureGradient(USER_INC, u, f, regSocial * inc / (double)numVAssociations);
						}
					}
				}				
			}
		}
		
		public static void stocasticGradientDescent()
		{
			for (int u = 0; u <= MAX_USER_ID; u++) {
				userBias[u] -= lrate * userBiasGradient[u];
				for (int f = 0; f < numFeatures ; f++) {
					userFeature[f,u] -= lrate * userFeatureGradient[f,u];
				}
			}
			
			for (int i = 0; i <= MAX_ITEM_ID; i++) {
				itemBias[i] -= lrate * itemBiasGradient[i];
				for (int f = 0; f < numFeatures; f++) {
					itemFeature[f, i] -= lrate * itemFeatureGradient[f, i];
				}
			}
		}		
		
		public static void socialMF()
		{		
			double rmse;
			int numTestEntries; 
			
			numTestEntries = testCheck();
			
			for (int itr = 1; itr <= numEpochs; itr++) {
				predictionErrorGradientLearn();	
				regularizeGradient();					
				socialRelGradientLearn();
				stocasticGradientDescent();	
				
				rmse = errTestSet(numTestEntries);
				Console.WriteLine("\t\t- RMSE[{0}]: {1}", itr, rmse);
			}
		}
	}
}

