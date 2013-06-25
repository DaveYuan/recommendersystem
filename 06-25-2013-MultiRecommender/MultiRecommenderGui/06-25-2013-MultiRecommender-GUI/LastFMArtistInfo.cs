using System;
using System.IO;
using System.Collections.Generic;

namespace MultiRecommenderGUI
{
	public sealed class Artist
	{
		// TODO: Add url and picture
		public int id{ get; private set; }
		public string name { get; private set; }

		public Artist(int _id, string _name)
		{
			id = _id;
			name = _name;
		}
	}

	public class LastFMArtistInfo
	{
		public List<Artist> artistList;

		public void readArtistInfo(StreamReader reader)
		{
			artistList = new List<Artist>();
			var separators = new string[]{"\t"};
			string line;

			while (!reader.EndOfStream)
			{
				line = reader.ReadLine();

				string[] words = line.Split(separators, StringSplitOptions.None);

				if (words.Length != 4)
					throw new FormatException("Expected 4 columns: " + line);

				int artistId  = Convert.ToInt32(words[0]);
				string artistName = words[1];
				// TODO: Include these in class Artist
//				string artistUrl = words[2];
//				string artistPictureUrl = words[3];
				artistList.Add(new Artist(artistId, artistName));
			}
		}
	}
}

