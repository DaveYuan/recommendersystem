using System;
using System.Collections.Generic;
using ProtoBuf;

namespace HPlearnSocialBPRMF
{
    [ProtoContract]
	public class Association
	{
		[ProtoMember(1)]
		public List<int> user1List {get; set;}
		[ProtoMember(2)]
		public List<int> user2List {get; set;}
	}

    [ProtoContract]
    public class Train
    {
        [ProtoMember(1)]
        public List<int> usersList {get; set;}
        [ProtoMember(2)]
        public List<int> itemsList {get; set;}
		[ProtoMember(3)]
		public List<int> ratingsList {get; set;}
    }
	
	[ProtoContract]
    public class Validation
    {
        [ProtoMember(1)]
        public List<int> usersList {get; set;}
        [ProtoMember(2)]
        public List<int> itemsList {get; set;}
		[ProtoMember(3)]
		public List<int> ratingsList {get; set;}
    }
	
	[ProtoContract]
    public class Test
    {
        [ProtoMember(1)]
        public List<int> usersList {get; set;}
        [ProtoMember(2)]
        public List<int> itemsList {get; set;}
		[ProtoMember(3)]
		public List<int> ratingsList {get; set;}
    }
}