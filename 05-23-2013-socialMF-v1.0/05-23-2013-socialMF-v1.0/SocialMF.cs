using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using ProtoBuf;

namespace socialMFv1
{
	class SocialMF : PredictRecall
	{			
		public SocialMF(Association associationObj, Train trainObj, Test testObj)
		{
			lrate = 0.09;
			regUser = 0.015;
			regItem = 0.015;
			regBias = 0.33;
			regSocial = 0.1;
			numEpochs = 40;
			numFeatures = 10;
			
			USER_INC = 1;
			ITEM_INC = -1;
			MAX_RATING = 5;
			MIN_RATING = 1;
			
			random = new Random();	
			
			trainUsersArray = trainObj.usersList.ToArray();
			trainItemsArray = trainObj.itemsList.ToArray();
			trainRatingsArray = trainObj.ratingsList.ToArray();
			testUsersArray = testObj.usersList.ToArray();
			testItemsArray = testObj.itemsList.ToArray();
			testRatingsArray = testObj.ratingsList.ToArray();
			numEntries = trainUsersArray.Count();
			
			userAssociations = new SparseMatrix();
			userAssociations.createSparseMatrix(associationObj.user1List.ToArray(), associationObj.user2List.ToArray());
			
			MAX_USER_ID = Math.Max(MAX_USER_ID, userAssociations.numRows-1);
			MAX_USER_ID = Math.Max(MAX_USER_ID, userAssociations.numColumns-1);
			MAX_ITEM_ID = trainItemsArray.Max();

			Console.WriteLine("\t\t- #epochs: {0}, #features: {1}, lrate: {2}, regUser: {3}, regItem: {4}, regBias: {5}, regSocial: {6}",
			                  numEpochs, numFeatures, lrate, regUser, regItem, regBias, regSocial);			    
			Console.WriteLine("\t\t- MAX_USER_ID: {0}, MAX_ITEM_ID: {1}", MAX_USER_ID, MAX_ITEM_ID);
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
				
				predictScore = globalBias + PredictRating(user, item);			
				sigScore = g(predictScore);
				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
				err = mappedPredictScore - rating;
				
				// Point1
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
			var userReverseAssociations = userAssociations.transpose();
			
			for (int u = 0; u <= MAX_USER_ID; u++) {
				double sumUserUAssociationBias = 0.0;
				double[] sumUserUAssociationFeatures = new double[numFeatures];				
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
		
		public static double errTestSet() 
		{
			double err;
			double rmse;
			double sigScore;
			double predictScore;
			double mappedPredictScore;
			int numTestEntries;
			
			rmse = 0.0;
			numTestEntries = testItemsArray.Length;	
			
			for (int i = 0; i < numTestEntries; i++) {
				int user = testUsersArray[i];
				int item = testItemsArray[i];
				double rating = testRatingsArray[i];
				predictScore = PredictRating(user, item);
				sigScore = g(predictScore);
				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
				err = mappedPredictScore - rating;
				
				rmse += err * err;				
			}
			rmse = Math.Sqrt(rmse/numTestEntries);			
			return rmse;
		}
		
		public static void socialMF()
		{		
			double rmse;
			for (int itr = 1; itr <= numEpochs; itr++) {
				predictionErrorGradientLearn();	
				regularizeGradient();					
				socialRelGradientLearn();
				stocasticGradientDescent();	
				
				rmse = errTestSet();
				Console.WriteLine("\t\t- {0}, RMSE: {1}", itr, rmse);
			}
		}
		
		public static void Main (string[] args)
		{	
			Test testObj;
			Train trainObj;
			Association associationObj;		
			
			Stopwatch loadTime = new Stopwatch();
			Stopwatch trainTime = new Stopwatch();
			
			loadTime.Start();
			writeToConsole("Loading train.bin");
			using (FileStream file = File.OpenRead("train.bin"))
			{
				trainObj = Serializer.Deserialize<Train>(file);
			}
			
			writeToConsole("Loading test.bin");
			using (FileStream file = File.OpenRead("test.bin"))
			{
				testObj = Serializer.Deserialize<Test>(file);
			}			
			
			writeToConsole("Loading trust.bin");
			using (FileStream file = File.OpenRead("trust.bin"))
			{
				associationObj = Serializer.Deserialize<Association>(file);
			}
			loadTime.Stop();
			
			writeToConsole("Constructor call");
			new SocialMF(associationObj, trainObj, testObj);
			GC.Collect();
			
			writeToConsole("Initialize features");
			init();
			
			trainTime.Start();
			socialMF();
			trainTime.Stop();
			
			Console.WriteLine("\t- Time(load): {0}, Time(train+test): {1}", 
					loadTime.Elapsed, trainTime.Elapsed);
			
			writeToConsole("Done!");
		}
	}
}