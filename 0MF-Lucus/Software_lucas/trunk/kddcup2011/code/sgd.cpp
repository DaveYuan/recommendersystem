#include <assert.h>
#include <time.h>
#include <stdlib.h>
#include <math.h>
#include <limits.h>
#include <map>
#include "globals.h"
#include "sgd.h"
#include "utils.h"


void gradientDescent(int nIterations, double & correspondingTrainingRmse, double & bestValidationRmse, int & iterCount)
{	
	time_t Start_t, End_t; 	
	double trainingRmse = 999999, validationRmse = 999999,  prevValidationRmse = 999999, prevTrainingRmse=999999;
	unsigned int nFaults=0;
	bestValidationRmse = 999999; correspondingTrainingRmse = 999999;
	double err, estScore;
	unsigned int user, i=0;
	double sqErrSum;
	double totalMse;	

	cout<<"Starting gradientDescent... (mean score is:"<<TrainingMetaData.totalMeanScore<<")"<<endl;
	FactorizationManager->allocate();


	//##################################################################################
	unsigned int currTrain = 0;
	unsigned int currValidation = 0;
	unsigned int currTest = 0;


	for (user=0; user<TrainingMetaData.nUsers; user++){ //iterate on all users
	  unsigned int userRatings = pUsersData[user].ratings;
	  assert(userRatings);
	  unsigned int userStart = pItemRatings_training[currTrain].day;

	  unsigned int userEnd = pItemRatings_test[(user+1)*RATINGS_PER_USER_TEST - 1].day - userStart;
	  unsigned int binLength = ( userEnd / timeBins) + 1;  

	  for (i=0; i<userRatings; i++){
	    unsigned int day = pItemRatings_training[currTrain].day - userStart;
	    unsigned int bin = day/binLength;
	    pItemRatings_training[currTrain].day =  bin;
	    //	    pItemRatings_training[currTrain].day++;
	    currTrain++;
	  }
	  for (i=0; i<RATINGS_PER_USER_VALIDATION; i++){
	    unsigned int day = pItemRatings_validation[currValidation].day - userStart;
	    unsigned int bin = day/binLength;
	    pItemRatings_validation[currValidation].day =  bin;
	    //	    pItemRatings_validation[currValidation].day++;
	    currValidation++;
	  }
	  for (i=0; i<RATINGS_PER_USER_TEST; i++){
	    unsigned int day = pItemRatings_test[currTest].day - userStart;
	    unsigned int bin = day/binLength;
	    pItemRatings_test[currTest].day =  bin;
	    //	    pItemRatings_test[currTest].day++;
	    currTest++;
	  }
	}

	//##################################################################################
	
	//Iterate on training data:
	for (iterCount=0; iterCount<nIterations; iterCount++) //Run for nIterations iterations
	{		
		sqErrSum = 0;		
		unsigned int currentRatingIdx = 0;
		Start_t = time(NULL);
		
		for (user=0; user<TrainingMetaData.nUsers; user++) //iterate on all users
		{		  
		        unsigned int userRatings = pUsersData[user].ratings;
			assert(userRatings);
			for (i=0; i<userRatings; i++) 
			{
				// Change coefficients along computed gradient:
				double err = FactorizationManager->update(pItemRatings_training[currentRatingIdx],user);
				sqErrSum += err*err;
				currentRatingIdx++;
			}
		}

		trainingRmse = sqrt(sqErrSum/TrainingMetaData.nRecords);	
		//if(trainingRmse > prevTrainingRmse){
		//step *= 0.5;
		//} else {
		//  step += step*0.05;
		//}
		//prevTrainingRmse = trainingRmse;
		
		End_t = time(NULL);
		cout<<"Sweep: "<<iterCount+1<<" trainingRMSE="<<trainingRmse;		
		
		//Iterate on validation data:
		totalMse=0;
		currentRatingIdx = 0;

		double typeRMSE[4];
		int numberCases[4];
		for(int m = 0; m < 4; m++){
		   typeRMSE[m] = 0;
		   numberCases[m] = 0;
		}

		for (user=0; user<ValidationMetaData.nUsers; user++) 
		{						
			for (i=0; i<RATINGS_PER_USER_VALIDATION; i++) 
			{
				// compute error:				
				estScore = FactorizationManager->estimate(pItemRatings_validation[currentRatingIdx],user);
				estScore = min(estScore,(double)100);
				estScore = max(estScore,(double)0);
				
				err = pItemRatings_validation[currentRatingIdx].score-estScore;
				totalMse += err*err;
				
				int item = pItemRatings_validation[currentRatingIdx].item;
				typeRMSE[pItems[item].type] += err*err;
				numberCases[pItems[item].type]++;
				currentRatingIdx++;				
			}			
		}					
		validationRmse = sqrt(totalMse/ValidationMetaData.nRecords);
		cout<<"\t\ttvalidationRMSE="<<validationRmse<<"\t\t("<<difftime(End_t, Start_t)<<" secs)"<<endl;	 
		for(int m = 0; m < 4; m++) 
		cout<<"\t\t Error on " << m << " = "<<sqrt(typeRMSE[m]/numberCases[m])<< " Number of ratings: " << numberCases[m] << endl;	 
		
		if (validationRmse>=prevValidationRmse) 
		{
			nFaults++;
			if (nFaults>GRADIENT_DESCENT_FAULTS) 
			{
				cout<<"Early termination since current Validation RMSE ("<<validationRmse<<") is higher than prev. best ("<<bestValidationRmse<<") (number of faults: "<<nFaults<<")"<<endl;
				iterCount -= (GRADIENT_DESCENT_FAULTS);
				break;
			}
		}
		else
		{
			nFaults = 0;
			bestValidationRmse		  = validationRmse;
			correspondingTrainingRmse = trainingRmse;	
			//Reduce steps size: ????
			step				*= decay;
		}
		prevValidationRmse = validationRmse;						
	}	
}

void predictTrack1TestRatings(char * filename)
{
	unsigned int user=0, i=0, currentRatingIdx=0;
	double estScore=0;
	cout<<"Predicting Track1 TEST data into: "<<filename<<endl;
	FILE * fp = fopen(filename, "wt");
	for (user=0; user<TestMetaData.nUsers; user++) 
	{		
		for (i=0; i<RATINGS_PER_USER_TEST; i++) 
		{	
			estScore = FactorizationManager->estimate(pItemRatings_test[currentRatingIdx],user);
			estScore = min(estScore,(double)100);
			estScore = max(estScore,(double)0);				
			currentRatingIdx++;
			fprintf(fp, "%lf\n", estScore);
		}
	}
	assert(TestMetaData.nRecords == currentRatingIdx);				
	fclose(fp);
	cout<<"Done!"<<endl;
	return;
}
