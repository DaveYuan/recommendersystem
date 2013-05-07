package matrixfactorization;

public class ExperimentalMRMF extends MatrixFactorization {


	double[] wf;
	
	public void initialize(){
		super.initialize();
		
		double rest = (1-alpha)/((double) relations.length-1);
		wf = new double[relations.length];
		wf[0] = alpha;
		for(int rel = 0; rel < this.relations.length; rel++){
			wf[rel] = rest;
		}
	}
	@Override
	public double error(int u, int i, int targetRow) {
		return 0;
	}

	@Override
	public void iterate() {
		
		for(int rel = 0; rel < this.relations.length; rel++){			
			for(int row = 0; row < data[rel].length; row++){			
				int u = (int)data[rel][row][0];
				int i = (int)data[rel][row][1];
				double rating = data[rel][row][2];

				double error = rating - predict(u,i, rel);

				for(int f = 0; f < getNumFeatures(); f++){

					double grad1 = 2*wf[rel]*error*entityTypes[relations[rel][2]][i][f] - reg*entityTypes[relations[rel][1]][u][f];
					double grad2 = 2*wf[rel]*error*entityTypes[relations[rel][1]][u][f] - reg*entityTypes[relations[rel][2]][i][f];

					entityTypes[relations[rel][1]][u][f] += getLearnRate() * grad1;
					entityTypes[relations[rel][2]][i][f] += getLearnRate() * grad2;				
				}
			}
		}


	}

	@Override
	public double predict(int u, int i, int rel) {
		double pred = 0;
		
		for(int k = 0; k < getNumFeatures(); k++){
			pred += entityTypes[relations[rel][1]][u][k]*entityTypes[relations[rel][2]][i][k];
		}
		return pred;
	}
	
	
	

}

