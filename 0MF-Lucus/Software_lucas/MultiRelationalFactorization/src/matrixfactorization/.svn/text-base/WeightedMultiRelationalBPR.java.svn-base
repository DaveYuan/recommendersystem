package matrixfactorization;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;



public class WeightedMultiRelationalBPR extends MatrixFactorization {

	int numSamples = 10000;
	
	double[] wAlpha;
	TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>> relationsSparseFormat;

	public void initialize(){
		super.initialize();		
		
		
		relationsSparseFormat = new TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>>();
		
		for(int i = 0; i < relations.length; i++){
			relationsSparseFormat.put(i, new TIntObjectHashMap<TIntHashSet>());
			for(int row = 0; row < data[i].length; row++){
				if(!relationsSparseFormat.get(i).containsKey( (int) data[i][row][0]) ){
					relationsSparseFormat.get(i).put((int) data[i][row][0], new TIntHashSet());
				}
				relationsSparseFormat.get(i).get((int) data[i][row][0]).add((int) data[i][row][1]);
			}
		}
		
		
	}	

	
	@Override
	public double error(int u, int i, int targetRow) {
		// TODO Auto-generated method stubExperimentalMF
		return 0;
	}

	@Override
	public void iterate() {
		for(int rel = relations.length-1; rel >= 0; rel--){			
			for(int s = 0; s < data[rel].length; s++){
				int row = (int) (Math.random()*data[rel].length);
				int u = (int)data[rel][row][0];
				int i =(int)data[rel][row][1];
				int	j = -1;
				do{
					j = (int) Math.floor(Math.random()*entityTypes[relations[rel][2]].length);
				}while(relationsSparseFormat.get(rel).get(u).contains(j));
				
		
				double xuij = predict(u, i, rel) - predict(u, j, rel);
				double e = 1 + Math.exp(xuij);			
				
				for(int f = 0; f < getNumFeatures(); f++){
					double grad1 = ((entityTypes[relations[rel][2]][i][f] - entityTypes[relations[rel][2]][j][f]) / e - reg*entityTypes[relations[rel][1]][u][f]);
					double grad2 = (entityTypes[relations[rel][1]][u][f] / e - reg*entityTypes[relations[rel][2]][i][f]);
					double grad3 = -(entityTypes[relations[rel][1]][u][f] / e - reg_neg*entityTypes[relations[rel][2]][j][f]);

					if(rel == 0){
						grad1*=(1-alpha);
						grad2*=alpha;
						grad3*=alpha;
					}
					entityTypes[relations[rel][1]][u][f] += getLearnRate() * grad1;
					entityTypes[relations[rel][2]][i][f] += getLearnRate() * grad2;				
					entityTypes[relations[rel][2]][j][f] += getLearnRate() * grad3;
				}
								
			}
		}			
	}

	@Override
	public double predict(int u, int i, int rel) {
//		return Math.random();
		double pred = 0;
		
		for(int k = 0; k < getNumFeatures(); k++){
			pred += entityTypes[relations[rel][1]][u][k]*entityTypes[relations[rel][2]][i][k];
		}
		
		return pred;
	}


}
