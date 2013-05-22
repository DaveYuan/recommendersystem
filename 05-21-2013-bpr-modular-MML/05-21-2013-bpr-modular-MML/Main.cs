using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using ProtoBuf;

namespace bprmodularMML
{
	class MainClass : Eval
	{			
		public MainClass(Train trainObj, Test testObj)
		{
			lrate = 0.05;
			regUser = 0.0025;
			regPostv = 0.0025;
			regNegtv = 0.00025;
			regBias = 0.33;
			numEpochs = 25;
			numFeatures = 10;
			
			random = new Random();
			trainUsersArray = trainObj.usersList.ToArray();
			trainItemsArray = trainObj.itemsList.ToArray();
			testUsersArray = testObj.usersList.ToArray();
			testItemsArray = testObj.itemsList.ToArray();
			MAX_USER_ID = trainUsersArray.Max();
			MAX_ITEM_ID = trainItemsArray.Max();
			Console.WriteLine("\t\t- #epochs: {0}, #features: {1}, lrate: {2}, regUser: {3}, regPostv: {4}, regNegtv: {5}, regBias: {6}",
			                  numEpochs, numFeatures, lrate, regUser, regPostv, regNegtv, regBias);			    
			Console.WriteLine("\t\t- MAX_USER_ID: {0}, MAX_ITEM_ID: {1}", MAX_USER_ID, MAX_ITEM_ID);
		}				
				
		protected static double updateFeatures(int userId, int itemIdPostv, int itemIdNegtv)
		{
			double xuPostv;
			double xuNegtv;
			double xuPostvNegtv;
			double dervxuPostvNegtv;
			double oneByOnePlusExpX;

			xuPostv = userBias[userId] + itemBias[itemIdPostv] +  dotProduct(userId, itemIdPostv);
			xuNegtv = userBias[userId] + itemBias[itemIdNegtv] + dotProduct(userId, itemIdNegtv);
			xuPostvNegtv = xuPostv - xuNegtv;
			oneByOnePlusExpX = 1.0 / (1.0 + Math.Exp(xuPostvNegtv));
		
			userBias[userId] += (double) (lrate * (oneByOnePlusExpX - regBias * userBias[userId]));
			itemBias[itemIdPostv] += (double) (lrate * (oneByOnePlusExpX - regBias * itemBias[itemIdPostv]));
			itemBias[itemIdNegtv] += (double) (lrate * (-oneByOnePlusExpX - regBias * itemBias[itemIdNegtv]));

			for (int f = 0; f < numFeatures; f++) {
				double userF = userFeature[f, userId];
				double itemPostvF = itemFeature[f, itemIdPostv];
				double itemNegtvF = itemFeature[f, itemIdNegtv];

				dervxuPostvNegtv = itemPostvF - itemNegtvF;
				userFeature[f, userId] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
									  regUser * userF));
				
				dervxuPostvNegtv = userF;
				itemFeature[f, itemIdPostv] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
										    regPostv * itemPostvF));
				
				dervxuPostvNegtv = -userF;
				itemFeature[f, itemIdNegtv] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
											    regNegtv * itemNegtvF));
			}
			return xuPostvNegtv;
		}	
		
		protected static int drawUser()
		{
			int userId;
			
			while(true) {
				userId = random.Next(0, MAX_USER_ID);
				if (!trainRatedItems.ContainsKey(userId)) {
					continue;
				}
				return userId;
			}
		}

		protected static double bprTrain()
		{
			int numRatedItems = 0;
			int userId;
			int itemIdPostv = 1;
			int itemIdNegtv = 1;
			double xuPostvNegtv = 1;
			double bprOpt = 0.0;
			
			numEntries = trainItemsArray.Length;
			Console.WriteLine("\t\t- #TrainEntries: {0}, Uniq(#Users): {1}, Uniq(#Items): {2}", 
					numEntries, numUsers, numItems);
		
			for (int epoch = 1; epoch <= numEpochs; epoch++) {
				bprOpt = 0.0;
				for (int n = 0; n < numEntries; n++) {
					userId = drawUser();
					numRatedItems = trainRatedItems[userId].Length;
					
					itemIdPostv = trainRatedItems[userId][random.Next(0, numRatedItems)];
					itemIdNegtv = uniqueItemsArray[random.Next(0, numItems)];
					
					while (trainRatedItems[userId].Contains(itemIdNegtv)) {
						itemIdNegtv = uniqueItemsArray[random.Next(0, numItems)];
					}
					
					xuPostvNegtv = updateFeatures(userId, itemIdPostv, itemIdNegtv);
					bprOpt += Math.Log(sigmoid(xuPostvNegtv));
				}
//				Console.WriteLine("\t\t- {0}: Opt(Usr+Itm): {1}", epoch, bprOpt);
			}	
			return bprOpt;
		}
			
		public static void Main (string[] args)
		{			
			Train trainObj;
			Test testObj;
			Stopwatch loadTime = new Stopwatch();
			Stopwatch trainTime = new Stopwatch();
			Stopwatch testTime = new Stopwatch();
			
			loadTime.Start();
			writeToConsole("Loading u1.base.bin");
			using (FileStream file = File.OpenRead("u1.base.bin"))
			{
				trainObj = Serializer.Deserialize<Train>(file);
			}
			
			writeToConsole("Loading u1.test.bin");
			using (FileStream file = File.OpenRead("u1.test.bin"))
			{
				testObj = Serializer.Deserialize<Test>(file);
			}
			loadTime.Stop();
			
			writeToConsole("Constructor call");
			new MainClass(trainObj, testObj);
			GC.Collect();
			
			writeToConsole("Find: RatedItemsPerUser");
			trainItemsRatedByUser();
			testItemsRatedByUser();
			
			writeToConsole("Find: uniqueUsers and uniqueItems");
			calUniqueUsersnItems();
			
			writeToConsole("Initialize features");
			init();
			
			trainTime.Start();
			writeToConsole("Bpr Training");
			bprTrain();
			trainTime.Stop();
		
			testTime.Start();
			writeToConsole("Bpr Evaluation");
			var result = bprEval();	
			testTime.Stop();
			
			displayResult(result);
			Console.WriteLine("\t- Time(load): {0}, Time(train): {1}, Time(test): {2}", 
					loadTime.Elapsed, trainTime.Elapsed, testTime.Elapsed);

			writeToConsole("Done!");
		}
	}
}
