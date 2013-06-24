using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace MultiRecommender
{
	public class SocialMF : MatrixFactorizationBiasReg, ISocial
	{
		public double lrateRegConst;	
		public double regSocial {get; set;}
		public SparseMatrix userAssociations {get; set;}
		
		public SocialMF(Association associationObj)
		{		
			lrate = 0.009;
            regUser = 0.0015;
            regItem = 0.0015;
            regBias = 0.01;
            regGlbAvg = 0.01;	
			lrateRegConst = 0.0001;	
			regSocial = 5;
			userAssociations = new SparseMatrix();
			userAssociations.createSparseMatrix(associationObj.user1List.ToArray(), associationObj.user2List.ToArray());								
							
			csvFileName = "SocialMF.csv";				
			csvHeadLine = new string[]{"#itr", "#feature", "lrate", "regUser", "regItem", "regBias", 
										"regGlbAvg", "regSocial", "RMSE(R)", "RMSE(Test)"};		
			File.Open(csvFileName, FileMode.Create).Close();
			writeToLog(csvHeadLine);						
		}		
		
		public void socialRelLearn()
		{
			int MAX_USER_ID_ASSOCIATION_DATA = userAssociations.sparseMatrix.Count - 1;
			// TODO: Can do it only once in INIT
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
					decBias(USER, u, lrate *(regSocial * (userBias[u] - sumUserUAssociationBias/numUAssociations)));
					for (int f = 0; f < numFeatures; f++) {
						decFeature(USER, u, f, lrate * (regSocial * (userFeature[f,u] - sumUserUAssociationFeatures[f]/numUAssociations)));
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
						decBias(USER, u, lrate * (regSocial * biasInc / (double)numVAssociations));
						for (int f = 0; f < numFeatures; f++) {
							double dec = -userFeature[f,v] + sumUserWAssociationFeatures[f]/(double)numVAssociations;
							decFeature(USER, u, f, lrate * (regSocial * dec / (double)numVAssociations));
						}
					}
				}				
			}
		}	

		public double productSumFeatures(int type, int user, int item, double err)
		{
			double sum = 0.0;
			if (type == USER) {
				for (int f = 0; f < numFeatures; f++) {
					double itemFeatureT1 = itemFeature[f, item] - lrate*(err*userFeature[f, user] + regItem*itemFeature[f, item]);
					sum += (userFeature[f, user] * itemFeatureT1);
				}
			} else if (type == ITEM) {
				for (int f = 0; f < numFeatures; f++) {
					double userFeatureT1 = userFeature[f, user] - lrate*(err*itemFeature[f, item] + regUser*userFeature[f, user]);
					sum += (itemFeature[f, item] * userFeatureT1);
				}
			}
			return sum;
		}

		public double productSumLearnRate(int user, int item, double err)
		{
			double sum = 0.0;		
			for (int f = 0; f < numFeatures; f++) {
				double itemFeatureT1 = itemFeature[f, item] - lrate*(err*userFeature[f, user] + regItem*itemFeature[f, item]);
				double userFeatureT1 = userFeature[f, user] - lrate*(err*itemFeature[f, item] + regUser*userFeature[f, user]);
				sum += ( ( (err*userFeature[f, user] + regItem*itemFeature[f, item]) * userFeatureT1 ) +
					 ( (err*itemFeature[f, item] + regUser*userFeature[f, user]) * itemFeatureT1 ) );
			}
			return sum;
		}
		
		public void updateReg(int user, int item, double err)
		{
			double oldRegGlbAvg = regGlbAvg;
			double oldRegItem = regItem;
			double oldRegUser = regUser;

			double dervWRTregGlbAvg = -lrate * err * globalAvg;
			regGlbAvg = regGlbAvg - lrateRegConst *dervWRTregGlbAvg;
			
			double dervWRTregItem = -lrate*err*(itemBias[item] + productSumFeatures(ITEM, user, item, err));
			regItem = regItem - lrateRegConst*dervWRTregItem;
			
			double dervWRTregUser = -lrate*err*(userBias[user] + productSumFeatures(USER, user, item, err));
			regUser = regUser - lrateRegConst*dervWRTregUser;

			double dervWRTlRate = -(3*err + 
						oldRegGlbAvg * globalAvg +
						oldRegItem * itemBias[item] +
						oldRegUser * userBias[user] +
						productSumLearnRate(user, item, err));
//			Console.WriteLine("dervWRTlRate: {0}", dervWRTlRate);
//			lrate = 0.00001 * dervWRTlRate;
		}		
		
		public void adaptRegularization()
		{
			double err;
			double errT1;
			double mappedPredictScore;
			double sigScore;
			double predictT;
			double predictT1;
			
			for (int i = 0; i < numValidationEntries; i++) {
				int user = validatationUsersArray[i];
				int item = validationItemsArray[i];
				double rating = validationRatingsArray[i];
				predictT = PredictRating(user, item);
				err = predictT - rating;
//				sigScore = g(predictT);
//				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
//				err = mappedPredictScore - rating;
//				err = err * sigScore * (1 - sigScore) * (MAX_RATING - MIN_RATING);
				
				predictT1 = predictAtT1(user, item, err);
//				sigScore = g(predictT1);
//				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
//				errT1 = mappedPredictScore - rating;
//				errT1 = errT1 * sigScore * (1 - sigScore) * (MAX_RATING - MIN_RATING);
				errT1 = predictT1 - rating;
				updateReg(user, item, errT1);				
			}
		}
	
		public void socialMFTrain()
		{	
			double rmse_R;	
			double rmse_T;

			int numTestEntries; 
			
			numTestEntries = testCheck();
			
			for (int itr = 1; itr <= numEpochs; itr++) {
				rmse_R = predictionErrorLearn();	
				regularization();					
				socialRelLearn();				
				rmse_T = errTestSet(numTestEntries);
	//			adaptRegularization();
				Console.WriteLine("\t\t- {0}: RMSE_R: {1}, RMSE_T: {2}", itr, rmse_R, rmse_T);

				string[] rowData = new string[]{itr.ToString(), 
												numFeatures.ToString(),
												lrate.ToString(),	
												regUser.ToString(),
												regItem.ToString(),
												regBias.ToString(),
												regGlbAvg.ToString(),
												regSocial.ToString(),
												rmse_R.ToString(),
												rmse_T.ToString()};
				writeToLog(rowData);
			}
		}
	}
}
