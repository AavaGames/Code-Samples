//
//  GameManager.cpp
//  SDL_Tutorial
//
//  Created by Philip Fertsman on 2020-01-14.
//  Copyright © 2020 Philip Fertsman. All rights reserved.
//
#include "GameManager.h"

namespace SDLFramework {

GameManager * GameManager::sInstance = nullptr;

GameManager * GameManager::Instance()
{
	if (sInstance == nullptr)
	{
		sInstance = new GameManager();
	}
	return sInstance;
}

void GameManager::Release()
{
	delete sInstance;
	sInstance = nullptr;
}

GameManager::GameManager() : mQuit(false)
{
	mGraphics = Graphics::Instance();
	//If failed to create correctly shut down
	if (!Graphics::Initialized())
	{
		mQuit = true;
	}
	
	mAssetManager = AssetManager::Instance();
	mTimer = Timer::Instance();
	mInputManager = InputManager::Instance();
	mAudioManager = AudioManager::Instance();
	mStage = Stage::Instance();
	
	gameOverTimer = 0.0f;
	
	dominoPlayerOne = nullptr;
	dominoPlayerTwo = nullptr;
	dominoEnemyOne = nullptr;
	dominoEnemyTwo = nullptr;
	
	BuildStartBoard();
	currentScreenType = Start;
	onePlayer = true;
	
	mBackgroundTexture = new Texture("Art/Dominos_SpriteSheet.png", 0, 0, 512, 448, true);
	mBackgroundTexture->Position(Vector2(Graphics::SCREEN_WIDTH * 0.5f, Graphics::SCREEN_HEIGHT*0.5f));
	
	int size = 16;
	mGameOverFont = new Texture("GAME OVER", "Fonts/emulogic.TTF", size, { 255, 255, 255 }, false);
	mGameOverFont->Position(Vector2(Graphics::SCREEN_WIDTH * 0.5f, size/2));
	
	mStartScreenPerCoinFont = new Texture("1 PLAY PER COIN", "Fonts/emulogic.TTF", size, { 255, 255, 255 }, false);
	mStartScreenPerCoinFont->Position(Graphics::SCREEN_WIDTH * 0.5f, Graphics::SCREEN_HEIGHT * 0.18);
	
	mStartScreenWinGameFont = new Texture("4 POINTS WINS GAME", "Fonts/emulogic.TTF", size, { 255, 255, 255 }, false);
	mStartScreenWinGameFont->Position(Graphics::SCREEN_WIDTH * 0.5f, Graphics::SCREEN_HEIGHT * 0.90);
	
	mLeftPoint0 = new Texture("Art/Dominos_SpriteSheet.png", 0, 524, 16, 16, false);
	mLeftPoint0->Position(128+8, 16+8);
	mLeftPoint1 = new Texture("Art/Dominos_SpriteSheet.png", 18, 524, 16, 16, false);
	mLeftPoint1->Position(128+8, 16+8);
	mLeftPoint2 = new Texture("Art/Dominos_SpriteSheet.png", 36, 524, 16, 16, false);
	mLeftPoint2->Position(128+8, 16+8);
	mLeftPoint3 = new Texture("Art/Dominos_SpriteSheet.png", 54, 524, 16, 16, false);
	mLeftPoint3->Position(128+8, 16+8);
	mLeftPoint4 = new Texture("Art/Dominos_SpriteSheet.png", 72, 524, 16, 16, false);
	mLeftPoint4->Position(128+8, 16+8);
	mCurrentLeftPoint = mLeftPoint0;
	
	mRightPoint0 = new Texture("Art/Dominos_SpriteSheet.png", 0, 542, 16, 16, false);
	mRightPoint0->Position(384+8, 16+8);
	mRightPoint1 = new Texture("Art/Dominos_SpriteSheet.png", 18, 542, 16, 16, false);
	mRightPoint1->Position(384+8, 16+8);
	mRightPoint2 = new Texture("Art/Dominos_SpriteSheet.png", 36, 542, 16, 16, false);
	mRightPoint2->Position(384+8, 16+8);
	mRightPoint3 = new Texture("Art/Dominos_SpriteSheet.png", 54, 542, 16, 16, false);
	mRightPoint3->Position(384+8, 16+8);
	mRightPoint4 = new Texture("Art/Dominos_SpriteSheet.png", 72, 542, 16, 16, false);
	mRightPoint4->Position(384+8, 16+8);
	mCurrentRightPoint = mRightPoint0;
}

GameManager::~GameManager()
{
	delete mBackgroundTexture;
	mBackgroundTexture = nullptr;
	
	delete mGameOverFont;
	mGameOverFont = nullptr;
	
	delete mStartScreenPerCoinFont;
	mStartScreenPerCoinFont = nullptr;
	
	delete mStartScreenWinGameFont;
	mStartScreenWinGameFont = nullptr;
	
	ClearBoard();
	
	Stage::Release();
	mStage = nullptr;
	
	Timer::Release();
	mTimer = nullptr;
	
	AssetManager::Release();
	mAssetManager = nullptr;
	
	InputManager::Release();
	mInputManager = nullptr;
	
	AudioManager::Release();
	mAudioManager = nullptr;
	
	Graphics::Release();
	mGraphics = nullptr;
	
	SDL_Quit();
}

void GameManager::Run()
{
	while (!mQuit)
	{
		mTimer->Update();
		
		while (SDL_PollEvent(&mEvent)) {
			switch (mEvent.type)
			{
				case SDL_QUIT:
					mQuit = true;
					break;
			}
		}
		
		if (mTimer->DeltaTime() >= 1.0f / FRAME_RATE)
		{
			Update();
			LateUpdate();
			Render();
			mTimer->Reset();
		}
	}
}

void GameManager::Update()
{
	mInputManager->Update();
	
	if (mInputManager->KeyPressed(SDL_SCANCODE_EQUALS))
	{
		mGraphics->IncreaseScale();
	}
	else if (mInputManager->KeyPressed(SDL_SCANCODE_MINUS))
	{
		mGraphics->DecreaseScale();
	}
	
	if (currentScreenType == Start)
	{
		if (dominoEnemyOne->currentUnitState == DominoUnit::UnitState::Lose)
		{
			dominoEnemyTwo->currentUnitState = DominoUnit::UnitState::Win;
			
			if (dominoEnemyOne->mDominosFallen)
			{
				gameOverTimer += mTimer->DeltaTime();
				if (gameOverTimer >= maxGameOverTimer)
				{
					gameOverTimer = 0.0f;
					BuildStartBoard();
				}
			}
		}
		else if (dominoEnemyTwo->currentUnitState == DominoUnit::UnitState::Lose)
		{
			dominoEnemyOne->currentUnitState = DominoUnit::UnitState::Win;
			
			if (dominoEnemyTwo->mDominosFallen)
			{
				gameOverTimer += mTimer->DeltaTime();
				if (gameOverTimer >= maxGameOverTimer)
				{
					gameOverTimer = 0.0f;
					BuildStartBoard();
				}
			}
		}
		
		dominoEnemyOne->Update();
		dominoEnemyTwo->Update();
		
		if (mInputManager->KeyPressed(SDL_SCANCODE_1))
		{
			mAudioManager->PlaySFX("SFX/StartGame.wav", 0, -1);
			gameOverMusic = true;
			leftPoints = 0;
			rightPoints = 0;
			
			currentScreenType = Playing;
			BuildBoard(true);
		}
		else if (mInputManager->KeyPressed(SDL_SCANCODE_2))
		{
			mAudioManager->PlaySFX("SFX/StartGame.wav", 0, -1);
			gameOverMusic = true;
			leftPoints = 0;
			rightPoints = 0;
			
			currentScreenType = Playing;
			BuildBoard(false);
		}
	}
	else if (currentScreenType == Playing)
	{
		if (onePlayer)
		{
			if (dominoPlayerOne->currentUnitState == DominoUnit::UnitState::Lose)
			{
				dominoEnemyOne->currentUnitState = DominoUnit::UnitState::Win;
				
				if (dominoPlayerOne->mDominosFallen)
				{
					AddPoint(false);
					currentScreenType = GameOver;
				}
			}
			else if (dominoEnemyOne->currentUnitState == DominoUnit::UnitState::Lose)
			{
				dominoPlayerOne->currentUnitState = DominoUnit::UnitState::Win;
				
				if (dominoEnemyOne->mDominosFallen)
				{
					AddPoint(true);
					currentScreenType = GameOver;
				}
			}
			dominoPlayerOne->Update();
			dominoEnemyOne->Update();
		}
		else
		{
			if (dominoPlayerOne->currentUnitState == DominoUnit::UnitState::Lose)
			{
				dominoPlayerTwo->currentUnitState = DominoUnit::UnitState::Win;
				
				if (dominoPlayerOne->mDominosFallen)
				{
					AddPoint(false);
					currentScreenType = GameOver;
				}
			}
			else if (dominoPlayerTwo->currentUnitState == DominoUnit::UnitState::Lose)
			{
				dominoPlayerOne->currentUnitState = DominoUnit::UnitState::Win;
				
				if (dominoPlayerTwo->mDominosFallen)
				{
					AddPoint(true);
					currentScreenType = GameOver;
				}
			}
			dominoPlayerOne->Update();
			dominoPlayerTwo->Update();
		}
		if (mInputManager->KeyPressed(SDL_SCANCODE_ESCAPE))
		{
			currentScreenType = Start;
			BuildStartBoard();
		}
	}
	else if (currentScreenType == GameOver)
	{
		if ((leftPoints == 4 || rightPoints == 4) && gameOverMusic)
		{
			mAudioManager->PlayMusic("SFX/EndMatch.wav", 0);
			gameOverMusic = false;
		}
		gameOverTimer += mTimer->DeltaTime();
		if (gameOverTimer >= maxGameOverTimer)
		{
			gameOverTimer = 0.0f;
			if (leftPoints == 4 || rightPoints == 4)
			{
				BuildStartBoard();
				currentScreenType = Start;
			}
			else
			{
				BuildBoard(onePlayer);
				currentScreenType = Playing;
			}
		}
	}
}

void GameManager::LateUpdate()
{
	mInputManager->UpdatePrevInput();
}

void GameManager::Render()
{
	mGraphics->ClearBackBuffer();
	
	mBackgroundTexture->Render();
	mCurrentLeftPoint->Render();
	mCurrentRightPoint->Render();

	if (currentScreenType == Start)
	{
		mGameOverFont->Render();
		mStartScreenPerCoinFont->Render();
		mStartScreenWinGameFont->Render();
		
		dominoEnemyOne->RenderDominos();
		dominoEnemyTwo->RenderDominos();
		dominoEnemyOne->Render();
		dominoEnemyTwo->Render();
	}
	else if (currentScreenType == Playing)
	{
		if (onePlayer)
		{
			dominoPlayerOne->RenderDominos();
			dominoEnemyOne->RenderDominos();
			dominoPlayerOne->Render();
			dominoEnemyOne->Render();
		}
		else
		{
			dominoPlayerOne->RenderDominos();
			dominoPlayerTwo->RenderDominos();
			dominoPlayerOne->Render();
			dominoPlayerTwo->Render();
		}
	}
	else if (currentScreenType == GameOver)
	{
		mGameOverFont->Render();
		
		if (onePlayer)
		{
			dominoPlayerOne->RenderDominos();
			dominoEnemyOne->RenderDominos();
			dominoPlayerOne->Render();
			dominoEnemyOne->Render();
		}
		else
		{
			dominoPlayerOne->RenderDominos();
			dominoPlayerTwo->RenderDominos();
			dominoPlayerOne->Render();
			dominoPlayerTwo->Render();
		}
	}

	mGraphics->Render();
}

void GameManager::BuildBoard(bool isOnePlayer)
{
	ClearBoard();
	
	onePlayer = isOnePlayer;
	if (onePlayer)
	{
		dominoPlayerOne = new DominoPlayer(DominoUnit::LeftSide);
		dominoEnemyOne = new DominoEnemy(DominoUnit::RightSide);
	}
	else
	{
		dominoPlayerOne = new DominoPlayer(DominoUnit::LeftSide);
		dominoPlayerTwo = new DominoPlayer(DominoUnit::RightSide);
	}
}

void GameManager::BuildStartBoard()
{
	ClearBoard();
	
	mCurrentLeftPoint = mLeftPoint0;
	mCurrentRightPoint = mRightPoint0;
	
	dominoEnemyOne = new DominoEnemy(DominoUnit::LeftSide);
	dominoEnemyTwo = new DominoEnemy(DominoUnit::RightSide);
}

void GameManager::ClearBoard()
{
	mBackgroundTexture = new Texture("Art/Dominos_SpriteSheet.png", 0, 0, 512, 448, true);
	mBackgroundTexture->Position(Vector2(Graphics::SCREEN_WIDTH * 0.5f, Graphics::SCREEN_HEIGHT*0.5f));
	
	delete dominoPlayerOne;
	dominoPlayerOne = nullptr;
	
	delete dominoPlayerTwo;
	dominoPlayerTwo = nullptr;
	
	delete dominoEnemyOne;
	dominoEnemyOne = nullptr;
	
	delete dominoEnemyTwo;
	dominoEnemyTwo = nullptr;
	
	mStage->InitializeStage();
}

void GameManager::AddPoint(bool leftSide)
{
	if (leftSide)
	{
		leftPoints++;
		if (leftPoints == 1)
		{
			mCurrentLeftPoint = mLeftPoint1;
		}
		else if (leftPoints == 2)
		{
			mCurrentLeftPoint = mLeftPoint2;
		}
		else if (leftPoints == 3)
		{
			mCurrentLeftPoint = mLeftPoint3;
		}
		else if (leftPoints == 4)
		{
			mCurrentLeftPoint = mLeftPoint4;
		}
	}
	else
	{
		rightPoints++;
		if (rightPoints == 1)
		{
			mCurrentRightPoint = mRightPoint1;
		}
		else if (rightPoints == 2)
		{
			mCurrentRightPoint = mRightPoint2;
		}
		else if (rightPoints == 3)
		{
			mCurrentRightPoint = mRightPoint3;
		}
		else if (rightPoints == 4)
		{
			mCurrentRightPoint = mRightPoint4;
		}
	}
}

}
