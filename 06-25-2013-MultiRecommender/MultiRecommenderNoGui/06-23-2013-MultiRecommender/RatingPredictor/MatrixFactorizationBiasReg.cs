using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;
using MultiRecommender;
using MultiRecommender.Evaluation;

namespace MultiRecommender.RatingPredictor
{
	public class MatrixFactorizationBiasReg : Eval
	{		
		double lrateRegConst;
		public MatrixFactorizationBiasReg()
		{
			lrate = 0.005;
			regUser = 0.005;
			regItem = 0.005;
			regBias = 0.1;
			regGlbAvg = 0.01;
			lrateRegConst = 0.0001;	

			csvFileName = "../../../../log/MatrixFactorizationBiasReg.csv";				
			csvHeadLine = new string[]{"#itr", "#feature", "lrate", "regUser", "regItem", "regBias",
				"regGlbAvg", "RMSE(R)", "RMSE(Test)"};		
			File.Open(csvFileName, FileMode.Create).Close();
			writeToLog(csvHeadLine);		
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

		public void updateReg(int user, int item, double err)
		{			
			double dervWRTregGlbAvg = -lrate * err * globalAvg;
			regGlbAvg = regGlbAvg - lrateRegConst *dervWRTregGlbAvg;

			double dervWRTregItem = -lrate*err*(itemBias[item] + productSumFeatures(ITEM, user, item, err));
			regItem = regItem - lrateRegConst*dervWRTregItem;

			double dervWRTregUser = -lrate*err*(userBias[user] + productSumFeatures(USER, user, item, err));
			regUser = regUser - lrateRegConst*dervWRTregUser;					
		}		

		public void adaptRegularization()
		{
			double err;
			double errT1;
			double predictT;
			double predictT1;

			for (int i = 0; i < numValidationEntries; i++) {
				int user = validatationUsersArray[i];
				int item = validationItemsArray[i];
				double rating = validationRatingsArray[i];
				predictT = PredictRating(user, item);
				err = predictT - rating;

				predictT1 = predictAtT1(user, item, err);
				errT1 = predictT1 - rating;
				updateReg(user, item, errT1);				
			}
		}

		public void matrixFactorizationBiasRegTrain()
		{		
			int numTestEntries;

			double rmse_R;
			double rmse_T;									
			numTestEntries = testCheck();

			for (int itr = 1; itr <= numEpochs; itr++) {
				regularization();					
				rmse_R = predictionErrorLearn();					
				rmse_T = errTestSet(numTestEntries);
				//adaptRegularization();

				Console.WriteLine("\t\t- {0}: RMSE_R: {1}, RMSE_T: {2}", itr, rmse_R, rmse_T);
				string[] rowData = new string[]{itr.ToString(), 
					numFeatures.ToString(),
					lrate.ToString(),	
					regUser.ToString(),
					regItem.ToString(),
					regBias.ToString(),
					regGlbAvg.ToString(),
					rmse_R.ToString(),
					rmse_T.ToString()};
				writeToLog(rowData);
			}								
		}				
	}
}
