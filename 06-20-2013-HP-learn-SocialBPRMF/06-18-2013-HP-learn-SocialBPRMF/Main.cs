using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using ProtoBuf;

namespace HPlearnSocialBPRMF
{
	public class MainClass : Init
	{	
		public static int model;
		public const int BIAS_LEARN_MF = 1;
		public const int SOCIAL_MF = 2;
		public const int BPR_SOCIAL_JOINT_MF = 3;
		public const int SQ_ERR_SOCIAL_MF = 4;
		public const int BPR_SOCIAL_BPR = 5;
		public const int BPR_SOCIAL_MF = 6;
		
		protected static Test testObj;
		protected static Train trainObj;
		protected static Validation validationObj;
		protected static Association associationObj;											
		
		public static void writeToConsole(string mssg) 
		{
			Console.WriteLine("\t- " + mssg);
		}
		
		public static void loadDatasetToMemory() 
		{
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
					writeToConsole("Loading train.bin");
					using (FileStream file = File.OpenRead("train.bin"))
					{
						trainObj = Serializer.Deserialize<Train>(file);
					}
			
					writeToConsole("Loading validation.bin");
					using (FileStream file = File.OpenRead("validation.bin"))
					{
						validationObj = Serializer.Deserialize<Validation>(file);
					}
					
					writeToConsole("Loading test.bin");
					using (FileStream file = File.OpenRead("test.bin"))
					{
						testObj = Serializer.Deserialize<Test>(file);
					}			
					
					writeToConsole("Loading trust.bin");
					using (FileStream file = File.OpenRead("trust.bin"))
					{
						associationObj = Serializer.Deserialize<Association>(file);
					}
					loadDatasetToMemory();
			loadTime.Stop();
				
			Console.WriteLine("\t\t: #train: {0}, #validation: {1}", numTrainEntries, numValidationEntries);
			Console.WriteLine("\t\t- MAX_USER_ID: {0}, MAX_ITEM_ID: {1}", MAX_USER_ID, MAX_ITEM_ID);	
			
			trainTime.Start();
					writeToConsole("Initialize features");
					init();	
					
					model = BPR_SOCIAL_JOINT_MF;
					
					switch (model) {
						case BIAS_LEARN_MF: 
								writeToConsole("BIAS_LEARN_MF: RMSE trace per epoch on test");
								BiasLearnMF biasLearnMF = new BiasLearnMF();
								biasLearnMF.biasLearnMFTrain();							
								break;
						case SOCIAL_MF:
								writeToConsole("SOCIAL_MF: RMSE trace per epoch on test");							
								SocialMF socialMF = new SocialMF(associationObj);
								socialMF.socialMFTrain();
								break;	
						case BPR_SOCIAL_JOINT_MF:
								writeToConsole("BPR_SOCIAL_JOINT_MF: RMSE trace per epoch on test");
								BprSocialJointMF bprSocialJointMF = new BprSocialJointMF(associationObj);
								bprSocialJointMF.bprSocialJointMFTrain();
//								bprSocialJointMF.hyperParameterSearch();
								break;
						case SQ_ERR_SOCIAL_MF:
								writeToConsole("SQ_ERR_SOCIAL_MF: RMSE trace per epoch on test");
								SqErrSocialMF sqErrrSocialMF = new SqErrSocialMF(associationObj);
								sqErrrSocialMF.sqErrSocialMFTrain();
								break;
						case BPR_SOCIAL_BPR:
								writeToConsole("BPR_SOCIAL_BPR: RMSE trace per epoch on test");
								BPRSocialBPR bprSocialBpr = new BPRSocialBPR(associationObj);
								bprSocialBpr.BPRSocialBPRTrain();
								break;
						case BPR_SOCIAL_MF:
								writeToConsole("BPR_SOCIAL_BPR: RMSE trace per epoch on test");
								BPRSocialMF bprSocialMf = new BPRSocialMF(associationObj);
								bprSocialMf.BPRSocialMFTrain();
								break;
					}				
			trainTime.Stop();
			
			Console.WriteLine("\t- time(load): {0}, time(train-test): {1}", 
					loadTime.Elapsed, trainTime.Elapsed);
			
			writeToConsole("Done!");
		}	
	}
}
