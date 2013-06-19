using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace HPlearnSocialBPRMF
{
	public class SqErrSocialMF : BiasLearnMF, ISocial
	{
		public double regSocial {get; set;}
		public SparseMatrix userAssociations {get; set;}
		public List<HashSet<int>> userSymmetricAssociations;
		
		public SqErrSocialMF(Association associationObj)
		{
			regUser = 0.5;
			regItem = 0.9;
			regBias = 1;
			regSocial = 0.007;

			Console.WriteLine(", regSocial: {0}", regSocial);		
			userAssociations = new SparseMatrix();
			userAssociations.createSparseMatrix(associationObj.user1List.ToArray(), associationObj.user2List.ToArray());	
			userSymmetricAssociations = userAssociations.symmetricMatrix();
		}
		
		public int drawUser() 
		{
			int userId;
			while(true) {
				// TODO: check of MAX_USER_ID+1/MAX_USER_ID
				userId = random.Next(0, MAX_USER_ID+1);
				if (userSymmetricAssociations[userId].Count == 0) {
					continue;
				}
				return userId;
			}
		}		
		
		public int drawPostvUser(int userId)
		{
			int numAssociations = userSymmetricAssociations[userId].Count;
			return userSymmetricAssociations[userId].ElementAt(random.Next(0, numAssociations));			
		}	
		
		public double updateSocialFeatures(int userId, int userIdPostv)
		{
			double err;
			double sigScore;
			double predictScore;
			
			predictScore = predictRatingSocialRel(userId, userIdPostv);
			sigScore = g(predictScore);
			err = sigScore - 1;
			err = err * sigScore * (1 - sigScore);			
		
		//	decBias(USER, userId, lrate * regSocial * (err + regBias * regUser * userBias[userId]));
			decBias(USER, userIdPostv, lrate * regSocial * (err + regBias * regUser * userBias[userIdPostv]));
							
			for (int f = 0; f < numFeatures; f++) {
				double uF = userFeature[f, userId];
				double iF = userFeature[f, userIdPostv];
				decFeature(USER, userId, f, lrate * regSocial * (err * iF + regUser * userFeature[f,userId]));
				decFeature(USER, userIdPostv, f, lrate * regSocial * (err * uF + regUser * userFeature[f, userIdPostv]));						
			}	
			return err;
		}
		
		public double socialRelLearn()
		{	
			int userId = drawUser();
			int userIdPostv = drawPostvUser(userId);
			
			return updateSocialFeatures(userId, userIdPostv);			
		}
		
		public void predictionErrorLearn(int n)
		{			
			double err;
			double sigScore;
			double predictScore;
			double mappedPredictScore;				
			
			int user = trainUsersArray[n];
			int item = trainItemsArray[n];
			int rating = trainRatingsArray[n];	
			
			predictScore = PredictRating(user, item);	
			sigScore = g(predictScore);
			mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
			err = mappedPredictScore - rating;
			err = err * sigScore * (1 - sigScore) * (MAX_RATING - MIN_RATING);
			
			decBias(USER, user, lrate * (err));
			decBias(ITEM, item, lrate * (err));
							
			for (int f = 0; f < numFeatures; f++) {
				double uF = userFeature[f, user];
				double iF = itemFeature[f, item];
				decFeature(USER, user, f, lrate * (err * iF));
				decFeature(ITEM, item, f, lrate * (err * uF));						
			}							
		}	
		
		public void sqErrSocialMFTrain()
		{
			double rmse;
			double rmseSocialRel;
			int numTestEntries; 
			
			numTestEntries = testCheck();
			lrate = 0.7;
			for (int itr = 1; itr <= numEpochs; itr++) {
				regularization();
				rmseSocialRel = 0.0;				
				for (int n = 0; n < numTrainEntries; n++) {
					lrate = 0.004;		
					predictionErrorLearn(n);	
					rmseSocialRel += socialRelLearn();
				}
				rmse = errTestSet(numTestEntries);
//				Console.WriteLine("\t\t- RMSE[{0}]: {1}", itr, rmse);
				rmseSocialRel = rmseSocialRel * rmseSocialRel;
				Console.WriteLine("\t\t- RMSE[{0}]: {1}, {2}", itr, rmse, Math.Sqrt(rmseSocialRel/numTrainEntries));
			}
		}
	}
}
