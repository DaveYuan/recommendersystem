
#ifndef _FACTORIZATION_H_
#define _FACTORIZATION_H_

#include "utils.h"
#include <iostream>

using namespace std;

#define max(A,B) ((A)>=(B) ? (A) : (B))
#define min(A,B) ((A)<=(B) ? (A) : (B))


/*
 *
 * The Factorization Manager class handles the items and users biases, 
 * as well as the latent factors.
 *
*/
class Factorization
{
public:	

  Factorization(){};
  ~Factorization(){};

	virtual void allocate(){return;} // Allocate resources needed

	virtual inline double getMu(){return 0;}

	
	
	//This method takes a rating instance and a user ID, and predicts the rating score 
	virtual inline double estimate(ItemRating itemRating,unsigned int user){return 0;}
	
	//Called by SGD to update biases after each rating line:
	virtual inline double	update(ItemRating ratingData, unsigned int user){return 0;}
	
private:
	double  globalBias;

};




#endif


