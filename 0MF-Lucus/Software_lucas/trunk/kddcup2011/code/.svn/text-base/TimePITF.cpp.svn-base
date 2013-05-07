#include <assert.h>
#include <time.h>
#include <stdlib.h>
#include <math.h>
#include <limits.h>
#include "globals.h"
#include "TimePITF.h"
#include "utils.h"

TimeBasedPITF::TimeBasedPITF()
{		
	pItemsBase    = 0;
	pUsersBase    = 0;	
	pTimeBase    = 0;	
	ppUserFactors = 0;
	ppItemFactors = 0;
	ppTimeFactors = 0;
	ppUserItemFactors = 0;
	ppItemUserFactors = 0;
	ppUserTimeFactors = 0;
	ppItemTimeFactors = 0;
	ppTimeUserFactors = 0;
	ppTimeItemFactors = 0;

}

TimeBasedPITF::~TimeBasedPITF()
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
	if(pTimeBase)
	{
		delete pTimeBase;
		pTimeBase = 0;
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
	if(ppTimeFactors)
	{
	        for (int i = 0; i < timeBins; i++)
		  delete[] ppTimeFactors[i];
		delete[] ppTimeFactors;
	}
	if(ppUserItemFactors)
	{
	        for (int i = 0; i < TrainingMetaData.trainingTotalUsers; i++)
		  delete[] ppUserItemFactors[i];
		delete[] ppUserItemFactors;
	}
	if(ppItemUserFactors)
	{
	        for (int i = 0; i < TrainingMetaData.trainingTotalItems; i++)
		  delete[] ppItemUserFactors[i];
		delete[] ppItemUserFactors;
	}
	if(ppUserTimeFactors)
	{
	        for (int i = 0; i < TrainingMetaData.trainingTotalUsers; i++)
		  delete[] ppUserTimeFactors[i];
		delete[] ppUserTimeFactors;
	}
	if(ppItemTimeFactors)
	{
	        for (int i = 0; i < TrainingMetaData.trainingTotalItems; i++)
		  delete[] ppItemTimeFactors[i];
		delete[] ppItemTimeFactors;
	}
	if(ppTimeUserFactors)
	{
	        for (int i = 0; i < timeBins; i++)
		  delete[] ppTimeUserFactors[i];
		delete[] ppTimeUserFactors;
	}
	if(ppTimeItemFactors)
	{
	        for (int i = 0; i < timeBins; i++)
		  delete[] ppTimeItemFactors[i];
		delete[] ppTimeItemFactors;
	}
	
	return;
}

bool TimeBasedPITF::allocateOrCleanBaseVector(double** ppBase, unsigned int length)
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

bool TimeBasedPITF::allocateOrCleanBaseMatrix(double*** ppBase, unsigned int rows, int cols)
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


void TimeBasedPITF::allocate()
{			
	allocateOrCleanBaseVector(&pUsersBase,TrainingMetaData.trainingTotalUsers);
	allocateOrCleanBaseVector(&pItemsBase,TrainingMetaData.trainingTotalItems);
	allocateOrCleanBaseVector(&pTimeBase,timeBins);
	//allocateOrCleanBaseMatrix(&ppUserFactors,TrainingMetaData.trainingTotalUsers, dim);
	//allocateOrCleanBaseMatrix(&ppItemFactors,TrainingMetaData.trainingTotalItems, dim);	
	allocateOrCleanBaseMatrix(&ppUserItemFactors,TrainingMetaData.trainingTotalUsers, dim);
	allocateOrCleanBaseMatrix(&ppItemUserFactors,TrainingMetaData.trainingTotalItems, dim);	
	allocateOrCleanBaseMatrix(&ppUserTimeFactors,TrainingMetaData.trainingTotalUsers, dim);
	allocateOrCleanBaseMatrix(&ppItemTimeFactors,TrainingMetaData.trainingTotalItems, dim);	
	//allocateOrCleanBaseMatrix(&ppTimeFactors,timeBins, dim);	
	allocateOrCleanBaseMatrix(&ppTimeUserFactors,timeBins, dim);	
	allocateOrCleanBaseMatrix(&ppTimeItemFactors,timeBins, dim);	
	
	
	double global_avg = TrainingMetaData.totalMeanScore;	
	//globalBias = global_avg;
	//globalBias    = (global_avg - MIN_RATING) / (MAX_RATING - MIN_RATING);
	globalBias = log((global_avg - MIN_RATING)/(MAX_RATING - global_avg));
	binLength = (DAYS_TOTAL / timeBins) + 1;
}



