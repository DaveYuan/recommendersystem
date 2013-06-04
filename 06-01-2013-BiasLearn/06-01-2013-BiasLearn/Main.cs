using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using ProtoBuf;

namespace MultiRecommender
{
	public class MainClass : Init
	{		
		public const int BIAS_LEARN_MF = 1;
		public const int SOCIAL_MF = 2;
		public static int model;
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
			
			SocialMF.userAssociations = new SparseMatrix();
			SocialMF.userAssociations.createSparseMatrix(associationObj.user1List.ToArray(), associationObj.user2List.ToArray());								
							
			MAX_USER_ID = Math.Max(MAX_USER_ID, SocialMF.userAssociations.numRows-1);
			MAX_USER_ID = Math.Max(MAX_USER_ID, SocialMF.userAssociations.numColumns-1);	
			MAX_ITEM_ID = trainItemsArray.Max();									    					
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
						
			trainTime.Start();
					writeToConsole("Initialize features");
					init();	
					
					model = SOCIAL_MF;
					
					switch (model) {
						case BIAS_LEARN_MF: 
								writeToConsole("BIAS_LEARN_MF: RMSE trace per epoch on test");
								BiasLearnMF.biasLearnMF();								
								break;
						case SOCIAL_MF:
								writeToConsole("SOCIAL_MF: RMSE trace per epoch on test");
								SocialMF.socialMFInit();
								SocialMF.socialMF();
								break;					
					}				
			trainTime.Stop();
			
			Console.WriteLine("\t- Time(LoadDataset): {0}, Time(Train+Test): {1}", 
					loadTime.Elapsed, trainTime.Elapsed);
			
			writeToConsole("Done!");
		}	
	}
}

