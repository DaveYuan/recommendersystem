package matrixfactorization;

import data.RatingsData;

public class MultiRelationalMF extends MatrixFactorization {
	
	double[] wAlpha;

	public void initialize(){
		super.initialize();				
		wAlpha = new double[relations.length];		
		wAlpha[0] = 1;
		for(int rel = 1; rel < this.relations.length; rel++){
			wAlpha[rel] = Math.exp(-alpha);
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
				int i =(int)data[rel][row][1];
				double rating = data[rel][row][2];

				doStep(rel, u, i, rating);
			}
		}

	}

	private void doStep(int relation, int user, int item, double rating) {
		double sigm = sig(dot(user,item, relation));
		double prediction = relations[relation][RatingsData.MIN] + sigm * relations[relation][RatingsData.RANGE];
		double error = wAlpha[relation] * (rating - prediction);
		
		double commonGrad = error*sigm*(1-sigm)*relations[relation][RatingsData.RANGE];
		
		for(int f = 0; f < getNumFeatures(); f++){

			double grad1 = 2*commonGrad*entityTypes[relations[relation][2]][item][f] - reg*entityTypes[relations[relation][1]][user][f];
			double grad2 = 2*commonGrad*entityTypes[relations[relation][1]][user][f] - reg*entityTypes[relations[relation][2]][item][f];

			entityTypes[relations[relation][1]][user][f] += getLearnRate() * grad1;
			entityTypes[relations[relation][2]][item][f] += getLearnRate() * grad2;				
		}
	}

	@Override
	public double predict(int u, int i, int rel) {
		double pred = 0;
		
		for(int k = 0; k < getNumFeatures(); k++){
			pred += entityTypes[relations[rel][1]][u][k]*entityTypes[relations[rel][2]][i][k];
		}
		
		return relations[rel][RatingsData.MIN] + sig(pred) * relations[rel][RatingsData.RANGE];
	}
	
	private double dot(int u, int i, int rel) {
		double pred = 0;
		
		for(int k = 0; k < getNumFeatures(); k++){
			pred += entityTypes[relations[rel][1]][u][k]*entityTypes[relations[rel][2]][i][k];
		}
		
		return pred;
	}
	
	private double sig(double d){
		return 1 / (1 + Math.exp(-d));
	}

}
