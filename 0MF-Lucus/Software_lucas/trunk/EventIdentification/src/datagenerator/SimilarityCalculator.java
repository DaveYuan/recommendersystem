package datagenerator;

import java.util.StringTokenizer;

import utils.SparseArray;
import gnu.trove.TIntIntHashMap;
import gnu.trove.TIntObjectHashMap;

/**
 * IMPORTANT: all the ids passed to the similarity methods are the picture_id fields from the database
 * @author lucas
 *
 */
public class SimilarityCalculator {

	
	DataInput data;
	
	TIntIntHashMap picIds2position;
	
	public SimilarityCalculator(DataInput d){
		data = d;
//		data.getData_time() = data_int;
//		data.getData_location() = data_double;
//		
		picIds2position = new TIntIntHashMap();
		
		for(int i = 0; i < data.getData_time().length; i++){
			picIds2position.put(data.getData_time()[i][DataInput.PICID], i);
		}
	}
	
	
	public double timeSimilarity(int a, int b){
		
		int picA = picIds2position.get(a);
		int picB = picIds2position.get(b);
		
		return 1.0 - 
			Math.abs(
					data.getData_time()[picA][DataInput.DATETAKEN] - data.getData_time()[picB][DataInput.DATETAKEN])/31536000.0;
		
	}
	
	/// @brief The usual PI/180 constant
//	static final double DEG_TO_RAD = 0.017453292519943295769236907684886;
//	static final double DEG_TO_RAD = 0.017453;
	static final double DEG_TO_RAD = 1.0;
	
	
	public double locationSimilarity(int a, int b) {
		
		int picA = picIds2position.get(a);
		int picB = picIds2position.get(b);
		
		if((data.getData_location()[picA][DataInput.LATITUDE] == 0 && data.getData_location()[picA][DataInput.LONGITUDE] == 0)
				|| (data.getData_location()[picB][DataInput.LATITUDE] == 0 && data.getData_location()[picB][DataInput.LONGITUDE] == 0)){
			return -1;
		}

		double latitudeArc  = (data.getData_location()[picA][DataInput.LATITUDE] - data.getData_location()[picB][DataInput.LATITUDE]) * DEG_TO_RAD;
	    double longitudeArc = (data.getData_location()[picA][DataInput.LONGITUDE] - data.getData_location()[picB][DataInput.LONGITUDE]) * DEG_TO_RAD;
	    double latitudeH = Math.sin(latitudeArc * 0.5);
	    
	    latitudeH *= latitudeH;
	    double longitudeH = Math.sin(longitudeArc * 0.5);
	    
	    longitudeH *= longitudeH;
	    double tmp = Math.cos(data.getData_location()[picA][DataInput.LATITUDE]*DEG_TO_RAD) * Math.cos(data.getData_location()[picB][DataInput.LATITUDE]*DEG_TO_RAD);
	    
	    return 1.0 - (2.0 * Math.asin(Math.sqrt(latitudeH + tmp*longitudeH)));
	}
	
		
	public double textualSimilarity(int a, int b){
		try{
			return data.getVectors().get(a).cosine(data.getVectors().get(b));
		}catch(Exception e){
			return -1;
		}
		
	}
	
	public double textualSimilarity(int a, int b, TIntObjectHashMap<SparseArray> vectors){
		try{
			return vectors.get(a).cosine(vectors.get(b));
		}catch(Exception e){
			return -1;
		}		
	}
	
	public int ownerSimilarity(int a, int b){
		int picA = picIds2position.get(a);
		int picB = picIds2position.get(b);
		try{
			return data.getOwners()[picA].equals(data.getOwners()[picB]) ? 1 : 0;
		}catch(Exception e){
			return -1;
		}
		
	}
	
	public boolean sameEvent(int a, int b, String dataset){
		
		int picA = picIds2position.get(a);
		int picB = picIds2position.get(b);
	
		String tags1 = data.getTags()[picA];
		String tags2 = data.getTags()[picB];
	
		int offset = (dataset+":event=").length();
		
		StringTokenizer st = new StringTokenizer(tags1, " ");
		String ev1 = "-1";
		while(st.hasMoreTokens()){
			String tmp = st.nextToken();
			if(tmp.startsWith(dataset+":event=")){
				StringTokenizer st2 = new StringTokenizer(tags2, " ");
				while(st2.hasMoreTokens()){
					if(tmp.equals(st2.nextToken())){return true;}
				}
			}
		}
		return false;
//		String ev1 = "-1";
//		while(st.hasMoreTokens()){
//			String tmp = st.nextToken();
//			if(tmp.startsWith(dataset+":event=")){
//				ev1 = tmp;
//				break;
//			}
//		}
//		
//		st = new StringTokenizer(tags2, " ");
//		String ev2 = "-2";
//		while(st.hasMoreTokens()){
//			String tmp = st.nextToken();
//			if(tmp.startsWith(dataset+":event=")){
//				ev2 = tmp;
//				break;
//			}
//		}
		
//		if(!ev1.equals(ev2)){					
//			return false;
//		}else{
//			return true;
//		}
	}


	public TIntIntHashMap getPicIds2position() {
		return picIds2position;
	}


	public void setPicIds2position(TIntIntHashMap picIds2position) {
		this.picIds2position = picIds2position;
	}

}
