import gnu.trove.TIntIntHashMap;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.util.Arrays;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.StringTokenizer;


public class ClassCounts {
	
	private int labelColumn;
	private String delim = "\t";
	
	public ClassCounts(int col, String sep){
		labelColumn = col;
		delim = sep;
	}
	
	public ClassCounts(int col){
		labelColumn = col;		
	}
	
	public void createClassCounts(String inputFile, String outputFile){
		BufferedReader input = null;
		BufferedWriter output = null;
		
		try {
			input = new BufferedReader(new FileReader(inputFile));
			output = new BufferedWriter(new FileWriter(outputFile));
			
			String line = null;
			StringTokenizer st = null;
			
			HashMap<Integer, Integer> classes = new HashMap<Integer, Integer>();
			
			while((line = input.readLine())!=null){
				st = new StringTokenizer(line, delim);
				
				for(int i = 1; i < labelColumn; i++){
					st.nextToken();
				}
				
				int label = Integer.parseInt(st.nextToken());
				
				if(!classes.containsKey(label)){
					classes.put(label, 0);
				}
				
				classes.put(label, classes.get(label)+1);								
			}
			
			Map sortedClasses = sortByValue(classes);
			int i = 1;
			for (Iterator it = sortedClasses.keySet().iterator(); it.hasNext();) {
	    		Integer label = (Integer) it.next();
	    		output.write(i++ + " " + sortedClasses.get(label) + "\n");
	    	}
			
			input.close();
			output.close();
			
		} catch (Exception e) {

			e.printStackTrace();
		}
		
	}
	
	
	public Map sortByValue(Map map) {
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
