package tagrecommender;

import java.beans.FeatureDescriptor;

import data.TripleDataInput;

public class PITF {
	
	private float  reg = 0.02f;
	private float  learnRate = 0.001f;
	private int    numFeatures = 2;
	private float  tolerance = 0.000001f;
	private int    maxIter = 1000;
	
	int[] dimensions;
	
	double[][] factor1, factor2, factor3;	
	
	int[][] data;
	double labels[];
	
	public PITF(){
		
	}
	
	private void initialize(){
		factor1 = new double[dimensions[0]][numFeatures];
		factor2 = new double[dimensions[1]][numFeatures];
		factor3 = new double[dimensions[2]][numFeatures];			
		
		for(int i = 0; i < factor1.length; i++){
			for(int j = 0; j < numFeatures; j++){
				factor1[i][j] = ((double) Math.random()*2)-1.0;
			}
		}
		
		for(int i = 0; i < factor2.length; i++){
			for(int j = 0; j < numFeatures; j++){
				factor2[i][j] = ((double) Math.random()*2)-1.0;
			}
		}
		
		for(int i = 0; i < factor3.length; i++){
			for(int j = 0; j < numFeatures; j++){
				factor3[i][j] = ((double) Math.random()*2)-1.0;
			}
		}
	}
	
	public void train(){
		double lastRMSE;
		double currRMSE = 100;
		int iter = 0;
		
		do{
			lastRMSE = currRMSE;
			iterate();
			currRMSE = evaluate();
			System.out.println("Iteration: " + iter + " -- RMSE: " + currRMSE + " -- Difference: " + (currRMSE - lastRMSE));
		}while(Math.abs(currRMSE - lastRMSE) > tolerance && iter++ < maxIter);
	}
	
	public void iterate(){
		int numSamples = (int)(data.length*0.3);
		for(int o = 0; o < numSamples; o++){
			System.out.print(".");
			int row = (int) (Math.random() * (data.length-1));
			int comp1 = data[row][0];
			int comp2 = data[row][1];
			int comp3 = data[row][2];
			

			for(int f = 0; f < numFeatures; f++){
				double rPred = predict(data[row]);
				double r = labels[row];

				double e = 2*(rPred - r);
				
				double grad1 = 0;
				for(int j = 0; j < factor2.length; j++){
					for(int k = 0; k < factor3.length; k++){
						grad1 += factor2[j][f]*factor3[k][f];
					}
				}
				double grad2 = 0;
				for(int i = 0; i < factor1.length; i++){
					for(int k = 0; k < factor3.length; k++){
						grad2 += factor1[i][f]*factor3[k][f];
					}
				}
				double grad3 = 0;
				for(int i = 0; i < factor1.length; i++){
					for(int j = 0; j < factor2.length; j++){					
						grad3 += factor2[j][f]*factor1[i][f];
					}
				}
				double grad4 = 0;
				for(int i = 0; i < factor1.length; i++){
					for(int j = 0; j < factor2.length; j++){					
						for(int k = 0; k < factor3.length; k++){
							grad4 += factor1[i][f]*factor2[j][f]*factor3[k][f];
						}
					}
				}
				
				factor1[comp1][f] -= learnRate * e * grad1;
				factor2[comp2][f] -= learnRate * e * grad2;
				factor3[comp3][f] -= learnRate * e * grad3;				
			}
		}

	}
	
	//TODO adicionar resíduo
	public double predict(int[] triple){
		double pred = 0;	
		for(int i = 0; i < numFeatures; i++){			
			pred += factor1[triple[0]][i]*factor2[triple[1]][i]*factor3[triple[2]][i];			
		}
		return pred;
	}
	
	public double evaluate(){
		double error = 0;
		for(int row = 0; row < data.length; row++){
			error += labels[row]-predict(data[row]);
			
		}
		return error;
	}
	
	public static void main(String args[]){
		PITF parafac = new PITF();
		
		TripleDataInput input = new TripleDataInput();
		System.out.println("Reading Data...");
//		parafac.data = input.loadTrainingData("/home/lucas/workspace/RDFParsing/tagrecommender/original/beatles1-0.train.txt");
		parafac.data = input.loadTrainingData("test.txt");
		System.out.println("Data read!");
		parafac.labels = input.labels;
		parafac.dimensions = new int[3];
		parafac.dimensions[0] = input.biggest1;
		parafac.dimensions[1] = input.biggest2;
		parafac.dimensions[2] = input.biggest3;
		
		parafac.initialize();
		System.out.print("Training");
		parafac.train();
		parafac.printResults();
	}
	
	
	private void printResults(){
		System.out.println("Factor1: ");
		for(int i = 0; i < factor1.length; i++){
			for(int j = 0; j < numFeatures; j++){
				System.out.print(factor1[i][j] + " ");
			}
			System.out.println();
		}
		
		System.out.println("\n\nFactor2: ");
		for(int i = 0; i < factor2.length; i++){
			for(int j = 0; j < numFeatures; j++){
				System.out.print(factor2[i][j] + " ");
			}
			System.out.println();
		}
		System.out.println("\n\nFactor3: ");
		for(int i = 0; i < factor3.length; i++){
			for(int j = 0; j < numFeatures; j++){
				System.out.print(factor3[i][j] + " ");
			}
			System.out.println();
		}
	}
}
