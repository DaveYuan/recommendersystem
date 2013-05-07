package matrixfactorization;

import java.util.Iterator;

import gnu.trove.TIntArrayList;
import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;
import data.RatingsData;

public class MultiRelationalBprFoaf extends MatrixFactorization {

	int numSamples = 10000;
	
	double[] wAlpha;
	TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>> denseRelations;
	
	TIntObjectHashMap<TIntArrayList> friendsOfAFriend = new TIntObjectHashMap<TIntArrayList>();

	public void initialize(){
		super.initialize();		
		
		wAlpha = new double[relations.length];		
		wAlpha[0] = 1;
		for(int rel = 1; rel < this.relations.length; rel++){
			wAlpha[rel] = alpha;
//			wAlpha[rel] = 0;
		}						
		
		denseRelations = new TIntObjectHashMap<TIntObjectHashMap<TIntHashSet>>();
		
		for(int i = 0; i < relations.length; i++){
			//if(relations[i][RatingsData.SPARSE] == 1) continue;
			denseRelations.put(i, new TIntObjectHashMap<TIntHashSet>());
			for(int row = 0; row < data[i].length; row++){
				if(!denseRelations.get(i).containsKey( (int) data[i][row][0]) ){
					denseRelations.get(i).put((int) data[i][row][0], new TIntHashSet());
				}
				denseRelations.get(i).get((int) data[i][row][0]).add((int) data[i][row][1]);
			}
		}
		
		// friends:
		TIntObjectHashMap<TIntHashSet> users = denseRelations.get(1);
		
		// friends of a friend
		for (int user : users.keys()) {
			// setup
			friendsOfAFriend.put(user, new TIntArrayList());
			
			// for all his friends
			for (int friend : users.get(user).toArray()) {
				// add friends of friends
				// but remove direct friends
				TIntHashSet candidates = users.get(friend);
				candidates.removeAll(users.get(user).toArray());

				friendsOfAFriend.get(user).add(candidates.toArray());
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
//		int rel = 0;
		for(int rel = relations.length-1; rel >= 0; rel--){			
			for(int s = 0; s < data[rel].length; s++){
				int row = (int) (Math.random()*data[rel].length);
				int u = (int)data[rel][row][0];
				int i =(int)data[rel][row][1];
				int	j = -1;
				if (rel == 1) {
					// just draw a friend of a friend 
					j = friendsOfAFriend.get(u).get((int) Math.floor(Math.random()* friendsOfAFriend.get(u).size()));
				}
				else
				{
					do{
						j = (int) Math.floor(Math.random()*entityTypes[relations[rel][2]].length);
					}while(denseRelations.get(rel).get(u).contains(j));
				}
				
		
				double xuij = predict(u, i, rel) - predict(u, j, rel);
				double e = 1 + Math.exp(xuij);			
				
				for(int f = 0; f < getNumFeatures(); f++){
					double grad1 = wAlpha[rel]*((entityTypes[relations[rel][2]][i][f] - entityTypes[relations[rel][2]][j][f]) / e - reg*entityTypes[relations[rel][1]][u][f]);
					double grad2 = wAlpha[rel]*(entityTypes[relations[rel][1]][u][f] / e - reg*entityTypes[relations[rel][2]][i][f]);
					double grad3 = -wAlpha[rel]*(entityTypes[relations[rel][1]][u][f] / e - reg_neg*entityTypes[relations[rel][2]][j][f]);

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
