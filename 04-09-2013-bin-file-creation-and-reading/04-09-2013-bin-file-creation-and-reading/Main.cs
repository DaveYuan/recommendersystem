using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace binfilecreationandreading
{
	class MainClass
	{
		static string[] readAllLines(string fileName)
		{
			return System.IO.File.ReadAllLines(fileName);
		}
		
		static void loadFile(string fileName1, 
		                     string fileName2,
		                     ref List<string> linkUser1,
		                     ref List<string> linkUser2,
		                     ref List<string> ratingUser,
		                     ref List<string> ratingItem,
		                     ref List<string> ratingRating)
		{
			int rowIndexCounter;
			string[] links = readAllLines(fileName1);
			string[] ratings = readAllLines(fileName2);			
			
//			foreach (string line in links) {
//				string[] stringSeparator = new string[] { "\t" };			
//				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
//				rowIndexCounter = 0;
//				
//				foreach (string s in result)
//				{						
//					if (rowIndexCounter == 0) {					
//						linkUser1.Add(s);
//					} else {
//						linkUser2.Add(s);
//					}
//					rowIndexCounter++;
//				}
//			}
			
			foreach (string line in ratings) {
				string[] stringSeparator = new string[] { "\t" };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{						
					if (rowIndexCounter == 0) {					
						ratingUser.Add(s);
					} 
					if (rowIndexCounter == 1) {
						ratingItem.Add(s);
					}
					if (rowIndexCounter == 2) {
						ratingRating.Add(s);
					}						
					rowIndexCounter++;
				}
			}			
		}
		
		public static void createBinFile(string fileName1,
		                                 string fileName2,
		                                 List<string> linkUser1,
		                     			 List<string> linkUser2,
		                     			 List<string> ratingUser,
		                                 List<string> ratingItem,
		                                 List<string> ratingRating)
		{
			int indx;
	
			string[] linkUser = linkUser2.ToArray();
			string[] item = ratingItem.ToArray();
			string[] rating = ratingRating.ToArray();
	
//			indx = 0;
//			using (BinaryWriter linkStream = new BinaryWriter(File.Open(fileName1, FileMode.Create)))
//			{
//	   			foreach (string s in linkUser1)
//	    		{					
//					linkStream.Write(s);					
//					linkStream.Write(linkUser[indx]);			
//					indx++;
//	    		}				
//			}
			
			indx = 0;
			using (BinaryWriter ratingStream = new BinaryWriter(File.Open(fileName2, FileMode.Create)))
			{
	   			foreach (string s in ratingUser)
	    		{					
					ratingStream.Write(s);
					ratingStream.Write(item[indx]);
					ratingStream.Write(rating[indx]);					
					indx++;
	    		}
			}					
		}
		
		static void readBinToTxt(string linksBin,
		                         string ratingsBin,
		                         string linksTxt,
		                         string ratingsTxt)
		{
			using (BinaryReader links = new BinaryReader(File.Open(linksBin, FileMode.Open)))
			{					    
			    int pos = 0;			 
			    int length = (int)links.BaseStream.Length;				
				
			    while (pos < length)
			    {				
					try {					
						string s = links.ReadString();		
						pos += s.Length + 1;
					//	Console.Write(s + " ");	
						System.IO.File.AppendAllText(linksTxt, s + "\t");
						
						s = links.ReadString();
						pos += s.Length + 1;
					//	Console.WriteLine(s);	
						System.IO.File.AppendAllText(linksTxt, s + "\n");
					} catch(IOException e) {
						Console.WriteLine("Error: " + e.Message);
					}
			    }
			}
			
			Console.WriteLine("Written Links");
			
			using (BinaryReader ratings = new BinaryReader(File.Open(ratingsBin, FileMode.Open)))
			{					    
			    int pos = 0;			 
			    int length = (int)ratings.BaseStream.Length;				
				
			    while (pos < length)
			    {				
					try {					
						string s = ratings.ReadString();		
						pos += s.Length + 1;
					//	Console.Write(s + " ");	
						System.IO.File.AppendAllText(ratingsTxt, s + "\t");
						
						s = ratings.ReadString();
						pos += s.Length + 1;
					//	Console.Write(s + " ");
						System.IO.File.AppendAllText(ratingsTxt, s + "\t");
						
						s = ratings.ReadString();
						pos += s.Length + 1;
					//	Console.WriteLine(s);	
						System.IO.File.AppendAllText(ratingsTxt, s + "\n");
					} catch(IOException e) {
						Console.WriteLine("Error: " + e.Message);
					}
			    }
			}
		}
		
		public static void Main (string[] args)
		{			
			List<string> linkUser1 = new List<string>();
			List<string> linkUser2 = new List<string>();
			List<string> ratingUser = new List<string>();
			List<string> ratingItem = new List<string>();
			List<string> ratingRating = new List<string>();
						
			loadFile("links.txt", 
			         "ratings.txt",
			         ref linkUser1,
			         ref linkUser2,
			         ref ratingUser,
			         ref ratingItem,
			         ref ratingRating);
			
			Console.WriteLine("Files loaded in memory");
			Console.WriteLine("{0}", ratingItem.Count);
			createBinFile("links.bin",
			              "ratings.bin",
			              linkUser1,
			              linkUser2,
			              ratingUser,
			              ratingItem,
			              ratingRating);
			
			Console.WriteLine(".bin created");
			
			//System.IO.File.Create("linksTxt.txt");
			//System.IO.File.Create("ratingsTxt.txt");
			
//			readBinToTxt("links.bin",
//			             "ratings.bin",
//			             "linksTxt.txt",
//			             "ratingsTxt.txt");
//			Console.WriteLine(".bin converted to .txt");
		}
	}
}