using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;
using MultiRecommender.Evaluation;

namespace MultiRecommender.RatingPredictor
{
	public class MatrixFactorizationBias : Eval
	{
		public MatrixFactorizationBias()
		{
			lrate = 0.05;
			csvFileName = "../../../../log/MatrixFactorizationBias.csv";				
			csvHeadLine = new string[]{"#itr", "#feature", "lrate", "RMSE(R)", "RMSE(Test)"};

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

				//				err = predictScore - rating;
				globalAvg = globalAvg - lrate*err;
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

		public void matrixFactorizationBiasTrain()
		{		
			int numTestEntries;

			double rmse_R;
			double rmse_T;									
			numTestEntries = testCheck();

			for (int itr = 1; itr <= numEpochs; itr++) {
				rmse_R = predictionErrorLearn();	
				rmse_T = errTestSet(numTestEntries);
				Console.WriteLine("\t\t- {0}: RMSE_R: {1}, RMSE_T: {2}", itr, rmse_R, rmse_T);
				string[] rowData = new string[]{itr.ToString(), 
					numFeatures.ToString(),
					lrate.ToString(),															
					rmse_R.ToString(),
					rmse_T.ToString()};
				writeToLog(rowData);
			}								
		}	
	}
}