//
//  Domino.cpp
//  SDL_Dominos
//
//  Created by Philip Fertsman on 2020-01-23.
//  Copyright © 2020 Philip Fertsman. All rights reserved.
//

#include "Domino.h"

Domino::Domino(Vector2 pos, bool leftSide, Direction currentDirection, Direction prevDirection)
{
	mAudio = AudioManager::Instance();
	currentDominoState = Standing;
	
	if (leftSide)
	{
		if (currentDirection == Up && prevDirection == Up)
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 144, 488, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 162, 488, 16, 16, true);
		}
		else if (currentDirection == Down && prevDirection == Down)
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 0, 488, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 18, 488, 16, 16, true);
		}
		else if (currentDirection == Right && prevDirection == Right)
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 216, 488, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 234, 488, 16, 16, true);
		}
		else if (currentDirection == Left && prevDirection == Left)
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 72, 488, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 90, 488, 16, 16, true);
		}
		else if ((currentDirection == Up && prevDirection == Left) || (currentDirection == Right && prevDirection == Down))
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 108, 488, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 126, 488, 16, 16, true);
		}
		else if ((currentDirection == Up && prevDirection == Right) || (currentDirection == Left && prevDirection == Down))
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 36, 488, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 54, 488, 16, 16, true);
		}
		else if ((currentDirection == Down && prevDirection == Left) || (currentDirection == Right && prevDirection == Up))
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 180, 488, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 198, 488, 16, 16, true);
		}
		else if ((currentDirection == Down && prevDirection == Right) || (currentDirection == Left && prevDirection == Up))
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 252, 488, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 270, 488, 16, 16, true);
		}
	}
	else
	{
		if (currentDirection == Up && prevDirection == Up)
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 144, 506, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 162, 506, 16, 16, true);
		}
		else if (currentDirection == Down && prevDirection == Down)
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 0, 506, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 18, 506, 16, 16, true);
		}
		else if (currentDirection == Right && prevDirection == Right)
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 216, 506, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 234, 506, 16, 16, true);
		}
		else if (currentDirection == Left && prevDirection == Left)
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 72, 506, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 90, 506, 16, 16, true);
		}
		else if ((currentDirection == Up && prevDirection == Left) || (currentDirection == Right && prevDirection == Down))
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 108, 506, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 126, 506, 16, 16, true);
		}
		else if ((currentDirection == Up && prevDirection == Right) || (currentDirection == Left && prevDirection == Down))
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 36, 506, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 54, 506, 16, 16, true);
		}
		else if ((currentDirection == Down && prevDirection == Left) || (currentDirection == Right && prevDirection == Up))
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 180, 506, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 198, 506, 16, 16, true);
		}
		else if ((currentDirection == Down && prevDirection == Right) || (currentDirection == Left && prevDirection == Up))
		{
			mStandingTexture = new Texture("Art/Dominos_SpriteSheet.png", 252, 506, 16, 16, true);
			mFallenTexture = new Texture("Art/Dominos_SpriteSheet.png", 270, 506, 16, 16, true);
		}
	}
	
	mStandingTexture->Position(pos);
	mFallenTexture->Position(pos);
	mCurrentTexture = mStandingTexture;
}

Domino::~Domino()
{
	mCurrentTexture = nullptr;
	
	delete mStandingTexture;
	mStandingTexture = nullptr;
	
	delete mFallenTexture;
	mFallenTexture = nullptr;
}

void Domino::Render()
{
	mCurrentTexture->Render();
}

void Domino::Fall()
{
	mAudio->PlaySFX("SFX/Fall.wav", 0, 2);
	mCurrentTexture = mFallenTexture;
}
