// Copyright 2011 Yahoo!. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
// 
//    1. Redistributions of source code must retain the above copyright notice, this list of
//       conditions and the following disclaimer.
// 
//    2. Redistributions in binary form must reproduce the above copyright notice, this list
//       of conditions and the following disclaimer in the documentation and/or other materials
//       provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY Yahoo! ``AS IS'' AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Yahoo! OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// The views and conclusions contained in the software and documentation are those of the
// authors and should not be interpreted as representing official policies, either expressedyy


#include "globals.h"
#include "ratingStructures.h"
#include "Factorization.h"
#include <assert.h>
#include <iostream>

using namespace std;

int numberPasses = 0;

ItemRating *pItemRatings_training				= 0;
ItemRating *pItemRatings_validation				= 0;
ItemRating *pItemRatings_test					= 0;
UserData	*pUsersData				        = 0;

Item *pItems                                                    = 0;

//Gradient Descent:
int iterations					= 20;
double step					= 0.5;
double decay                                    = 1;


//Other Hyper Parameters
double factorsReg				= 1.5;
double biasReg					= 0.000005;
double relationReg				= 0.005;
int timeBins 				        = 100;
int dim  					= 20;
int spec_dim                                    = 10;



//Meta Data Structures:
MetaData TrainingMetaData   = {0};
MetaData ValidationMetaData = {0};
MetaData TestMetaData = {0};

//Factorization Model:
Factorization *FactorizationManager = 0;


vector<int> albumIds;
vector<int> artistIds;
vector<int> genreIds;



