#include "HelloWorldScene.h"
#include "SimpleAudioEngine.h"
#include "Monster.h"
#include "sqlite3.h"
#include "string.h"
#pragma execution_character_set("utf-8")

USING_NS_CC;

Scene* HelloWorld::createScene()
{
	auto scene = Scene::create();
	auto layer = HelloWorld::create();
	scene->addChild(layer);
	return scene;
	//return HelloWorld::create();
}

// Print useful error message instead of segfaulting when files are not there.
static void problemLoading(const char* filename)
{
	printf("Error while loading: %s\n", filename);
	printf("Depending on how you compiled you might have to add 'Resources/' in front of filenames in HelloWorldScene.cpp\n");
}

// on "init" you need to initialize your instance
bool HelloWorld::init()
{
	//////////////////////////////
	// 1. super init first
	if (!Scene::init())
	{
		return false;
	}

	visibleSize = Director::getInstance()->getVisibleSize();
	origin = Director::getInstance()->getVisibleOrigin();

	//´´½¨Ò»ÕÅÌùÍ¼
	auto texture = Director::getInstance()->getTextureCache()->addImage("$lucia_2.png");
	//´ÓÌùÍ¼ÖÐÒÔÏñËØµ¥Î»ÇÐ¸î£¬´´½¨¹Ø¼üÖ¡
	auto frame0 = SpriteFrame::createWithTexture(texture, CC_RECT_PIXELS_TO_POINTS(Rect(0, 0, 113, 113)));
	//Ê¹ÓÃµÚÒ»Ö¡´´½¨¾«Áé
	player = Sprite::createWithSpriteFrame(frame0);
	player->setPosition(Vec2(origin.x + visibleSize.width / 2,
		origin.y + visibleSize.height / 2));
	addChild(player, 3);

	//hpÌõ
	Sprite* sp0 = Sprite::create("hp.png", CC_RECT_PIXELS_TO_POINTS(Rect(0, 320, 420, 47)));
	Sprite* sp = Sprite::create("hp.png", CC_RECT_PIXELS_TO_POINTS(Rect(610, 362, 4, 16)));

	//Ê¹ÓÃhpÌõÉèÖÃprogressBar
	pT = ProgressTimer::create(sp);
	pT->setScaleX(90);
	pT->setAnchorPoint(Vec2(0, 0));
	pT->setType(ProgressTimerType::BAR);
	pT->setBarChangeRate(Point(1, 0));
	pT->setMidpoint(Point(0, 1));
	pT->setPercentage(100);
	pT->setPosition(Vec2(origin.x + 14 * pT->getContentSize().width, origin.y + visibleSize.height - 2 * pT->getContentSize().height));
	addChild(pT, 2);
	sp0->setAnchorPoint(Vec2(0, 0));
	sp0->setPosition(Vec2(origin.x + pT->getContentSize().width, origin.y + visibleSize.height - sp0->getContentSize().height));
	addChild(sp0, 1);

	// ¾²Ì¬¶¯»­
	idle.reserve(1);
	idle.pushBack(frame0);

	// ¹¥»÷¶¯»­
	attack.reserve(17);
	for (int i = 0; i < 17; i++) {
		auto frame = SpriteFrame::createWithTexture(texture, CC_RECT_PIXELS_TO_POINTS(Rect(113 * i, 0, 113, 113)));
		attack.pushBack(frame);
	}

	// ¿ÉÒÔ·ÂÕÕ¹¥»÷¶¯»­
	// ËÀÍö¶¯»­(Ö¡Êý£º22Ö¡£¬¸ß£º90£¬¿í£º79£©
	auto texture2 = Director::getInstance()->getTextureCache()->addImage("$lucia_dead.png");
	// Todo
	dead.reserve(22);
	for (int i = 0; i < 22; i++) {
		auto frame = SpriteFrame::createWithTexture(texture2, CC_RECT_PIXELS_TO_POINTS(Rect(79 * i, 0, 79, 90)));
		dead.pushBack(frame);
	}

	// ÔË¶¯¶¯»­(Ö¡Êý£º8Ö¡£¬¸ß£º101£¬¿í£º68£©
	auto texture3 = Director::getInstance()->getTextureCache()->addImage("$lucia_forward.png");
	// Todo
	run.reserve(8);
	for (int i = 0; i < 8; i++) {
		auto frame = SpriteFrame::createWithTexture(texture3, CC_RECT_PIXELS_TO_POINTS(Rect(68 * i, 0, 68, 101)));
		run.pushBack(frame);
	}

	// Labels(W,A,S,D,X,Y)
	auto w = Label::createWithTTF("W", "fonts/arial.ttf", 36);
	auto a = Label::createWithTTF("A", "fonts/arial.ttf", 36);
	auto s = Label::createWithTTF("S", "fonts/arial.ttf", 36);
	auto d = Label::createWithTTF("D", "fonts/arial.ttf", 36);
	auto x = Label::createWithTTF("X", "fonts/arial.ttf", 36);
	auto y = Label::createWithTTF("Y", "fonts/arial.ttf", 36);

	// MenuItems
	auto menuItemW = MenuItemLabel::create(w, CC_CALLBACK_0(HelloWorld::move, this, 'W'));
	auto menuItemA = MenuItemLabel::create(a, CC_CALLBACK_0(HelloWorld::move, this, 'A'));
	auto menuItemS = MenuItemLabel::create(s, CC_CALLBACK_0(HelloWorld::move, this, 'S'));
	auto menuItemD = MenuItemLabel::create(d, CC_CALLBACK_0(HelloWorld::move, this, 'D'));
	auto menuItemX = MenuItemLabel::create(x, CC_CALLBACK_0(HelloWorld::changeHP, this, 'X'));
	auto menuItemY = MenuItemLabel::create(y, CC_CALLBACK_0(HelloWorld::changeHP, this, 'Y'));

	// Place the MenuItem
	menuItemA->setPosition(Vec2(origin.x + w->getContentSize().width, origin.y + w->getContentSize().height));
	menuItemS->setPosition(Vec2(menuItemA->getPosition().x + 1.5 * menuItemS->getContentSize().width, menuItemA->getPosition().y));
	menuItemD->setPosition(Vec2(menuItemA->getPosition().x + 3 * menuItemD->getContentSize().width, menuItemA->getPosition().y));
	menuItemW->setPosition(Vec2(menuItemS->getPosition().x, menuItemS->getPosition().y + menuItemW->getContentSize().height));
	menuItemX->setPosition(Vec2(origin.x + visibleSize.width - menuItemX->getContentSize().width, menuItemA->getPosition().y));
	menuItemY->setPosition(Vec2(menuItemX->getPosition().x - 2 * menuItemY->getContentSize().width, menuItemA->getPosition().y));

	auto menu = Menu::create(menuItemA, menuItemS, menuItemD, menuItemW, menuItemX, menuItemY, nullptr);
	menu->setPosition(0, 0);
	this->addChild(menu, 1);
	canMove = true;

	// Time counting
	time = Label::createWithTTF("180", "fonts/arial.ttf", 36);
	time->setPosition(visibleSize.width / 2, visibleSize.height - time->getContentSize().height - 10);
	this->addChild(time, 1);
	dtime = 180;

	// Score
	score = Label::createWithTTF("0", "fonts/arial.ttf", 36);
	score->setPosition(visibleSize.width - 50 - score->getContentSize().width, visibleSize.height - score->getContentSize().height - 10);
	killedMonster = UserDefault::getInstance()->getIntegerForKey("killedMonster");
	char temp[3];
	sprintf(temp, "%d", killedMonster);
	score->setString(temp);
	this->addChild(score, 1);
	// killedMonster = 0;
	

	// database

	printf(FileUtils::getInstance()->getWritablePath().c_str());
	auto path = FileUtils::getInstance()->getWritablePath() + "save.db";
	int result = sqlite3_open(path.c_str(), &pdb);
	std::string sql = "create table hero(ID int primary key not null, name char(10));";
	
	result = sqlite3_exec(pdb, sql.c_str(), NULL, NULL, NULL);
	sql = "insert into hero values(1, 'iori');";
	result = sqlite3_exec(pdb, sql.c_str(), NULL, NULL, NULL);

	 // update time per second
	schedule(schedule_selector(HelloWorld::updateTime), 1.0f, kRepeatForever, 0);
	// generate monster per 5 seconds
	schedule(schedule_selector(HelloWorld::generateMonster), 5.0f, kRepeatForever, 0);
	// check collision per frame
	scheduleUpdate();
	
	mutex = false;

	// add tile map
	TMXTiledMap* tmx = TMXTiledMap::create("untitled.tmx");
	tmx->setPosition(visibleSize.width / 2, visibleSize.height / 2);
	tmx->setAnchorPoint(Vec2(0.5, 0.5));
	tmx->setScale(Director::getInstance()->getContentScaleFactor());
	this->addChild(tmx, 0);

	// keyboard input
	// create a lister to listen keyboard event
	auto *dispatcher = Director::getInstance()->getEventDispatcher();
	auto *keyListener = EventListenerKeyboard::create();

	// listen to the key pressed event
	keyListener->onKeyPressed = CC_CALLBACK_2(HelloWorld::onKeyPressed, this);
	dispatcher->addEventListenerWithSceneGraphPriority(keyListener, this);
	return true;
}


void HelloWorld::move(char direction) {
	// generate animation
	auto animationMove = Animation::createWithSpriteFrames(run, 0.1f);
	animationMove->setRestoreOriginalFrame(true);
	auto animate = Animate::create(animationMove);

	if (!canMove) {
		return;
	}
	switch (direction) {
	case 'W':
		if (player->getPosition().y + 20 < visibleSize.height) {
			Point destination = Point(player->getPosition().x, player->getPosition().y + 20);
			auto move = MoveTo::create(0.4f, destination);
			auto spawn = Spawn::createWithTwoActions(move, animate);
			player->runAction(spawn);
		}
		break;
	case 'A':
		if (lastCid != 'A') {
			player->setFlipX(true);
		}
		lastCid = 'A';
		if (player->getPosition().x - 20 > 0) {
			Point destination = Point(player->getPosition().x - 20, player->getPosition().y);
			auto move = MoveTo::create(0.25f, destination);
			auto spawn = Spawn::createWithTwoActions(move, animate);
			player->runAction(spawn);
		}
		break;
	case 'S':
		if (player->getPosition().y - 20 > 0) {
			Point destination = Point(player->getPosition().x, player->getPosition().y - 20);
			auto move = MoveTo::create(0.25f, destination);
			auto spawn = Spawn::createWithTwoActions(move, animate);
			player->runAction(spawn);
		}
		break;
	case 'D':
		if (lastCid != 'D') {
			player->setFlipX(false);
		}
		lastCid = 'D';
		if (player->getPosition().x + 20 < visibleSize.width) {
			Point destination = Point(player->getPosition().x + 20, player->getPosition().y);
			auto move = MoveTo::create(0.25f, destination);
			auto spawn = Spawn::createWithTwoActions(move, animate);
			player->runAction(spawn);
		}
		break;
	default:
		break;
	}
}

void HelloWorld::changeHP(char cmd) {
	switch (cmd) {
		// dead
	case 'X':
		//doDead();
		break;
		// attact
	case 'Y':
		doAttact();
		break;
	default:
		break;
	}
}

void HelloWorld::signal() {
	mutex = false;
}

void HelloWorld::doAttact()
{
	if (!mutex && canAttack()) {
		mutex = true;

		killedMonster++;
		// Refresh the score
		char* mtime = new char[4];
		sprintf(mtime, "%d", killedMonster);
		score->setString(mtime);

		// generate attact animation
		auto animationAttact = Animation::createWithSpriteFrames(attack, 0.2f);
		animationAttact->setRestoreOriginalFrame(true);
		auto animate1 = Animate::create(animationAttact);

		auto hpAnimate = ProgressTo::create(0.8f, pT->getPercentage() + 10);
		// pT->setPercentage(pT->getPercentage() + 10);
		auto sequence = Sequence::create(animate1, CallFunc::create(CC_CALLBACK_0(HelloWorld::signal, this)), NULL);
		player->runAction(sequence);
		pT->runAction(hpAnimate);
	}
}

void HelloWorld::doDead() {
	if (!mutex) {
		mutex = true;

		// generate dead animation
		auto animationDead = Animation::createWithSpriteFrames(dead, 0.2f);
		// animationDead->setRestoreOriginalFrame(true);
		auto animate2 = Animate::create(animationDead);
		// pT->setPercentage(pT->getPercentage() - 10);
		auto temp = pT->getPercentage();
		auto hpAnimate = ProgressTo::create(0.8f, pT->getPercentage() - 20);

		// hurt
		if (pT->getPercentage() != 20) {
			auto sequence = Sequence::create(hpAnimate, CallFunc::create(CC_CALLBACK_0(HelloWorld::signal, this)), NULL);
			pT->runAction(sequence);
		}
		// dead
		else {
			auto sequence = Sequence::create(animate2, CallFunc::create(CC_CALLBACK_0(HelloWorld::signal, this)), NULL);
			player->runAction(sequence);
			pT->runAction(hpAnimate);

			// disable time counting
			unschedule(schedule_selector(HelloWorld::updateTime));
			// disable generate monsters
			unschedule(schedule_selector(HelloWorld::generateMonster));

			canMove = false;
			// record the score
			// Loacation: C:/Users/梁育诚/AppData/Local/hw11/
			UserDefault::getInstance()->setIntegerForKey("killedMonster", killedMonster);

			time->setString("Game Over!!");
		}
	}
}
void HelloWorld::updateTime(float dt) {
	if (dtime <= 0) {
		return;
	}
	dtime--;
	char* mtime = new char[4];
	sprintf(mtime, "%d", dtime);
	time->setString(mtime);
	
	auto fac = Factory::getInstance();
	fac->moveMonster(player->getPosition(), 1.5f);
}

void HelloWorld::generateMonster(float dt) {
	auto fac = Factory::getInstance();
	auto m = fac->createMonster();
	float x = random(origin.x, visibleSize.width);
	float y = random(origin.y, visibleSize.height);
	m->setPosition(x, y);
	this->addChild(m, 3);

	// fac->moveMonster(player->getPosition(), 1.5f);
}

void HelloWorld::hitByMonster(float dt) {
	auto fac = Factory::getInstance();
	// get the collisions around the player
	Sprite* collision = fac->collider(player->getBoundingBox());
	if (collision != NULL) {
		// todo
		// remove collision monster
		fac->removeMonster(collision);
		// hurt the player
		//changeHP('X');
		doDead();
	}
}

void HelloWorld::update(float dt) {
	hitByMonster(dt);
}

bool HelloWorld::canAttack() {
	auto fac = Factory::getInstance();
	// get attack range
	Rect playerRect = player->getBoundingBox();
	Rect attackRect = Rect(playerRect.getMinX() - 40, playerRect.getMinY(), playerRect.getMaxX() - playerRect.getMinX() + 80, playerRect.getMaxY() - playerRect.getMinY());
	
	// get the collision around the player
	Sprite* collision = fac->collider(attackRect);
	if (collision != NULL) {
		// remove the monster
		fac->removeMonster(collision);
		return true;
	}
	else {
		return false;
	}
}

void HelloWorld::onKeyPressed(EventKeyboard::KeyCode keycode, Event* event) {
	if (keycode == EventKeyboard::KeyCode::KEY_W || keycode == EventKeyboard::KeyCode::KEY_CAPITAL_W) {
		move('W');
	}
	else if (keycode == EventKeyboard::KeyCode::KEY_A || keycode == EventKeyboard::KeyCode::KEY_CAPITAL_A) {
		move('A');
	}
	else if (keycode == EventKeyboard::KeyCode::KEY_S || keycode == EventKeyboard::KeyCode::KEY_CAPITAL_S) {
		move('S');
	}
	else if (keycode == EventKeyboard::KeyCode::KEY_D || keycode == EventKeyboard::KeyCode::KEY_CAPITAL_D) {
		move('D');
	}
	else if (keycode == EventKeyboard::KeyCode::KEY_Y || keycode == EventKeyboard::KeyCode::KEY_CAPITAL_Y) {
		changeHP('Y');
	}
}