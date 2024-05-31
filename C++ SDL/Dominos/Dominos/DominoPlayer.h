//
//  DominoPlayer.h
//  SDL_Dominos
//
//  Created by Aava Fertsman on 2020-01-23.
//  Copyright © 2020 Aava Fertsman. All rights reserved.
//

#ifndef __DOMINOPLAYER_H
#define __DOMINOPLAYER_H
#include "DominoUnit.h"

class DominoPlayer : public DominoUnit
{
public:
	DominoPlayer(UnitSide side);
	~DominoPlayer();
	
	void Input() override;
private:
	
};

#endif /* DominoPlayer_h */
