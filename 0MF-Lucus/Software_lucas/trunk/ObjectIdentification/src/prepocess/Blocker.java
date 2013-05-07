package prepocess;

import io.DataInput;

import java.io.IOException;

public abstract class Blocker {

	public abstract String samplePairs(DataInput in) throws IOException;
}
