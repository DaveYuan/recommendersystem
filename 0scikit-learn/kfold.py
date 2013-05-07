import sys
import numpy as np
from sklearn import cross_validation
from scipy.sparse import coo_matrix
from scipy.io import mmread, mmwrite
from array import *

users = np.array([line.strip() for line in open('extra/users/users')])
movies = np.array([line.strip() for line in open('extra/movies/movies')])
ratings = np.array([line.strip() for line in open('extra/ratings/ratings')])
sparse_matrix=coo_matrix((ratings,(users,movies)), shape=(99999999,99999999), dtype=np.double)

kf = cross_validation.KFold(len(np.atleast_1d(users)), n_folds=5, shuffle=True)
print(kf)

index=0
for train_index, probe_index in kf:
	index=index+1
	train=open('extra/train'+str(index),'r+')
	test=open('extra/test'+str(index),'r+')
	probe=open('extra/probe'+str(index),'r+')

	for i in train_index:
		out=''
		out += str(users[i]) + '\t' + str(movies[i]) + '\t' + str(ratings[i]) + '\n';
		train.write(out)
	for i in probe_index:
		out=''
		out += str(users[i]) + '\t' + str(movies[i]) + '\t' + str(ratings[i]) + '\n';
		probe.write(out)
		r = ratings[i]
		if r == "5":
			test.write(out)


#train, probe = train_test_split(sparse_matrix, test_size=0.25, random_state=42)

#probe=coo_matrix(probe)
#test=open('extra/test','r+')

#for i,j,v in zip(probe.row, probe.col, probe.data):
#	if v == 5:
#		out =''
#		out += str(i+1) + '\t' + str(j+1) + '\t' + str(v) + '\n'
#		test.write(out)

#mmwrite('extra/train', train)
#mmwrite('extra/probe', probe)
