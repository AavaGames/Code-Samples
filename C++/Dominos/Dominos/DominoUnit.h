//
//  DominoUnit.h
//  SDL_Dominos
//
//  Created by Philip Fertsman on 2020-01-23.
//  Copyright © 2020 Philip Fertsman. All rights reserved.
//

#ifndef __DOMINOUNIT_H
#define __DOMINOUNIT_H
#include "Domino.h"
#include "Texture.h"
#include <vector>
#include "InputManager.h"
#include "Stage.h"
#include "Timer.h"
#include <iostream>

class DominoUnit
{
public:
	enum UnitState { Waiting, Playing, Win, Lose };
	UnitState currentUnitState;
	
	enum UnitSide { LeftSide, RightSide };
	UnitSide currentUnitSide;
	Stage::TileType currentTileType;
	
	Domino::Direction currentDirection;
	Domino::Direction prevDirection;
	Vector2 mapPosition = Vec2_Zero;
	
	DominoUnit(UnitSide side);
	~DominoUnit();
	
	void Update();
	void Render();
	void RenderDominos();

	float turnTimer;
	float turnTime;
	const float minTurnTime = 0.2f;
	
	virtual void Input() {};
	bool Turn();
	void Movement();
	void PlaceDomino();
	void FallDominos();
	
	bool mDominosFallen;
protected:
	Timer * mTimer;
	InputManager * mInputManager;
	AudioManager * mAudio;
	Stage * mStage;
	Mix_Chunk * mMoveSFX;
	
	std::vector<Domino *> dominoList;
	float dominoFallDelayTimer;
	const float maxDominoFallDelay = 0.05f;
	int dominoIterator;
	
	Texture * mUpSprite;
	Texture * mDownSprite;
	Texture * mRightSprite;
	Texture * mLeftSprite;
	
	Texture * mCurrentSprite;
	
	void MoveToMapCoord(Vector2 mapPos);
	
	void CollisionCheck();
	void Lost();
};

#endif /* DominoUnit_h */
