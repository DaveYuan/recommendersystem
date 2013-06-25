using System;
using MultiRecommender.Datatype;

namespace MultiRecommender
{
	public interface ISocial
	{
		double regSocial {get; set;}
		SparseMatrix userAssociations {get; set;}
	}
}

