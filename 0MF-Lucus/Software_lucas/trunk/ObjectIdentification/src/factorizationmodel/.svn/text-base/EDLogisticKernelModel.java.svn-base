package factorizationmodel;

import java.io.PrintStream;

import util.DistanceCalculator;
import util.SparseArray;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;



/**
 * Optimizes for Euclidean distances but using a logistic kernel in the mapping function
 * @author Lucas
 *
 */
public class EDLogisticKernelModel extends GeneralModel {

	double[][] gradients;
	
	TIntObjectHashMap<double[]> objMaps;
	
	int[] poskeys, negkeys;
	TIntObjectHashMap<int[]> pospairs, negpairs;
	
	public void init(){
		super.init();		
		gradients = new double[k][f];		
		objMaps = new TIntObjectHashMap<double[]>();
	}
	
	@Override
	public void iterate() {
		TIntHashSet indexes = new TIntHashSet();
		TIntHashSet negIndexes = new TIntHashSet();
		
		
		for (int samp = 0; samp < 100; samp++) {
		
			
			/*
			 * TODO Try to sample always a positive pair and a negative pair from the same 
			 * object
			 */
			int count = 0;
			int ob1;
			do{
				ob1 = objectToCluster.keys()[(int) (objectToCluster.size() * Math.random())];
			}while(featureVectors.get(ob1) == null);
			
			int ob2;
			do{
				ob2 = objectToCluster.get(ob1).toArray()[(int) (objectToCluster.get(ob1).size() * Math.random())];
				count++;
				if(count == 10){
					do{
						ob1 = objectToCluster.keys()[(int) (objectToCluster.size() * Math.random())];
					}while(featureVectors.get(ob1) == null);
					count = 0;
				}
			}while(ob1 == ob2 || featureVectors.get(ob2) == null);
			
			int negob1;// = ob1;
			do{
				negob1 = objectToCluster.keys()[(int) (objectToCluster.size() * Math.random())];
			}while(featureVectors.get(negob1) == null);
			count = 0;
			int negob2;
			do{
				negob2 = objectToCluster.keys()[(int) (objectToCluster.size() * Math.random())];
				count++;
				if(count==10){
					do{
						negob1 = objectToCluster.keys()[(int) (objectToCluster.size() * Math.random())];
					}while(featureVectors.get(negob1) == null);
					count = 0;
				}
			}while(objectToCluster.get(negob1).contains(negob2) || featureVectors.get(negob2) == null);
			
			
			indexes.clear();
				
			indexes.addAll(featureVectors.get(ob1).getNonZeroIndexes());
			indexes.addAll(featureVectors.get(ob2).getNonZeroIndexes());

			
			negIndexes.clear();
			
			negIndexes.addAll(featureVectors.get(negob1).getNonZeroIndexes());
			negIndexes.addAll(featureVectors.get(negob2).getNonZeroIndexes());	
			
			for (int i = 0; i < k; i++) {
				double flinOb1 = flinear(ob1, i);
				double flinOb2 = flinear(ob2, i);
				double logOb1 = logistic(flinOb1);
				double logOb2 = logistic(flinOb2);
				double diffOb = logOb1 - logOb2;
				double derivativeOb1 = logOb1 * (1-logOb1);
				double derivativeOb2 = logOb2 * (1-logOb2);
				
				
				double flinNegOb1 = flinear(negob1, i);
				double flinNegOb2 = flinear(negob2, i);
				double logNegOb1 = logistic(flinNegOb1);
				double logNegOb2 = logistic(flinNegOb2);
				double diffNegOb = logNegOb1 - logNegOb2;
				double derivativeNegOb1 = logNegOb1 * (1-logNegOb1);
				double derivativeNegOb2 = logNegOb2 * (1-logNegOb2);
				
				for (int j = 0; j < f; j++) {

					gradients[i][j] = 0;
					
					double truePart = derivativeOb1*featureVectors.get(ob1).get(j) - derivativeOb2*featureVectors.get(ob2).get(j);
					truePart *= diffOb;
					
					double falsePart = derivativeNegOb1*featureVectors.get(negob1).get(j) - derivativeNegOb2*featureVectors.get(negob2).get(j);
					falsePart *= diffNegOb;
					
					gradients[i][j] = truePart - classConst * falsePart;					
				}
			}

			for (int i = 0; i < k; i++) {
				for (int j = 0; j < f; j++) {
					params[i][j] -= learnRate * (gradients[i][j] + reg*params[i][j]);
					if(Double.isNaN(params[i][j])){
						System.err.println("NaN found!!!");
						
						System.err.println("Gradient: " + gradients[i][j]);
						System.err.println("Param: " + params[i][j]);
						System.err.println("Sum: " + (gradients[i][j] + reg*params[i][j]));
						System.err.println("i: " + i);
						System.err.println("j: " + j);
						System.exit(0);
					}
					if(Double.isInfinite(params[i][j])){
						System.err.println("Inffinity found!!!");
						System.err.println("Gradient: " + gradients[i][j]);
						System.err.println("Param: " + params[i][j]);
						System.err.println("Sum: " + (gradients[i][j] + reg*params[i][j]));
						System.err.println("i: " + i);
						System.err.println("j: " + j);
						System.exit(0);
					}					
				}
			}

		}
	}

	@Override
	public double loss() {	
		double posDist = 0;
		double negDist = 0;
		
		objMaps = new TIntObjectHashMap<double[]>();
				
		for(int i = 0; i < orderedIDs.length; i++){ 
			if(!objMaps.containsKey(orderedIDs[i]))
				map(orderedIDs[i]);
			for(int j = i; j < (i + 100) && j < orderedIDs.length; j++){
				if(!objMaps.containsKey(orderedIDs[j]))
					map(orderedIDs[j]);
				
				Double distance = DistanceCalculator.euclideanDistance(objMaps.get(orderedIDs[i]), objMaps.get(orderedIDs[j]));
				
				if(in.getObjectClusters().get(orderedIDs[i]).equals(in.getObjectClusters().get(orderedIDs[j]))){
					posDist += distance;
				} else {
					negDist += distance;
				}
			} 
					
		}	
									
		return (posDist-classConst*negDist);
	}

	@Override
	public double[] map(int object) {
		if(!objMaps.containsKey(object))
			objMaps.put(object, new double[k]);
			
		for(int i = 0; i < k; i++){
			objMaps.get(object)[i] = 0.0;
			for(int j : featureVectors.get(object).getNonZeroIndexes()){
				objMaps.get(object)[i] += params[i][j]*featureVectors.get(object).get(j);
			}
			objMaps.get(object)[i] = logistic(objMaps.get(object)[i]);			
		}
		return objMaps.get(object);
	}
	
	@Override
	public double[] map(SparseArray object) {
		double[] mapped = new double[k];
		for(int i = 0; i < k; i++){
			mapped[i] = 0.0;
			for(int j : object.getNonZeroIndexes()){
				if(j >= f) continue;
				mapped[i] += params[i][j]*object.get(j);
			}
			mapped[i] = logistic(mapped[i]);			
		}
		return mapped;
	}
	
	
	@Override
	public void printResults(PrintStream out) {
		for(int i : featureVectors.keys()){
			map(i);
		}
		
		for(int i : objMaps.keys()){
			for(double k : objMaps.get(i) ){
				out.print(k + " ");
			}
			out.println();
		}
		
	}
	
	private double logistic(double num){
		return 1.0 / (1.0 + Math.exp(-num));
	}


	private double flinear(int obj, int k){
		double feature = 0;
		for(int j : featureVectors.get(obj).getNonZeroIndexes()){
			feature += params[k][j]*featureVectors.get(obj).get(j);
		}
		return feature;
	}


	
	
}

