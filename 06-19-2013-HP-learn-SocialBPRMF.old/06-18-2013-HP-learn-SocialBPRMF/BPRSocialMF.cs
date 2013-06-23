using System;
using System.Linq;
using System.Collections.Generic;

namespace HPlearnSocialBPRMF
{
	public class BPRSocialMF : ItemAssociation, ISocial
	{
		public double regPostv;
		public double regNegtv;
		public double regSocial {get; set;}
		public SparseMatrix userAssociations {get; set;}
		public List<HashSet<int>> userSymmetricAssociations;
		public string[] csvHeadLine;
		public string csvFileName;					
		
		public BPRSocialMF(Association associationObj)
		{	
			csvFileName = "06-05-2013-JointFactBPR.csv";				
			csvHeadLine = new string[]{"#itr", "#feature", "lrate", "regSocial", "regBias", "regPostv", "regNegtv",
			"regUser", "regItem", "RMSE"};
		
			lrate = 0.01;
			regUser = 0.015;
			regItem = 0.09; //0.15
			regBias = 0.99; //0.1

			regPostv = 0.0005; //1
			regNegtv = 0.0009; //22
			regSocial = 1; //0.1

			Console.WriteLine(", regSocial: {0}, regPostv: {1}, regNegtv: {2}", regSocial, regPostv, regNegtv);		
			userAssociations = new SparseMatrix();
			userAssociations.createSparseMatrix(associationObj.user1List.ToArray(), associationObj.user2List.ToArray());	
			userSymmetricAssociations = userAssociations.symmetricMatrix();
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
		
		public int drawPostvItem(int userId)
		{
			int numRatedItems = trainRatedItems[userId].Length;
			return trainRatedItems[userId][random.Next(0, numRatedItems)];
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
		
		public double bprTargetTrain()
		{
			int userId = drawTargetUser();
			int itemIdPostv = drawPostvItem(userId);
			int itemIdNegtv = drawNegtvItem(userId);
					
			return updateFeatures(userId, itemIdPostv, itemIdNegtv);
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
		
		public void BPRSocialMFTrain() 
		{
			double rmse;
			double bprOptTarget;
		
			trainItemsRatedByUser();
			int numTestEntries = testCheck();
			numEpochs = 500;
			
			for (int itr = 1; itr <= numEpochs; itr++) {
				//predictionErrorLearn();	
				//regularization();					
				bprOptTarget = 0.0;
				socialRelLearn();
				for (int n = 0; n < numTrainEntries; n++) {	
					bprOptTarget += Math.Log(g(bprTargetTrain()));
				}
				adaptRegularization();
				rmse = errTestSet(numTestEntries);
				Console.WriteLine("\t\t- RMSE[{0}]: {1}, OPT(target): {2}", itr, rmse, bprOptTarget);
			}
		}
		
	}
}

