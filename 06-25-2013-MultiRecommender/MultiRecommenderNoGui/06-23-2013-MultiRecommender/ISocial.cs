using System;
using MultiRecommender.Datatype;

namespace MultiRecommender
{
	public interface ISocial
	{
		SparseMatrix userAssociations {get; set;}
	}
}

