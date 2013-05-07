package evaluation;

import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;

import matrixfactorization.MatrixFactorization;
import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;

public class AUCEvaluator extends Evaluator {

	
	
	@Override
	public double evaluate(int[] real, double[] predicted) {
		// TODO Auto-generated method stub
		return 0;
	}
	
	public static double auc(TIntObjectHashMap<TIntHashSet> trainObs, TIntObjectHashMap<TIntHashSet> testObs, MatrixFactorization model, TIntHashSet testItems) {
		
		double aucVal = 0;
		for(int u : testObs.keys()){
						
			
			int temp = 0;
			int eu = 0;
			for(int j : testItems.toArray()){
				if(testObs.get(u).contains(j)){
					continue;
				}
				for(int i : testObs.get(u).toArray()){
					eu++;
					if(model.predict(u, i, 0) > model.predict(u, j, 0)){
						temp++;
					}
				}					
			}
			aucVal += (double) ((double) temp / (double) eu);
		}
		aucVal /= (double) testObs.keys().length;
		return aucVal;
	}
	
	
	public static double aucExternal(TIntObjectHashMap<TIntHashSet> testObs, TIntObjectHashMap<double[]> predictions, TIntHashSet testItems) {
		
		double aucVal = 0;
		int numCases = 0;
		for(int u : testObs.keys()){
		    if(predictions.get(u) == null){
		    	System.err.println("Warning: user in test missing in predicion file");
		    	continue;		
		    }
			int temp = 0;
			int eu = 0;
			
			for(int j : testItems.toArray()){
				if(testObs.get(u).contains(j)){
					continue;
				}
				
				for(int i : testObs.get(u).toArray()){
					eu++;
					if(predictions.get(u)[i] > predictions.get(u)[j]){
						temp++;
					}
				}					
			}
			
			aucVal += (double) ((double) temp / (double) eu);
			numCases++;
		}
		
		aucVal /= (double) numCases;
		return aucVal;
	}
	
	
	public static double aucKoren(TIntObjectHashMap<TIntHashSet> trainObs, TIntObjectHashMap<TIntHashSet> testObs, MatrixFactorization model, TIntHashSet testItems) {
		
		double aucVal = 0;
		for(int u : testObs.keys()){
			
			TIntHashSet evItems =  new TIntHashSet();
			for(int k = 0; k < 1000; k++){
				int item =(int) (Math.random()*model.numEntities[1]);
				if(trainObs.get(u).contains(item) || testObs.get(u).contains(item) || evItems.contains(item)){
					k--;
				} else {
					evItems.add(item);
				}		        		
			}
			
			int temp = 0;
			int eu = 0;
			for(int j : evItems.toArray()){				
				for(int i : testObs.get(u).toArray()){
					eu++;
					if(model.predict(u, i, 0) > model.predict(u, j, 0)){
						temp++;
					}
				}					
			}
			aucVal += (double) ((double) temp / (double) eu);
		}
		aucVal /= (double) testObs.keys().length;
		return aucVal;
	}
	public static double precAtN(TIntObjectHashMap<TIntHashSet> trainObs, TIntObjectHashMap<TIntHashSet> testObs, MatrixFactorization model, TIntHashSet testItems, int n) {
		
		double precVal = 0;
		double recVal = 0;
		double f = 0;
		double sumCorrects = 0;
		double sumPred = 0;
		for(int u : testObs.keys()){
			int temp = 0;
			
			Map<Integer,Double> predicted = new HashMap<Integer,Double>();
		    
			for(int j : testItems.toArray()){				
		        predicted.put(j, model.predict(u, j, 0));
			}
			
		    //pegar o top five
		    Map<Integer, Double> predOr = sortByValue(predicted);
		    int count = 0;
		    for (Iterator<Integer> it = predOr.keySet().iterator(); it.hasNext() && count < testObs.get(u).size(); count++) {
		    	int p = it.next();	
		    	
		    	if(testObs.get(u).contains(p)){
		    		temp++;
		    	}
		    }
		    sumCorrects += temp;
		    sumPred += testObs.get(u).size();
		    double tempPrec = (double) ((double) temp / (double) testObs.get(u).size());
		    double tempRec = (double) ((double) temp / (double) testObs.get(u).size());
			precVal += tempPrec;
			recVal += tempRec;
			f += (double) ((double) temp / (double) testObs.get(u).size());
			
//			if(temp > 0){
//				f += 2 * tempPrec * tempRec / (tempPrec + tempRec);
//			}
			
		}
		precVal /= (double) testObs.keys().length;
		recVal /= (double) testObs.keys().length;
		f /= (double) testObs.keys().length;
		
		System.out.print(" -- recall@5: " + recVal);
		
		System.out.print(" -- precision@5: " + precVal);
		
		System.out.print(" -- f@5: " + f);
		System.out.print(" -- microf: " + (sumCorrects/sumPred));
		return precVal;
	}

	public static double precAtNTang(TIntObjectHashMap<TIntHashSet> trainObs, TIntObjectHashMap<TIntHashSet> testObs, MatrixFactorization model, TIntHashSet testItems, int itemNumber) {
		
		double precVal = 0;
		double recVal = 0;
		double f = 0;
		double sumCorrects = 0;
		double sumPred = 0;
		double sumTrue = 0;
		
		int[] numCorrect = new int[itemNumber];
		int[] numTrue = new int[itemNumber];
		int[] numPredicted = new int[itemNumber];
		
		for(int u : testObs.keys()){
			int temp = 0;
			
			Map<Integer,Double> predicted = new HashMap<Integer,Double>();
		    
			for(int j : testItems.toArray()){				
		        predicted.put(j, model.predict(u, j, 0));
			}
			
		    //pegar o top five
		    Map<Integer, Double> predOr = sortByValue(predicted);
		    int count = 0;
		    for (Iterator<Integer> it = predOr.keySet().iterator(); it.hasNext() && count < testObs.get(u).size(); count++) {
		    	int p = it.next();	
		    	numPredicted[p]++;
		    	numTrue[testObs.get(u).toArray()[count]]++;
		    	if(testObs.get(u).contains(p)){
		    		temp++;
		    		numCorrect[p]++;
		    	}
		    }
		    					    
		}
		
		for(int i = 0; i < itemNumber; i++){
			if(numTrue[i] == 0) continue;
			if(numPredicted[i]>0)
				precVal += (double) numCorrect[i] / (double) numPredicted[i];
			recVal += (double) numCorrect[i] / (double) numTrue[i];
			f += (double) 2*numCorrect[i] / (double) (numTrue[i] + numPredicted[i]);
			sumCorrects += numCorrect[i];
			sumPred += numPredicted[i];
			sumTrue += numTrue[i];
		}
		
		precVal /= (double) testItems.size();
		recVal /= (double) testItems.size();
		f /= (double) testItems.size();
		
		System.out.print(" -- macro-recall: " + recVal);		
		System.out.print(" -- macro-prec: " + precVal);		
		System.out.print(" -- macro-f: " + f);
		
		System.out.print(" -- microf: " + (2*sumCorrects/(sumTrue+sumPred)));
		return precVal;
	}
	
	
	public static double recallAtNKoren(TIntObjectHashMap<TIntHashSet> trainObs, TIntObjectHashMap<TIntHashSet> testObs, MatrixFactorization model, int numItems, int n) {
		
		double recVal = 0;
		int hits = 0;
		int numInstances = 0;
		for(int u : testObs.keys()){
			
		
			
			for(int j : testObs.get(u).toArray()){
				numInstances++;
				TIntHashSet evItems =  new TIntHashSet();
				for(int k = 0; k < 1000; k++){
					int item =(int) (Math.random()*numItems);

					if(trainObs.get(u).contains(item) || testObs.get(u).contains(item) || evItems.contains(item)){
						k--;
					} else {
						evItems.add(item);
					}		        		
				}
				
				Map<Integer,Double> predicted = new HashMap<Integer,Double>();
		        predicted.put(j, model.predict(u, j, 0));
		        
		        for(int item : evItems.toArray()){		        	
		        	predicted.put(item, model.predict(u, item, 0));		        			        	
		        }
		        
		        //pegar o top five
			    Map<Integer, Double> predOr = sortByValue(predicted);
			    int count = 0;
			    for (Iterator<Integer> it = predOr.keySet().iterator(); it.hasNext() && count < n; count++) {
			    	int p = it.next();				    	
			    	if(p == j){
			    		hits++;
			    	}
			    }				
			}
			
		}
		recVal = (double) ((double) hits / numInstances );
		
		return recVal;
	}
	    
    public static Map sortByValue(Map map) {
        List<Integer> list = new LinkedList(map.entrySet());
        Collections.sort(list, new Comparator() {

            public int compare(Object o1, Object o2) {
                return -((Comparable) ((Map.Entry) (o1)).getValue())
              .compareTo(((Map.Entry) (o2)).getValue());
            }
        });
// logger.info(list);
        Map result = new LinkedHashMap();
        for (Iterator it = list.iterator(); it.hasNext();) {
            Map.Entry entry = (Map.Entry) it.next();
            result.put(entry.getKey(), entry.getValue());
        }
        return result;
    }

}
