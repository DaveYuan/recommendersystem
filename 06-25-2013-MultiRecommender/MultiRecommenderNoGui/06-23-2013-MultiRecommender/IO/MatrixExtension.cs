using System;
using System.IO;

namespace MultiRecommender.IO
{
	public static class MatrixExtension
	{
		// Remember: I store file in a different format than the internal matrix to make it readable
		// Format_USER: numUsers, numFeatures 
		//        (userId, featureId, featureVal)
		// Format_ITEM: numItems, numFeatures 
		//        (itemId, featureId, featureVal)
		public static void WriteMatrix(this StreamWriter writer, int type)
		{
			if (type == Init.USER) {
				writer.WriteLine(Init.MAX_USER_ID+1 + " " + Init.numFeatures);
				for (int j = 0; j <= Init.MAX_USER_ID; j++) {
					for (int i = 0; i < Init.numFeatures; i++) {
						writer.WriteLine(j + " " + i + " " + Init.userFeature[i,j].ToString());
					}
				}
				writer.WriteLine();
			} else if (type == Init.ITEM) {			
				writer.WriteLine(Init.MAX_ITEM_ID+1 + " " + Init.numFeatures);
				for (int j = 0; j <= Init.MAX_ITEM_ID; j++) {
					for (int i = 0; i < Init.numFeatures; i++) {
						writer.WriteLine(j + " " + i + " " + Init.itemFeature[i,j].ToString());
					}
				}
				writer.WriteLine();			
			}			                              
		}

		public static void WriteVector(this StreamWriter writer, int type)
		{
			if (type == Init.USER) {
				writer.WriteLine(Init.MAX_USER_ID+1);
				for (int j = 0; j <= Init.MAX_USER_ID; j++) {
					writer.WriteLine(Init.userBias[j].ToString());
				}
				writer.WriteLine();
			} else if (type == Init.ITEM) {
				writer.WriteLine(Init.MAX_ITEM_ID+1);
				for (int j = 0; j <= Init.MAX_ITEM_ID; j++) {
					writer.WriteLine(Init.itemBias[j].ToString());
				}
				writer.WriteLine();
			}
		}
	}
}