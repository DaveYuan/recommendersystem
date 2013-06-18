using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using ProtoBuf;

namespace JointFactBPR
{
	public class MainClass : Init
	{	
		public static int model;
		public const int BIAS_LEARN_MF = 1;
		public const int SOCIAL_MF = 2;
		public const int BPR_SOCIAL_JOINT_MF = 3;
		public const int SQ_ERR_SOCIAL_MF = 4;
		
		protected static Test testObj;
		protected static Train trainObj;
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
			
			testUsersArray = testObj.usersList.ToArray();
			testItemsArray = testObj.itemsList.ToArray();
			testRatingsArray = testObj.ratingsList.ToArray();
			
			numEntries = trainUsersArray.Count();
			
			MAX_USER_ID = Math.Max(MAX_USER_ID, associationObj.user1List.Max());
			MAX_USER_ID = Math.Max(MAX_USER_ID, associationObj.user2List.Max());	
			MAX_ITEM_ID = trainItemsArray.Max();									    					
		}
						
		public static void Main (string[] args)
		{				
			Stopwatch loadTime = new Stopwatch();			
			Stopwatch trainTime = new Stopwatch();
			
			loadTime.Start();				
					writeToConsole("Loading train.bin");
					using (FileStream file = File.OpenRead("train1.bin"))
					{
						trainObj = Serializer.Deserialize<Train>(file);
					}
					
					writeToConsole("Loading test.bin");
					using (FileStream file = File.OpenRead("test1.bin"))
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
						
			trainTime.Start();
					writeToConsole("Initialize features");
					init();	
					
					model = SQ_ERR_SOCIAL_MF;
					
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
					}				
			trainTime.Stop();
			
			Console.WriteLine("\t- time(load): {0}, time(train-test): {1}", 
					loadTime.Elapsed, trainTime.Elapsed);
			
			writeToConsole("Done!");
		}	
	}
}
