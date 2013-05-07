package FactorizationModel;

import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.io.PrintStream;

import gnu.trove.TIntDoubleHashMap;
import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;
import gnu.trove.TIntProcedure;


/**
 * Optimizes for euclidean distances but using a logistic kernel in the mapping function
 * @author lucas
 *
 */
public class ScalarProductModel extends GeneralModel {

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
		TIntHashSet negIndexes = new TIntHashSet();
		int[] keys;
		TIntObjectHashMap<int[]> pairs;
		
		
		for (int samp = 0; samp < 100; samp++) {

			keys = poskeys;
			pairs = pospairs;
			
			int ob1;
			do{
			  ob1 = keys[(int) (keys.length * Math.random())];
			}while(featureVectors.get(ob1) == null);
			
			int ob2;
			do{
				ob2 = pairs.get(ob1)[(int) (pairs.get(ob1).length * Math.random())];
			}while(featureVectors.get(ob2) == null);
			
			keys = negkeys;
			pairs = negpairs;
			
			int negob1;
			do{
				negob1= keys[(int) (keys.length * Math.random())];
			}while(featureVectors.get(negob1) == null);
			
			int negob2;
			do{
				negob2 = pairs.get(negob1)[(int) (pairs.get(negob1).length * Math.random())];
			}while(featureVectors.get(negob2) == null);
			

			
			indexes.clear();
				
			indexes.addAll(featureVectors.get(ob1).getNonZeroIndexes());
			indexes.addAll(featureVectors.get(ob2).getNonZeroIndexes());

			
			negIndexes.clear();
			
			negIndexes.addAll(featureVectors.get(negob1).getNonZeroIndexes());
			negIndexes.addAll(featureVectors.get(negob2).getNonZeroIndexes());	
			
			for (int i = 0; i < k; i++) {
				double flinOb1 = flinear(ob1, i);
				double flinOb2 = flinear(ob2, i);				
										
				double flinNegOb1 = flinear(negob1, i);
				double flinNegOb2 = flinear(negob2, i);				
				
				for (int j = 0; j < f; j++) {

					gradients[i][j] = 0;
					
					double truePart = - (featureVectors.get(ob1).get(j)*flinOb2
							+flinOb1*featureVectors.get(ob2).get(j));
					
					
					double falsePart = - (featureVectors.get(negob1).get(j)*flinNegOb1
							+flinNegOb2*featureVectors.get(negob2).get(j));
					
					gradients[i][j] = truePart - classConst * falsePart;
				}
			}

			for (int i = 0; i < k; i++) {
				for (int j = 0; j < f; j++) {
					params[i][j] += learnRate * (gradients[i][j] + reg*params[i][j]);
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
	
	private double flinear(int obj, int k){
		double feature = 0;
		for(int j : featureVectors.get(obj).getNonZeroIndexes()){
			feature += params[k][j]*featureVectors.get(obj).get(j);
		}
		return feature;
	}
	
	
}

