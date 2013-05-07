#include <math.h>
#include <stdio.h>
#include <time.h>
#include <ctime>
#include <opt.h>
#include "readUserData.h"
#include "globals.h"
#include "ratingStructures.h"
#include "sgd.h"
#include "BiasedMF.h"
#include "VariousBiasMF.h"
#include "MRMF.h"
#include "PairBiasMF.h"
#include "TimePITF.h"

char* outFile                                   = TRACK_1_RESULTS_FILE;
short int readAttributes                        = false;
int method                                      = 1;


double newStep = 0.0001;
double newReg = 1.5;


//TODO give the option to choose those files here
//char* statsFile                                 = TRACK1_STATS_FILE;
//char* trainFile                                 = TRACK1_TRAINING_FILE;
//char* validationFile                            = TRACK1_VALIDATION_FILE;
//char* testFile                                  = TRACK1_TEST_FILE;

extern Factorization* FactorizationManager;

int method_main(int argc, char *argv[]) 
{			

        double trainingRmse=0, validationRmse=0;
	int iterCount=-1, i=0;	
	time_t Start_t, End_t;
	unsigned int tmp = 0;

	cout<<"Testing if a binary dataset already exists..."<<endl;

	if(!readAllTrack1BinData())
	{
		cout<<"Couldn't find the binary dataset files => Creating a binary dataset from text files..."<<endl;			
		
		cout<<"-----------------  LOADING STATISTICS -----------------"<<endl;		
		readStats(TRACK1_STATS_FILE);


       		cout<<"-----------------  LOADING TRAINING DATA -----------------"<<endl;
		readTrack1DBFromTextFiles(TRACK1_TRAINING_FILE,TRAINING);	
		
				
		cout<<endl<<"-----------------  LOADING VALIDATION DATA -----------------"<<endl;
		readTrack1DBFromTextFiles(TRACK1_VALIDATION_FILE,VALIDATION);	


		cout<<endl<<"-----------------  LOADING TEST DATA -----------------"<<endl;
		readTrack1DBFromTextFiles(TRACK1_TEST_FILE,TEST);	


		
		cout<<"-----------------  LOADING ATTRIBUTES -----------------"<<endl;	  
		readItemAttributesFromTextFiles();
		
		//biases();
		writeTrack1DBIntoBinFile(TRAINING);
		writeTrack1DBIntoBinFile(VALIDATION);
		writeTrack1DBIntoBinFile(TEST);

	}
	else
	{
		cout<<"Loaded binary dataset files correctly!"<<endl;
	}

	if(readAttributes)
	{
     	        cout<<"-----------------  LOADING ATTRIBUTES -----------------"<<endl;	  
                readItemAttributesFromTextFiles();
	}

	
        switch(method){
	case BMF: 	
	  cout << "*** Method: Biased Matrix Factorization" << endl;
	  FactorizationManager = new BiasedMatrixFactorization;
	  break;
	case EXTENDED_BMF:
	  cout << "*** Method: Extended Biased Matrix Factorization" << endl;
	  FactorizationManager = new MultiBiasMatrixFactorization;
	  break;
	case MRMF:
	  cout << "*** Method: Multi Relational Matrix Factorization" << endl;
	  FactorizationManager = new MultiRelationalMatrixFactorization;
	  break;
	case PAIR_BMF:
	  cout << "*** Method: Biased Matrix Factorization with Pairwise Biases" << endl;
	  FactorizationManager = new PairwiseBiasMatrixFactorization;
	  break;
	case PITF:
	  cout << "*** Method: Time Based Pairwise Interaction Tensor Factorization" << endl;
	  FactorizationManager = new TimeBasedPITF;
	  break;
	case 10:
	  cout << "*** Method: Gambi" << endl;
	  //meanPrediction();
	  return 0;
	  break;
	case 11:
	  cout << "*** Method: TimeBins Averages" << endl;
	  //meanTimePrediction();
	  freeAll();
	  return 0;
	  break;
	default:
	  cout << "Couldn't find method!!" << endl;
          cout << "Using Biased Matrix Factorization" << endl;
	  FactorizationManager = new BiasedMatrixFactorization;
	  break;
	}
        


	cout<<endl<<"---------"<<endl;
	Start_t = time(NULL);
	gradientDescent(iterations,trainingRmse,validationRmse,iterCount);
	End_t = time(NULL);		
	cout<<endl<<"---------"<<endl;
			
	cout<<"Training RMSE: "<<trainingRmse<<"\tValidation RMSE: "<<validationRmse<<", secs:"<<(difftime(End_t, Start_t))<<endl;					
	
	cout<<endl<<"---------"<<endl;
	predictTrack1TestRatings(outFile);

	freeAll();
	delete FactorizationManager;
				
	return 0;
}


int main(int argc, char** argv)
{
  OptRegister(&iterations,'i' , "iter", "The number of iterations");
  OptRegister(&step, 'l', "learn", "The Learn Rate");
  OptRegister(&factorsReg, 'r', "reg", "The latent factors regularization constant");
  OptRegister(&biasReg, 'b', "bias_reg", "The bias regularization constant");
  OptRegister(&dim, 'f', "features", "the number of latent features");
  OptRegister(&outFile, 'o', "predictions", "The file containing the predictions");
  OptRegister(&decay, 'd', "decay", "The learn rate decay after each iteration");
  OptRegister(&readAttributes, 'a', "attr", "whether to read item attributes or not");
  OptRegister(&method, 'm', "method", "The method to be used.\n\t1 - Biased MF\n\t2 - Biased MF with Additional Biases\n\t3 - MultiRelationalMF\n\t4 - Biased Factorization\n\t5 - Time Based PITF");
  OptRegister(&spec_dim, 's', "specDim", "The number of specific latent factors (only for MultiRelationalMF)");
  OptRegister(&newStep, 'k', "step2", "The new step for the second pass");
  OptRegister(&relationReg, 'h', "rel_reg", "The weight for the relations with metadata");
  OptRegister(&timeBins, 't', "time", "The number of time bins to be used (Method 5)");

  optVersion("1.0");

  optMain(method_main);
  opt(&argc, &argv);

  return method_main(argc, argv);
}
