package matrixfactorization;

public class ShrunkBiasedMultiRelationalMF extends BiasedMultiRelationalMF {

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
				userBias[u] = ((globalAverage * getShrinkage() + (double) userBias[u]) 
                             / ((double) userRatings[u] + getShrinkage()))
						- globalAverage;
			}

		}
		for (int i = 0; i < itemBias.length; i++) {
			if (itemRatings[i] == 0) {
				itemBias[i] = 0.5;
			} else {
				itemBias[i] = ((globalAverage * getShrinkage() + (double) itemBias[i])
				             / ((double) itemRatings[i]) + getShrinkage())
						- globalAverage;
			}

		}

	}
}
