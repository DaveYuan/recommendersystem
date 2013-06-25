using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using MultiRecommender.Datatype;
using MultiRecommender.IO;

namespace MultiRecommender.JointFactorization
{
	public class BprSocialJointMF : ItemAssociation, ISocial
	{
		public double lrateBPR;
		public double lrateRegConst;
		public double regPostv;
		public double regNegtv;
		public double regSocial {get; set;}
		public SparseMatrix userAssociations {get; set;}
		public List<HashSet<int>> userSymmetricAssociations;					
				
		public BprSocialJointMF(Association associationObj)
		{	
			numEpochs = 20;		
			lrateRegConst = 0.001;
			lrate = 0.009;
			regUser = 4; //0.015
			regItem = -4; //0.019
			regBias = 0.01; //0.77

			regPostv = 0.001; //0.015
			regNegtv = 0.05; //0.019
			regSocial = 0.03; //0.7
			lrateBPR = 0.02;

			userAssociations = new SparseMatrix();
			userAssociations.createSparseMatrix(associationObj.user1List.ToArray(), associationObj.user2List.ToArray());	
			userSymmetricAssociations = userAssociations.symmetricMatrix();

			csvFileName = "./log/BprSocialJointMF.csv";				
			csvHeadLine = new string[]{"#itr", "#feature", "lrate", "regSocial", "regBias", "regPostv", "regNegtv",
				"regUser", "regItem", "RMSE(R)", "BPTOPT(S)", "RMSE(T)", "Prec[5]", "Prec[10]", "Recl[5]", "Recl[10]"};
			File.Open(csvFileName, FileMode.Create).Close();
			writeToLog(csvHeadLine);
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
		
		public double predictionErrorLearn(int n)
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
			decBias(USER, user, lrate * (err));
			decBias(ITEM, item, lrate * (err));
							
			for (int f = 0; f < numFeatures; f++) {
				double uF = userFeature[f, user];
				double iF = itemFeature[f, item];
				decFeature(USER, user, f, lrate * (err * iF));
				decFeature(ITEM, item, f, lrate * (err * uF));						
			}
			return err*err;			
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
		
			userBias[userIdPostv] += (double) (lrateBPR * regSocial * (oneByOnePlusExpX - regBias * regPostv * userBias[userIdPostv]));
			userBias[userIdNegtv] += (double) (lrateBPR * regSocial * (-oneByOnePlusExpX - regBias * regNegtv * userBias[userIdNegtv]));

			for (int f = 0; f < numFeatures; f++) {
				double userF = userFeature[f, userId];
				double userPostvF = userFeature[f, userIdPostv];
				double userNegtvF = userFeature[f, userIdNegtv];

				dervxuPostvNegtv = userPostvF - userNegtvF;
				userFeature[f, userId] += (double) (lrateBPR * regSocial * (oneByOnePlusExpX * dervxuPostvNegtv -
									  regUser * userF));
				
				dervxuPostvNegtv = userF;
				userFeature[f, userIdPostv] += (double) (lrateBPR * regSocial * (oneByOnePlusExpX * dervxuPostvNegtv -
										    regPostv * userPostvF));
				
				dervxuPostvNegtv = -userF;
				userFeature[f, userIdNegtv] += (double) (lrateBPR * regSocial * (oneByOnePlusExpX * dervxuPostvNegtv -
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
//			lrate = lrate + 0.00001 * dervWRTlRate;
		}		

		public void updateSocialReg(int userId, int userIdPostv, int userIdNegtv, double optDiff)
		{
			double xuPostv;
			double xuNegtv;
			double xuPostvNegtv;
			double oneByOnePlusExpX;

			double dervWRTregUser;
			double dervWRTregPostv;
			double dervWRTregNegtv;

			xuPostv = predictRatingSocialRel(userId, userIdPostv);
			xuNegtv = predictRatingSocialRel(userId, userIdNegtv);
			xuPostvNegtv = xuPostv - xuNegtv;
			oneByOnePlusExpX = 1.0 / (1.0 + Math.Exp(xuPostvNegtv));

			dervWRTregUser = dervWRTregPostv = dervWRTregNegtv = 0.0;

			for (int f = 0; f < numFeatures; f++) {
				double userF = userFeature[f, userId];
				double userPostvF = userFeature[f, userIdPostv];
				double userNegtvF = userFeature[f, userIdNegtv];

				dervWRTregUser += (userF * (userPostvF + lrate*regSocial*(oneByOnePlusExpX*userF - regPostv*userPostvF))) +
					(userF * (userNegtvF + lrate*regSocial*(-oneByOnePlusExpX*userF - regNegtv*userNegtvF)));
				dervWRTregPostv +=
					(userF * lrate*regSocial*(oneByOnePlusExpX*(userPostvF - userNegtvF) - regUser*userF))*userPostvF;
				dervWRTregNegtv += 
					(userF * lrate*regSocial*(oneByOnePlusExpX*(userPostvF - userNegtvF) - regUser*userF))*userNegtvF;
			}
			dervWRTregUser *= optDiff * lrate * regSocial;
			regUser = regUser - lrateRegConst * dervWRTregUser;

			dervWRTregPostv *= -optDiff * lrate * regSocial;
			regPostv = regPostv - lrateRegConst * dervWRTregPostv;

			dervWRTregNegtv *= -optDiff * lrate * regSocial;
			regNegtv = regNegtv - lrateRegConst * dervWRTregNegtv;
		}

		public double optAtT1(int userId, int userIdPostv, int userIdNegtv)
		{
			double sum = 0;
			double xuPostv;
			double xuNegtv;
			double xuPostvNegtv;
			double dervxuPostvNegtv;
			double oneByOnePlusExpX;

			xuPostv = predictRatingSocialRel(userId, userIdPostv);
			xuNegtv = predictRatingSocialRel(userId, userIdNegtv);
			xuPostvNegtv = xuPostv - xuNegtv;
			oneByOnePlusExpX = 1.0 / (1.0 + Math.Exp(xuPostvNegtv));

			for (int f = 0; f < numFeatures; f++) {
				double userF = userFeature[f, userId];
				double userPostvF = userFeature[f, userIdPostv];
				double userNegtvF = userFeature[f, userIdNegtv];

				dervxuPostvNegtv = userPostvF - userNegtvF;
				double t1 = (userF + lrate * regSocial * (oneByOnePlusExpX * dervxuPostvNegtv - regUser * userF)) * 
					(userPostvF + lrate * regSocial * (oneByOnePlusExpX * userF - regPostv * userPostvF));

				double t2 = (userF + lrate * regSocial * (oneByOnePlusExpX * dervxuPostvNegtv - regUser * userF)) *
					(userNegtvF + lrate * regSocial * (-oneByOnePlusExpX * userF - regNegtv * userNegtvF));
				sum += t1 - t2;
			}
			return sum;
		}

		public void adaptRegularization()
		{
			double err;
			double errT1;
			double opt;
			double optT1;
			double predictT;
			double predictT1;
			
			for (int i = 0; i < numValidationEntries; i++) {
				int userId = validatationUsersArray[i];
				int itemId = validationItemsArray[i];
				double rating = validationRatingsArray[i];
				predictT = PredictRating(userId, itemId);
				err = predictT - rating;
				
				predictT1 = predictAtT1(userId, itemId, err);
				errT1 = predictT1 - rating;

				int userIdPostv = drawPostvUser(userId);
				int userIdNegtv = drawNegtvUser(userId);
				opt = predictRatingSocialRel(userId, userIdPostv) - predictRatingSocialRel(userId, userIdNegtv);
				optT1 = optAtT1(userId, userIdPostv, userIdNegtv);

				updateReg(userId, itemId, errT1);				
				updateSocialReg(userId, userIdPostv, userIdNegtv, optT1 - opt);
			}
		}
		
		public void bprSocialJointMFTrain()
		{		
			bool SOCIAL_FLAG = true;
			double rmse_XY;
			double rmse_X;
			double bprOpt;
			double bprOptMax;
			double bprOptDec = 0;
			int numTestEntries; 
			numTestEntries = testCheck();
		
			fetchUniqueItems();	
			trainItemsRatedByUser();
			testItemsRatedByUser();

			Dictionary<string, double> result = new Dictionary<string, double>();	
			
			bprOptMax = Double.MinValue;
			for (int itr = 1; itr <= numEpochs; itr++) {
				regularization();
				bprOpt = rmse_X = 0.0;							
				for (int n = 0; n < numTrainEntries; n++) {
					rmse_X += predictionErrorLearn(n);	
					if (SOCIAL_FLAG) {
						bprOpt += Math.Log(g(socialRelLearn()));
					}
				}
				if (SOCIAL_FLAG) {
					bprOptMax = Math.Max(bprOptMax, bprOpt);
				
					if (bprOpt < bprOptMax) {
						bprOptDec = Math.Abs((bprOptMax - bprOpt) / bprOptMax);
						if (bprOptDec * 100 > 2) {
			//				SOCIAL_FLAG = false;
						}
					}
				}
				rmse_X = Math.Sqrt(rmse_X/(double)numTrainEntries);
				rmse_XY = errTestSet(numTestEntries);
	
				string[] rowData;	
				if (itr < 0) {
					result = bprEval();
					Console.WriteLine("\t\t- {0}: RMSE_X :{1}, RMSE_XY: {2}, OPT: {3}, Prec[5]: {4}, Prec[10]: {5}, OPTDec: {6}", 
				                  itr, rmse_X, rmse_XY, bprOpt, result["Prec[5]"], result["Prec[10]"], bprOptDec);
					rowData = new string[]{itr.ToString(), 
								numFeatures.ToString(),
								lrate.ToString(),
								regSocial.ToString(),
								regBias.ToString(),
								regPostv.ToString(),
								regNegtv.ToString(),
								regUser.ToString(),
								regItem.ToString(),
								rmse_X.ToString(),
								bprOpt.ToString(),
								rmse_XY.ToString(),
								result["Prec[5]"].ToString(),
								result["Prec[10]"].ToString(),
								result["Recl[5]"].ToString(),
								result["Recl[10]"].ToString()};
	
				} else {
					Console.WriteLine("\t\t- {0}: RMSE_X: {1}, RMSE_XY:{2}, OPT: {3}, OPTDec: {4}",
				                  itr, rmse_X, rmse_XY, bprOpt, bprOptDec);
					rowData = new string[]{itr.ToString(), 
								numFeatures.ToString(),
								lrate.ToString(),
								regSocial.ToString(),
								regBias.ToString(),
								regPostv.ToString(),
								regNegtv.ToString(),
								regUser.ToString(),
								regItem.ToString(),
								rmse_X.ToString(),
								bprOpt.ToString(),
								rmse_XY.ToString()};	
				}
				adaptRegularization();
			
				writeToLog(rowData);
			}				
			result = bprEval();
//			Console.WriteLine("\t\t- Prec[5]: {0}, Prec[10]: {1}", 
//			                  result["Prec[5]"], result["Prec[10]"]);
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
