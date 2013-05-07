#include <assert.h>
#include <time.h>
#include <stdlib.h>
#include <math.h>
#include <limits.h>
#include "globals.h"
#include "MRMF.h"
#include "utils.h"

MultiRelationalMatrixFactorization::MultiRelationalMatrixFactorization()
{		
	pItemsBase    = 0;
	pUsersBase    = 0;	
	ppUserFactors = 0;
	ppItemFactors = 0;
	pppUserSpecificFactors = 0;
	ppItemSpecificFactors = 0;

}

MultiRelationalMatrixFactorization::~MultiRelationalMatrixFactorization()
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
	if(pppUserSpecificFactors)
	{
	  for(int j = 0; j < NUM_RELATIONS; j++){
	        for (int i = 0; i < TrainingMetaData.trainingTotalUsers; i++)
		  delete[] pppUserSpecificFactors[j][i];
		delete[] pppUserSpecificFactors[j];
	  }
		delete[] pppUserSpecificFactors;
	}
	if(ppItemSpecificFactors)
	{
      	        for (int i = 0; i < TrainingMetaData.trainingTotalItems; i++)
		  delete[] ppItemSpecificFactors[i];
		delete[] ppItemSpecificFactors;
	  
	
	}

	
	return;
}

bool MultiRelationalMatrixFactorization::allocateOrCleanBaseVector(double** ppBase, unsigned int length)
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

bool MultiRelationalMatrixFactorization::allocateOrCleanBaseMatrix(double*** ppBase, unsigned int rows, int cols)
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

bool MultiRelationalMatrixFactorization::allocateOrCleanBaseTensor(double**** pppBase, unsigned int rows, int cols)
{
  unsigned int i,j;
	if(!(*pppBase))
	{
	        (*pppBase) = new double**[NUM_RELATIONS]; 
		
		for(i = 0; i < NUM_RELATIONS; i++)
		{
 		       (*pppBase)[i] = new double*[rows]; 
		       for(j = 0; j < rows; j++)
		       {
			     (*pppBase)[i][j] = new double[cols];
		       }
		}
		assert(*pppBase);
	}


	//Init all values:
	for(int r = 0; r < NUM_RELATIONS; r++)
        {
	        for(i=0;i<rows;i++)
	        {
		       for(j=0;j<cols;j++)
     		              (*pppBase)[r][i][j] = randDouble(-0.01,0.01);
		       //		              (*pppBase)[r][i][j] = 0;
	        }
        }

	return true;
}


void MultiRelationalMatrixFactorization::allocate()
{			
  if(numberPasses == 0){

	allocateOrCleanBaseVector(&pUsersBase,TrainingMetaData.trainingTotalUsers);
	allocateOrCleanBaseVector(&pItemsBase,TrainingMetaData.trainingTotalItems);
	allocateOrCleanBaseMatrix(&ppUserFactors,TrainingMetaData.trainingTotalUsers, dim);
	allocateOrCleanBaseMatrix(&ppItemFactors,TrainingMetaData.trainingTotalItems, dim);	
	allocateOrCleanBaseTensor(&pppUserSpecificFactors,TrainingMetaData.trainingTotalUsers, spec_dim);
	//allocateOrCleanBaseTensor(&pppItemSpecificFactors,TrainingMetaData.trainingTotalItems, spec_dim);
	allocateOrCleanBaseMatrix(&ppItemSpecificFactors,TrainingMetaData.trainingTotalItems, spec_dim);
	
	double global_avg = TrainingMetaData.totalMeanScore;	
	globalBias = log((global_avg - MIN_RATING)/(MAX_RATING - global_avg));

  } else {
    // initializeBaseTensor(&pppUserSpecificFactors,TrainingMetaData.trainingTotalUsers, spec_dim);
    // initializeBaseTensor(&pppItemSpecificFactors,TrainingMetaData.trainingTotalItems, spec_dim);
  }
}


bool MultiRelationalMatrixFactorization::initializeBaseTensor(double**** pppBase, unsigned int rows, int cols)
{
  unsigned int i,j;

	//Init all values:
	for(int r = 0; r < NUM_RELATIONS; r++)
        {
	        for(i=0;i<rows;i++)
	        {
		       for(j=0;j<cols;j++)
     		              (*pppBase)[r][i][j] = randDouble(-0.01,0.01);
	        }
        }

	return true;
}
