using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

/*
 * Epinion Dataset
 */

namespace protobufnetdataset
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
		
		static void trustDataManipulation() 
		{
			Trust trust = new Trust();
			trust.trustUserList1 = new List<int>();
			trust.trustUserList2 = new List<int>();
			
			writeToConsole("trust.txt data loading");
			
			int rowIndexCounter;
			string[] links = readAllLines("trust.txt");
			
			foreach (string line in links) {
				string[] stringSeparator = new string[] { "\t" };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{						
					if (rowIndexCounter == 0) {					
						trust.trustUserList1.Add(Convert.ToInt32(s));
					} else {
						trust.trustUserList2.Add(Convert.ToInt32(s));
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
		
		static void ratingDataManipulation()
		{
			Rating rating = new Rating();
			rating.usersList = new List<int>();
			rating.itemsList = new List<int>();
			rating.ratingsList = new List<int>();						
			
			writeToConsole("train.txt data loading");
			
			int rowIndexCounter;
			string[] ratings = readAllLines("train.txt");			
			
			foreach (string line in ratings) {
				string[] stringSeparator = new string[] { "\t" };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{						
					if (rowIndexCounter == 0) {					
						rating.usersList.Add(Convert.ToInt32(s));
					} 
					if (rowIndexCounter == 1) {
						rating.itemsList.Add(Convert.ToInt32(s));
					}
					if (rowIndexCounter == 2) {
						rating.ratingsList.Add(Convert.ToInt32(s));
					}						
					rowIndexCounter++;
				}
			}		

			writeToConsole("train.txt data loaded");						           		
			writeToConsole("train.bin creation in progress");
			
			using (FileStream file = File.Create("train.bin"))
			{
				Serializer.Serialize(file, rating);
			}
			
			writeToConsole("train.bin creation completed");			
			writeToConsole("train.bin data loading");

			Rating r1;
			using (FileStream file = File.OpenRead("train.bin"))
			{
				r1 = Serializer.Deserialize<Rating>(file);
			}
			           
			writeToConsole("train.bin data loaded");			
			Console.WriteLine("OrigUser => User[0]: {0}, User[end]: {1}", rating.usersList[0], rating.usersList[rating.usersList.Count-1]);
			Console.WriteLine("ActUser => User[0]: {0}, User[end]: {1}", r1.usersList[0], r1.usersList[r1.usersList.Count-1]);
			
			Console.WriteLine("OrigItem => Item[0]: {0}, Item[end]: {1}", rating.itemsList[0] , rating.itemsList[rating.itemsList.Count-1]);
			Console.WriteLine("Actitem => Item[0]: {0}, Item[end]: {1}", r1.itemsList[0] , r1.itemsList[r1.itemsList.Count-1]);
			
			Console.WriteLine("OrigRating => Rating[0]: {0}, Rating[end]: {1}", rating.ratingsList[0] , rating.ratingsList[rating.ratingsList.Count-1]);			
			Console.WriteLine("ActRating => Rating[0]: {0}, Rating[end]: {1}", r1.ratingsList[0] , r1.ratingsList[r1.ratingsList.Count-1]);	
		}
		
		static void Main(string[] args)
        {			
			trustDataManipulation();					
			ratingDataManipulation();
        }
    }

}