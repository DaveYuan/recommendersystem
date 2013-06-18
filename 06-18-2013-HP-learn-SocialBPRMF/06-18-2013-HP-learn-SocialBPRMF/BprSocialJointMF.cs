using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace HPlearnSocialBPRMF
{
	public class BprSocialJointMF : BiasLearnMF, ISocial
	{
		public double regPostv;
		public double regNegtv;
		public double regSocial {get; set;}
		public SparseMatrix userAssociations {get; set;}
		public List<HashSet<int>> userSymmetricAssociations;
		public string[] csvHeadLine;
		public string csvFileName;						
				
		public BprSocialJointMF(Association associationObj)
		{	
			csvFileName = "06-05-2013-JointFactBPR.csv";				
			csvHeadLine = new string[]{"#itr", "#feature", "lrate", "regSocial", "regBias", "regPostv", "regNegtv",
			"regUser", "regItem", "RMSE"};
		
			regUser = 0.0015;
			regItem = 0.001; //0.15
			regBias = 0.1; //0.1

			regPostv = 0.0015; //1
			regNegtv = 0.001; //22
			regSocial = 1; //0.1

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
			while (userSymmetricAssociations[userId].Contains(userIdNegtv) || userId == userIdNegtv || 
					userSymmetricAssociations[userIdNegtv].Count == 0) {
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
			
			globalAvg = globalAvg - lrate*err - regGlbAvg*globalAvg;
			decBias(USER_DEC, user, lrate * (err));
			decBias(ITEM_DEC, item, lrate * (err));
							
			for (int f = 0; f < numFeatures; f++) {
				double uF = userFeature[f, user];
				double iF = itemFeature[f, item];
				decFeature(USER_DEC, user, f, lrate * (err * iF));
				decFeature(ITEM_DEC, item, f, lrate * (err * uF));						
			}							
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
				
		public double socialRelLearn()
		{	
			int userId = drawUser();
			int userIdPostv = drawPostvUser(userId);
			int userIdNegtv = drawNegtvUser(userId);
			
			return updateSocialFeatures(userId, userIdPostv, userIdNegtv);			
		}
		
		public void writeToLog(string[] rowData)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Join(",", rowData));
			File.AppendAllText(csvFileName, builder.ToString());
		}
		
		public double sumFeatures(int type, int indx)
		{
			double sum = 0.0;
			if (type == USER_DEC) {
				for (int f = 0; f < numFeatures; f++) {
					sum += userFeature[f, indx];
				}
			} else if (type == ITEM_DEC) {
				for (int f = 0; f < numFeatures; f++) {
					sum += itemFeature[f, indx];
				}
			}
			return sum;
		}
		
		public void updateReg(int user, int item, double err)
		{
			double dervWRTregGlbAvg = -lrate * err * globalAvg;
			regGlbAvg = regGlbAvg - lrate*dervWRTregGlbAvg;
			
			double dervWRTregItem = -lrate*err*(itemBias[item] + sumFeatures(ITEM_DEC, item));
			regItem = regItem - lrate*dervWRTregItem;
			
			double dervWRTregUser = -lrate*err*(userBias[user] + sumFeatures(USER_DEC, user));
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
			//	err = predictT - rating;
				sigScore = g(predictT);
				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
				err = mappedPredictScore - rating;
				err = err * sigScore * (1 - sigScore) * (MAX_RATING - MIN_RATING);
				
				predictT1 = predictAtT1(user, item, err);
				sigScore = g(predictT1);
				mappedPredictScore = MIN_RATING + sigScore * (MAX_RATING - MIN_RATING);
				errT1 = mappedPredictScore - rating;
				errT1 = errT1 * sigScore * (1 - sigScore) * (MAX_RATING - MIN_RATING);
//				errT1 = predictT1 - rating;
				updateReg(user, item, errT1);				
			}
		}
		
		public void bprSocialJointMFTrain()
		{		
			double rmse;
			double bprOpt;
			int numTestEntries; 
			
			numTestEntries = testCheck();
			
			lrate = 0.001;
			for (int itr = 1; itr <= numEpochs; itr++) {
				regularization();
				bprOpt = 0.0;							
				for (int n = 0; n < numTrainEntries; n++) {
					predictionErrorLearn(n);	
					bprOpt += Math.Log(g(socialRelLearn()));
				}
				rmse = errTestSet(numTestEntries);
				adaptRegularization();
				Console.WriteLine("\t\t- RMSE[{0}]: {1}, {2}", itr, rmse, bprOpt);
				string[] rowData = new string[]{itr.ToString(), 
								numFeatures.ToString(),
								lrate.ToString(),
								regSocial.ToString(),
								regBias.ToString(),
								regPostv.ToString(),
								regNegtv.ToString(),
								regUser.ToString(),
								regItem.ToString(),
								rmse.ToString()};
				writeToLog(rowData);
			}				
		}

		public void hyperParameterSearch() 
		{ 
			double LRATE_MIN = 0.001;
			double LRATE_MAX = 0.05;
			double LRATE_INC = 0.005;

			double REG_SOCIAL_MIN = 1;
			double REG_SOCIAL_MAX = 15;
			double REG_SOCIAL_INC = 5;

			double REG_BIAS_MIN = 0.01;
			double REG_BIAS_MAX = 0.30;
			double REG_BIAS_INC = 0.05;

			double REG_POSTV_MIN = 0.001;
			double REG_POSTV_MAX = 0.06;
			double REG_POSTV_INC = 0.005;

			double REG_NEGTV_MIN = 0.0001;
			double REG_NEGTV_MAX = 0.006;
			double REG_NEGTV_INC = 0.0005;

			double REG_USER_MIN = 0.001;
			double REG_USER_MAX = 0.06;
			double REG_USER_INC = 0.005;

			double REG_ITEM_MIN = 0.001;
			double REG_ITEM_MAX = 0.06;
			double REG_ITEM_INC = 0.005;

			numEpochs = 50;
			File.Open(csvFileName, FileMode.Create).Close();
			writeToLog(csvHeadLine);
			for (double lr = LRATE_MIN; lr <= LRATE_MAX; lr=lr+LRATE_INC) {
				lrate = lr;
				for (double rs = REG_SOCIAL_MIN; rs <= REG_SOCIAL_MAX; rs=rs+REG_SOCIAL_INC) {
					regSocial = rs;
					for (double rb = REG_BIAS_MIN; rb <= REG_BIAS_MAX; rb=rb+REG_BIAS_INC) {
						regBias = rb;
						for (double rp = REG_POSTV_MIN; rp <= REG_POSTV_MAX; rp=rp+REG_POSTV_INC) {
							regPostv = rp;
							for (double rn = REG_NEGTV_MIN; rn <= REG_NEGTV_MAX; rn=rn+REG_NEGTV_INC) {
								regNegtv = rn;
								for (double ru = REG_USER_MIN; ru <= REG_USER_MAX; ru=ru+REG_USER_INC) {
									regUser = ru;
									for (double ri = REG_ITEM_MIN; ri <= REG_ITEM_MAX; ri=ri+REG_ITEM_INC) {
										regItem = ri;
										bprSocialJointMFTrain();
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
