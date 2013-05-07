package FactorizationModel;

import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.io.PrintStream;

import gnu.trove.TIntDoubleHashMap;
import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;
import gnu.trove.TIntProcedure;

public class EuclideanDistanceModel extends GeneralModel {

	double[][] gradients;
	
	TIntObjectHashMap<double[]> objMaps;
	
	int[] poskeys, negkeys;
	TIntObjectHashMap<int[]> pospairs, negpairs;
	
	public void init(){
		super.init();
		
		gradients = new double[k][f];
		
		objMaps = new TIntObjectHashMap<double[]>();
		
		
		poskeys = positivePairs.keys();
		pospairs = new TIntObjectHashMap<int[]>();
		for(int i = 0; i < poskeys.length; i++){
			pospairs.put(poskeys[i], positivePairs.get(poskeys[i]).toArray());
		}
		
		negkeys = negativePairs.keys();
		negpairs = new TIntObjectHashMap<int[]>();
		for(int i = 0; i < negkeys.length; i++){
			negpairs.put(negkeys[i], negativePairs.get(negkeys[i]).toArray());
		}
	}
	
	@Override
	public void iterate() {
		TIntHashSet indexes = new TIntHashSet();
		int[] keys;
		TIntObjectHashMap<int[]> pairs;
		
		
		for (int samp = 0; samp < 100; samp++) {
//			TIntObjectHashMap<TIntHashSet> pairs = Math.random() > 0.5 ? positivePairs : negativePairs;
									
//			int ob1 = pairs.keys()[(int) (pairs.keys().length * Math.random())];
//			int ob2 = pairs.get(ob1).toArray()[(int) (pairs.get(ob1).size() * Math.random())];
			
			boolean pos = true;
			
			if(Math.random() > 0.5){
				keys = poskeys;
				pairs = pospairs;
			} else {
				keys = negkeys;
				pairs = negpairs;
				pos = false;
			}
			
			int ob1 = keys[(int) (keys.length * Math.random())];
			int ob2 = pairs.get(ob1)[(int) (pairs.get(ob1).length * Math.random())];
			
			indexes.clear();
			
			if (!featureVectors.contains(ob1) || !featureVectors.contains(ob2)) continue;
			
			indexes.addAll(featureVectors.get(ob1).getNonZeroIndexes());
			indexes.addAll(featureVectors.get(ob2).getNonZeroIndexes());

			
			TIntDoubleHashMap featureDiff = new TIntDoubleHashMap();
			
						
			for (int l : indexes.toArray()) {
				featureDiff.put(l,((double)featureVectors.get(ob1).get(l) - featureVectors.get(ob2).get(l)));
			}
				
			
			for (int i = 0; i < k; i++) {
				for (int j = 0; j < f; j++) {

					gradients[i][j] = 0;
				
					double grad = 0;
																
					
					if (indexes.contains(j)) {					
						for (int l : featureDiff.keys()) {
							if(l != j)
								grad +=params[i][l] * featureDiff.get(l);
						}
						grad *= featureDiff.get(j);
					}
								
					gradients[i][j] = pos ? (1-classConst) * grad  : - classConst * grad;

				}
			}

			for (int i = 0; i < k; i++) {
				for (int j = 0; j < f; j++) {
					if(Double.isNaN(params[i][j] + learnRate * (gradients[i][j] + reg*params[i][j]))){
						System.err.println("NaN found!!!");
						System.err.println("Gradient: " + gradients[i][j]);
						System.err.println("Param: " + params[i][j]);
						System.err.println("Sum: " + (gradients[i][j] + reg*params[i][j]));
						System.exit(0);
					}
					if(Double.isInfinite(params[i][j] + learnRate * (gradients[i][j] + reg*params[i][j]))){
						System.err.println("Inffinity found!!!");
						System.err.println("Gradient: " + gradients[i][j]);
						System.err.println("Param: " + params[i][j]);
						System.err.println("Sum:l " + (gradients[i][j] + reg*params[i][j]));
						System.exit(0);
					}
					params[i][j] += learnRate * (gradients[i][j] + reg*params[i][j]);
					
				}
			}

		}
	}

	@Override
	public double loss() {
		
		for(int i : featureVectors.keys()){
			map(i);
		}
		
		double posGrad = 0;
		
		for (int ob1 : positivePairs.keys()) {
			for (int ob2 : positivePairs.get(ob1).toArray()) {
				if(!featureVectors.contains(ob1) || !featureVectors.contains(ob2)) continue;
				double[] o1 = objMaps.get(ob1);
				double[] o2 = objMaps.get(ob2);

				for (int i = 0; i < k; i++) {
					posGrad += (o1[i] - o2[i]) * (o1[i] - o2[i]);
				}
			}
		}
		
		double negGrad = 0;
		
		for (int ob1 : negativePairs.keys()) {
			for (int ob2 : negativePairs.get(ob1).toArray()) {
				if(!featureVectors.contains(ob1) || !featureVectors.contains(ob2)) continue;
				double[] o1 = objMaps.get(ob1);
				double[] o2 = objMaps.get(ob2);

				for (int i = 0; i < k; i++) {
					negGrad += (o1[i] - o2[i]) * (o1[i] - o2[i]);
				}
			}
		}
		return posGrad - negGrad;
	}

	@Override
	public double[] map(int object) {
		if(!objMaps.containsKey(object))
			objMaps.put(object, new double[k]);
			
		for(int i = 0; i < k; i++){
			objMaps.get(object)[i] = 0;
			for(int j : featureVectors.get(object).getNonZeroIndexes()){
				objMaps.get(object)[i] += params[i][j]*featureVectors.get(object).get(j);
			}
		}
		return objMaps.get(object);
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

	
}

