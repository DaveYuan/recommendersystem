using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Collections.Generic;

namespace MultiRecommender
{
	public class Init {
		public static double regUser;
		public static double regItem;
		public static double regBias;
		public static double globalAvg;
		public static double lrate;
		public const int USER_INC = 1;
		public const int ITEM_INC = -1;
		public static int numEpochs;
		public static int numEntries;
		public static int numFeatures;		
		public static int MAX_USER_ID;
		public static int MAX_ITEM_ID;
		public static int MIN_RATING;
		public static int MAX_RATING;
		public static int[] trainUsersArray;
		public static int[] trainItemsArray;
		public static int[] trainRatingsArray;
		public static int[] testUsersArray;
		public static int[] testItemsArray;
		public static int[] testRatingsArray;		
		public static double[] userBias;
		public static double[] itemBias;	
		public static double[,] userFeature;
		public static double[,] itemFeature;
		public static Random random;
			
		public static void init()
		{	
			lrate = 0.001;
			regUser = 0.015;
			regItem = 0.015;
			regBias = 0.01;
			numEpochs = 30;
			numFeatures = 10;	
			MAX_RATING = 5;
			MIN_RATING = 1;
			
			userFeature = new double[numFeatures,MAX_USER_ID+1];
			itemFeature = new double[numFeatures,MAX_ITEM_ID+1];
			userBias = new double[MAX_USER_ID+1];
			itemBias = new double[MAX_ITEM_ID+1];
			
			random = new Random();								
			for (int i = 0; i < numFeatures; i++) {
				for (int j = 0; j <= MAX_USER_ID; j++) {
					userFeature[i,j] = random.NextDouble();
				}
			}
			
			random = new Random();
			for (int i = 0; i < numFeatures; i++) {
				for (int j = 0; j <= MAX_ITEM_ID; j++) {
					itemFeature[i,j] = random.NextDouble();
				}
			}
			
			random = new Random();
			for (int j = 0; j <= MAX_USER_ID; j++) {
				userBias[j] = random.NextDouble();
			}
			
			random = new Random();
			for (int j = 0; j <= MAX_ITEM_ID; j++) {
				itemBias[j] = random.NextDouble();
			}
			
			int ratingSum = trainRatingsArray.Sum();
			globalAvg = (ratingSum/numEntries - MIN_RATING) / (MAX_RATING - MIN_RATING);
			//globalAvg = Math.Log(ratingAvg / (1 - ratingAvg));					
			
			Console.WriteLine("\t\t- #epochs: {0}, #features: {1}, lrate: {2}, regUser: {3}, regItem: {4}, regBias: {5}",
			                  numEpochs, numFeatures, lrate, regUser, regItem, regBias);	
			Console.WriteLine("\t\t- MAX_USER_ID: {0}, MAX_ITEM_ID: {1}", MAX_USER_ID, MAX_ITEM_ID);			
		}
		
		public static void incBias(int incRel, int indx, double inc) 
		{
			if (incRel == USER_INC) {
				userBias[indx] += inc;
			} else if (incRel == ITEM_INC) {
				itemBias[indx] += inc;
			}
		}
		
		public static void incFeature(int incRel, int indx, int feature, double inc)
		{
			if (incRel == USER_INC) {
				userFeature[feature, indx] += inc;
			} else if (incRel == ITEM_INC) {
				itemFeature[feature, indx] += inc;
			}
		}
					
		/*
		 * Logistic sigmoid function
		 * g(x) = 1 / (1 + e^(-x))
		 */
		public static double g(double x) 
		{
			double sigmoid = 1.0 / (double)(1.0 + Math.Exp(-x));
			return sigmoid;			
		}
		
		/*
		 * Derivative of sigmoid 
		 * g'(x) = e^(-x)/((1 + e^(-x))^2)
		 */
		public static double gDerv(double x)
		{
			double sigmoidDerv = Math.Exp(-x) / (double)(Math.Pow( (1.0 + Math.Exp(-x)), 2.0));
			return sigmoidDerv;
		}									
	}
}