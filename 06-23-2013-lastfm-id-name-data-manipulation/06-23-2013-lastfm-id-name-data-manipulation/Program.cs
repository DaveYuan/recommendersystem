using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace lastfmidnamedatamanipulation
{
	class MainClass
	{
		// Last-fm social network data manipulation with no mapping of item-ids as they have to be mapped backed to artist info
		static void writeToConsole(string mssg) 
		{			
			Console.WriteLine("\t" + mssg);
		}

		static string[] readAllLines(string fileName)
		{
			return System.IO.File.ReadAllLines(fileName);
		}				

		static void mapRatings(string fileName) 
		{
			string[] dataset = readAllLines(fileName);
			int rowIndexCounter;
//			int userIdIndx = 0;
			string user;
			string item;
			long rating;
			string mappedRating;

			File.Open("rating_mapped.txt", FileMode.Create).Close();
			user = item = mappedRating = null;

			foreach (string line in dataset) {
				string[] stringSeparator = new string[] { "\t" };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;

				foreach (string s in result)					
				{
					if (rowIndexCounter == 0) {
						user = s;
					}
					if (rowIndexCounter == 1) {
						item = s;
					}
					if (rowIndexCounter == 2) {
						rating = Convert.ToInt64(s);		
						if (rating >= 1 && rating < 50) {
							mappedRating = "1";
						} else if (rating < 200) {
							mappedRating = "2";
						} else if (rating < 500) {
							mappedRating = "3";
						} else if (rating < 1000) {
							mappedRating = "4";
						} else {
							mappedRating = "5";
						}
					}
					rowIndexCounter++;
				}
				File.AppendAllText("rating_mapped.txt", user + "\t" + item + "\t" + mappedRating + "\n");
			}			
		}

		static void trustDataManipulation(ref Dictionary<string, int> userIdToIndx) 
		{
			Trust trust = new Trust();
			trust.trustUserList1 = new List<int>();
			trust.trustUserList2 = new List<int>();

			writeToConsole("trust.txt data loading");

			int rowIndexCounter;
			int userIdIndx = 0;
			string[] links = readAllLines("trust.txt");

			foreach (string line in links) {
				string[] stringSeparator = new string[] { "\t", " " };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;

				foreach (string s in result)					
				{						
					if (!userIdToIndx.ContainsKey(s)) {				
						userIdToIndx.Add(s, userIdIndx);
						userIdIndx++;
					} 
					if (rowIndexCounter == 0) {											
						trust.trustUserList1.Add(userIdToIndx[s]);
					} else {
						trust.trustUserList2.Add(userIdToIndx[s]);
					}									
					rowIndexCounter++;
				}
			}				

			writeToConsole("trust.txt data loaded");			
			writeToConsole("trust.bin creation in progress");

			using (FileStream file = File.Create("trust.bin"))
			{
				ProtoBuf.Serializer.Serialize(file, trust);                
			}

			writeToConsole("trust.bin creation completed");	
			writeToConsole("trust.bin data loading");

			Trust t1;
			using (FileStream file = File.OpenRead("trust.bin"))
			{
				t1 = ProtoBuf.Serializer.Deserialize<Trust>(file);
			}

			writeToConsole("trust.bin data loaded");
			Console.WriteLine("t1: {0}, t2: {1}", trust.trustUserList1.Count, trust.trustUserList2.Count);
			Console.WriteLine("OrigList1 => List1[0]: {0}, List1[end]: {1}", trust.trustUserList1[0], trust.trustUserList1[trust.trustUserList1.Count-1]);
			Console.WriteLine("ActList1 => List1[0]: {0}, List1[end]: {1}", t1.trustUserList1[0], t1.trustUserList1[t1.trustUserList1.Count-1]);

			Console.WriteLine("OrigList2 => List2[0]: {0}, List2[end]: {1}", trust.trustUserList2[0], trust.trustUserList2[trust.trustUserList2.Count-1]);            		
			Console.WriteLine("ActList2 => List2[0]: {0}, List2[end]: {1}", t1.trustUserList2[0], t1.trustUserList2[t1.trustUserList2.Count-1]);            			
		}


		static List<int> validationData(List<int> usersList,
		                                List<int> itemsList,
		                                List<int> ratingsList)
		{			
			int trainSize = usersList.Count;
			int valSize = (int)(0.2*trainSize);
			Random r = new Random();
			List<int> indxRemvTrain = new List<int>();

			Validation val = new Validation();
			val.usersList = new List<int>();
			val.itemsList = new List<int>();
			val.ratingsList = new List<int>();

			writeToConsole("validation sampling");
			Console.WriteLine("\t\t- Train size: {0}, Validation size: {1}", trainSize, valSize);

			for(int i = 0; i < valSize; i++) {
				int indx = r.Next(0, trainSize-1);	
				while (indxRemvTrain.Contains(indx)) {
					indx = r.Next(0, trainSize-1);
				}
				val.usersList.Add(usersList[indx]);
				val.itemsList.Add(itemsList[indx]);
				val.ratingsList.Add(ratingsList[indx]);
				indxRemvTrain.Add(indx);				
			}

			writeToConsole("validation.bin creation in progress");
			using (FileStream file = File.Create("validation.bin"))
			{
				ProtoBuf.Serializer.Serialize(file, val);
			}			
			writeToConsole("validation.bin creation completed");

			return indxRemvTrain;			
		}

		static void ratingDataManipulation(ref Dictionary<string, int> userIdToIndx)
		{
			Train rating = new Train();
			rating.usersList = new List<int>();
			rating.itemsList = new List<int>();
			rating.ratingsList = new List<int>();
			var usersList = new List<int>();
			var itemsList = new List<int>();
			var ratingsList = new List<int>();						

			writeToConsole("train.txt data loading");

			int user = 0;
			int rowIndexCounter;
			int removedEntriesCount = 0;
			string[] ratings = readAllLines("train.txt");			

			foreach (string line in ratings) {
				string[] stringSeparator = new string[] { "\t", " " };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;

				foreach (string s in result)
				{						
					if (rowIndexCounter == 0) {					
						if (userIdToIndx.ContainsKey(s)) {
							user = userIdToIndx[s];
							usersList.Add(userIdToIndx[s]);
						} else {
							// TODO: right now removing users who are in train but not in trust
							removedEntriesCount++;
							break;
						}
					} 
					if (rowIndexCounter == 1) {						
						itemsList.Add(Convert.ToInt32(s));						
					}
					if (rowIndexCounter == 2) {
						ratingsList.Add(Convert.ToInt32(s));
					}						
					rowIndexCounter++;
				}								
			}		

			writeToConsole("train.txt data loaded");						           		

			var indxRemvTrain = validationData(usersList, itemsList, ratingsList);
			int pseudoTrainSize = ratingsList.Count;
			for (int i = 0; i < pseudoTrainSize; i++) {
				if(indxRemvTrain.Contains(i)) {
					continue;
				} else {
					rating.usersList.Add(usersList[i]);
					rating.itemsList.Add(itemsList[i]);
					rating.ratingsList.Add(ratingsList[i]);
				}
			}

			Console.WriteLine("\nNum entries removed from train dataset: {0}\n", removedEntriesCount);
			Console.WriteLine("\nPsTrain: {0}, Val: {1}, Train: {2}", pseudoTrainSize, indxRemvTrain.Count, rating.usersList.Count);
			writeToConsole("train.bin creation in progress");

			using (FileStream file = File.Create("train.bin"))
			{
				ProtoBuf.Serializer.Serialize(file, rating);
			}

			writeToConsole("train.bin creation completed");			
		}

		static void testDataManipulation(ref Dictionary<string, int> userIdToIndx)
		{						
			Test test = new Test();
			//	test.testUserItem = new List<string>();
			test.usersList = new List<int>();
			test.itemsList = new List<int>();
			test.ratingsList = new List<int>();

			writeToConsole("test.txt data loading");

			int user = -1;
			int item = -1;
			int rowIndexCounter;
			int removedEntriesCount = 0;
			string[] ratings = readAllLines("test.txt");			

			//Rated items per user from test data			
			foreach (string line in ratings) {
				string[] stringSeparator = new string[] { "\t", " " };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				foreach (string s in result)
				{						
					if (rowIndexCounter == 0) {					
						if (userIdToIndx.ContainsKey(s)) {
							user = userIdToIndx[s];						
						} else {
							// TODO: right now removing users who are in train but not in trust
							removedEntriesCount++;							
							break;
						}
					} 
					if (rowIndexCounter == 1) {	
						item = Convert.ToInt32(s);
					}
					if (rowIndexCounter == 2) {
						test.usersList.Add(user);
						test.itemsList.Add(item);
						test.ratingsList.Add(Convert.ToInt32(s));
					}
					rowIndexCounter++;
				}				
			}		

			Console.WriteLine("\nNum entries removed from test dataset: {0}\n", removedEntriesCount);
			writeToConsole("test.bin creation in progress");

			using (FileStream file = File.Create("test.bin"))
			{
				ProtoBuf.Serializer.Serialize(file, test);
			}

			writeToConsole("test.bin creation completed");			
			writeToConsole("test.bin data loading");

			Test t1;
			using (FileStream file = File.OpenRead("test.bin")) 
			{
				t1 = Serializer.Deserialize<Test>(file);
			}

			writeToConsole("test.bin data loaded");
			if (test.usersList[0] == t1.usersList[0]){
				Console.WriteLine("Serialization properly completed");
			} else {
				Console.WriteLine("Problem with serialization");
			}

			//Console.WriteLine("OrigUser => User[0]: {0}, User[end]: {1}", test.testUserItem[0][0], test.testUserItem[test.testUserItem.Count-1]);
			//Console.WriteLine("ActUser => User[0]: {0}, User[end]: {1}", t1.testUserItem[0][0], t1.testUserItem[t1.testUserItem.Count-1]);			
		}

		static void Main(string[] args)
		{			
			Dictionary<string, int> userIdToIndx = new Dictionary<string, int>();
//			Dictionary<string, int> movieIdToIndx = new Dictionary<string, int>();		

			//mapRatings("rating.txt");

			trustDataManipulation(ref userIdToIndx);

			ratingDataManipulation(ref userIdToIndx);

			testDataManipulation(ref userIdToIndx);

			Console.WriteLine("Done!");
		}
	}
}
