using System;
using System.Linq;
using System.Collections.Generic;

namespace socialMFv1
{
	class SparseMatrix
	{
		public List<HashSet<int>> sparseMatrix = new List<HashSet<int>>();
		
		public void createSparseMatrix(int[] userList1, int[] userList2) 
		{
			int user1;
			int user2;
			int listSize = userList1.Length;
			
			for (int i = 0; i < listSize; i++) {
				user1 = userList1[i];
				user2 = userList2[i];
				if (user1 >= sparseMatrix.Count) {
					for (int n = sparseMatrix.Count; n <= user1; n++) {
						sparseMatrix.Add(new HashSet<int>());
					}
				}
				sparseMatrix[user1].Add(user2);
			}
		}			
		
		public int numRows {
			get {
				return sparseMatrix.Count;
			}
		}
		
		public int numColumns {
			get {
				int maxCol = -1;
				foreach (var row in sparseMatrix) {
					if (row.Count > 0) {
						maxCol = Math.Max(maxCol, row.Max());
					}
				}
				return maxCol+1;
			}
		}
		
		public List<HashSet<int>> transpose() {
			var transpose = new List<HashSet<int>>();
			
			for (int i = 0; i < sparseMatrix.Count; i++) {
				foreach (int j in sparseMatrix[i]) {
					if (j >= transpose.Count) {
						for (int n = transpose.Count; n <= j; n++) {
						transpose.Add(new HashSet<int>());					
						}
					transpose[j].Add(i);
					}
				}
			}			
			return transpose;
		}
			
	}
}
