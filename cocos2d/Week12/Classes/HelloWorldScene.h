#pragma once
#include "cocos2d.h"
#include "sqlite3.h"
using namespace cocos2d;

class HelloWorld : public cocos2d::Scene
{
public:
	static cocos2d::Scene* createScene();

	virtual bool init();

	void move(char direction);

	void changeHP(char cmd);

	void wait();

	void signal();

	void doAttact();

	void doDead();

	void updateTime(float dt);
	void generateMonster(float dt);
	void hitByMonster(float dt);
	void update(float dt) override;
	bool canAttack();

	virtual void onKeyPressed(EventKeyboard::KeyCode keycode, Event *event);
	// implement the "static create()" method manually
	CREATE_FUNC(HelloWorld);
private:
	cocos2d::Sprite* player;
	cocos2d::Vector<SpriteFrame*> attack;
	cocos2d::Vector<SpriteFrame*> dead;
	cocos2d::Vector<SpriteFrame*> run;
	cocos2d::Vector<SpriteFrame*> idle;
	cocos2d::Size visibleSize;
	cocos2d::Vec2 origin;
	cocos2d::Label* time;
	cocos2d::Label* score;
	int dtime;
	cocos2d::ProgressTimer* pT;
	bool mutex;
	char lastCid;
	int killedMonster;
	bool canMove;
	sqlite3* pdb;
};
