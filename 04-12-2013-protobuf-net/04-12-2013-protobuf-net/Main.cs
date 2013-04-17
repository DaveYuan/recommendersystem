using System;
using System.IO;
using ProtoBuf;

namespace protobufnet
{
	[ProtoContract]
	public class Person
	{
	    [ProtoMember(1)]
	    public string Name { get; set; }
	    [ProtoMember(2)]
	    public int Age { get; set; }
	    [ProtoMember(3)]
	    public DateTime DateOfBirth { get; set; }        
	    [ProtoMember(4)]        
	    public Address Address { get; set; }
	}
	
	[ProtoContract]
	public class Address
	{
	    [ProtoMember(1)]
	    public string Number { get; set; }
	    [ProtoMember(2)]
	    public string StreetName { get; set; }
	}
	
	class MainClass
	{		
		public void serializeDeserializeData()
		{
			var person = new Person {
		        Name = "Fred",
		        Address = new Address {
		            Number = "Flat 1",
		            StreetName = "The Meadows"
		        }
		    };
		    using (var file = File.Create("person.bin")) {
		        Serializer.Serialize(file, person);
		    }	
			
			Person newPerson;
            using (FileStream file = File.OpenRead("person.bin"))
            {
                newPerson = Serializer.Deserialize<Person>(file);
            }

            Console.WriteLine("expected Name {0}", person.Name);
            Console.WriteLine("actual Name {0}", newPerson.Name);
		}
		
		public static void Main (string[] args)
		{			
			MainClass mainclass = new MainClass();
			mainclass.serializeDeserializeData();
			Console.WriteLine ("Hello World!");
		}
	}
}

