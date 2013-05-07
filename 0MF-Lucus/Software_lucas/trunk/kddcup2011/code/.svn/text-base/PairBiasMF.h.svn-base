
#ifndef _PAIRBIASMF_H_
#define _PAIRBIASMF_H_

#include "Factorization.h"
#include "readUserData.h"
#include "utils.h"
#include <assert.h>
#include <iostream>
#include <map>

using namespace std;

#define ALBUM_BIAS  0
#define ARTIST_BIAS 1
#define GENRE_BIAS  2

/*
 *
 * PairwiseBiasMatrixFactorization class 
 * It works exactly like BiasedMF but the prediction for a track or an album 
 * contains biases specifics for Artists and Genres
 *
*/
class PairwiseBiasMatrixFactorization: public Factorization
{
public:	

	PairwiseBiasMatrixFactorization();
	~PairwiseBiasMatrixFactorization();
	void allocate(); // Allocate biases. This method is called before SGD starts iterating on the training dataset.

	inline double getMu();

	//Item Biases:
	inline double getItemBias(ItemRating ratingData);
	inline double getSumItemBias(ItemRating ratingData, int user);
	inline void updateItemBias(ItemRating ratingData, double gradient);

	//User biases:
	inline double getUserBias(unsigned int user);
	inline double getPairBias(unsigned int user, unsigned int item, map<int,float>** biases);
	inline void updateUserBias(unsigned int user, double gradient);
	inline void updateUserItemBiases(ItemRating ratingData, int user, double gradient);

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
	map<int,float> *pUserAlbumBase; //An array of all pairwise user-album biases
	map<int,float> *pUserArtistBase; //An array of all pairwise user-artist biases
	map<int,float> *pUserGenreBase; //An array of all pairwise user-genre biases
	double  globalBias;

	
	bool allocateOrCleanBaseVector(double** ppBase, unsigned int length);  // A utility method to allocate new arrays
	bool allocateOrCleanBaseMatrix(double*** ppBase, unsigned int rows, int cols, double init);
	bool allocateOrCleanPairVector(map<int,float>** ppBase, unsigned int length);
	
};



/**********************************************************************
			Inline Functions Go Below:
**********************************************************************/


inline double PairwiseBiasMatrixFactorization::dot(ItemRating itemRating, unsigned int user)
{		
	unsigned int item = itemRating.item;
	double estimation = 0;		

	estimation = getMu();
	estimation+= getUserBias(user);				
	estimation+= getItemBias(itemRating);
	estimation+= getSumItemBias(itemRating, user);
		
	int k = 0;
       	for(k = 0; k < dim; k++)
	{
	  estimation += ppUserFactors[user][k]*ppItemFactors[item][k];
	}

	return estimation;
}

inline double PairwiseBiasMatrixFactorization::estimate(ItemRating itemRating, unsigned int user)
{		
	double estimation = dot(itemRating,user);		
	return MIN_RATING + sig(estimation) * RANGE;
}

inline double PairwiseBiasMatrixFactorization::update(ItemRating ratingData, unsigned int user)
{			
	unsigned int item = ratingData.item;
	double estimation = dot(ratingData, user);
	double sigm = sig(estimation);
	double estScore = MIN_RATING + sigm * RANGE;

	double err = ratingData.score-estScore;			      

        double gradient = err * sigm * (1 - sigm) * RANGE;
	
	updateItemBias(ratingData,gradient);
	updateUserBias(user,gradient);
	updateUserItemBiases(ratingData,user,gradient);
	updateLatentFactors(user,ratingData,gradient);

	return err;
}

inline double PairwiseBiasMatrixFactorization::getMu()
{		
         return globalBias;
}

inline double PairwiseBiasMatrixFactorization::getItemBias(ItemRating ratingData)
{		
	assert(pItemsBase); 	
	assert(ratingData.item<TrainingMetaData.trainingTotalItems);		
	return pItemsBase[ratingData.item];	
}

inline double PairwiseBiasMatrixFactorization::getSumItemBias(ItemRating ratingData, int user)
{		
        int item = ratingData.item;
	assert(pItemsBase); 	
	assert(ratingData.item<TrainingMetaData.trainingTotalItems);		
	double biasSum = 0;
	if(pItems[item].artist != NONE){
	    int id = pItems[item].artist;
	    biasSum += getPairBias(user,id,&pUserArtistBase);
	}
	if(pItems[item].genre != NULL){
	    for(int i = 0; i < pItems[item].genre->size(); i++ ){
	      int id = pItems[item].genre->at(i);
	      biasSum += getPairBias(user,id,&pUserGenreBase);
	    }
	}
        if(pItems[item].album != NONE){
	    int id = pItems[item].album;
	    biasSum += getPairBias(user,id,&pUserAlbumBase);
	}
	
	return biasSum;
}

inline void PairwiseBiasMatrixFactorization::updateItemBias(ItemRating ratingData, double gradient)
{
	unsigned int item = ratingData.item;
	double oldBias=-1, newBias=-1; 
	assert(pItemsBase); assert(item<=TrainingMetaData.trainingTotalItems);		
	oldBias = pItemsBase[ratingData.item];
	pItemsBase[ratingData.item] += (double) step * (gradient - biasReg*oldBias);

}

inline void PairwiseBiasMatrixFactorization::updateUserItemBiases(ItemRating ratingData, int user, double gradient)
{
	unsigned int item = ratingData.item;
	double oldBias=-1, newBias=-1; 
	assert(pItemsBase); assert(item<=TrainingMetaData.trainingTotalItems);		

	if(pItems[item].artist != NONE){
    	    int id = pItems[item].artist;
	    oldBias = getPairBias(user,id,&pUserArtistBase);
	    if(oldBias==0) pUserArtistBase[user][id] = 0;
	    pUserArtistBase[user][id] += (double) step * (gradient - biasReg*oldBias);
        }
        if(pItems[item].genre != NULL){
	    for(int i = 0; i < pItems[item].genre->size(); i++ ){
	      int id = pItems[item].genre->at(i);
	      oldBias = getPairBias(user,id,&pUserGenreBase);
	      if(oldBias==0) pUserGenreBase[user][id] = 0;
	      pUserGenreBase[user][id] += (double) step * (gradient - biasReg*oldBias);
	    }
        }  
        if(pItems[item].album != NONE){
	    int id = pItems[item].album;
	    oldBias = getPairBias(user,id,&pUserAlbumBase);
    	    if(oldBias==0) pUserAlbumBase[user][id] = 0;
	    pUserAlbumBase[user][id] += (double) step * (gradient - biasReg*oldBias);
        }

}

inline double PairwiseBiasMatrixFactorization::getUserBias(unsigned int user)
{
	assert(pUsersBase);	
	return pUsersBase[user];
}

inline void PairwiseBiasMatrixFactorization::updateUserBias(unsigned int user, double gradient)
{
	assert(pUsersBase);
	double oldBias=0, newBias=0; 
	oldBias = pUsersBase[user];
	pUsersBase[user] += (double) step * (gradient - biasReg*oldBias);
}


inline double PairwiseBiasMatrixFactorization::getItemLatentFactor(ItemRating ratingData, int k)
{		
	assert(ppItemFactors); 	
	assert(ratingData.item<TrainingMetaData.trainingTotalItems);		
	return ppItemFactors[ratingData.item][k];	
}


inline double PairwiseBiasMatrixFactorization::getUserLatentFactor(unsigned int user, int k)
{		
	assert(ppUserFactors); 	
  	return ppUserFactors[user][k];	
}


inline void PairwiseBiasMatrixFactorization::updateLatentFactors(unsigned int user, ItemRating ratingData, double gradient)
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


inline double PairwiseBiasMatrixFactorization::getPairBias(unsigned int user, unsigned int item, map<int,float>** biases)
{
        if((*biases)[user].count(item) > 0)
	{
   	     return (*biases)[user][item];
        }
	return 0;   
}

#endif

