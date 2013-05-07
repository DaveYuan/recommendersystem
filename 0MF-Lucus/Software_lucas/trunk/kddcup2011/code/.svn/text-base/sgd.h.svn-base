
#ifndef _SGD_H_
#define _SGD_H_

#include "readUserData.h"
#include "utils.h"
#include "Factorization.h"
#include <assert.h>
#include <iostream>

using namespace std;

#define max(A,B) ((A)>=(B) ? (A) : (B))
#define min(A,B) ((A)<=(B) ? (A) : (B))


extern Factorization* FactorizationManager;

//A Simple implmentation of Stochastic Gradient Descent (SGD):
void gradientDescent(int nIterations, double & correspondingTrainingRmse, double & bestValidationRmse, int & iterCount);


//After SGD, this method iterates on the test dataset (for track-1), and predicts result
void predictTrack1TestRatings(char * filename);

void meanPrediction();

void biases();

void meanTimePrediction();

#endif

