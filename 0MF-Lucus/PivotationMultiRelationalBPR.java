package matrixfactorization;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;



public class PivotationMultiRelationalBPR extends MatrixFactorization {

int numSamples = 10000;
	
	double[] wAlpha;
	TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>> relationsSparseFormatRow;
	TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>> relationsSparseFormatColumn;

	private double[] regConsts;


	public void initialize(){
		super.initialize();		
	
		wAlpha = new double[relations.length];		

		wAlpha[0] = alpha;
		for(int rel = 1; rel < this.relations.length; rel++){
			wAlpha[rel] = 1-alpha;
		}
		
//		wAlpha[0] = 1;
//		for(int rel = 1; rel < this.relations.length; rel++){
//			wAlpha[rel] = alpha;
//		}	
		
		regConsts = new double[entityTypes.length];
		regConsts[0] = reg;
		regConsts[1] = reg_neg;
		
		
		relationsSparseFormatRow = new TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>>();
		relationsSparseFormatColumn = new TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>>();
		
		for(int i = 0; i < relations.length; i++){
			relationsSparseFormatRow.put(i, new TIntObjectHashMap<TIntHashSet>());
			relationsSparseFormatColumn.put(i, new TIntObjectHashMap<TIntHashSet>());
			for(int row = 0; row < data[i].length; row++){				
				if(!relationsSparseFormatRow.get(i).containsKey( (int) data[i][row][0]) ){
					relationsSparseFormatRow.get(i).put((int) data[i][row][0], new TIntHashSet());
				}				
				if(!relationsSparseFormatColumn.get(i).containsKey( (int) data[i][row][1]) ){
					relationsSparseFormatColumn.get(i).put((int) data[i][row][1], new TIntHashSet());
				}				
				relationsSparseFormatColumn.get(i).get((int) data[i][row][1]).add((int) data[i][row][0]);
				// Remember to remove this. This works only when Relation 1 is symmetric!!!!!
				if(i==1){
					if(!relationsSparseFormatRow.get(i).containsKey( (int) data[i][row][1]) ){
						relationsSparseFormatRow.get(i).put((int) data[i][row][1], new TIntHashSet());
					}				
					relationsSparseFormatRow.get(i).get((int) data[i][row][1]).add((int) data[i][row][0]);
										
				}
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
			int entityA = relations[rel][1];
			int entityB = relations[rel][2];
			for(int s = 0; s < data[rel].length; s++){
//			for(int s = 0; s < numSamples; s++){
				int row = (int) (Math.random()*data[rel].length);
				int u = (int)data[rel][row][0];
				int i =(int)data[rel][row][1];
				int	j = -1;
	
				//Compute BPR Row-Wise
				do{
					j = (int) Math.floor(Math.random()*entityTypes[entityB].length);
				}while(relationsSparseFormatRow.get(rel).get(u).contains(j));
						
				double xuij = predict(u, i, rel) - predict(u, j, rel);
				double e = 1 + Math.exp(xuij);			
				
				for(int f = 0; f < getNumFeatures(); f++){
					double grad1 = wAlpha[rel]*((entityTypes[entityB][i][f] - entityTypes[entityB][j][f]) / e) - regConsts[entityA]*entityTypes[entityA][u][f];
					double temp = wAlpha[rel]*(entityTypes[entityA][u][f] / e);
					double grad2 = temp - regConsts[entityB]*entityTypes[entityB][i][f];
					double grad3 = -temp - regConsts[entityB]*entityTypes[entityB][j][f];

					entityTypes[entityA][u][f] += getLearnRate() * grad1;
					entityTypes[entityB][i][f] += getLearnRate() * grad2;				
					entityTypes[entityB][j][f] += getLearnRate() * grad3;
				}
				
				//compute BPR Column Wise
				j = -1;
				do{
					j = (int) Math.floor(Math.random()*entityTypes[entityA].length);
				}while(!relationsSparseFormatRow.get(rel).containsKey(j) || relationsSparseFormatColumn.get(rel).get(i).contains(j));
								
						
				xuij = predict(u, i, rel) - predict(j, i, rel);
				e = 1 + Math.exp(xuij);			
				
				for(int f = 0; f < getNumFeatures(); f++){
					
					double grad1 = wAlpha[rel]*((entityTypes[entityA][u][f] - entityTypes[entityA][j][f]) / e) - regConsts[entityB]*entityTypes[entityB][i][f];
					double temp = wAlpha[rel]*(entityTypes[entityB][i][f] / e);
					double grad2 = temp - regConsts[entityA]*entityTypes[entityA][u][f];
					double grad3 = -temp - regConsts[entityA]*entityTypes[entityA][j][f];
					
					entityTypes[entityB][i][f] += getLearnRate() * grad1;
					entityTypes[entityA][u][f] += getLearnRate() * grad2;				
					entityTypes[entityA][j][f] += getLearnRate() * grad3;
				}
			}
		}
			
	}


	@Override
// column wise iteration
	public void iterate(int rel) {
		int entityA = relations[rel][1];
		int entityB = relations[rel][2];
			for(int s = 0; s < data[rel].length; s++){
				int row = (int) (Math.random()*data[rel].length);
				int user = (int)data[rel][row][0];
				int label =(int)data[rel][row][1];
				int	j = -1;
				do{
					j = (int) Math.floor(Math.random()*entityTypes[relations[rel][1]].length);
				}while(!relationsSparseFormatRow.get(rel).containsKey(j) || relationsSparseFormatColumn.get(rel).get(label).contains(j));
								
						
				double xuij = predict(user, label, rel) - predict(j, label, rel);
				double e = 1 + Math.exp(xuij);			
				
				for(int f = 0; f < getNumFeatures(); f++){
					
					double grad1 = wAlpha[rel]*((entityTypes[relations[rel][1]][user][f] - entityTypes[relations[rel][1]][j][f]) / e - regConsts[entityB]*entityTypes[relations[rel][2]][label][f]);
					double grad2 = wAlpha[rel]*(entityTypes[relations[rel][2]][label][f] / e - regConsts[entityA]*entityTypes[relations[rel][1]][user][f]);
					double grad3 = -wAlpha[rel]*(entityTypes[relations[rel][2]][label][f] / e - regConsts[entityA]*entityTypes[relations[rel][1]][j][f]);
					
					entityTypes[relations[rel][2]][label][f] += getLearnRate() * grad1;
					if(rel == 1) entityTypes[relations[rel][1]][user][f] += getLearnRate() * grad2;				
					if(rel == 1) entityTypes[relations[rel][1]][j][f] += getLearnRate() * grad3;
				}
				
				j = -1;
				do{
					j = (int) Math.floor(Math.random()*entityTypes[relations[rel][2]].length);
				}while(relationsSparseFormatRow.get(rel).get(user).contains(j));
						
				xuij = predict(user, label, rel) - predict(user, j, rel);
				e = 1 + Math.exp(xuij);			
				
				for(int f = 0; f < getNumFeatures(); f++){
					
					double grad1 = wAlpha[rel]*((entityTypes[relations[rel][2]][label][f] - entityTypes[relations[rel][2]][j][f]) / e - regConsts[entityA]*entityTypes[relations[rel][1]][user][f]);
					double grad2 = wAlpha[rel]*(entityTypes[relations[rel][1]][user][f] / e - regConsts[entityB]*entityTypes[relations[rel][2]][label][f]);
					double grad3 = -wAlpha[rel]*(entityTypes[relations[rel][1]][user][f] / e - regConsts[entityB]*entityTypes[relations[rel][2]][j][f]);
					
					if(rel == 1) entityTypes[relations[rel][1]][user][f] += getLearnRate() * grad1;
					entityTypes[relations[rel][2]][label][f] += getLearnRate() * grad2;				
					entityTypes[relations[rel][2]][j][f] += getLearnRate() * grad3;
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
