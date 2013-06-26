using System;
using System.IO;

namespace MultiRecommender.IO
{
	public static class MatrixExtension
	{	
		public static void ReadVector(StreamReader reader, int type)
		{
			int size = int.Parse(reader.ReadLine());

			if (type == Initialize.USER) {
				if (size > Initialize.MAX_USER_ID+1) {
					throw new IOException(string.Format(
						"UserBias(SizeMismatch): size > MAX_USER_ID+1, {0}:{1}", size, Initialize.MAX_USER_ID+1));
				}
				for (int i = 0; i < size; i++) {
					Initialize.userBias[i] = Double.Parse(reader.ReadLine());
				}	
			} else if (type == Initialize.ITEM) {
				if (size > Initialize.MAX_ITEM_ID+1) {
					throw new IOException(string.Format(
						"ItemBias(SizeMismatch): size > MAX_ITEM_ID+1, {0}:{1}", size, Initialize.MAX_ITEM_ID+1));
				}
				for (int i = 0; i < size; i++) {
					Initialize.itemBias[i] = Double.Parse(reader.ReadLine());
				}
			}
		}

		public static void WriteVector(this StreamWriter writer, int type)
		{
			if (type == Initialize.USER) {
				writer.WriteLine(Initialize.MAX_USER_ID+1);
				for (int j = 0; j <= Initialize.MAX_USER_ID; j++) {
					writer.WriteLine(Initialize.userBias[j].ToString());
				}
			} else if (type == Initialize.ITEM) {
				writer.WriteLine(Initialize.MAX_ITEM_ID+1);
				for (int j = 0; j <= Initialize.MAX_ITEM_ID; j++) {
					writer.WriteLine(Initialize.itemBias[j].ToString());
				}
			}
		}

		// Remember: I store file in a different format than the internal matrix to make it readable
		// Format_USER: numUsers, numFeatures 
		//        (userId, featureId, featureVal)
		// Format_ITEM: numItems, numFeatures 
		//        (itemId, featureId, featureVal)
		public static void ReadMatrix(StreamReader reader, int type)
		{
			string[] line = reader.ReadLine().Split(' ');

			int dim2 = int.Parse(line[0]);
			int dim1 = int.Parse(line[1]);
		
			while ((line = reader.ReadLine().Split(' ')).Length == 3) {
				int j = int.Parse(line[0]);
				int i = int.Parse(line[1]);
				double val = Double.Parse(line[2]);

				if (i >= dim1) 
					throw new IOException("i = " + i + " >= " + dim1);
				if ( j >= dim2) 
					throw new IOException("j = " + j + " >= " + dim2);
				if (type == Initialize.USER) {
					Initialize.userFeature[i,j] = val;
				} else if (type == Initialize.ITEM) {
					Initialize.itemFeature[i,j] = val;
				}
			}
		}

		public static void WriteMatrix(this StreamWriter writer, int type)
		{
			if (type == Initialize.USER) {
				writer.WriteLine(Initialize.MAX_USER_ID+1 + " " + Initialize.numFeatures);
				for (int j = 0; j <= Initialize.MAX_USER_ID; j++) {
					for (int i = 0; i < Initialize.numFeatures; i++) {
						writer.WriteLine(j + " " + i + " " + Initialize.userFeature[i,j].ToString());
					}
				}
			} else if (type == Initialize.ITEM) {			
				writer.WriteLine(Initialize.MAX_ITEM_ID+1 + " " + Initialize.numFeatures);
				for (int j = 0; j <= Initialize.MAX_ITEM_ID; j++) {
					for (int i = 0; i < Initialize.numFeatures; i++) {
						writer.WriteLine(j + " " + i + " " + Initialize.itemFeature[i,j].ToString());
					}
				}
			}
			writer.WriteLine();
		}
	}
}