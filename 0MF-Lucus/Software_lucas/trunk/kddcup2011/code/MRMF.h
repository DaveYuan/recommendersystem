
#ifndef _MRMF_H_
#define _MRMF_H_

#include "Factorization.h"
#include "readUserData.h"
#include "globals.h"
#include "utils.h"
#include <assert.h>
#include <iostream>

#define TRACK_ALBUM   4
#define TRACK_ARTIST  5
#define TRACK_GENRE   6
#define ALBUM_ARTIST  7
#define ALBUM_GENRE   8

using namespace std;


/*
 *
 * Multi Relational Matrix Factorization class 
 * 
 * Right now it treats the user x items matrix as a set of different
 * relations between users and different types of items (i.e. tracks,
 * albums, artists and genres
*/
class MultiRelationalMatrixFactorization: public Factorization
{
public:	

	MultiRelationalMatrixFactorization();
	~MultiRelationalMatrixFactorization();
	void allocate(); // Allocate biases. This method is called before SGD starts iterating on the training dataset.

	inline double getMu();

	//Item Biases:
	inline double getItemBias(ItemRating ratingData);
	inline void updateItemBias(ItemRating ratingData, double gradient);

	//User biases:
	inline double getUserBias(unsigned int user);
	inline void updateUserBias(unsigned int user, double gradient);

	//Item latent factors
        inline double getItemLatentFactor(ItemRating ratingData, int k);
	inline double getItemSpecificLatentFactor(ItemRating ratingData, int k, int relation);
	
	//User latent factors
        inline double getUserLatentFactor(unsigned int user, int k);
        inline double getUserSpecificLatentFactor(unsigned int user, int k, int relation);
	
	//Update all the latent factors of a given user and item
	inline void updateLatentFactors(unsigned int user, ItemRating ratingData, double gradient);
        inline void updateSpecificLatentFactors(unsigned int user, ItemRating ratingData, double gradient);

	inline void updateItemLatentFactors(int item1, int item2, double gradient);

	//This method takes a rating instance and a user ID, and predicts the rating score 
	inline double estimate(ItemRating itemRating,unsigned int user);
	inline double estimateItem(int item1, int item2);

	inline double dot(ItemRating itemRating, unsigned int user);

	//Called by SGD to update biases after each rating line:
	inline double	update(ItemRating ratingData, unsigned int user);
	inline double   updateItem(ItemRating ratingData, int item2, double score);
	
private:
	double	*pItemsBase; //An array of all the items biases
	double  *pUsersBase; //An array of all the users biases
	double  globalBias;

	double   **ppUserFactors; //A matrix of all user shared latent factors
	double   **ppItemFactors; //A matrix of all item shared latent factors

	double   ***pppUserSpecificFactors; //A matrix of all user latent factors specific to a relation
	double   **ppItemSpecificFactors; //A matrix of all item latent factors specific to a relation
	
	bool allocateOrCleanBaseVector(double** ppBase, unsigned int length);  // A utility method to allocate new arrays
	bool allocateOrCleanBaseMatrix(double*** ppBase, unsigned int rows, int cols);
        bool allocateOrCleanBaseTensor(double**** pppBase, unsigned int rows, int cols);

	bool initializeBaseTensor(double**** pppBase, unsigned int rows, int cols);
	
};



/**********************************************************************
			Inline Functions Go Below:
**********************************************************************/


inline double MultiRelationalMatrixFactorization::dot(ItemRating itemRating, unsigned int user)
{		
	unsigned int item = itemRating.item;
	double estimation = 0;		
	int rel = pItems[item].type;

	estimation = getMu();
	estimation+= getUserBias(user);				
	estimation+= getItemBias(itemRating);
	
		
	for(int k = 0; k < dim; k++)
	{
	  estimation += ppUserFactors[user][k]*ppItemFactors[item][k];
	}

	for(int k = 0; k < spec_dim; k++)
	{
	   estimation += pppUserSpecificFactors[rel][user][k]*ppItemSpecificFactors[item][k];
	}	
	
	

	return estimation;
}

inline double MultiRelationalMatrixFactorization::estimateItem(int item1, int item2)
{		
	double estimation = 0;		
		
	for(int k = 0; k < dim; k++)
	{
	  estimation += ppItemFactors[item1][k]*ppItemFactors[item2][k];
	}

	return sig(estimation);
}

inline double MultiRelationalMatrixFactorization::estimate(ItemRating itemRating, unsigned int user)
{		
        double estimation = dot(itemRating,user);			
	return MIN_RATING + sig(estimation) * RANGE;

}

inline double MultiRelationalMatrixFactorization::update(ItemRating ratingData, unsigned int user)
{			
	unsigned int item = ratingData.item;
	double estimation = dot(ratingData, user);
	double sigm = sig(estimation);
	double estScore = MIN_RATING + sigm * RANGE;
	double err = ratingData.score-estScore;			      
        double gradient = err * sigm * (1 - sigm) * RANGE;
	

        updateLatentFactors(user,ratingData,gradient);
 	updateItemBias(ratingData,gradient);
	updateUserBias(user,gradient);
        updateSpecificLatentFactors(user,ratingData,gradient);

	srand ( time(NULL) );
	int count;

	//	cout << "album" << endl;
	if(pItems[item].album != NONE){
	    int item2 = pItems[item].album;
	    updateItem(ratingData, item2, 1);
	    count = 0;
	    do{
	      item2 = albumIds[randInt(0,albumIds.size())];
	    }while(item2 == pItems[item].album && count++ < 100);
	    updateItem(ratingData, item2, 0);	    
        }
	//	cout << "artist" << endl;
	if(pItems[item].artist != NONE){
	    int item2 = pItems[item].artist;
	    updateItem(ratingData, item2, 1);
	    count = 0;
	    do{
	      item2 = artistIds[randInt(0,artistIds.size())];
	    }while(item2 == pItems[item].artist && count++ < 100);
	    updateItem(ratingData, item2, 0);
        }
	//	cout << "genres" << endl;
        if(pItems[item].genre != NULL){
	    for(int i = 0; i < pItems[item].genre->size(); i++ ){
	         int item2 = pItems[item].genre->at(i);
	         updateItem(ratingData, item2, 1);
		 count = 0;
		 do{
		    item2 = genreIds[randInt(0,genreIds.size())];
		 }while(item2 == pItems[item].genre->at(i) && count++ < 100);
		 updateItem(ratingData, item2, 0);
	    }
        }  
        
	
	return err;
}

inline double MultiRelationalMatrixFactorization::updateItem(ItemRating ratingData, int item2, double score)
{
	int item = ratingData.item;
        double estimation = estimateItem(item, item2);
	double err = score-estimation;			      
	double gradient = err * estimation * (1 - estimation);
        updateItemLatentFactors(item,item2,gradient);
}

inline double MultiRelationalMatrixFactorization::getMu()
{		
         return globalBias;
}

inline double MultiRelationalMatrixFactorization::getItemBias(ItemRating ratingData)
{		
	assert(pItemsBase); 	
	assert(ratingData.item<TrainingMetaData.trainingTotalItems);		
	return pItemsBase[ratingData.item];	
}

inline void MultiRelationalMatrixFactorization::updateItemBias(ItemRating ratingData, double gradient)
{
	unsigned int item = ratingData.item;
	double oldBias=-1, newBias=-1; 
	assert(pItemsBase); assert(item<=TrainingMetaData.trainingTotalItems);		
	oldBias = pItemsBase[ratingData.item];
	pItemsBase[ratingData.item] += (double) step * (gradient - biasReg*oldBias);
}

inline double MultiRelationalMatrixFactorization::getUserBias(unsigned int user)
{
	assert(pUsersBase);	
	return pUsersBase[user];
}

inline void MultiRelationalMatrixFactorization::updateUserBias(unsigned int user, double gradient)
{
	assert(pUsersBase);
	double oldBias=0, newBias=0; 
	oldBias = pUsersBase[user];
	pUsersBase[user] += (double) step * (gradient - biasReg*oldBias);
}


inline double MultiRelationalMatrixFactorization::getItemLatentFactor(ItemRating ratingData, int k)
{		
	assert(ppItemFactors); 	
	assert(ratingData.item<TrainingMetaData.trainingTotalItems);		
	return ppItemFactors[ratingData.item][k];	
}

inline double MultiRelationalMatrixFactorization::getItemSpecificLatentFactor(ItemRating ratingData, int k, int relation)
{		
	assert(ppItemSpecificFactors); 	
	assert(ratingData.item<TrainingMetaData.trainingTotalItems);		
	return ppItemSpecificFactors[ratingData.item][k];	
}


inline double MultiRelationalMatrixFactorization::getUserLatentFactor(unsigned int user, int k)
{		
	assert(ppUserFactors); 	
  	return ppUserFactors[user][k];	
}

inline double MultiRelationalMatrixFactorization::getUserSpecificLatentFactor(unsigned int user, int k, int relation)
{		
	assert(pppUserSpecificFactors); 	
  	return pppUserSpecificFactors[relation][user][k];	
}


inline void MultiRelationalMatrixFactorization::updateLatentFactors(unsigned int user, ItemRating ratingData, double gradient)
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

inline void MultiRelationalMatrixFactorization::updateItemLatentFactors(int item1, int item2, double gradient)
{
	assert(ppItemFactors);	 assert(item1<=TrainingMetaData.trainingTotalItems);		
	assert(item2<=TrainingMetaData.trainingTotalItems);		

       	for(int k = 0; k < dim; k++)
	{
	  double u_k = ppItemFactors[item1][k];
	  double i_k = ppItemFactors[item2][k];
	  
	  ppItemFactors[item1][k] += (double) step*(relationReg*gradient*i_k - factorsReg*u_k);	  	  
	  ppItemFactors[item2][k] += (double) step*(relationReg*gradient*u_k - factorsReg*i_k);	  	  
	}
}


inline void MultiRelationalMatrixFactorization::updateSpecificLatentFactors(unsigned int user, ItemRating ratingData, double gradient)
{
	unsigned int item = ratingData.item;
	assert(pppUserSpecificFactors);	
	assert(ppItemSpecificFactors);	 assert(item<=TrainingMetaData.trainingTotalItems);	 

	int rel = pItems[item].type;

       	for(int k = 0; k < spec_dim; k++)
	{
	  double u_k = pppUserSpecificFactors[rel][user][k];
	  double i_k = ppItemSpecificFactors[item][k];
	  
	  pppUserSpecificFactors[rel][user][k] += (double) step*(gradient*i_k - factorsReg*u_k); 
	  ppItemSpecificFactors[item][k] += (double) step*(gradient*u_k - factorsReg*i_k); 
	}



}


#endif

