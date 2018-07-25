#include "GameScene.h"

USING_NS_CC;

Scene* GameScene::createScene()
{
	// return Scene::create();
	auto scene = Scene::create();
	auto layer = GameScene::create();
	scene->addChild(layer);
	return scene;
}
// Print useful error message instead of segfaulting when files are not there.
static void problemLoading(const char* filename) {
	printf("Error while loading: %s\n", filename);
	printf("Depending on how you compiled you might have to add 'Resources/' in front of filenames in HelloWorldScene.cpp\n");
}
// on "init" you need to initialize your instance
bool GameScene::init()
{
	//////////////////////////////
	// 1. super init first
	if (!Scene::init())
	{
		return false;
	}

	// 单点触摸事件监听器
	EventListenerTouchOneByOne* listener = EventListenerTouchOneByOne::create();
	
	// 给触摸监听函数设置吞没时间，使得触摸上面的层的时候事件不会向下传递
	listener->setSwallowTouches(true);
	
	// 设置回调函数
	listener->onTouchBegan = CC_CALLBACK_2(GameScene::onTouchBegan, this);
	
	// 使用EventDispatcher注册时间监听器
	// _eventDispatcher->addEventListenerWithSceneGraphPriority(listener, this)
	Director::getInstance()->getEventDispatcher()->addEventListenerWithSceneGraphPriority(listener, this);


	Size visibleSize = Director::getInstance()->getVisibleSize();
	Vec2 origin = Director::getInstance()->getVisibleOrigin();

	// 背景
	auto sprite = Sprite::create("level-background-0.jpg");
	if (sprite == nullptr) {
		problemLoading("'level-background-o.png'");
	}
	else {
		// position the sprite on the center of the screen
		sprite->setPosition(Vec2(visibleSize.width / 2 + origin.x, visibleSize.height / 2 +  origin.y));

		// add the sprite as a child to this layer
		this->addChild(sprite, 0);
	}

	// 射击按钮
	auto label = Label::createWithTTF("Shoot", "fonts/Marker Felt.ttf", 80);
	label->setColor(ccc3(255, 0, 0));
	auto shoot = MenuItemLabel::create(label, CC_CALLBACK_1(GameScene::shootMenuCallback, this));
	shoot->setPosition(Point(visibleSize.width / 2 + 200, 450));
	auto _menu = Menu::create();
	_menu->setPosition(Vec2::ZERO);
	_menu->addChild(shoot);
	this->addChild(_menu, 1);
	
	// 石头
	stoneLayer = Layer::create();
	stone = Sprite::create("stone.png");
	
	stoneLayer->setAnchorPoint(Point(0, 0));
	stoneLayer->setPosition(Point(0, 0));

	stone->setPosition(Point(560, 480));

	stoneLayer->addChild(stone);

	// 老鼠
	mouseLayer = Layer::create();
	mouse = Sprite::createWithSpriteFrameName("gem-mouse-0.png");
	mouseLayer->setAnchorPoint(Point(0, 0));
	mouseLayer->setPosition(Point(0, visibleSize.height / 2));
	mouse->setPosition(Point(visibleSize.width / 2, 0));
	// 获得动画
	Animate* mouseAnimate = Animate::create(AnimationCache::getInstance()->getAnimation("mouseAnimation"));
	// 添加动画（一直重复）
	mouse->runAction(RepeatForever::create(mouseAnimate));
	mouseLayer->addChild(mouse);

	this->addChild(stoneLayer);
	this->addChild(mouseLayer);
	return true;
}

void GameScene::shootMenuCallback(Ref* pSender) {
	Point mouseLocation = mouse->getPosition();

	// mouseLocation是老鼠当前位置(相对位置)
	//auto diamond = Sprite::create("diamond.png");
	auto diamond = Sprite::createWithSpriteFrameName("diamond-0.png");
	Animate* diamondAnimate = Animate::create(AnimationCache::getInstance()->getAnimation("diamondAnimation"));
	diamond->runAction(RepeatForever::create(diamondAnimate));
	diamond->setPosition(mouseLocation);
	mouseLayer->addChild(diamond);

	// 石头移动并消失
	// stoneLocation是石头要移动到的位置，mouse的世界坐标
	Point stoneLocation = mouseLayer->convertToWorldSpaceAR(mouseLocation);
	auto stoneMove = MoveTo::create(1.2f, stoneLocation);
	//stone->runAction(stoneMove);
	auto disappear = FadeOut::create(2.0f);
	auto sequence1 = Sequence::create(stoneMove, disappear, nullptr);
	stone->runAction(sequence1);

	// 老鼠逃离，随机
	auto visibleSize = Director::getInstance()->getVisibleSize();
	auto x = RandomHelper::random_int(0, (int)(visibleSize.width));
	auto y = RandomHelper::random_int(0, (int)(visibleSize.height));
	Point destination = Point(x, y);
	// 转化为mouse层下的本地坐标
	//auto move = MoveTo::create(0.8f, mouseLayer->convertToNodeSpaceAR(destination));
	//mouse->runAction(move);

	// 旋转
	auto diff = mouseLayer->convertToNodeSpaceAR(destination) - mouseLocation;
	float angleRadians = atanf((float)diff.y / (float)diff.x);
	float cocosAngle = CC_RADIANS_TO_DEGREES(angleRadians);

	if (diff.x < 0) {
		cocosAngle += 180;
	}

	cocosAngle = -cocosAngle;

	auto actionRotate = RotateTo::create(0.5f, cocosAngle);
	auto move = MoveTo::create(0.8f, mouseLayer->convertToNodeSpaceAR(destination));
	auto sequence2 = Sequence::create(actionRotate, move, nullptr);

	mouse->runAction(sequence2);


	// 装载石头
	stone = Sprite::create("stone.png");
	stone ->setPosition(540, 480);
	stoneLayer->addChild(stone);
}

bool GameScene::onTouchBegan(Touch *touch, Event *unused_event) {
	// 鼠标点击的相对位置
	auto location = touch->getLocation();
	// 添加奶酪
	auto chess = Sprite::create("cheese.png");
	chess->setPosition(Point(location.x, location.y));
	this->addChild(chess, 0);
	auto disappear = FadeOut::create(5.0f);
	chess->runAction(disappear);


	// 将鼠标位置转为世界坐标
	Point touchLocation = Director::getInstance()->convertToGL(location);


	// 得到相对于老鼠层的本地坐标
	// mouseLocation是老鼠要去的位置
	Point mouseLocation = mouseLayer->convertToNodeSpace(touchLocation);
	mouseLocation = Point(mouseLocation.x, -mouseLocation.y);
	// current是老鼠当前位置
	Point current = mouse->getPosition();

	// 旋转
	auto diff = mouseLocation - current;
	float angleRadians = atanf((float)diff.y / (float)diff.x);
	float cocosAngle = CC_RADIANS_TO_DEGREES(angleRadians);

	if (diff.x < 0) {
		cocosAngle += 180;
	}
	
	cocosAngle = -cocosAngle;

	auto actionRotate = RotateTo::create(1.0f, cocosAngle);
	auto run = MoveTo::create(1.2f, mouseLocation);
	auto sequence = Sequence::create(actionRotate, run, nullptr);
	
	mouse->runAction(sequence);
	//mouse->runAction(actionRotate);
	
	return true;
}

