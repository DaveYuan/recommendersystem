using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Collections.Generic;

namespace socialMFv1
{
	class Init {
		static protected double regUser;
		static protected double regItem;
		static protected double regBias;
		static protected double regSocial;
		static protected double lrate;
		static protected double globalBias;
		static protected int numEpochs;
		static protected int numEntries;
		static protected int numFeatures;		
		static protected int MIN_RATING;
		static protected int MAX_RATING;
		static protected int MAX_USER_ID;
		static protected int MAX_ITEM_ID;
		static protected int USER_INC;
		static protected int ITEM_INC;
		static protected int[] trainUsersArray;
		static protected int[] trainItemsArray;
		static protected int[] trainRatingsArray;
		static protected int[] testUsersArray;
		static protected int[] testItemsArray;
		static protected int[] testRatingsArray;		
		static protected double[] userBias;
		static protected double[] itemBias;	
		static protected double[] userBiasGradient;
		static protected double[] itemBiasGradient;
		static protected double[,] userFeature;
		static protected double[,] itemFeature;
		static protected double[,] userFeatureGradient;
		static protected double[,] itemFeatureGradient;
		static protected Random random;
		static protected SparseMatrix userAssociations;
		
		protected static void writeToConsole(string mssg) 
		{
			Console.WriteLine("\t- " + mssg);
		}
		
		protected static void init()
		{
			userFeature = new double[numFeatures,MAX_USER_ID+1];
			itemFeature = new double[numFeatures,MAX_ITEM_ID+1];
			userBias = new double[MAX_USER_ID+1];
			itemBias = new double[MAX_ITEM_ID+1];
			
			userFeatureGradient = new double[numFeatures, MAX_USER_ID+1];
			itemFeatureGradient = new double[numFeatures, MAX_ITEM_ID+1];
			userBiasGradient = new double[MAX_USER_ID+1];
			itemBiasGradient = new double[MAX_ITEM_ID+1];			
			
			Random rd = new Random();
			
			for (int i = 0; i < numFeatures; i++) {
				for (int j = 0; j <= MAX_USER_ID; j++) {
					userFeature[i,j] = (float) rd.NextDouble();
				}
			}
			
			rd = new Random();
			for (int i = 0; i < numFeatures; i++) {
				for (int j = 0; j <= MAX_ITEM_ID; j++) {
					itemFeature[i,j] = (float) rd.NextDouble();
				}
			}
			
			int ratingSum = trainRatingsArray.Sum();
			double ratingAvg = (ratingSum/numEntries - MIN_RATING) / (MAX_RATING - MIN_RATING);
			globalBias = Math.Log(ratingAvg / (1 - ratingAvg));
		}
		
		protected static void incBiasGradient(int incRel, int indx, double inc) 
		{
			if (incRel == USER_INC) {
				userBiasGradient[indx] += inc;
			} else if (incRel == ITEM_INC) {
				itemBiasGradient[indx] += inc;
			}
		}
		
		protected static void incFeatureGradient(int incRel, int indx, int feature, double inc)
		{
			if (incRel == USER_INC) {
				userFeatureGradient[feature, indx] += inc;
			} else if (incRel == ITEM_INC) {
				itemFeatureGradient[feature, indx] += inc;
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