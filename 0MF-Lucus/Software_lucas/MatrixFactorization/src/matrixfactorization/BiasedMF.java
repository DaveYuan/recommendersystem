package matrixfactorization;


public class BiasedMF extends MatrixFactorization {	

	double[] userBias, itemBias;
	
	@Override
	public double error(int u, int i, int targetRow) {
		// TODO Auto-generated method stub
		return 0;
	}

	@Override
	public void initialize() {
		super.initialize();						
		
		userBias = new double[dimensions[USER]];
		itemBias = new double[dimensions[ITEM]];
		
		//counts for user and item ratings
		int[] userRatings = new int[dimensions[USER]];
		int[] itemRatings = new int[dimensions[ITEM]];
		
		for(int u = 0; u < userBias.length; u++){
			userBias[u] = 0;			
			userRatings[u] = 0;
		}
		
		for(int i = 0; i < itemBias.length; i++){
			itemBias[i] = 0;			
			itemRatings[i] = 0;
		}
		
		for(int row = 0; row < data.length; row++){
			userBias[data[row][USER]] += data[row][2];			
			userRatings[data[row][USER]]++;
			
			itemBias[data[row][ITEM]] += data[row][2];			
			itemRatings[data[row][ITEM]]++;
		}
		
		for(int u = 0; u < userBias.length; u++){
			userBias[u] = ((double)userBias[u]/(double) userRatings[u]) - globalAverage;
			
		}
		for(int i = 0; i < itemBias.length; i++){
			itemBias[i] = ((double)itemBias[i]/(double) itemRatings[i]) - globalAverage;
			
		}				
		

	}

	@Override
	public void iterate() {
//		int numSamples = (int)(data.length*samples);
		for(int row = 0; row < data.length; row++){

//			int row = (int) (Math.random() * (data.length-1));
			int u = data[row][USER];
			int i = data[row][ITEM];
			int rating = data[row][2];
			
			double error = rating - predict(u,i);
			
			for(int f = 0; f < getNumFeatures(); f++){

				double grad1 = error*factor2[i][f] - reg*factor1[u][f];
				double grad2 = error*factor1[u][f] - reg*factor2[i][f];

				factor1[u][f] += getLearnRate() * grad1;
				factor2[i][f] += getLearnRate() * grad2;
			}
			double grad3 = error - reg*userBias[u];
			double grad4 = error - reg*itemBias[i];
			
				
			userBias[u] += getLearnRate() * grad3;
			itemBias[i] += getLearnRate() * grad4;
		
			
		}
		
		

	}

	@Override
	public double predict(int u, int i) {
		double pred = 0;
		
		if(u >= dimensions[USER] || i >= dimensions[ITEM]){
			return globalAverage;
		}
		
		for(int k = 0; k < getNumFeatures(); k++){
			pred += factor1[u][k]*factor2[i][k];
		}
		return itemBias[i] + userBias[u] + pred;
	}
	

}
