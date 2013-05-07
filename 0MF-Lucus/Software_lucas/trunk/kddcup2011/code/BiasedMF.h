
#ifndef _BIASEDMF_H_
#define _BIASEDMF_H_

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
class BiasedMatrixFactorization: public Factorization
{
public:	

	BiasedMatrixFactorization();
	~BiasedMatrixFactorization();
	void allocate(); // Allocate biases. This method is called before SGD starts iterating on the training dataset.

	inline double getMu();

	//Item biases:
	inline double getItemBias(ItemRating ratingData);
	inline void updateItemBias(ItemRating ratingData, double gradient);

	inline void updateGlobalBias(double gradient);

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
	double   **ppItemFactors; //A matrix of all item latent factors

	//This method takes a rating instance and a user ID, and predicts the rating score 
	inline double estimate(ItemRating itemRating,unsigned int user);
	inline double dot(ItemRating itemRating, unsigned int user);
	//Called by SGD to update biases after each rating line:
	inline double	update(ItemRating ratingData, unsigned int user);
	
private:
	double	*pItemsBase; //An array of all the items biases
	double  *pUsersBase; //An array of all the users biases
	double  globalBias;

	
	bool allocateOrCleanBaseVector(double** ppBase, unsigned int length);  // A utility method to allocate new arrays
	bool allocateOrCleanBaseMatrix(double*** ppBase, unsigned int rows, int cols);
	
};

/**********************************************************************
			Inline Functions Go Below:
**********************************************************************/
inline double BiasedMatrixFactorization::dot(ItemRating itemRating, unsigned int user)
{	 	
	unsigned int item = itemRating.item;
	double estimation = 0;		

	estimation = getMu();
	//estimation += itemRating.bias;
	estimation += getUserBias(user);				
	estimation += getItemBias(itemRating);
			
	int k = 0;
       	for(k = 0; k < dim; k++)
	{
	  estimation += ppUserFactors[user][k]*ppItemFactors[item][k];
	}

	return estimation;
}

inline double BiasedMatrixFactorization::estimate(ItemRating itemRating, unsigned int user)
{		
	double estimation = dot(itemRating,user);
	return MIN_RATING + sig(estimation) * RANGE;
}


inline double BiasedMatrixFactorization::update(ItemRating ratingData, unsigned int user)
{			
	
	unsigned int item = ratingData.item;      	        
        double estimation = dot(ratingData, user);
	double sigm = sig(estimation);
	double estScore = MIN_RATING + sigm * RANGE;

	double err = ratingData.score-estScore;			      

        double gradient = err * sigm * (1 - sigm) * RANGE;
	
	updateGlobalBias(gradient);
	updateItemBias(ratingData,gradient);
	updateUserBias(user,gradient);
	updateLatentFactors(user,ratingData,gradient);

	return err;

}

inline double BiasedMatrixFactorization::getMu()
{		
         return globalBias;
}

inline void BiasedMatrixFactorization::updateGlobalBias(double gradient)
{
	double oldBias=-1, newBias=-1; 
	oldBias = globalBias;
	globalBias += (double) step * (gradient - biasReg*oldBias);
}

inline double BiasedMatrixFactorization::getItemBias(ItemRating ratingData)
{		
	assert(pItemsBase); 	
	assert(ratingData.item<TrainingMetaData.trainingTotalItems);		
	return pItemsBase[ratingData.item];	
}

inline void BiasedMatrixFactorization::updateItemBias(ItemRating ratingData, double gradient)
{
	unsigned int item = ratingData.item;
	double oldBias=-1, newBias=-1; 
	assert(pItemsBase); assert(item<=TrainingMetaData.trainingTotalItems);		
	oldBias = pItemsBase[ratingData.item];
	pItemsBase[ratingData.item] += (double) step * (gradient - biasReg*oldBias);
}

inline double BiasedMatrixFactorization::getUserBias(unsigned int user)
{
	assert(pUsersBase);	
	return pUsersBase[user];
}

inline void BiasedMatrixFactorization::updateUserBias(unsigned int user, double gradient)
{
	assert(pUsersBase);
	double oldBias=0, newBias=0; 
	oldBias = pUsersBase[user];
	pUsersBase[user] += (double) step * (gradient - biasReg*oldBias);
}


inline double BiasedMatrixFactorization::getItemLatentFactor(ItemRating ratingData, int k)
{		
	assert(ppItemFactors); 	
	assert(ratingData.item<TrainingMetaData.trainingTotalItems);		
	return ppItemFactors[ratingData.item][k];	
}


inline double BiasedMatrixFactorization::getUserLatentFactor(unsigned int user, int k)
{		
	assert(ppUserFactors); 	
  	return ppUserFactors[user][k];	
}


inline void BiasedMatrixFactorization::updateLatentFactors(unsigned int user, ItemRating ratingData, double gradient)
{
	unsigned int item = ratingData.item;
	assert(ppUserFactors);	
	assert(ppItemFactors);	 assert(item<=TrainingMetaData.trainingTotalItems);		

       	for(int k = 0; k < dim; k++)
	{
	  double u_k = ppUserFactors[user][k];
	  double i_k = ppItemFactors[item][k];
	  
	  ppUserFactors[user][k] += (double) step*(gradient*i_k - factorsReg*u_k);	  	  
	  ppItemFactors[item][k] += (double) step*(gradient*u_k - factorsReg*i_k);	  	  
	}
}


#endif

