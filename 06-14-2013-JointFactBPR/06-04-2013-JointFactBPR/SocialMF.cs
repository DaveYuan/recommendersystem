using System;
using System.Linq;
using System.Collections.Generic;

namespace JointFactBPR
{
	public class SocialMF : BiasLearnMF, ISocial
	{	
		public double regSocial {get; set;}
		public SparseMatrix userAssociations {get; set;}
		
		public SocialMF(Association associationObj)
		{				
			regSocial = 10;
			userAssociations = new SparseMatrix();
			userAssociations.createSparseMatrix(associationObj.user1List.ToArray(), associationObj.user2List.ToArray());								
							
			Console.WriteLine(", regSocial: {0}", regSocial);				
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
					decBias(USER_DEC, u, lrate *(regSocial * (userBias[u] - sumUserUAssociationBias/numUAssociations)));
					for (int f = 0; f < numFeatures; f++) {
						decFeature(USER_DEC, u, f, lrate * (regSocial * (userFeature[f,u] - sumUserUAssociationFeatures[f]/numUAssociations)));
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
						decBias(USER_DEC, u, lrate * (regSocial * biasInc / (double)numVAssociations));
						for (int f = 0; f < numFeatures; f++) {
							double dec = -userFeature[f,v] + sumUserWAssociationFeatures[f]/(double)numVAssociations;
							decFeature(USER_DEC, u, f, lrate * (regSocial * dec / (double)numVAssociations));
						}
					}
				}				
			}
		}	
		
		public void socialMFTrain()
		{		
			double rmse;
			int numTestEntries; 
			
			numTestEntries = testCheck();
			
			for (int itr = 1; itr <= numEpochs; itr++) {
				predictionErrorLearn();	
				regularization();					
				socialRelLearn();
				
				rmse = errTestSet(numTestEntries);
				Console.WriteLine("\t\t- RMSE[{0}]: {1}", itr, rmse);
			}
		}
	}
}

