using System;

namespace MultiRecommender
{
	public interface ISocial
	{
		double regSocial {get; set;}
		SparseMatrix userAssociations {get; set;}
	}
}

