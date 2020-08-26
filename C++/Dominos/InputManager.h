//
//  InputManager.h
//  SDL_Tutorial
//
//  Created by Philip Fertsman on 2020-01-15.
//  Copyright © 2020 Philip Fertsman. All rights reserved.
//

#ifndef __INPUTMANAGER_H
#define __INPUTMANAGER_H
#include <SDL2/SDL.h>
#include "MathHelper.h"
#include <string>

namespace SDLFramework {

class InputManager
{
public:
	static InputManager * Instance();
	static void Release();
	
	bool KeyDown(SDL_Scancode scancode);
	bool KeyPressed(SDL_Scancode scancode);
	bool KeyReleased(SDL_Scancode scancode);
	
	enum MouseButton { Left = 0, Right, Middle, Back, Forward };
	
	bool MouseButtonDown(MouseButton button);
	bool MouseButtonPressed(MouseButton button);
	bool MouseButtonReleased(MouseButton button);
	Vector2 MousePosition();
	
	void Update();
	void UpdatePrevInput();
	
private:
	static InputManager * sInstance;
	const Uint8 * mKeyboardState;
	Uint8 * mPrevKeyboardState;
	int mKeyLength;
	
	Uint32 mMouseState;
	Uint32 mPrevMouseState;
	int mMouseXPos;
	int mMouseYPos;
	
	InputManager();
	~InputManager();
};

}

#endif /* InputManager_h */
