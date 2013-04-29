using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace privatepublicclassfield
{
	class MainClass
	{
		public List<int> publicList {get; set;}
		private List<int> privateList;
		
		MainClass() 
		{
			publicList = new List<int>(10000000);
			privateList = new List<int>(10000000);
			
			for (int i = 0; i < 10000000; i++) {
				publicList.Add(5);
				privateList.Add(5);
			}
		}
		
		public void testTime() {
			int tmp;
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			for (int z = 0; z < 100; z++) {
				for (int i = 0; i < 10000000; i++) {
					tmp = publicList[i];
				}
			}
			stopwatch.Stop();
			Console.WriteLine("Time(public_field): {0}", stopwatch.Elapsed);
			
			stopwatch = new Stopwatch();
			stopwatch.Start();
			for (int z = 0; z < 100; z++) {
				for(int i = 0; i < 10000000; i++) {
					tmp = privateList[i];
				}
			}
			stopwatch.Stop();
			Console.WriteLine("Time(private_field): {0}", stopwatch.Elapsed);
		}
		
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
			MainClass mainclass = new MainClass();
			mainclass.testTime();
		}
	}
}
