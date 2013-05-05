using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace lastfmsndatamanipulation
{
    [ProtoContract]
    class Trust
    {
        [ProtoMember(1)]
        public List<int> trustUserList1 {get;set;}
        [ProtoMember(2)]
        public List<int> trustUserList2 {get; set;}
    }

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
