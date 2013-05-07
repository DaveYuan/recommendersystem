package matrixfactorization;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;
import data.RatingsData;

public class SamplingMultiRelationalMF extends MatrixFactorization {
	
	double[] wAlpha;
	TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>> denseRelations;

	public void initialize(){
		super.initialize();				
		wAlpha = new double[relations.length];		
		wAlpha[0] = 1;
		for(int rel = 1; rel < this.relations.length; rel++){
			wAlpha[rel] = Math.exp(-alpha);
		}		
				
		
		denseRelations = new TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>>();
		
		for(int i = 0; i < relations.length; i++){
			if(relations[i][RatingsData.SPARSE] == 1) continue;
			denseRelations.put(i, new TIntObjectHashMap<TIntHashSet>());
			for(int row = 0; row < data[i].length; row++){
				if(!denseRelations.get(i).containsKey( (int) data[i][row][0]) ){
					denseRelations.get(i).put((int) data[i][row][0], new TIntHashSet());
				}
				denseRelations.get(i).get((int) data[i][row][0]).add((int) data[i][row][1]);
			}
		}
		
		
	}	

	
	@Override
	public double error(int u, int i, int targetRow) {
		return 0;
	}

	@Override
	public void iterate() {
		
		
		for(int rel = relations.length-1; rel >= 0; rel--){			
			for(int row = 0; row < data[rel].length; row++){			
				int u = (int)data[rel][row][0];
				int i =(int)data[rel][row][1];
				double rating = data[rel][row][2];

				doStep(rel, u, i, rating);
				
//				if(denseRelations.containsKey(rel)){					
//					int newUser =(int) (Math.random()*entityTypes[relations[rel][1]].length);
//					int	newItem = -1;
//					do{
//						newItem = (int) (Math.random()*entityTypes[relations[rel][2]].length);
//					}while(denseRelations.get(rel).get(newUser).contains(newItem));
//												
//					doStep(rel, newUser, newItem, 0);
//				}
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
