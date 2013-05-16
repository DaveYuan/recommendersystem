using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace mldatamanipulation
{
	class MainClass
	{
		static void writeToConsole(string mssg) 
		{			
			Console.WriteLine("\t" + mssg);
		}
		
		static string[] readAllLines(string fileName)
		{
			return System.IO.File.ReadAllLines(fileName);
		}									
		
		static void ratingDataManipulation(ref Dictionary<string, int> userIdToIndx,
		                                   ref Dictionary<string, int> movieIdToIndx,
		                                   ref List<int> uniqueItemList,
		                                   ref Dictionary<int, List<int>> itemsRatedPerUser)
		{
			Train rating = new Train();
			rating.usersList = new List<int>();
			rating.itemsList = new List<int>();
			rating.ratingsList = new List<int>();						
			
			writeToConsole("u1.base data loading");
			
			int user = 0;
			int userIdIndx = 0;
			int movieIdIndx = 0;
			int rowIndexCounter;
			string[] ratings = readAllLines("u1.base");			
			
			foreach (string line in ratings) {
				string[] stringSeparator = new string[] { "\t" };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{						
					if (rowIndexCounter == 0) {					
						if (!userIdToIndx.ContainsKey(s)) {																					
							userIdToIndx.Add(s, userIdIndx);
							userIdIndx++;					
						}
						user = userIdToIndx[s];
						rating.usersList.Add(user);
					} 
					if (rowIndexCounter == 1) {						
						if (!movieIdToIndx.ContainsKey(s)) {
							movieIdToIndx.Add(s, movieIdIndx);
							movieIdIndx++;							
						}
						rating.itemsList.Add(movieIdToIndx[s]);
						if (!uniqueItemList.Contains(movieIdToIndx[s])) {
							uniqueItemList.Add(movieIdToIndx[s]);
						}
						if (itemsRatedPerUser.ContainsKey(user)) {
							itemsRatedPerUser[user].Add(movieIdToIndx[s]);
						} else {
							List<int> tmp = new List<int>();
							tmp.Add(movieIdToIndx[s]);
							itemsRatedPerUser.Add(user, tmp);
						}
					}
					if (rowIndexCounter == 2) {
						rating.ratingsList.Add(Convert.ToInt32(s));
					}						
					rowIndexCounter++;
				}
			}		
			
			writeToConsole("u1.base data loaded");						           		
			//Console.WriteLine("\nNum entries removed from train dataset: {0}\n", removedEntriesCount);
			writeToConsole("u1.base.bin creation in progress");
			
			using (FileStream file = File.Create("u1.base.bin"))
			{
				Serializer.Serialize(file, rating);
			}
			
			writeToConsole("u1.base.bin creation completed");			
			writeToConsole("u1.base.bin data loading");

			Train r1;
			using (FileStream file = File.OpenRead("u1.base.bin"))
			{
				r1 = Serializer.Deserialize<Train>(file);
			}
			           
			writeToConsole("u1.base.bin data loaded");			
			Console.WriteLine("OrigUser => User[0]: {0}, User[end]: {1}", rating.usersList[0], rating.usersList[rating.usersList.Count-1]);
			Console.WriteLine("ActUser => User[0]: {0}, User[end]: {1}", r1.usersList[0], r1.usersList[r1.usersList.Count-1]);
			
			Console.WriteLine("OrigItem => Item[0]: {0}, Item[end]: {1}", rating.itemsList[0] , rating.itemsList[rating.itemsList.Count-1]);
			Console.WriteLine("Actitem => Item[0]: {0}, Item[end]: {1}", r1.itemsList[0] , r1.itemsList[r1.itemsList.Count-1]);
			
			Console.WriteLine("OrigRating => Rating[0]: {0}, Rating[end]: {1}", rating.ratingsList[0] , rating.ratingsList[rating.ratingsList.Count-1]);			
			Console.WriteLine("ActRating => Rating[0]: {0}, Rating[end]: {1}", r1.ratingsList[0] , r1.ratingsList[r1.ratingsList.Count-1]);	
		}
		
		static void testDataManipulation(ref Dictionary<string, int> userIdToIndx,
		                                   ref Dictionary<string, int> movieIdToIndx)
		{
			Train rating = new Train();
			rating.usersList = new List<int>();
			rating.itemsList = new List<int>();
			rating.ratingsList = new List<int>();						
			
			writeToConsole("u1.test data loading");
			
			int user = 0;
			int userIdIndx = 0;
			int movieIdIndx = 0;
			int rowIndexCounter;
			string[] ratings = readAllLines("u1.test");			
			
			foreach (string line in ratings) {
				string[] stringSeparator = new string[] { "\t" };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{						
					if (rowIndexCounter == 0) {					
						if (!userIdToIndx.ContainsKey(s)) {																					
							continue;					
						}
						user = userIdToIndx[s];
						rating.usersList.Add(user);
					} 
					if (rowIndexCounter == 1) {						
						if (!movieIdToIndx.ContainsKey(s)) {
							continue;							
						}
						rating.itemsList.Add(movieIdToIndx[s]);						
					}
					if (rowIndexCounter == 2) {
						rating.ratingsList.Add(Convert.ToInt32(s));
					}						
					rowIndexCounter++;
				}
			}		
			
			writeToConsole("u1.test data loaded");						           		
			//Console.WriteLine("\nNum entries removed from train dataset: {0}\n", removedEntriesCount);
			writeToConsole("u1.test.bin creation in progress");
			
			using (FileStream file = File.Create("u1.test.bin"))
			{
				Serializer.Serialize(file, rating);
			}
			
			writeToConsole("u1.test.bin creation completed");			
			writeToConsole("u1.test.bin data loading");

			Train r1;
			using (FileStream file = File.OpenRead("u1.test.bin"))
			{
				r1 = Serializer.Deserialize<Train>(file);
			}
			           
			writeToConsole("u1.test.bin data loaded");			
			Console.WriteLine("OrigUser => User[0]: {0}, User[end]: {1}", rating.usersList[0], rating.usersList[rating.usersList.Count-1]);
			Console.WriteLine("ActUser => User[0]: {0}, User[end]: {1}", r1.usersList[0], r1.usersList[r1.usersList.Count-1]);
			
			Console.WriteLine("OrigItem => Item[0]: {0}, Item[end]: {1}", rating.itemsList[0] , rating.itemsList[rating.itemsList.Count-1]);
			Console.WriteLine("Actitem => Item[0]: {0}, Item[end]: {1}", r1.itemsList[0] , r1.itemsList[r1.itemsList.Count-1]);
			
			Console.WriteLine("OrigRating => Rating[0]: {0}, Rating[end]: {1}", rating.ratingsList[0] , rating.ratingsList[rating.ratingsList.Count-1]);			
			Console.WriteLine("ActRating => Rating[0]: {0}, Rating[end]: {1}", r1.ratingsList[0] , r1.ratingsList[r1.ratingsList.Count-1]);	
		}
		
		/*
		static void testDataManipulation(ref Dictionary<string, int> userIdToIndx,
		                                 ref Dictionary<string, int> movieIdToIndx,
		                                 ref List<int> uniqueItemList,
		                                 ref Dictionary<int, List<int>> itemsRatedPerUser)
		{						
			Test test = new Test();
			test.testUserItem = new List<string>();
				
			writeToConsole("u1.test data loading");
			
			int user = -1;
			int ratedItem = -1;
			int flag;
			int rowIndexCounter;
			int removedEntriesCount = 0;
			string[] ratings = readAllLines("u1.test");			
			
			//Rated items per user from test data			
			foreach (string line in ratings) {
				string[] stringSeparator = new string[] { "\t" };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				flag = 1;
				foreach (string s in result)
				{						
					if (rowIndexCounter == 0) {					
						if (userIdToIndx.ContainsKey(s)) {
							user = userIdToIndx[s];						
						} else {
							// TODO: right now removing users who are in test but not in train
							removedEntriesCount++;							
							break;
						}
					} 
					if (rowIndexCounter == 1) {	
						if (movieIdToIndx.ContainsKey(s)) {
							if (itemsRatedPerUser.ContainsKey(user)) {
								itemsRatedPerUser[user].Add(movieIdToIndx[s]);
							} else {
								List<int> tmp = new List<int>();
								tmp.Add(movieIdToIndx[s]);
								itemsRatedPerUser.Add(user, tmp);
							}
						} else {							
							break;
						}
					}										
					rowIndexCounter++;
				}								
			}		
			
			//test_mapped will have id->indx of user and item
			File.Open("test_mapped.txt", FileMode.Create).Close();
			
			foreach (string line in ratings) {
				string[] stringSeparator = new string[] { "\t" };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				flag = 1;
				foreach (string s in result)
				{						
					if (rowIndexCounter == 0) {					
						if (userIdToIndx.ContainsKey(s)) {
							user = userIdToIndx[s];						
						} else {
							// TODO: right now removing users who are in train but not in trust
							removedEntriesCount++;		
							flag = 0;
							break;
						}
					} 
					if (rowIndexCounter == 1) {	
						if (movieIdToIndx.ContainsKey(s)) {
							ratedItem = movieIdToIndx[s];
						} else {	
							flag = 0;
							break;
						}
					}										
					rowIndexCounter++;
				}		
				if (flag == 1) {
					File.AppendAllText("test_mapped.txt", user + "\t" + ratedItem);		
					string str;
					str = user + "\t" + ratedItem;
					//testList.Add(user);
					//testList.Add(ratedItem);
					
					int indx;
					int itemCnt = 1000;
					int tmpCnt = 0;
					int pseudoNonRatedItem;
					int len = uniqueItemList.Count;
					List<int> indxUsed = new List<int>();
					Random r = new Random();

					while(true) {
						do {
							indx = r.Next(0, len);
						} while(indxUsed.Contains(indx));

						indxUsed.Add(indx);
						pseudoNonRatedItem = uniqueItemList[indx];

						if (!itemsRatedPerUser[user].Contains(pseudoNonRatedItem)) {
							if (tmpCnt < itemCnt) {
								File.AppendAllText("test_mapped.txt", "\t" + pseudoNonRatedItem);
								str = str + "\t" + pseudoNonRatedItem;
								tmpCnt++;
							} else {
								File.AppendAllText("test_mapped.txt", "\n");
								break;
							}
						}
					}
					test.testUserItem.Add(str);				
				}
			}		
			
			writeToConsole("test_mapped.txt data loaded");						           		
			Console.WriteLine("\nNum entries removed from test dataset: {0}\n", removedEntriesCount);
			writeToConsole("u1.test.bin creation in progress");
			
			using (FileStream file = File.Create("u1.test.bin"))
			{
				Serializer.Serialize(file, test);
			}
			
			writeToConsole("u1.test.bin creation completed");			
			writeToConsole("u1.test.bin data loading");
			
			Test t1;
			using (FileStream file = File.OpenRead("u1.test.bin")) 
			{
				t1 = Serializer.Deserialize<Test>(file);
			}
			
			writeToConsole("u1.test.bin data loaded");
			if (test.testUserItem[0].Equals(t1.testUserItem[0])){
				Console.WriteLine("Serialization properly completed");
			} else {
				Console.WriteLine("Problem with serialization");
			}
					
			//Console.WriteLine("OrigUser => User[0]: {0}, User[end]: {1}", test.testUserItem[0][0], test.testUserItem[test.testUserItem.Count-1]);
			//Console.WriteLine("ActUser => User[0]: {0}, User[end]: {1}", t1.testUserItem[0][0], t1.testUserItem[t1.testUserItem.Count-1]);			
		}*/
		
		static void Main(string[] args)
        {			
			Dictionary<string, int> userIdToIndx = new Dictionary<string, int>();
			Dictionary<string, int> movieIdToIndx = new Dictionary<string, int>();
			Dictionary<int, List<int>> itemsRatedPerUser = new Dictionary<int, List<int>>();
			List<int> uniqueItemList = new List<int>();
						
	//		trustDataManipulation(ref userIdToIndx);
			
			ratingDataManipulation(ref userIdToIndx,
			                       ref movieIdToIndx,
			                       ref uniqueItemList,
			                       ref itemsRatedPerUser);
			
			testDataManipulation(ref userIdToIndx,
			                     ref movieIdToIndx);
			
//			testDataManipulation(ref userIdToIndx,
//			                     ref movieIdToIndx,
//			                     ref uniqueItemList,
//			                     ref itemsRatedPerUser);
			
			Console.WriteLine("Done!");
        }
    }
}

/*
 * 	static void trustDataManipulation(ref Dictionary<string, int> userIdToIndx) 
		{
			Trust trust = new Trust();
			trust.trustUserList1 = new List<int>();
			trust.trustUserList2 = new List<int>();
			
			writeToConsole("trust.txt data loading");
			
			int rowIndexCounter;
			int userIdIndx = 0;
			string[] links = readAllLines("trust.txt");
			
			foreach (string line in links) {
				string[] stringSeparator = new string[] { "\t" };			
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
				
                Serializer.Serialize(file, trust);                
            }
			
			writeToConsole("trust.bin creation completed");	
			writeToConsole("trust.bin data loading");

            Trust t1;
            using (FileStream file = File.OpenRead("trust.bin"))
            {
                t1 = Serializer.Deserialize<Trust>(file);
            }
			
			writeToConsole("trust.bin data loaded");
			
			Console.WriteLine("OrigList1 => List1[0]: {0}, List1[end]: {1}", trust.trustUserList1[0], trust.trustUserList1[trust.trustUserList1.Count-1]);
			Console.WriteLine("ActList1 => List1[0]: {0}, List1[end]: {1}", t1.trustUserList1[0], t1.trustUserList1[t1.trustUserList1.Count-1]);
            
            Console.WriteLine("OrigList2 => List2[0]: {0}, List2[end]: {1}", trust.trustUserList2[0], trust.trustUserList2[trust.trustUserList2.Count-1]);            		
			Console.WriteLine("ActList2 => List2[0]: {0}, List2[end]: {1}", t1.trustUserList2[0], t1.trustUserList2[t1.trustUserList2.Count-1]);            			
		}
 */
