using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using ProtoBuf;
using MultiRecommender.IO;
using MultiRecommender.Datatype;
using MultiRecommender.Evaluation;
using MultiRecommender.JointFactorization;
using MultiRecommender.RatingPredictor;
using MultiRecommender.SocialRatingPredictor;

namespace MultiRecommender
{
	public class MultiRecommenderMain : Init
	{	
		public static int model;
		public const int MATRIX_FACTORIZATION = 1;
		public const int MATRIX_FACTORIZATION_BIAS = 2;
		public const int MATRIX_FACTORIZATION_REG = 3;
		public const int MATRIX_FACTORIZATION_BIAS_REG = 4;
		public const int SOCIAL_MF = 5;
		public const int BPR_SOCIAL_JOINT_MF = 6;
		public const int SQ_ERR_SOCIAL_MF = 7;

		public static StreamWriter writer;
		public static Test testObj;
		public static Train trainObj;
		public static Validation validationObj;
		public static Association associationObj;	


		public static void writeToConsole(string mssg) 
		{
			Console.WriteLine("\t- " + mssg);
		}

		public static void loadDatasetToMemory() 
		{
			writeToConsole("Loading train.bin");
			using (FileStream file = File.OpenRead("../../../../data/train.bin"))
			{
				trainObj = Serializer.Deserialize<Train>(file);
			}

			writeToConsole("Loading validation.bin");
			using (FileStream file = File.OpenRead("../../../../data/validation.bin"))
			{
				validationObj = Serializer.Deserialize<Validation>(file);
			}

			writeToConsole("Loading test.bin");
			using (FileStream file = File.OpenRead("../../../../data/test.bin"))
			{
				testObj = Serializer.Deserialize<Test>(file);
			}			

			writeToConsole("Loading trust.bin");
			using (FileStream file = File.OpenRead("../../../../data/trust.bin"))
			{
				associationObj = Serializer.Deserialize<Association>(file);
			}

			trainUsersArray = trainObj.usersList.ToArray();
			trainItemsArray = trainObj.itemsList.ToArray();
			trainRatingsArray = trainObj.ratingsList.ToArray();

			validatationUsersArray = validationObj.usersList.ToArray();
			validationItemsArray = validationObj.itemsList.ToArray();
			validationRatingsArray = validationObj.ratingsList.ToArray();

			testUsersArray = testObj.usersList.ToArray();
			testItemsArray = testObj.itemsList.ToArray();
			testRatingsArray = testObj.ratingsList.ToArray();

			numTrainEntries = trainUsersArray.Count();
			numValidationEntries = validatationUsersArray.Count();

			MAX_USER_ID = Math.Max(MAX_USER_ID, associationObj.user1List.Max());
			MAX_USER_ID = Math.Max(MAX_USER_ID, associationObj.user2List.Max());
			MAX_ITEM_ID = validationItemsArray.Max();
			MAX_ITEM_ID = Math.Max(MAX_ITEM_ID, trainItemsArray.Max());				
		}

		public static void Main (string[] args)
		{				
			Stopwatch loadTime = new Stopwatch();			
			Stopwatch trainTime = new Stopwatch();

			loadTime.Start();				
			loadDatasetToMemory();
			loadTime.Stop();

			Console.WriteLine("\t\t: #train: {0}, #validation: {1}", numTrainEntries, numValidationEntries);
			Console.WriteLine("\t\t- MAX_USER_ID: {0}, MAX_ITEM_ID: {1}", MAX_USER_ID, MAX_ITEM_ID);	

			trainTime.Start();
			writeToConsole("Initialize features");
			init();	

			model = BPR_SOCIAL_JOINT_MF;

			switch (model) {
				case MATRIX_FACTORIZATION:
					writeToConsole("MATRIX_FACTORIZATION: RMSE trace per epoch on test");
					MatrixFactorization matrixFactorization = new MatrixFactorization();
					matrixFactorization.matrixFactorizationTrain();
					writer = Model.getWriter("MatrixFactorization.model", "MatrixFactorization");
					break;
				case MATRIX_FACTORIZATION_BIAS:
					writeToConsole("MATRIX_FACTORIZATION_BIAS: RMSE trace per epoch on test");
					MatrixFactorizationBias matrixFactorizationBias = new MatrixFactorizationBias();
					matrixFactorizationBias.matrixFactorizationBiasTrain();
					writer = Model.getWriter("MatrixFactorizationBias.model", "MatrixFactorizationBias");
					break;
				case MATRIX_FACTORIZATION_REG:
					writeToConsole("MATRIX_FACTORIZATION_REG: RMSE trace per epoch on test");
					MatrixFactorizationReg matrixFactorizationReg = new MatrixFactorizationReg();
					matrixFactorizationReg.matrixFactorizationRegTrain();
					writer = Model.getWriter("MatrixFactorizationReg.model", "MatrixFactorizationReg");
					break;
				case MATRIX_FACTORIZATION_BIAS_REG: 
					writeToConsole("MATRIX_FACTORIZATION_BIAS_REG: RMSE trace per epoch on test");
					MatrixFactorizationBiasReg matrixFactorizationBiasReg = new MatrixFactorizationBiasReg();
					matrixFactorizationBiasReg.matrixFactorizationBiasRegTrain();	
					writer = Model.getWriter("MatrixFactorizationBiasReg.model", "MatrixFactorizationBiasReg");
					break;
				case SOCIAL_MF:
					writeToConsole("SOCIAL_MF: RMSE trace per epoch on test");							
					SocialMF socialMF = new SocialMF(associationObj);
					socialMF.socialMFTrain();
					writer = Model.getWriter("SocialMF.model", "SocialMF");
					break;	
				case BPR_SOCIAL_JOINT_MF:
					writeToConsole("BPR_SOCIAL_JOINT_MF: RMSE trace per epoch on test");
					BprSocialJointMF bprSocialJointMF = new BprSocialJointMF(associationObj);
					bprSocialJointMF.bprSocialJointMFTrain();
					writer = Model.getWriter("BPRSocialJointMF.model", "BPRSocialJointMF");
					//bprSocialJointMF.hyperParameterSearch();
					break;
				case SQ_ERR_SOCIAL_MF:
					writeToConsole("SQ_ERR_SOCIAL_MF: RMSE trace per epoch on test");
					SqErrSocialMF sqErrrSocialMF = new SqErrSocialMF(associationObj);
					sqErrrSocialMF.sqErrSocialMFTrain();
					writer = Model.getWriter("SqErrSocialMF.model", "SqErrSocialMF");
					break;
			}			
			trainTime.Stop();

			writeToConsole("Saving model");		
			Model.saveModel(writer);

			Console.WriteLine("\t- time(load): {0}, time(train-test): {1}", 
			                  loadTime.Elapsed, trainTime.Elapsed);

			writeToConsole("Done!");
		}	
	}
}
