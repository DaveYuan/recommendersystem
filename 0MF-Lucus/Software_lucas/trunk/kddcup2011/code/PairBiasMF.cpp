#include <assert.h>
#include <time.h>
#include <stdlib.h>
#include <math.h>
#include <limits.h>
#include "globals.h"
#include "PairBiasMF.h"
#include "utils.h"

PairwiseBiasMatrixFactorization::PairwiseBiasMatrixFactorization()
{		
	pItemsBase     = 0;
	pUsersBase     = 0;	
	ppUserFactors  = 0;
	ppItemFactors  = 0;
	pUserAlbumBase = 0;
	pUserArtistBase = 0; 
	pUserGenreBase = 0;

}

PairwiseBiasMatrixFactorization::~PairwiseBiasMatrixFactorization()
{
	if(pItemsBase)
	{
		delete pItemsBase;
		pItemsBase = 0;
	}
	if(pUsersBase)
	{
		delete pUsersBase;
		pUsersBase = 0;
	}
	if(pUserAlbumBase)
	{
	        delete[] pUserAlbumBase;
		pUserAlbumBase = 0;
	}
	if(pUserArtistBase)
	{
		delete[] pUserArtistBase;
		pUserArtistBase = 0;
	}
	if(pUserGenreBase)
	{
		delete[] pUserGenreBase;
		pUserGenreBase = 0;
	}
	if(ppUserFactors)
	{
	        for (int i = 0; i < TrainingMetaData.trainingTotalUsers; i++)
		  delete[] ppUserFactors[i];
		delete[] ppUserFactors;
	}
	if(ppItemFactors)
	{
	        for (int i = 0; i < TrainingMetaData.trainingTotalItems; i++)
		  delete[] ppItemFactors[i];
		delete[] ppItemFactors;
	}
	
	
	return;
}

bool PairwiseBiasMatrixFactorization::allocateOrCleanPairVector(map<int,float>** ppBase, unsigned int length)
{

	if(!(*ppBase))
	{
	  (*ppBase) = new map<int,float>[length]; 
		assert(*ppBase);
	}

	return true;
}

bool PairwiseBiasMatrixFactorization::allocateOrCleanBaseVector(double** ppBase, unsigned int length)
{
	unsigned int i;
	if(!(*ppBase))
	{
		(*ppBase) = new double[length]; 
		assert(*ppBase);
	}
	//Clear all values:
	for(i=0;i<length;i++)
	{
		(*ppBase)[i]=0;
	}
	return true;
}

bool PairwiseBiasMatrixFactorization::allocateOrCleanBaseMatrix(double*** ppBase, unsigned int rows, int cols, double init)
{
	unsigned int i;
	if(!(*ppBase))
	{
		(*ppBase) = new double*[rows]; 
		for(i = 0; i < rows; i++)
		{
 			(*ppBase)[i] = new double[cols];
		}
		assert(*ppBase);
	}


	//Init all values:
	for(i=0;i<rows;i++)
	{
		for(int j=0;j<cols;j++)
		  (*ppBase)[i][j] = randDouble(-init,init);
	}

	return true;
}


void PairwiseBiasMatrixFactorization::allocate()
{			
	allocateOrCleanBaseVector(&pUsersBase,TrainingMetaData.trainingTotalUsers);
	allocateOrCleanBaseVector(&pItemsBase,TrainingMetaData.trainingTotalItems);
	allocateOrCleanBaseMatrix(&ppUserFactors,TrainingMetaData.trainingTotalUsers, dim,0.01);
	allocateOrCleanBaseMatrix(&ppItemFactors,TrainingMetaData.trainingTotalItems, dim,0.01);	
	allocateOrCleanPairVector(&pUserAlbumBase,TrainingMetaData.trainingTotalUsers); 
	allocateOrCleanPairVector(&pUserArtistBase,TrainingMetaData.trainingTotalUsers); 
	allocateOrCleanPairVector(&pUserGenreBase,TrainingMetaData.trainingTotalUsers); 
	
	double global_avg = TrainingMetaData.totalMeanScore;	
	//globalBias = global_avg;
	//globalBias    = (global_avg - MIN_RATING) / (MAX_RATING - MIN_RATING);
	globalBias = log((global_avg - MIN_RATING)/(MAX_RATING - global_avg));
}



