from sklearn import cross_validation
import numpy as np

X = np.array([[1, 2], [3, 4], [1, 2], [3, 4], [66,77],[99,33]])
kf = cross_validation.KFold(6, n_folds=5)
 
i=1
t=open('t'+str(i),'r+')
j=0;
for train_index, test_index in kf:
	j = j+1	
	print(j)
	print("TRAIN:", train_index, "TEST:", test_index)
    	X_train, X_test = X[train_index], X[test_index]
