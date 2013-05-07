package matrixfactorization;

public class WeightedMRMF extends MatrixFactorization {

double[] wAlpha;
	
	double[] W;
	
	public void initialize(){
		super.initialize();
		
		double rest = (1-alpha)/((double) relations.length-1);
		wAlpha = new double[relations.length];
		wAlpha[0] = alpha;
		for(int rel = 1; rel < this.relations.length; rel++){
			wAlpha[rel] = rest;
		}
		
		W = new double[relations.length];
//		W[0]=0.1;
//		W[1]=0.9;
		for(int rel = 0; rel < this.relations.length; rel++){			
//			W[rel] = (double) 1/(this.numEntities[relations[rel][1]]*this.numEntities[relations[rel][2]]);	
//			W[rel] = (double) 10*data[rel].length/(this.numEntities[relations[rel][1]]*this.numEntities[relations[rel][2]]);
			W[rel] = (double) 10000/(this.numEntities[relations[rel][1]]*this.numEntities[relations[rel][2]]);
			System.out.println(W[rel]);
		}
		
//		for(int rel = 0; rel < this.relations.length; rel++){
//			W[rel] = (double)(1.0/data[rel].length);
//			System.out.println(W[rel]);
//		}
		double mod = 0;
		for(int rel = 0; rel < this.relations.length; rel++){
			mod += (double)W[rel]*W[rel];
		}
		mod = Math.sqrt(mod);
		for(int rel = 0; rel < this.relations.length; rel++){
			W[rel] /= mod;
			W[rel] += 1;
			System.out.println(W[rel]);
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

				double error = W[rel]* (rating - predict(u,i, rel));

				for(int f = 0; f < getNumFeatures(); f++){

					double grad1 = 1*wAlpha[rel]*error*entityTypes[relations[rel][2]][i][f] - reg*entityTypes[relations[rel][1]][u][f];
					double grad2 = 1*wAlpha[rel]*error*entityTypes[relations[rel][1]][u][f] - reg*entityTypes[relations[rel][2]][i][f];

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
