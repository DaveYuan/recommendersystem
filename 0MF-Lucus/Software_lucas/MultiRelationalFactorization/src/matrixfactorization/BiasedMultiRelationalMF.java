package matrixfactorization;

public class BiasedMultiRelationalMF extends MatrixFactorization {

	double[] userBias, itemBias;

	@Override
	public void initialize() {
		super.initialize();

		userBias = new double[numEntities[0]];
		itemBias = new double[numEntities[1]];

		// counts for user and item ratings
		int[] userRatings = new int[numEntities[0]];
		int[] itemRatings = new int[numEntities[1]];

		for (int u = 0; u < userBias.length; u++) {
			userBias[u] = 0;
			userRatings[u] = 0;
		}

		for (int i = 0; i < itemBias.length; i++) {
			itemBias[i] = 0;
			itemRatings[i] = 0;
		}

		for (int row = 0; row < data[0].length; row++) {
			userBias[(int)data[0][row][0]] += data[0][row][2];
			userRatings[(int)data[0][row][0]]++;

			itemBias[(int)data[0][row][1]] += data[0][row][2];
			itemRatings[(int)data[0][row][1]]++;
		}

		for (int u = 0; u < userBias.length; u++) {
			if (userRatings[u] == 0) {
				userBias[u] = 0.5;
			} else {
				userBias[u] = ((double) userBias[u] / (double) userRatings[u])
						- globalAverage;
			}

		}
		for (int i = 0; i < itemBias.length; i++) {
			if (itemRatings[i] == 0) {
				itemBias[i] = 0.5;
			} else {
				itemBias[i] = ((double) itemBias[i] / (double) itemRatings[i])
						- globalAverage;
			}

		}

	}

	@Override
	public double error(int u, int i, int targetRow) {
		// TODO Auto-generated method stub
		return 0;
	}

	@Override
	public void iterate() {
		for (int rel = 0; rel < this.relations.length; rel++) {
			for (int row = 0; row < data[rel].length; row++) {
				int u = (int) data[rel][row][0];
				int i = (int) data[rel][row][1];
				double rating = data[rel][row][2];

				double error = rating - predict(u, i, rel);

				for (int f = 0; f < getNumFeatures(); f++) {

					double grad1 = 2 * error
							* entityTypes[relations[rel][2]][i][f] - reg
							* entityTypes[relations[rel][1]][u][f];
					double grad2 = 2 * error
							* entityTypes[relations[rel][1]][u][f] - reg
							* entityTypes[relations[rel][2]][i][f];

					entityTypes[relations[rel][1]][u][f] += getLearnRate()
							* grad1;
					entityTypes[relations[rel][2]][i][f] += getLearnRate()
							* grad2;
				}
				if (rel == 0) {
					double grad3 = error - reg * userBias[u];
					double grad4 = error - reg * itemBias[i];

					userBias[u] += getLearnRate() * grad3;

					itemBias[i] += getLearnRate() * grad4;

					
				}
			}
		}

	}

	@Override
	public double predict(int u, int i, int rel) {
		double pred = 0;

		for (int k = 0; k < getNumFeatures(); k++) {
			pred += entityTypes[relations[rel][1]][u][k]
					* entityTypes[relations[rel][2]][i][k];
		}

		if (rel == 0)
			return itemBias[i] + userBias[u] + pred;
		else
			return pred;
	}

}
