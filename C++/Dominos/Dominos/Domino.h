//
//  Domino.h
//  SDL_Dominos
//
//  Created by Philip Fertsman on 2020-01-23.
//  Copyright © 2020 Philip Fertsman. All rights reserved.
//

#ifndef __DOMINO_H
#define __DOMINO_H
#include "Texture.h"
#include "AudioManager.h"

using namespace SDLFramework;

class Domino //: public Texture
{
public:
	enum DominoState { Standing, Fallen };
	DominoState currentDominoState;
	
	enum Direction { Up, Right, Down, Left };
	
	Domino(Vector2 pos, bool leftSide, Direction currentDirection, Direction prevDirection);
	~Domino();
	
	void Fall();
	void Render();
private:
	AudioManager * mAudio;
	
	Texture * mStandingTexture;
	Texture * mFallenTexture;
	
	Texture * mCurrentTexture;
};

#endif /* Domino_h */
