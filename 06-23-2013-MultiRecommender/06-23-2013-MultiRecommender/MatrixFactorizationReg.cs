using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace MultiRecommender
{
	public class MatrixFactorizationReg : Eval
	{
		public MatrixFactorizationReg ()
		{
			lrate = 0.005;
			regUser = 0.0015;
			regItem = 0.0015;
			regGlbAvg = 0.01;

			csvFileName = "MatrixFactorizationReg.csv";				
			csvHeadLine = new string[]{"#itr", "#feature", "lrate", "regUser", "regItem",
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

				predictScore = globalAvg + dotProduct(user, item);	
				sigScore = g(predictScore);
				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
				err = mappedPredictScore - rating;
				err = err * sigScore * (1 - sigScore) * (MAX_RATING - MIN_RATING);

				globalAvg = globalAvg - lrate*err  - regGlbAvg*globalAvg;

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
				for (int f = 0; f < numFeatures; f++) {
					decFeature(USER, u, f, lrate * userFeature[f,u] * regUser);
				}
			}

			for (int i = 0; i <= MAX_ITEM_ID; i++) {
				for (int f = 0; f < numFeatures; f++) {
					decFeature(ITEM, i, f, lrate * itemFeature[f,i] * regItem);
				}
			}			
		}


		public void matrixFactorizationRegTrain()
		{		
			int numTestEntries;

			double rmse_R;
			double rmse_T;									
			numTestEntries = testCheck();

			for (int itr = 1; itr <= numEpochs; itr++) {
				regularization();					
				rmse_R = predictionErrorLearn();	

				rmse_T = errNoBiasTestSet(numTestEntries);
				Console.WriteLine("\t\t- {0}: RMSE_R: {1}, RMSE_T: {2}", itr, rmse_R, rmse_T);
				string[] rowData = new string[]{itr.ToString(), 
					numFeatures.ToString(),
					lrate.ToString(),	
					regUser.ToString(),
					regItem.ToString(),
					regGlbAvg.ToString(),
					rmse_R.ToString(),
					rmse_T.ToString()};
				writeToLog(rowData);
			}								
		}

	}
}

