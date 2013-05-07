package datagenerator;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.StringTokenizer;

public class Blocker {
	
	SimilarityCalculator similarities;
	
	
	public Blocker(){
		
	}
	
	public void block(String input, String output){
		try {
			BufferedReader in = new BufferedReader(new FileReader(input));
			BufferedWriter out = new BufferedWriter(new FileWriter(output));
			
			String line = null;
			in.readLine();
			int lineNum = 0;
			while((line = in.readLine())!=null){
				StringTokenizer st = new StringTokenizer(line,"\t");
				int id = Integer.parseInt(st.nextToken());
				int pica = Integer.parseInt(st.nextToken());
				int picb = Integer.parseInt(st.nextToken());
								
				if(similarities.timeSimilarity(pica, picb) > 0.94){
					out.write(line + "\n");
				}
				if(++lineNum % 100000 == 0){
					System.out.print(".");
				}
												
			}
			System.out.println();
			
			in.close();
			out.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	
	
	}

	public SimilarityCalculator getSimilarities() {
		return similarities;
	}

	public void setSimilarities(SimilarityCalculator similarities) {
		this.similarities = similarities;
	}
}
