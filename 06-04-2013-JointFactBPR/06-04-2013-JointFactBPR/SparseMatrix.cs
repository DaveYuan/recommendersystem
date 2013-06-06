using System;
using System.Linq;
using System.Collections.Generic;

namespace JointFactBPR
{
	public class SparseMatrix
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
		
		public List<HashSet<int>> transpose() 
		{
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
			
		public List<HashSet<int>> symmetricMatrix()
		{
			var symmetricMatrix = new List<HashSet<int>>();
			
			for (int i = 0; i < sparseMatrix.Count; i++) {
				if (i >= symmetricMatrix.Count) {
					for (int n = symmetricMatrix.Count; n <= i; n++) {
						symmetricMatrix.Add(new HashSet<int>());						
					}
				}
				foreach (int j in sparseMatrix[i]) {
					symmetricMatrix[i].Add(j);
					if (j >= symmetricMatrix.Count) {
						for (int n = symmetricMatrix.Count; n <= j; n++) {
							symmetricMatrix.Add(new HashSet<int>());
						}
					}
					symmetricMatrix[j].Add(i);
				}
			}
		
			return symmetricMatrix;				
		}
	}
}
//			var transposeMatrix = transpose();
//			var symmetricMatrix = new List<HashSet<int>>();
//	
//			int frwdCnt;
//			int rvrsCnt;
//			int transpCnt = transposeMatrix.Count;
//			int sparseCnt = sparseMatrix.Count;
//			int MAX_INDX = Math.Max(sparseCnt, transpCnt) - 1;
//			
//			for (int i = 0; i <= MAX_INDX; i++) {
//				symmetricMatrix.Add(new HashSet<int>());
//				frwdCnt = (i < sparseCnt) ? sparseMatrix[i].Count : 0;
//				rvrsCnt = (i < transpCnt) ? transposeMatrix[i].Count : 0;
//				
//				for (int f = 0; f < frwdCnt; f++) {
//					symmetricMatrix[i].Add(sparseMatrix[i].ElementAt(f));
//				}
//				for (int r = 0; r < rvrsCnt; r++) {
//					symmetricMatrix[i].Add(transpose[i].ElementAt(f));
//				}
//			}