#include "MenuScene.h"
#include "SimpleAudioEngine.h"

USING_NS_CC;

Scene* MenuScene::createScene()
{
    // return MenuScene::create();
	auto scene = Scene::create();
	auto layer = MenuScene::create();
	scene->addChild(layer);
	return scene;
}

// Print useful error message instead of segfaulting when files are not there.
static void problemLoading(const char* filename)
{
    printf("Error while loading: %s\n", filename);
    printf("Depending on how you compiled you might have to add 'Resources/' in front of filenames in HelloWorldScene.cpp\n");
}

// on "init" you need to initialize your instance
bool MenuScene::init()
{
    //////////////////////////////
    // 1. super init first
    if ( !Scene::init() )
    {
        return false;
    }

    auto visibleSize = Director::getInstance()->getVisibleSize();
    Vec2 origin = Director::getInstance()->getVisibleOrigin();

	auto bg_sky = Sprite::create("menu-background-sky.jpg");
	bg_sky->setPosition(Vec2(visibleSize.width / 2 + origin.x, visibleSize.height / 2 + origin.y + 150));
	this->addChild(bg_sky, 0);

	auto bg = Sprite::create("menu-background.png");
	bg->setPosition(Vec2(visibleSize.width / 2 + origin.x, visibleSize.height / 2 + origin.y - 60));
	this->addChild(bg, 0);

	auto miner = Sprite::create("menu-miner.png");
	miner->setPosition(Vec2(150 + origin.x, visibleSize.height / 2 + origin.y - 60));
	this->addChild(miner, 1);
	
	auto text = Sprite::create("gold-miner-text.png");
	text->setPosition(Point(visibleSize.width / 2, 560));
	this->addChild(text, 0);

	auto gold = Sprite::create("menu-start-gold.png");
	gold->setPosition(Point(700, 150));
	this->addChild(gold, 0);

	// 抖腿动画
	auto leg = Sprite::createWithSpriteFrameName("miner-leg-0.png");
	// 获得动画
	Animate* legAnimate = Animate::create(AnimationCache::getInstance()->getAnimation("legAnimation"));
	// 添加动画（不断重复）
	leg->runAction(RepeatForever::create(legAnimate));
	leg->setPosition(110 + origin.x, origin.y + 102);
	this->addChild(leg, 1);
	
	// 开始按钮
	auto start = MenuItemImage::create("start-0.png", "start-1.png", CC_CALLBACK_1(MenuScene::startMenuCallback, this));
	start->setPosition(Point(380, 90));
	auto menu = Menu::create(start, NULL);
	menu->setPosition(Point(330, 120));
	this->addChild(menu, 1);

	return true;
}

void MenuScene::startMenuCallback(cocos2d::Ref* pSender) {
	CCTransitionScene * reScene = NULL;
	CCScene * s = GameScene::createScene();
	float t = 1.2f;
	reScene = CCTransitionProgressRadialCCW::create(t, s);
	CCDirector::sharedDirector()->replaceScene(reScene);
}

