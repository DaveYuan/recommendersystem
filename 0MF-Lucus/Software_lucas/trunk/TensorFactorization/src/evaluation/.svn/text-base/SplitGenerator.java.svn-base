package evaluation;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;


public class SplitGenerator {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		
		File fData = new File(args[0]);
		File fSplits = new File(args[1]);
				
		try {
			BufferedReader data = new BufferedReader(new FileReader(fData));
			BufferedReader splits = new BufferedReader(new FileReader(fSplits));

			BufferedWriter train = new BufferedWriter(new FileWriter(args[2] + "-train.txt"));
			BufferedWriter test = new BufferedWriter(new FileWriter(args[2] + "-test.txt"));

			String line;
			while((line = data.readLine())!=null){
				int split = Integer.parseInt(splits.readLine());
				if(split == 0){
					train.write(line+"\n");
				} else if(split == 1){
					test.write(line+"\n");
				}
			}
			
			data.close();
			splits.close();
			train.close();
			test.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} 
	}

}
