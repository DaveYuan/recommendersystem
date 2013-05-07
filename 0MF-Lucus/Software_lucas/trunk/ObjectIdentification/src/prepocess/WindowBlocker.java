package prepocess;

import io.DataInput;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;

import util.DistanceCalculator;

public class WindowBlocker extends Blocker {

	String tempFile = "./pairs";
	
	int positive = 0;
	int negative = 0;
	
	int windowSize = 100;
	
	@Override
	public String samplePairs(DataInput in) throws IOException {
		    double id = System.currentTimeMillis()*Math.random();
		    tempFile = tempFile+id+".txt";
		    
//		    BufferedWriter classifier = new BufferedWriter(new FileWriter("pairs-sim.txt"));
		    
			BufferedWriter output = new BufferedWriter(new FileWriter(tempFile));
			
			positive = 0;
			negative = 0;
			
			int[] objects = in.getOrderedIds();
			
			
			for(int i = 0; i < objects.length; i++){ 
				for(int j = i; j < (i + windowSize) && j < objects.length; j++){
					
					output.write(objects[i] + " " + objects[j] + "\n");
					if(in.getObjectClusters().get(objects[i]).equals(in.getObjectClusters().get(objects[j]))){
//						classifier.write(objects[i] + " " + objects[j] + " " + DistanceCalculator.computeDistance(objects[i], objects[j]) + " 1\n");
						positive++;
					} else {
//						classifier.write(objects[i] + " " + objects[j] + " " + DistanceCalculator.computeDistance(objects[i], objects[j]) + " 0\n");
						negative++;
					}
										
				}
			}
			
			output.close();	
//			classifier.close();
		return tempFile;
	}

	public void report(){
		System.out.println("****************************************************");
		System.out.println("Blocker results:");
		System.out.println("Number of positive pairs: " + positive);
		System.out.println("Number of negative pairs: " + negative);
		
		int size = positive + negative;
		System.out.println("Total: " + size);
		System.out.println("****************************************************");
	}

	public int getWindowSize() {
		return windowSize;
	}

	public void setWindowSize(int windowSize) {
		this.windowSize = windowSize;
	}
}
