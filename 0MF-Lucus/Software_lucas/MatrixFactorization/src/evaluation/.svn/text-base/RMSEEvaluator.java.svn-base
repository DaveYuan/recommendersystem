package evaluation;

public class RMSEEvaluator extends Evaluator {


	public double evaluate(int[] real, double[] predicted){
		double error = 0;
		
		for(int i = 0; i < predicted.length; i++){
			error += (predicted[i]-real[i])*(predicted[i]-real[i]);
		}
		
		error /= predicted.length;
		
		return Math.sqrt(error);
	}
	
	public static double rmse(int[] real, double[] predicted){
		double error = 0;
		
		for(int i = 0; i < predicted.length; i++){
			error += (predicted[i]-real[i])*(predicted[i]-real[i]);
		}
		
		error /= predicted.length;
		
		return Math.sqrt(error);
	}
}
