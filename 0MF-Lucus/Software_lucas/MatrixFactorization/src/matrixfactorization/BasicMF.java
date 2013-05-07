package matrixfactorization;



public class BasicMF extends MatrixFactorization {
	
	

	public BasicMF(){
	}
	
	
	
	@Override
	public void iterate(){
//		int numSamples = (int)(data.length*samples);
		for(int row = 0; row < data.length; row++){

//			int row = (int) (Math.random() * (data.length-1));
			int u = data[row][USER];
			int i = data[row][ITEM];
			int rating = data[row][2];
			
			double error = rating - predict(u,i);
			
			for(int f = 0; f < getNumFeatures(); f++){

				double grad1 = error*factor2[i][f] - reg*factor1[u][f];;
				double grad2 = error*factor1[u][f] - reg*factor2[i][f];
	
				factor1[u][f] += getLearnRate() * grad1;
				factor2[i][f] += getLearnRate() * grad2;				
			}
		}

	}
	
	
	@Override
	public double error(int u, int i, int targetRow){
		double e = 0;
		for(int k = 0; k < getNumFeatures(); k++){
			e += factor1[u][k]*factor2[i][k];
		}
		return data[targetRow][2] - e;
	}	
	
	
	@Override
	public double predict(int u, int i){
		double pred = 0;
		if(u >= dimensions[USER] || i >= dimensions[ITEM]){
			return globalAverage;
		}
		for(int k = 0; k < getNumFeatures(); k++){
			pred += factor1[u][k]*factor2[i][k];
		}
		return pred;
	}

}
