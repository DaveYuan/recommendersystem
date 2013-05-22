using ProtoBuf;
using System.Collections.Generic;

namespace bprmodularoldeval
{
	[ProtoContract]
    	class Train
    	{
        	[ProtoMember(1)]
	        public List<int> usersList {get; set;}
        	[ProtoMember(2)]
	        public List<int> itemsList {get; set;}
			[ProtoMember(3)]
			public List<int> ratingsList {get; set;}
    	}
	
	[ProtoContract]
	class Test
	{
	        [ProtoMember(1)]
        	public List<int> usersList {get; set;}
	        [ProtoMember(2)]
        	public List<int> itemsList {get; set;}
			[ProtoMember(3)]
			public List<int> ratingsList {get; set;}
	}

}