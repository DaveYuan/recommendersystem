package util;

import gnu.trove.TIntDoubleHashMap;

public class SparseArray {
	TIntDoubleHashMap values;
	public int lenght = 0;
	
	public SparseArray(){
		values = new TIntDoubleHashMap();
	}
	
	public double mod(){
		double m = 0;
		
		for(double d : values.getValues()){
			m += d*d;
		}
		
		return Math.sqrt(m);
	}
	
	public double sqr(){
		double m = 0;
		
		for(double d : values.getValues()){
			m += d*d;
		}
		
		return m;
	}
	
	public void set(int pos, double val){
		values.put(pos, val);
		if(pos > lenght){
			lenght = pos + 1;
		}
	}
	
	public double get(int pos){
		if(values.containsKey(pos))
			return values.get(pos);
		else
			return 0;
	}
	
	public double cosine(SparseArray sa){
		double cos = 0;
		
		int bigger = lenght;
		
		if(sa.lenght > lenght) bigger = sa.lenght;
		
		for(int i : this.values.keys()){
			if(sa.values.containsKey(i))
				cos += get(i)*sa.get(i);			
		}
		cos = cos / Math.sqrt(this.sqr() * sa.sqr());
		
		return cos;
	}
	
	public int[] getNonZeroIndexes(){
		return this.values.keys();
	}
	
	public void print(){
		for(int i = 0; i < lenght; i++){
			System.out.print(get(i) + " ");
		}
		System.out.println();
	}
}
