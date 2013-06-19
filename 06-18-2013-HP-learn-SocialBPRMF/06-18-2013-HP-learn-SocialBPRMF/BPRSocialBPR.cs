using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace HPlearnSocialBPRMF
{
	public class BPRSocialBPR : ItemAssociation, ISocial
	{	
		public double regPostv;
		public double regNegtv;
		public double regSocial {get; set;}
		public SparseMatrix userAssociations {get; set;}
		public List<HashSet<int>> userSymmetricAssociations;
		public string[] csvHeadLine;
		public string csvFileName;					
		
		public BPRSocialBPR(Association associationObj)
		{	
			csvFileName = "06-05-2013-JointFactBPR.csv";				
			csvHeadLine = new string[]{"#itr", "#feature", "lrate", "regSocial", "regBias", "regPostv", "regNegtv",
			"regUser", "regItem", "RMSE"};
		
			lrate = 0.01;
			regUser = 0.015;
			regItem = 0.09; //0.15
			regBias = 0.99; //0.1

			regPostv = 0.015; //1
			regNegtv = 0.09; //22
			regSocial = 0.7; //0.1

			Console.WriteLine(", regSocial: {0}, regPostv: {1}, regNegtv: {2}", regSocial, regPostv, regNegtv);		
			userAssociations = new SparseMatrix();
			userAssociations.createSparseMatrix(associationObj.user1List.ToArray(), associationObj.user2List.ToArray());	
			userSymmetricAssociations = userAssociations.symmetricMatrix();
		}
		
		public void writeToLog(string[] rowData)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Join(",", rowData));
			File.AppendAllText(csvFileName, builder.ToString());
		}		
		
		public int drawSocialUser() 
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
		
		public int drawTargetUser()
		{
			int userId;
			
			while(true) {
				// TODO: check if MAX_USER_ID+1
				userId = random.Next(0, MAX_USER_ID+1);
				if (!trainRatedItems.ContainsKey(userId)) {
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
		
		public int drawPostvItem(int userId)
		{
			int numRatedItems = trainRatedItems[userId].Length;
			return trainRatedItems[userId][random.Next(0, numRatedItems)];
		}
		
		public int drawNegtvUser(int userId) 
		{
			int userIdNegtv = random.Next(0, MAX_USER_ID+1);
			while (userSymmetricAssociations[userId].Contains(userIdNegtv) || userId == userIdNegtv || 
					userSymmetricAssociations[userIdNegtv].Count == 0) {
				userIdNegtv = random.Next(0, MAX_USER_ID+1);
			}
			return userIdNegtv;
		}
		
		public int drawNegtvItem(int userId)
		{
			int itemIdNegtv = random.Next(0, MAX_ITEM_ID+1);
					
			while (trainRatedItems[userId].Contains(itemIdNegtv)) {
				itemIdNegtv = random.Next(0, MAX_ITEM_ID+1);
			}
			return itemIdNegtv;
		}
		
		public double updateFeatures(int userId, int itemIdPostv, int itemIdNegtv)
		{
			double xuPostv;
			double xuNegtv;
			double xuPostvNegtv;
			double dervxuPostvNegtv;
			double oneByOnePlusExpX;

			xuPostv = userBias[userId] + itemBias[itemIdPostv] +  dotProduct(userId, itemIdPostv);
			xuNegtv = userBias[userId] + itemBias[itemIdNegtv] + dotProduct(userId, itemIdNegtv);
			xuPostvNegtv = xuPostv - xuNegtv;
			oneByOnePlusExpX = 1.0 / (1.0 + Math.Exp(xuPostvNegtv));
		
			userBias[userId] += (double) (lrate * (oneByOnePlusExpX - regBias * userBias[userId]));
			itemBias[itemIdPostv] += (double) (lrate * (oneByOnePlusExpX - regBias * itemBias[itemIdPostv]));
			itemBias[itemIdNegtv] += (double) (lrate * (-oneByOnePlusExpX - regBias * itemBias[itemIdNegtv]));

			for (int f = 0; f < numFeatures; f++) {
				double userF = userFeature[f, userId];
				double itemPostvF = itemFeature[f, itemIdPostv];
				double itemNegtvF = itemFeature[f, itemIdNegtv];

				dervxuPostvNegtv = itemPostvF - itemNegtvF;
				userFeature[f, userId] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
									  regUser * userF));
				
				dervxuPostvNegtv = userF;
				itemFeature[f, itemIdPostv] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
										    regPostv * itemPostvF));
				
				dervxuPostvNegtv = -userF;
				itemFeature[f, itemIdNegtv] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
											    regNegtv * itemNegtvF));
			}
			return xuPostvNegtv;
		}	
		
		public double updateSocialFeatures(int userId, int userIdPostv, int userIdNegtv)
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
		
			userBias[userIdPostv] += (double) (lrate * regSocial * (oneByOnePlusExpX - regBias * regPostv * userBias[userIdPostv]));
			userBias[userIdNegtv] += (double) (lrate * regSocial * (-oneByOnePlusExpX - regBias * regNegtv * userBias[userIdNegtv]));

			for (int f = 0; f < numFeatures; f++) {
				double userF = userFeature[f, userId];
				double userPostvF = userFeature[f, userIdPostv];
				double userNegtvF = userFeature[f, userIdNegtv];

				dervxuPostvNegtv = userPostvF - userNegtvF;
				userFeature[f, userId] += (double) (lrate * regSocial * (oneByOnePlusExpX * dervxuPostvNegtv -
									  regUser * userF));
				
				dervxuPostvNegtv = userF;
				userFeature[f, userIdPostv] += (double) (lrate * regSocial * (oneByOnePlusExpX * dervxuPostvNegtv -
										    regPostv * userPostvF));
				
				dervxuPostvNegtv = -userF;
				userFeature[f, userIdNegtv] += (double) (lrate * regSocial * (oneByOnePlusExpX * dervxuPostvNegtv -
											    regNegtv * userNegtvF));
			}
			return xuPostvNegtv;
		}		
		
		public double bprTargetTrain()
		{
			int userId = drawTargetUser();
			int itemIdPostv = drawPostvItem(userId);
			int itemIdNegtv = drawNegtvItem(userId);
					
			return updateFeatures(userId, itemIdPostv, itemIdNegtv);
		}
		
		public double bprSocialTrain()
		{	
			int userId = drawSocialUser();
			int userIdPostv = drawPostvUser(userId);
			int userIdNegtv = drawNegtvUser(userId);
			
			return updateSocialFeatures(userId, userIdPostv, userIdNegtv);			
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
			regGlbAvg = regGlbAvg - lrate*dervWRTregGlbAvg;
			
			double dervWRTregItem = -lrate*err*(itemBias[item] + productSumFeatures(ITEM, user, item, err));
			regItem = regItem - lrate*dervWRTregItem;
			
			double dervWRTregUser = -lrate*err*(userBias[user] + productSumFeatures(USER, user, item, err));
			regUser = regUser - lrate*dervWRTregUser;
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
				
		public void BPRSocialBPRTrain()
		{
			double bprOptTarget;
			double bprOptSocial;			
			
			trainItemsRatedByUser();
			int numTestEntries = testCheck();
			numEpochs = 500;
			for (int itr = 1; itr <= numEpochs; itr++) {
				bprOptTarget = bprOptSocial = 0.0;
				for (int n = 0; n < numTrainEntries; n++) {					
					bprOptSocial += Math.Log(g(bprSocialTrain()));
					bprOptTarget += Math.Log(g(bprTargetTrain()));
				}
				double rmse = errTestSet(numTestEntries);				
				adaptRegularization();
				Console.WriteLine("\t\t- RMSE[{0}]: {1}, OPT(trgt): {2}, OPT(social): {3}", 
				                  itr, rmse, bprOptTarget, bprOptSocial);
			}
		}		
	}
}
