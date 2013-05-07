import sys
import numpy as np
from sklearn.cross_validation import train_test_split
from scipy.sparse import coo_matrix
from scipy.io import mmread, mmwrite
from array import *

users = np.array([line.strip() for line in open('extra/users/users')])
movies = np.array([line.strip() for line in open('extra/movies/movies')])
ratings = np.array([line.strip() for line in open('extra/ratings/ratings')])
sparse_matrix=coo_matrix((ratings,(users,movies)), shape=(6045,6045), dtype=np.int32)

train, probe = train_test_split(sparse_matrix, test_size=0.25, random_state=42)

probe=coo_matrix(probe)
test=open('extra/train-test/test.txt.mtx','r+')

for i,j,v in zip(probe.row, probe.col, probe.data):
	if v == 5:
		out =''
		out += str(i+1) + '\t' + str(j+1) + '\t' + str(v) + '\n'
		test.write(out)

mmwrite('extra/train-test/train.txt', train)
mmwrite('extra/train-test/probe.txt', probe)
