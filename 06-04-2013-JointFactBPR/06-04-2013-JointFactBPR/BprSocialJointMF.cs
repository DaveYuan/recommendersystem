using System;
using System.Linq;
using System.Collections.Generic;

namespace JointFactBPR
{
	public class BprSocialJointMF : BiasLearnMF, ISocial
	{
		public double regPostv;
		public double regNegtv;
		public double regSocial {get; set;}
		public SparseMatrix userAssociations {get; set;}
		public List<HashSet<int>> userSymmetricAssociations;
				
		public BprSocialJointMF(Association associationObj)
		{			
			regSocial = 10;
			regPostv = 0.0025;
			regNegtv = 0.00025;
			Console.WriteLine(", regSocial: {0}, regPostv: {1}, regNegtv: {2}", regSocial, regPostv, regNegtv);			
			
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
		
		public int drawNegtvUser(int userId) 
		{
			int userIdNegtv = random.Next(0, MAX_USER_ID+1);
			while (userSymmetricAssociations[userId].Contains(userIdNegtv) || userId == userIdNegtv) {
				userIdNegtv = random.Next(0, MAX_USER_ID+1);
			}
			return userIdNegtv;
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
			
			decBias(USER_DEC, user, lrate * (err));
			decBias(ITEM_DEC, item, lrate * (err));
							
			for (int f = 0; f < numFeatures; f++) {
				double uF = userFeature[f, user];
				double iF = itemFeature[f, item];
				decFeature(USER_DEC, user, f, lrate * (err * iF));
				decFeature(ITEM_DEC, item, f, lrate * (err * uF));						
			}							
		}					
				
		public void updateSocialFeatures(int userId, int userIdPostv, int userIdNegtv)
		{
			double xuPostv;
			double xuNegtv;
			double xuPostvNegtv;
			double dervxuPostvNegtv;
			double oneByOnePlusExpX;

			xuPostv = predictRatingSocialRel(userId, userIdPostv);
			xuNegtv = predictRatingSocialRel(userId, userIdNegtv);
			xuPostvNegtv = xuPostv - xuNegtv;
			oneByOnePlusExpX = 1.0 / (1.0 + Math.Exp(xuPostvNegtv));
		
			userBias[userId] += (double) (lrate * (oneByOnePlusExpX - regBias * userBias[userId]));
			userBias[userIdPostv] += (double) (lrate * (oneByOnePlusExpX - regBias * userBias[userIdPostv]));
			userBias[userIdNegtv] += (double) (lrate * (-oneByOnePlusExpX - regBias * userBias[userIdNegtv]));

			for (int f = 0; f < numFeatures; f++) {
				double userF = userFeature[f, userId];
				double userPostvF = userFeature[f, userIdPostv];
				double userNegtvF = userFeature[f, userIdNegtv];

				dervxuPostvNegtv = userPostvF - userNegtvF;
				userFeature[f, userId] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
									  regUser * userF));
				
				dervxuPostvNegtv = userF;
				userFeature[f, userIdPostv] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
										    regPostv * userPostvF));
				
				dervxuPostvNegtv = -userF;
				itemFeature[f, userIdNegtv] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
											    regNegtv * userNegtvF));
			}
		}
				
		public void socialRelLearn()
		{	
			int userId = drawUser();
			int userIdPostv = drawPostvUser(userId);
			int userIdNegtv = drawNegtvUser(userId);
			
			updateSocialFeatures(userId, userIdPostv, userIdNegtv);			
		}
				
		public void bprSocialJointMFTrain()
		{		
			double rmse;
			int numTestEntries; 
			
			numTestEntries = testCheck();
			
			for (int itr = 1; itr <= numEpochs; itr++) {
				for (int n = 0; n < numEntries; n++) {
					predictionErrorLearn(n);	
					socialRelLearn();
				}
				regularization();					
				
				rmse = errTestSet(numTestEntries);
				Console.WriteLine("\t\t- RMSE[{0}]: {1}", itr, rmse);
			}
		}
	}
}
