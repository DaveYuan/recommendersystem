using System;
using System.Linq;
using MultiRecommender;
using MultiRecommender.IO;
using MultiRecommender.Datatype;
using MultiRecommender.Evaluation;
using System.Collections.Generic;

namespace MultiRecommenderGUI
{
	public class Recommender
	{
		public static double lrateBPR;
		public static double lrateRegConst;
		public static double regPostv;
		public static double regNegtv;
		public static double regSocial;
		public static SparseMatrix userAssociations {get; set;}
		public static List<HashSet<int>> userSymmetricAssociations;	

		public Recommender(Association associationObj)
		{	
			Initialize.numEpochs = 20;		
			Initialize.lrate = 0.009;
			Initialize.regUser = 4; //0.015
			Initialize.regItem = -4; //0.019
			Initialize.regBias = 0.01; //0.77

			regPostv = 0.001; //0.015
			regNegtv = 0.05; //0.019
			regSocial = 0.03; //0.7
			lrateBPR = 0.02;
			lrateRegConst = 0.001;

			userAssociations = new SparseMatrix();
			userAssociations.createSparseMatrix(associationObj.user1List.ToArray(), associationObj.user2List.ToArray());	
			userSymmetricAssociations = userAssociations.symmetricMatrix();
		}

		public static int[] FindAllIndexof(int[] values, int val)
		{
			return values.Select((b,i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
		}

		public static int drawPostvUser(int userId)
		{
			int numAssociations = userSymmetricAssociations[userId].Count;
			return userSymmetricAssociations[userId].ElementAt(Initialize.random.Next(0, numAssociations));			
		}

		public static int drawNegtvUser(int userId) 
		{
			int userIdNegtv = Initialize.random.Next(0, Initialize.MAX_USER_ID+1);
			while (userSymmetricAssociations[userId].Contains(userIdNegtv) || userId == userIdNegtv || 
			       userSymmetricAssociations[userIdNegtv].Count == 0) {
				userIdNegtv = Initialize.random.Next(0, Initialize.MAX_USER_ID+1);
			}
			return userIdNegtv;
		}

		public static void regularization(int type, int indx) 
		{
			if (type == Initialize.USER) {
				Initialize.decBias(Initialize.USER, indx, Initialize.lrate * Initialize.userBias[indx] 
				                   * Initialize.regUser * Initialize.regBias);
				for (int f = 0; f < Initialize.numFeatures; f++) {
					Initialize.decFeature(Initialize.USER, indx, f, Initialize.lrate * Initialize.userFeature[f,indx] * Initialize.regUser);
				}
			} else if (type == Initialize.ITEM) {
				Initialize.decBias(Initialize.ITEM, indx, Initialize.lrate * Initialize.itemBias[indx] * Initialize.regItem * Initialize.regBias);
				for (int f = 0; f < Initialize.numFeatures; f++) {
					Initialize.decFeature(Initialize.ITEM, indx, f, Initialize.lrate * Initialize.itemFeature[f,indx] * Initialize.regItem);
				}
			}
		}

		public static void predictionErrorLearn(int type, int[] indices)
		{			
			double err;
			double sigScore;
			double predictScore;
			double mappedPredictScore;				

			int size = indices.Length;

			for (int i = 0; i < size; i++) {
				int user = MainWindow.usersList[indices[i]];
				int artist = MainWindow.artistsList[indices[i]];
				int rating = MainWindow.ratingsList[indices[i]];

				predictScore = PredictRecall.PredictRating(user, artist);	
				sigScore = Initialize.g(predictScore);
				mappedPredictScore = Initialize.MIN_RATING + sigScore * (Initialize.MAX_RATING - Initialize.MIN_RATING);
				err = mappedPredictScore - rating;
				err = err * sigScore * (1 - sigScore) * (Initialize.MAX_RATING - Initialize.MIN_RATING);

				Initialize.globalAvg = Initialize.globalAvg - Initialize.lrate*err - Initialize.regGlbAvg*Initialize.globalAvg;
		
				if (type == Initialize.USER) {
					Initialize.decBias(Initialize.USER, user, Initialize.lrate * (err));
				} else if (type == Initialize.ITEM) {
					Initialize.decBias(Initialize.ITEM, artist, Initialize.lrate * (err));
				}

				for (int f = 0; f < Initialize.numFeatures; f++) {
					double uF = Initialize.userFeature[f, user];
					double iF = Initialize.itemFeature[f, artist];
					if (type == Initialize.USER) {
						Initialize.decFeature(Initialize.USER, user, f, Initialize.lrate * (err * iF));
					} else if (type == Initialize.ITEM) {
						Initialize.decFeature(Initialize.ITEM, artist, f, Initialize.lrate * (err * uF));						
					}
				}
			}	
		}

		public static void updateSocialFeatures(int userId, int userIdPostv, int userIdNegtv)
		{
			double xuPostv;
			double xuNegtv;
			double xuPostvNegtv;
			double dervxuPostvNegtv;
			double oneByOnePlusExpX;

			xuPostv = PredictRecall.predictRatingSocialRel(userId, userIdPostv);
			xuNegtv = PredictRecall.predictRatingSocialRel(userId, userIdNegtv);
			xuPostvNegtv = xuPostv - xuNegtv;
			oneByOnePlusExpX = 1.0 / (1.0 + Math.Exp(xuPostvNegtv));

			Initialize.userBias[userIdPostv] += (double) (lrateBPR * regSocial * (oneByOnePlusExpX - 
			 											Initialize.regBias * regPostv * Initialize.userBias[userIdPostv]));
			Initialize.userBias[userIdNegtv] += (double) (lrateBPR * regSocial * (-oneByOnePlusExpX - 
			                                    Initialize.regBias * regNegtv * Initialize.userBias[userIdNegtv]));

			for (int f = 0; f < Initialize.numFeatures; f++) {
				double userF = Initialize.userFeature[f, userId];
				double userPostvF = Initialize.userFeature[f, userIdPostv];
				double userNegtvF = Initialize.userFeature[f, userIdNegtv];

				dervxuPostvNegtv = userPostvF - userNegtvF;
				Initialize.userFeature[f, userId] += (double) (lrateBPR * regSocial * (oneByOnePlusExpX * dervxuPostvNegtv -
				                                                                       Initialize.regUser * userF));

				dervxuPostvNegtv = userF;
				Initialize.userFeature[f, userIdPostv] += (double) (lrateBPR * regSocial * (oneByOnePlusExpX * dervxuPostvNegtv -
				                                                                 regPostv * userPostvF));

				dervxuPostvNegtv = -userF;
				Initialize.userFeature[f, userIdNegtv] += (double) (lrateBPR * regSocial * (oneByOnePlusExpX * dervxuPostvNegtv -
				                                                                 regNegtv * userNegtvF));
			}
		}
		public static void socialRelLearn(int indx)
		{	
			int userId = indx;
			int userIdPostv = drawPostvUser(userId);
			int userIdNegtv = drawNegtvUser(userId);

			updateSocialFeatures(userId, userIdPostv, userIdNegtv);			
		}

		public static void bprSocialJointMFTrain(int type, int indx)
		{		
			RecommenderExtensions.fetchUniqueArtist();
			RecommenderExtensions.artistsRatedByUser();

			for (int itr = 1; itr <= Initialize.numEpochs; itr++) {
				regularization(type, indx);				
				if (type == Initialize.USER) {
					predictionErrorLearn(type, FindAllIndexof(MainWindow.usersList.ToArray(), indx));
					for (int i = 1; i < 30; i++) {
						socialRelLearn(indx);
					}
				} else if (type == Initialize.ITEM) {
					predictionErrorLearn(type, FindAllIndexof(MainWindow.artistsList.ToArray(), indx));
				}
			}					
		}

		public static void retrainUser(int user) 
		{
			Initialize.userBias[user] = Initialize.random.NextDouble();
			for (int f = 0; f < Initialize.numFeatures; f++) {
				Initialize.userFeature[f, user] = Initialize.random.NextDouble();
			}
			bprSocialJointMFTrain(Initialize.USER, user);
		}

		public static void reTrainArtist(int artist)
		{
			Initialize.itemBias[artist] = Initialize.random.NextDouble();
			for (int f = 0; f < Initialize.numFeatures; f++) {
				Initialize.itemFeature[f, artist] = Initialize.random.NextDouble();
			}
			bprSocialJointMFTrain(Initialize.ITEM, artist);
		}
	}
}