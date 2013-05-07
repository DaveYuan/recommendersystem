import sys
import numpy as np
from sklearn.cross_validation import train_test_split
from scipy.sparse import coo_matrix
from scipy.io import mmread, mmwrite

users = np.array([line.strip() for line in open('extra/users/users')])
movies = np.array([line.strip() for line in open('extra/movies/movies')])
ratings = np.array([line.strip() for line in open('extra/ratings/ratings')])
sparse_matrix=coo_matrix((ratings,(users,movies)), shape=(6045,6045), dtype=np.int32)

#docs_train, docs_test, y_train, y_test = train_test_split(dataset.data, dataset.target, test_size=0.25, random_state=42)
a, b = train_test_split(sparse_matrix, test_size=0.25, random_state=42)

mmwrite('extra/train', a)
mmwrite('extra/test', b)
