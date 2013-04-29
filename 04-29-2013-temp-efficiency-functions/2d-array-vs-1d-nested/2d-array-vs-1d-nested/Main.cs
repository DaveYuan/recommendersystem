using System;

namespace darrayvs1dnested
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			int numUser = 49288;
			int numItem = 123204;
			int numFeature = 50;
			double[,] array2d = new double[numFeature, numItem];
			double[] array1d = new double[numItem];
			//array1d[] nestedArray2d = new array1d[numFeature];
			
			double[][] nestedArray2d = new double[numFeature][];
			
			Console.WriteLine ("Hello World!");
		}
	}
}
