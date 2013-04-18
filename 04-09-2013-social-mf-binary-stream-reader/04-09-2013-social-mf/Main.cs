using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace socialmf
{
	class MainClass
	{
		public Dictionary<string, List<string>> trust {get; private set;}
		
		public void readAndLoadTrustData(string trustFileName)
		{
			string user1;
			string user2;
			
			using (BinaryReader links = new BinaryReader(File.Open(trustFileName, FileMode.Open)))
			{					    
			    int pos = 0;			 
			    int length = (int)links.BaseStream.Length;				
				
			    while (pos < length)
			    {				
					try {					
						user1 = links.ReadString();		
						pos += user1.Length + 1;
																	
						user2 = links.ReadString();
						pos += user2.Length + 1;
						
						if (this.trust.ContainsKey(user1)) {
							this.trust[user1].Add(user2);
						} else {
							List<string> tmp = new List<string>();
							tmp.Add(user2);
							this.trust.Add(user1, tmp);
						}
						
						if (this.trust.ContainsKey(user2)) {
							this.trust[user2].Add(user1);
						} else {
							List<string> tmp = new List<string>();
							tmp.Add(user1);
							this.trust.Add(user2, tmp);
						}
					} catch(IOException e) {
						Console.WriteLine("Error: " + e.Message);
					}
			    }
			}			
		}
		
		public static void Main (string[] args)
		{
			MainClass mainclass = new MainClass();
			mainclass.trust = new Dictionary<string, List<string>>();
			
			mainclass.readAndLoadTrustData("trust.bin");						
		}
	}
}