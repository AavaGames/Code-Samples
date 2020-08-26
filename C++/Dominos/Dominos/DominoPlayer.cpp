//
//  DominoPlayer.cpp
//  SDL_Dominos
//
//  Created by Philip Fertsman on 2020-01-23.
//  Copyright © 2020 Philip Fertsman. All rights reserved.
//

#include "DominoPlayer.h"

DominoPlayer::DominoPlayer(UnitSide side) : DominoUnit(side)
{
	
}

DominoPlayer::~DominoPlayer()
{
	
}

void DominoPlayer::Input()
{
	if (currentUnitSide == LeftSide)
	{
		if (mInputManager->KeyDown(SDL_SCANCODE_W) && prevDirection != Domino::Down)
		{
			currentDirection = Domino::Up;
		}
		else if (mInputManager->KeyDown(SDL_SCANCODE_S) && prevDirection != Domino::Up)
		{
			currentDirection = Domino::Down;
		}
		else if (mInputManager->KeyDown(SDL_SCANCODE_D) && prevDirection != Domino::Left)
		{
			currentDirection = Domino::Right;
		}
		else if (mInputManager->KeyDown(SDL_SCANCODE_A) && prevDirection != Domino::Right)
		{
			currentDirection = Domino::Left;
		}
		else
		{
			currentDirection = prevDirection;
		}
	}
	else
	{
		if (mInputManager->KeyDown(SDL_SCANCODE_O) && prevDirection != Domino::Down)
		{
			currentDirection = Domino::Up;
		}
		else if (mInputManager->KeyDown(SDL_SCANCODE_L) && prevDirection != Domino::Up)
		{
			currentDirection = Domino::Down;
		}
		else if (mInputManager->KeyDown(SDL_SCANCODE_SEMICOLON) && prevDirection != Domino::Left)
		{
			currentDirection = Domino::Right;
		}
		else if (mInputManager->KeyDown(SDL_SCANCODE_K) && prevDirection != Domino::Right)
		{
			currentDirection = Domino::Left;
		}
		else
		{
			currentDirection = prevDirection;
		}
	}
}
