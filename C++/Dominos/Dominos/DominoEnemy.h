//
//  DominoEnemy.h
//  SDL_Dominos
//
//  Created by Philip Fertsman on 2020-01-23.
//  Copyright © 2020 Philip Fertsman. All rights reserved.
//

#ifndef __DOMINOENEMY_H
#define __DOMINOENEMY_H
#include "DominoUnit.h"
#include <stdlib.h>

class DominoEnemy : public DominoUnit
{
public:
	DominoEnemy(UnitSide side);
	~DominoEnemy();

	Domino::Direction AI();
	void Input() override;
	void Update();
private:
	std::vector<int> previousDirections;
	const int turnRate = 10;
	const int maxLoops = 30;
	
	int RandomDirection();
	bool TurnTime();
	bool GhostCollisionCheck(Domino::Direction direction);
	
	unsigned int seedOffset;
	void SeedRandom();
};

#endif /* DominoEnemy_h */
