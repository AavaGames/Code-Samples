//
//  DominoEnemy.cpp
//  SDL_Dominos
//
//  Created by Philip Fertsman on 2020-01-23.
//  Copyright © 2020 Philip Fertsman. All rights reserved.
//

#include "DominoEnemy.h"

DominoEnemy::DominoEnemy(UnitSide side) : DominoUnit(side)
{
	seedOffset = 0;
	previousDirections.push_back(prevDirection);
}

DominoEnemy::~DominoEnemy()
{
	
}

void DominoEnemy::Update()
{
	if (currentUnitState == UnitState::Playing)
	{
		if (Turn())
		{
			Input();
			PlaceDomino();
			Movement();
		}
	}
	else if (currentUnitState == UnitState::Lose && !mDominosFallen)
	{
		FallDominos();
	}
}

void DominoEnemy::Input()
{
	currentDirection = AI();
}

Domino::Direction DominoEnemy::AI()
{	
	Domino::Direction possibleDirections[3] {};
	bool directionCollision[3] {};
	
	if (prevDirection == Domino::Direction::Up)
	{
		possibleDirections[0] = prevDirection;
		possibleDirections[1] = Domino::Direction::Right;
		possibleDirections[2] = Domino::Direction::Left;
		
		bool forwardCollision = GhostCollisionCheck(possibleDirections[0]);
		bool rightCollision = GhostCollisionCheck(possibleDirections[1]);
		bool leftCollision = GhostCollisionCheck(possibleDirections[2]);
		
		directionCollision[0] = forwardCollision;
		directionCollision[1] = rightCollision;
		directionCollision[2] = leftCollision;
	}
	else if (prevDirection == Domino::Direction::Down)
	{
		possibleDirections[0] = prevDirection;
		possibleDirections[1] = Domino::Direction::Left;
		possibleDirections[2] = Domino::Direction::Right;
		
		bool forwardCollision = GhostCollisionCheck(possibleDirections[0]);
		bool rightCollision = GhostCollisionCheck(possibleDirections[1]);
		bool leftCollision = GhostCollisionCheck(possibleDirections[2]);
		
		directionCollision[0] = forwardCollision;
		directionCollision[1] = rightCollision;
		directionCollision[2] = leftCollision;
	}
	else if (prevDirection == Domino::Direction::Right)
	{
		possibleDirections[0] = prevDirection;
		possibleDirections[1] = Domino::Direction::Down;
		possibleDirections[2] = Domino::Direction::Up;
		
		bool forwardCollision = GhostCollisionCheck(possibleDirections[0]);
		bool rightCollision = GhostCollisionCheck(possibleDirections[1]);
		bool leftCollision = GhostCollisionCheck(possibleDirections[2]);
		
		directionCollision[0] = forwardCollision;
		directionCollision[1] = rightCollision;
		directionCollision[2] = leftCollision;
	}
	else if (prevDirection == Domino::Direction::Left)
	{
		possibleDirections[0] = prevDirection;
		possibleDirections[1] = Domino::Direction::Up;
		possibleDirections[2] = Domino::Direction::Down;
		
		bool forwardCollision = GhostCollisionCheck(possibleDirections[0]);
		bool rightCollision = GhostCollisionCheck(possibleDirections[1]);
		bool leftCollision = GhostCollisionCheck(possibleDirections[2]);
		
		directionCollision[0] = forwardCollision;
		directionCollision[1] = rightCollision;
		directionCollision[2] = leftCollision;
	}
	
	int directionIter = 0;
	int timesLooped = 0;
	
	//Rules
	while (timesLooped != maxLoops)
	{
		//Random Turn
		if (TurnTime())
		{
			directionIter = RandomDirection();
		}

		//If Colliding
		if (directionCollision[directionIter])
		{
			directionIter = RandomDirection();
			
			timesLooped++;
			continue;
		}
		
		//Never make the same turn 3 times
		if ((directionIter != 0) && (directionIter == previousDirections.back()) &&
			(directionIter == previousDirections.back() - 1) && (directionIter == previousDirections.back() - 2))
		{
			timesLooped++;
			continue;
		}
		
		break;
	}
	
	previousDirections.push_back(directionIter);
	
	return possibleDirections[directionIter];
}

int DominoEnemy::RandomDirection()
{
	int directionIter;
	
	//Between 0 - 2
	directionIter = rand() % 2 + 1;
	
	return directionIter;
}

bool DominoEnemy::TurnTime()
{
	//Random between 1 - 100 less then turnRate
	if (rand() % 100 + 1 < turnRate)
	{
		return true;
	}
	else return false;
}

bool DominoEnemy::GhostCollisionCheck(Domino::Direction direction)
{
	Vector2 position = DominoUnit::mapPosition;
	
	if (direction == Domino::Up)
	{
		position.y--;
	}
	else if (direction == Domino::Down)
	{
		position.y++;
	}
	else if (direction == Domino::Right)
	{
		position.x++;
	}
	else if (direction == Domino::Left)
	{
		position.x--;
	}
	
	if (DominoUnit::mStage->stageMap[(int)position.y][(int)position.x] == 0)
	{
		return false;
	}
	else return true;
}
