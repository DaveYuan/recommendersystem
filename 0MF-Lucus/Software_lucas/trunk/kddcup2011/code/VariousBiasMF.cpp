#include <assert.h>
#include <time.h>
#include <stdlib.h>
#include <math.h>
#include <limits.h>
#include "globals.h"
#include "VariousBiasMF.h"
#include "utils.h"

MultiBiasMatrixFactorization::MultiBiasMatrixFactorization()
{		
	pItemsBase    = 0;
	pItemAttributesBase = 0;
	pUsersBase    = 0;	
	ppUserFactors = 0;
	ppItemFactors = 0;
	

}

MultiBiasMatrixFactorization::~MultiBiasMatrixFactorization()
{
	if(pItemsBase)
	{
		delete pItemsBase;
		pItemsBase = 0;
	}
	if(pItemAttributesBase)
	{
		delete pItemAttributesBase;
		pItemAttributesBase = 0;
	}
	if(pUsersBase)
	{
		delete pUsersBase;
		pUsersBase = 0;
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

bool MultiBiasMatrixFactorization::allocateOrCleanBaseVector(double** ppBase, unsigned int length)
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

bool MultiBiasMatrixFactorization::allocateOrCleanBaseMatrix(double*** ppBase, unsigned int rows, int cols)
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
		  (*ppBase)[i][j] = randDouble(-0.01,0.01);
	}

	return true;
}


void MultiBiasMatrixFactorization::allocate()
{			
	allocateOrCleanBaseVector(&pUsersBase,TrainingMetaData.trainingTotalUsers);
	allocateOrCleanBaseVector(&pItemsBase,TrainingMetaData.trainingTotalItems);
	allocateOrCleanBaseVector(&pItemAttributesBase,TrainingMetaData.trainingTotalItems);
	allocateOrCleanBaseMatrix(&ppUserFactors,TrainingMetaData.trainingTotalUsers, dim);
	allocateOrCleanBaseMatrix(&ppItemFactors,TrainingMetaData.trainingTotalItems, dim);	

	
	double global_avg = TrainingMetaData.totalMeanScore;	
	//globalBias = global_avg;
	//globalBias    = (global_avg - MIN_RATING) / (MAX_RATING - MIN_RATING);
	globalBias = log((global_avg - MIN_RATING)/(MAX_RATING - global_avg));
}



