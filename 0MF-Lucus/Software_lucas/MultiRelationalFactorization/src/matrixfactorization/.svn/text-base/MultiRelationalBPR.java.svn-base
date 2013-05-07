package matrixfactorization;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;



public class MultiRelationalBPR extends MatrixFactorization {

	int numSamples = 0;
	
	double[] wAlpha;
	TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>> relationsSparseFormatRow;

	double[] regConsts;
	public void initialize(){
		super.initialize();		
		
		wAlpha = new double[relations.length];		
		
		wAlpha[0] = (double)alpha;
		for(int rel = 1; rel < this.relations.length; rel++){
			wAlpha[rel] = 1-(double) alpha;
		}
		
//		wAlpha[0] = alpha;
//		for(int rel = 1; rel < this.relations.length; rel++){
//			wAlpha[rel] = alpha;
//		}
		regConsts = new double[entityTypes.length];
		regConsts[0] = (double)reg;
		regConsts[1] = (double)reg_neg;
		
		
		relationsSparseFormatRow = new TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>>();
		
		for(int i = 0; i < relations.length; i++){
			relationsSparseFormatRow.put(i, new TIntObjectHashMap<TIntHashSet>());
			for(int row = 0; row < data[i].length; row++){				
				if(!relationsSparseFormatRow.get(i).containsKey( (int) data[i][row][0]) ){
					relationsSparseFormatRow.get(i).put((int) data[i][row][0], new TIntHashSet());
				}				
				// Remember to remove this. This works only when Relation 1 is symmetric!!!!!
				if(i==1){
					if(!relationsSparseFormatRow.get(i).containsKey( (int) data[i][row][1]) ){
						relationsSparseFormatRow.get(i).put((int) data[i][row][1], new TIntHashSet());
					}				
					relationsSparseFormatRow.get(i).get((int) data[i][row][1]).add((int) data[i][row][0]);										
				}
			}
		}
		
		numSamples = 100000;
	}	

	
	@Override
	public double error(int u, int i, int targetRow) {
		// TODO Auto-generated method stubExperimentalMF
		return 0;
	}

	@Override
	public void iterate() {
		for(int rel = relations.length-1; rel >= 0; rel--){
//			int rel =0;
			int entityA = relations[rel][1];
			int entityB = relations[rel][2];
			for(int s = 0; s < data[rel].length; s++){
//			for(int s = 0; s < numSamples; s++){
				int row = (int) (Math.random()*(data[rel].length-1));
				int u = (int)   data[rel][row][0];
				int i = (int)   data[rel][row][1];
				int	j = -1;
								
				//Compute BPR Row-Wise
				do{
					j = (int) Math.floor(Math.random()*entityTypes[entityB].length);
				}while(relationsSparseFormatRow.get(rel).get(u).contains(j));
						
				double xuij = predict(u, i, rel) - predict(u, j, rel);
				double e = 1 + Math.exp(xuij);			
				
				for(int f = 0; f < getNumFeatures(); f++){

				
					
					double grad1 = wAlpha[rel]*((entityTypes[entityB][i][f] - entityTypes[entityB][j][f]) / e) - regConsts[entityA]*entityTypes[entityA][u][f];
					double temp =  wAlpha[rel]*(entityTypes[entityA][u][f] / e);
					double grad2 = temp - regConsts[entityB]*entityTypes[entityB][i][f];
					double grad3 = -temp - regConsts[entityB]*entityTypes[entityB][j][f];

					entityTypes[entityA][u][f] += getLearnRate() * grad1;
					entityTypes[entityB][i][f] += getLearnRate() * grad2;				
					entityTypes[entityB][j][f] += getLearnRate() * grad3;
				}
								
			}
		}
			
	}



	@Override
// normal iteration
	public void iterate(int rel) {
		int entityA = relations[rel][1];
		int entityB = relations[rel][2];
			for(int s = 0; s < data[rel].length; s++){
//			for(int s = 0; s < numSamples; s++){	
				int row = (int) (Math.random()*data[rel].length);
				int u = (int)data[rel][row][0];
				int i =(int)data[rel][row][1];
				int	j = -1;
				do{
					j = (int) Math.floor(Math.random()*entityTypes[entityB].length);
				}while(relationsSparseFormatRow.get(rel).get(u).contains(j));
						
				double xuij = predict(u, i, rel) - predict(u, j, rel);
				double e = 1 + Math.exp(xuij);			
				
				for(int f = 0; f < getNumFeatures(); f++){
					
					double grad1 = wAlpha[rel]*((entityTypes[entityB][i][f] - entityTypes[entityB][j][f]) / e) - regConsts[entityA]*entityTypes[entityA][u][f];
					double grad2 = wAlpha[rel]*(entityTypes[entityA][u][f] / e) - regConsts[entityB]*entityTypes[entityB][i][f];
					double grad3 = -wAlpha[rel]*(entityTypes[entityA][u][f] / e) - regConsts[entityB]*entityTypes[entityB][j][f];
					
					if(rel == 1) entityTypes[entityA][u][f] += getLearnRate() * grad1;
					entityTypes[entityB][i][f] += getLearnRate() * grad2;				
					entityTypes[entityB][j][f] += getLearnRate() * grad3;
				}
								
			}
			
	}

	

	@Override
	public double predict(int u, int i, int rel) {
//		return Math.random();
		double pred = 0;
		int entityA = relations[rel][1];
		int entityB = relations[rel][2];
		for(int k = 0; k < getNumFeatures(); k++){
			pred += entityTypes[entityA][u][k]*entityTypes[entityB][i][k];
		}
		
		return pred;
	}


}
