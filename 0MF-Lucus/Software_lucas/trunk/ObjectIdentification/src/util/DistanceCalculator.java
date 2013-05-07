package util;

import io.DataInput;
import factorizationmodel.GeneralModel;
import gnu.trove.TIntObjectHashMap;

public class DistanceCalculator {

	public static TIntObjectHashMap<double[]> objMaps;
	public static DataInput data;
	public static GeneralModel model;
	
	public static void init(GeneralModel m){
		objMaps = new TIntObjectHashMap<double[]>();
		model = m;
	}
	
	
	public static double computeDistance(int objA, int objB){
		return euclideanDistance(objA, objB);
//		return cosine(objA, objB);
//		return cheatSim(objA, objB);
	}
	
	private static double euclideanDistance(int objA, int objB){
		check(objA, objB);
		
		double d = 0.0;
		for(int i = 0; i < objMaps.get(objA).length; i++){
			d += (double)(objMaps.get(objA)[i] - objMaps.get(objB)[i])*(objMaps.get(objA)[i] - objMaps.get(objB)[i]);
		}
		
		return Math.sqrt(d);
//		return 1.0 / (1 + Math.exp(d));				
	}
	
	
	public static double euclideanDistance(double[] objA, double[] objB){
				
		double d = 0.0;
		for(int i = 0; i < objA.length; i++){
			d += (double)(objA[i] - objB[i])*(objA[i] - objB[i]);
		}
		
		return Math.sqrt(d);
		
	}

	
	private static double cosine(int objA, int objB){
		check(objA, objB);
				
		double scalar = 0.0;
		double modA = 0.0;
		double modB = 0.0;
		for(int i = 0; i < objMaps.get(objA).length; i++){
			scalar += (double)(objMaps.get(objA)[i]*objMaps.get(objB)[i]);
			modA += (double)(objMaps.get(objA)[i]*objMaps.get(objA)[i]);
			modB += (double)(objMaps.get(objB)[i]*objMaps.get(objB)[i]);
		}
		
		return scalar / Math.sqrt(modA*modB);				
	}

	
	private static double cheatDistance(int objA, int objB){
		if(data.getObjectClusters().get(objA).equals(data.getObjectClusters().get(objB))){
			return 0.0;
		} else {
			return 1.0;
		}
	}
	
	private static double cheatSim(int objA, int objB){
		if(data.getObjectClusters().get(objA).equals(data.getObjectClusters().get(objB))){
			return 1.0;
		} else {
			return 0.0;
		}
	}
	
	private static void check(int objA, int objB) {
		if(!objMaps.containsKey(objA)){
			objMaps.put(objA, model.map(data.getObjectFeatures().get(objA)));
		}
		if(!objMaps.containsKey(objB)){
			objMaps.put(objB, model.map(data.getObjectFeatures().get(objB)));
		}
	}
}
