package evaluation;

import gnu.trove.TIntHashSet;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.util.StringTokenizer;

public class Counter {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		File fData = new File(args[0]);
		
		TIntHashSet users = new TIntHashSet();
		TIntHashSet items = new TIntHashSet();
		int ratings = 0;
				
		try {
			BufferedReader data = new BufferedReader(new FileReader(fData));
					
			String line;
			while((line = data.readLine())!=null){
				StringTokenizer st = new StringTokenizer(line, "\t");
				ratings++;
				
				users.add(Integer.parseInt(st.nextToken()));
				items.add(Integer.parseInt(st.nextToken()));				
			}
			
			data.close();
			
			System.out.println("Users: " + users.size());
			System.out.println("Items: " + items.size());
			System.out.println("Ratings: " + ratings);
			
		} catch (IOException e) {
			e.printStackTrace();
		} 

	}

}
