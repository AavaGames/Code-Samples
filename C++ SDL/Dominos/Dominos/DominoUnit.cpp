//
//  DominoUnit.cpp
//  SDL_Dominos
//
//  Created by Aava Fertsman on 2020-01-23.
//  Copyright © 2020 Aava Fertsman. All rights reserved.
//

#include "DominoUnit.h"

DominoUnit::DominoUnit(UnitSide side)
{
	mAudio = AudioManager::Instance();
	mInputManager = InputManager::Instance();
	mTimer = Timer::Instance();
	mStage = Stage::Instance();
	mMoveSFX = mAudio->PlaySFX("SFX/Step.wav", 0, 1, true, true);
	
	currentUnitState = Playing;
	currentUnitSide = side;
	
	dominoIterator = 0;
	dominoFallDelayTimer = 0.0f;
	mDominosFallen = false;
	
	turnTime = 0.5f;
	
	mCurrentSprite = nullptr;
	
	if (currentUnitSide == LeftSide)
	{
		currentTileType = Stage::LEFTUNIT;
		turnTimer = turnTime / 2;
		
		mUpSprite = new Texture("Art/Dominos_SpriteSheet.png", 360, 488, 16, 16, true);
		mDownSprite = new Texture("Art/Dominos_SpriteSheet.png", 324, 488, 16, 16, true);
		mRightSprite = new Texture("Art/Dominos_SpriteSheet.png", 378, 488, 16, 16, true);
		mLeftSprite = new Texture("Art/Dominos_SpriteSheet.png", 342, 488, 16, 16, true);
		
		mCurrentSprite = mRightSprite;
		
		currentDirection = Domino::Right;
		prevDirection = Domino::Right;

		mapPosition = Vector2(8, 13);
	}
	else
	{
		currentTileType = Stage::RIGHTUNIT;
		turnTimer = 0.0f;
		
		mUpSprite = new Texture("Art/Dominos_SpriteSheet.png", 360, 506, 16, 16, true);
		mDownSprite = new Texture("Art/Dominos_SpriteSheet.png", 324, 506, 16, 16, true);
		mRightSprite = new Texture("Art/Dominos_SpriteSheet.png", 378, 506, 16, 16, true);
		mLeftSprite = new Texture("Art/Dominos_SpriteSheet.png", 342, 506, 16, 16, true);
		
		mCurrentSprite = mLeftSprite;
		
		currentDirection = Domino::Left;
		prevDirection = Domino::Left;
		
		mapPosition = Vector2(23, 12);
	}
	MoveToMapCoord(mapPosition);
	CollisionCheck();
}

DominoUnit::~DominoUnit()
{
	mCurrentSprite = nullptr;
	
	delete mUpSprite;
	mUpSprite = nullptr;
	
	delete mDownSprite;
	mDownSprite = nullptr;
	
	delete mRightSprite;
	mRightSprite = nullptr;
	
	delete mLeftSprite;
	mLeftSprite = nullptr;
	
	for (auto dom : dominoList)
	{
		delete dom;
		dom = nullptr;
	}
}

void DominoUnit::Update()
{
	if (currentUnitState == UnitState::Playing)
	{
		Input();
		if (Turn())
		{
			PlaceDomino();
			Movement();
		}
	}
	else if (currentUnitState == UnitState::Lose && !mDominosFallen)
	{
		FallDominos();
	}
}

void DominoUnit::Render()
{
	if (currentUnitState != Lose)
	{
		mCurrentSprite->Render();
	}
}

void DominoUnit::RenderDominos()
{
	for (auto dom : dominoList)
	{
		dom->Render();
	}
}

bool DominoUnit::Turn()
{
	turnTimer += mTimer->DeltaTime();
	if (turnTimer >= turnTime)
	{
		turnTimer = 0.0f;
		
		turnTime *= 0.98;
		if (turnTime <= minTurnTime)
		{
			turnTime = minTurnTime;
		}
		
		return true;
	}
	else return false;
}

void DominoUnit::Movement()
{
	if (currentDirection == Domino::Up)
	{
		mCurrentSprite = mUpSprite;
		mapPosition.y--;
	}
	else if (currentDirection == Domino::Down)
	{
		mCurrentSprite = mDownSprite;
		mapPosition.y++;
	}
	else if (currentDirection == Domino::Right)
	{
		mCurrentSprite = mRightSprite;
		mapPosition.x++;
	}
	else if (currentDirection == Domino::Left)
	{
		mCurrentSprite = mLeftSprite;
		mapPosition.x--;
	}
	
	mAudio->PlaySFX(mMoveSFX);
	prevDirection = currentDirection;
	MoveToMapCoord(mapPosition);
	CollisionCheck();
}

void DominoUnit::MoveToMapCoord(Vector2 mapPos)
{
	mCurrentSprite->Position(mStage->MapToWorld(mapPos));
}

void DominoUnit::CollisionCheck()
{
	if (mStage->stageMap[(int)mapPosition.y][(int)mapPosition.x] == 0)
	{
		mStage->SetTile(currentTileType, mapPosition);
	}
	else
	{
		Lost();
	}
}

void DominoUnit::PlaceDomino()
{
	Domino * domino;
	if (currentUnitSide == LeftSide)
	{
		domino = new Domino(mStage->MapToWorld(mapPosition), true, currentDirection, prevDirection);
	}
	else
	{
		domino = new Domino(mStage->MapToWorld(mapPosition), false, currentDirection, prevDirection);
	}
	
	dominoList.push_back(domino);
}

void DominoUnit::FallDominos()
{
	//loop through domino list from last to first set to fallen
	dominoFallDelayTimer += mTimer->DeltaTime();
	if (dominoFallDelayTimer >= maxDominoFallDelay)
	{
		dominoFallDelayTimer = 0.0f;
		
		dominoList[dominoIterator]->Fall();
		
		dominoIterator--;
		if (dominoIterator == -1)
		{
			mDominosFallen = true;
		}
	}
}

void DominoUnit::Lost()
{
	currentUnitState = Lose;
	dominoIterator = (int)dominoList.size() - 1;
}
