using System;

namespace HPlearnSocialBPRMF
{
	public interface ISocial
	{
		double regSocial {get; set;}
		SparseMatrix userAssociations {get; set;}
	}
}

