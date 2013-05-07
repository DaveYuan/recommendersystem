#ifndef _UTILS_H_
#define _UTILS_H_


#include <cstdlib>
#include <math.h>

inline float randDouble(float low, float high)
{
  float temp;

  /* swap low & high around if needed */
  if (low > high)
  {
    temp = low;
    low = high;
    high = temp;
  }

  /* calculate the random number & return it */
  temp = (rand() / (static_cast<float>(RAND_MAX) + 1.0))
    * (high - low) + low;
  return temp;
}

inline int randInt(int low, int high)
{
  int temp;

  /* swap low & high around if needed */
  if (low > high)
  {
    temp = low;
    low = high;
    high = temp;
  }

  int range=(high-low)+1; 

  /* calculate the random number & return it */
  temp = low+int(range*rand()/(RAND_MAX + 1.0)); 
  return temp;
}


inline float sig(float f){
  return 1.0 / (1.0 + exp(-f));
}

#endif

