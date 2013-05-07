
#ifndef _TIMEPITF_H_
#define _TIMEPITF_H_

#include "Factorization.h"
#include "readUserData.h"
#include "utils.h"
#include <assert.h>
#include <iostream>

using namespace std;

#define max(A,B) ((A)>=(B) ? (A) : (B))
#define min(A,B) ((A)<=(B) ? (A) : (B))


/*
 *
 * The Factorization Manager class (BiasedMatrixFactorization) handles the items and users biases, 
 * as well as the latent factors.
 *
*/
class TimeBasedPITF : public Factorization
{
public:	

	TimeBasedPITF();
	~TimeBasedPITF();
	void allocate(); // Allocate biases. This method is called before SGD starts iterating on the training dataset.

	inline double getMu();

	//Item biases:
	inline double getItemBias(ItemRating ratingData);
	inline void updateItemBias(ItemRating ratingData, double gradient);

	inline void updateGlobalBias(double gradient);

	//Time biases:
	inline double getTimeBias(ItemRating ratingData);
	inline void updateTimeBias(ItemRating ratingData, double gradient);

	//User biases:
	inline double getUserBias(unsigned int user);
	inline void updateUserBias(unsigned int user, double gradient);

	//Item latent factors
        inline double getItemLatentFactor(ItemRating ratingData, int k);
	
	//User latent factors
        inline double getUserLatentFactor(unsigned int user, int k);
	
	//Update all the latent factors of a given user and item
	inline void updateLatentFactors(unsigned int user, ItemRating ratingData, double gradient);

	double   **ppUserFactors; //A matrix of all user latent factors
	double   **ppUserItemFactors; //A matrix of all user latent factors
	double   **ppUserTimeFactors; //A matrix of all user latent factors
	double   **ppItemFactors; //A matrix of all item latent factors
	double   **ppItemUserFactors; //A matrix of all item latent factors
	double   **ppItemTimeFactors; //A matrix of all item latent factors
	double   **ppTimeFactors; //A matrix of all item latent factors
	double   **ppTimeUserFactors; //A matrix of all time latent factors
	double   **ppTimeItemFactors; //A matrix of all time latent factors

	//This method takes a rating instance and a user ID, and predicts the rating score 
	inline double estimate(ItemRating itemRating,unsigned int user);
	inline double dot(ItemRating itemRating, unsigned int user);
	//Called by SGD to update biases after each rating line:
	inline double update(ItemRating ratingData, unsigned int user);
	
private:
	double  *pItemsBase; //An array of all the items biases
	double  *pUsersBase; //An array of all the users biases
	double  *pTimeBase; //An array of all the users biases
	double  globalBias;

	unsigned int binLength;
	unsigned int day2bin(unsigned int day);
	bool allocateOrCleanBaseVector(double** ppBase, unsigned int length);  // A utility method to allocate new arrays
	bool allocateOrCleanBaseMatrix(double*** ppBase, unsigned int rows, int cols);
	
};

/**********************************************************************
			Inline Functions Go Below:
**********************************************************************/
inline unsigned int TimeBasedPITF::day2bin(unsigned int day){
  //unsigned int lastDay = pItemRatings_test[(user+1)*RATINGS_PER_USER_TEST - 1].day 
  //binLength = ( lastDay / timeBins) + 1;  
  //int bin = day/binLength;
  //if(day%binLength == 0) bin--;
  //return bin;
  return day;
}

inline double TimeBasedPITF::dot(ItemRating itemRating, unsigned int user)
{	 	
	unsigned int item = itemRating.item;
	unsigned int time = day2bin(itemRating.day);

	double estimation = 0;		

	estimation = getMu();
	estimation += getUserBias(user);				
	estimation += getItemBias(itemRating);
	estimation += getTimeBias(itemRating);
			
	int k = 0;
       	for(k = 0; k < dim; k++)
	{
	  estimation += ppUserItemFactors[user][k]*ppItemUserFactors[item][k];
	  estimation += ppUserTimeFactors[user][k]*ppTimeUserFactors[time][k];
	  estimation += ppTimeItemFactors[time][k]*ppItemTimeFactors[item][k];
	  //estimation += ppTimeFactors[time][k]*ppItemFactors[item][k]*ppUserFactors[user][k];
	}
	
	return estimation;
}

inline double TimeBasedPITF::estimate(ItemRating itemRating, unsigned int user)
{		
	double estimation = dot(itemRating,user);
	return MIN_RATING + sig(estimation) * RANGE;
}


inline double TimeBasedPITF::update(ItemRating ratingData, unsigned int user)
{			
	
	unsigned int item = ratingData.item;      	        
        double estimation = dot(ratingData, user);
	double sigm = sig(estimation);
	double estScore = MIN_RATING + sigm * RANGE;

	double err = ratingData.score-estScore;			      

        double gradient = err * sigm * (1 - sigm) * RANGE;
	
	updateItemBias(ratingData,gradient);
	updateUserBias(user,gradient);
	updateTimeBias(ratingData,gradient);
	updateLatentFactors(user,ratingData,gradient);

	return err;

}

inline double TimeBasedPITF::getMu()
{		
         return globalBias;
}

inline double TimeBasedPITF::getItemBias(ItemRating ratingData)
{		
	assert(pItemsBase); 	
	assert(ratingData.item<TrainingMetaData.trainingTotalItems);		
	return pItemsBase[ratingData.item];	
}

inline void TimeBasedPITF::updateItemBias(ItemRating ratingData, double gradient)
{
	unsigned int item = ratingData.item;
	double oldBias=-1, newBias=-1; 
	assert(pItemsBase); assert(item<=TrainingMetaData.trainingTotalItems);		
	oldBias = pItemsBase[ratingData.item];
	pItemsBase[ratingData.item] += (double) step * (gradient - biasReg*oldBias);
}


inline void TimeBasedPITF::updateGlobalBias(double gradient)
{
	double oldBias=-1, newBias=-1; 
	oldBias = globalBias;
	globalBias += (double) step * (gradient - biasReg*oldBias);
}

inline double TimeBasedPITF::getTimeBias(ItemRating ratingData)
{		
	assert(pTimeBase); 	
	assert(ratingData.item<TrainingMetaData.trainingTotalItems);		
	return pTimeBase[day2bin(ratingData.day)];	
}

inline void TimeBasedPITF::updateTimeBias(ItemRating ratingData, double gradient)
{
        unsigned int day = day2bin(ratingData.day);
	double oldBias=-1, newBias=-1; 
	assert(pTimeBase); //assert(item<=TrainingMetaData.trainingTotalItems);		
	oldBias = pTimeBase[day];
	pTimeBase[day] += (double) step * (gradient - biasReg*oldBias);
}

inline double TimeBasedPITF::getUserBias(unsigned int user)
{
	assert(pUsersBase);	
	return pUsersBase[user];
}

inline void TimeBasedPITF::updateUserBias(unsigned int user, double gradient)
{
	assert(pUsersBase);
	double oldBias=0, newBias=0; 
	oldBias = pUsersBase[user];
	pUsersBase[user] += (double) step * (gradient - biasReg*oldBias);
}


inline double TimeBasedPITF::getItemLatentFactor(ItemRating ratingData, int k)
{		
	assert(ppItemUserFactors); 	
	assert(ratingData.item<TrainingMetaData.trainingTotalItems);		
	return ppItemUserFactors[ratingData.item][k];	
}


inline double TimeBasedPITF::getUserLatentFactor(unsigned int user, int k)
{		
	assert(ppUserItemFactors); 	
  	return ppUserItemFactors[user][k];	
}


inline void TimeBasedPITF::updateLatentFactors(unsigned int user, ItemRating ratingData, double gradient)
{
	unsigned int item = ratingData.item;
	unsigned int time = day2bin(ratingData.day);
	assert(ppUserItemFactors);	
	assert(ppItemUserFactors);	 assert(item<=TrainingMetaData.trainingTotalItems);		

       	for(int k = 0; k < dim; k++)
	{
	  double ui_k = ppUserItemFactors[user][k];
	  double ut_k = ppUserTimeFactors[user][k];
	  //double u_k = ppUserFactors[user][k];
	  double tu_k = ppTimeUserFactors[time][k];
	  double ti_k = ppTimeItemFactors[time][k];
	  //double t_k = ppTimeFactors[time][k];
	  double iu_k = ppItemUserFactors[item][k];
	  double it_k = ppItemTimeFactors[item][k];
	  //double i_k = ppItemFactors[item][k];
	  
	  //ppUserFactors[user][k] += (double) step*(gradient*(i_k*t_k) - factorsReg*u_k);	  
	  ppUserItemFactors[user][k] += (double) step*(gradient*(iu_k) - factorsReg*ui_k);	  
	  ppUserTimeFactors[user][k] += (double) step*(gradient*(tu_k) - factorsReg*ut_k);
	  //ppItemFactors[item][k] += (double) step*(gradient*(u_k*t_k) - factorsReg*i_k);   
	  ppItemUserFactors[item][k] += (double) step*(gradient*(ui_k) - factorsReg*iu_k); 
	  ppItemTimeFactors[item][k] += (double) step*(gradient*(ti_k) - factorsReg*it_k); 
	  //ppTimeFactors[time][k] += (double) step*(gradient*(u_k*i_k) - factorsReg*t_k);   
	  ppTimeUserFactors[time][k] += (double) step*(gradient*(ut_k) - factorsReg*tu_k); 
	  ppTimeItemFactors[time][k] += (double) step*(gradient*(it_k) - factorsReg*ti_k);   
	}
}


#endif

