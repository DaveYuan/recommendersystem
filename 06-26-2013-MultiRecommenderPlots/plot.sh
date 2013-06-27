#!/bin/sh 

MODEL="SocialMF"

if [ "$1" = 'SocialMF' ]; then
	./SocialMF.sh
elif [ "$1" = 'BprSocialJointMF' ]; then
	./BprSocialJointMF.sh
elif [ "$1" = 'MatrixFactorization' ]; then
	./MatrixFactorization.sh
elif [ "$1" = 'MatrixFactorizationBiasReg' ]; then
	./MatrixFactorizationBiasReg.sh
elif [ "$1" = 'TestOverlay' ]; then
	./TestOverlay.sh
fi
