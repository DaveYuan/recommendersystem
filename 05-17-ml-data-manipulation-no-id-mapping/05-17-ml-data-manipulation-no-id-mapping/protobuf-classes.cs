using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace mldatamanipulationnoidmapping
{
    [ProtoContract]
    class Rating
    {
        [ProtoMember(1)]
        public List<int> usersList {get; set;}
        [ProtoMember(2)]
        public List<int> itemsList {get; set;}
		[ProtoMember(3)]
		public List<int> ratingsList {get; set;}
    }
	
}
