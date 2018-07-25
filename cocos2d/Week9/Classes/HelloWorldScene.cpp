#include "HelloWorldScene.h"
#include "SimpleAudioEngine.h"

USING_NS_CC;

Scene* HelloWorld::createScene()
{
    return HelloWorld::create();
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
    if ( !Scene::init() )
    {
        return false;
    }

    auto visibleSize = Director::getInstance()->getVisibleSize();
    Vec2 origin = Director::getInstance()->getVisibleOrigin();

    /////////////////////////////
    // 2. add a menu item with "X" image, which is clicked to quit the program
    //    you may modify it.

    // add a "close" icon to exit the progress. it's an autorelease object
    auto closeItem = MenuItemImage::create(
                                           "CloseNormal.png",
                                           "CloseSelected.png",
                                           CC_CALLBACK_1(HelloWorld::menuCloseCallback, this));

    if (closeItem == nullptr ||
        closeItem->getContentSize().width <= 0 ||
        closeItem->getContentSize().height <= 0)
    {
        problemLoading("'CloseNormal.png' and 'CloseSelected.png'");
    }
    else
    {
        float x = origin.x + visibleSize.width - closeItem->getContentSize().width/2;
        float y = origin.y + closeItem->getContentSize().height/2;
        closeItem->setPosition(Vec2(x,y));
    }

    // create menu, it's an autorelease object
    auto menu = Menu::create(closeItem, NULL);
    menu->setPosition(Vec2::ZERO);
    this->addChild(menu, 1);

    /////////////////////////////
    // 3. add your codes below...

    // add a label shows "Hello World"
    // create and initialize a label

	// ����CCDictionary����ȡxml
	CCDictionary *strings = CCDictionary::createWithContentsOfFile("fonts/strings.xml");
	const char *Name = ((String*)strings->objectForKey("name"))->getCString();

    auto labelName = CCLabelTTF::create(Name, "fonts/arial.ttf", 24);
	//auto labelName = Label::createWithSystemFont(Name, "fonts/arial.ttf", 24);
    if (labelName == nullptr)
    {
        problemLoading("'fonts/Marker Felt.ttf'");
    }
    else
    {
        // position the label on the center of the screen
        labelName->setPosition(Vec2(origin.x + visibleSize.width/2,
                                origin.y + visibleSize.height - 20));
		labelName->setColor(ccc3(255, 255, 0));
        // add the label as a child to this layer
        this->addChild(labelName, 1);
    }

	auto labelNum = Label::createWithTTF("16340133", "fonts/Marker Felt.ttf", 24);
	if (labelNum == nullptr)
	{
		problemLoading("'fonts/Marker Felt.ttf'");
	}
	else
	{
		// position the label on the center of the screen
		labelNum->setPosition(Vec2(origin.x + visibleSize.width / 2,
			origin.y + visibleSize.height - 50));

		// add the label as a child to this layer
		this->addChild(labelNum, 1);
	}

    // add "HelloWorld" splash screen"
    auto sprite = Sprite::create("sysu.png");
    if (sprite == nullptr)
    {
        problemLoading("'HelloWorld.png'");
    }
    else
    {
        // position the sprite on the center of the screen
        sprite->setPosition(Vec2(visibleSize.width/2 + origin.x, visibleSize.height - 190));

        // add the sprite as a child to this layer
        this->addChild(sprite, 0);
    }

	// add button
	// ����һ���ı�����ʾ����
	/*
	auto label = LabelTTF::create("Click me", "fonts/arial.ttf", 30);
	label->setPosition(Vec2(origin.x + visibleSize.width / 2,
		origin.y + visibleSize.height - 200));
	addChild(label, 1); // ��label��ӽ�ȥ

	// EventListenerTouchOneByOne��ʾһ����һ���Ĵ����� ÿ�δ���ֻ�ܼ�����һ��������
	auto listener = EventListenerTouchOneByOne::create();
	// onTouchBeagan��ʾ��ʼ�������¼�
	listener->onTouchBegan = [](Touch *t, Event *e) {
		log("onTouchBegan");
		return false;
	};
	// getEventDispatcher��ȡ�¼����ɷ���
	// addEventListenerWithSceneGraphPriority���¼��� �ڵ㣩������¼�������
	Director::getInstance()->getEventDispatcher()->addEventListenerWithSceneGraphPriority(listener, label);
    */
	/*
	// Create a label for display purposes
	label = LabelTTF::create("Last button: None", "fonts/Marker Felt.ttf,", 32);
	label->setPosition(Point(visibleSize.width / 2 + origin.x, origin.y + visibleSize.height - 80));
	label->setHorizontalAlignment(TextHAlignment::CENTER);
	this->addChild(label, 1);

	// Standard method to create a button
	auto starMenuItem = MenuItemImage::create("HelloWorld.png", "sysu.png", CC_CALLBACK_1(HelloWorld::starMenuCallback, this)); 
	starMenuItem->setPosition(Point(160, 220));
	starMenuItem->setPosition(Point(160, 220));
	auto starMenu = Menu::create(starMenuItem, NULL);
	starMenu->setPosition(Point::ZERO);
	this->addChild(starMenu, 1);
	*/
	// ����Menu
	auto _menu = Menu::create();
	// ����Menuλ������
	_menu->setPosition(Vec2::ZERO);

	// ����Label
	auto _label = Label::createWithSystemFont("Click Me", "Arial", 30);
	_label->setColor(ccc3(255, 0, 0));
	_label->setTag(5);
	// ͨ��Label����MenuItem
	// ������1.label 2.�ص�����
	auto _menuItemLabel = MenuItemLabel::create(_label, CC_CALLBACK_1(HelloWorld::menuItemCallback, this));
	_menuItemLabel->setTag(3);
	// ����MenuItemλ������
	_menuItemLabel->setPosition(Vec2(380, 20));
	
	// ��MenuItem��ӵ�Menu��
	_menu->addChild(_menuItemLabel);
	_menu->setTag(1);
	this->addChild(_menu);
	return true;
}

void HelloWorld::menuItemCallback(cocos2d::Ref* ref) {
	auto tmp = this->getChildByTag(1);
	auto temp = tmp->getChildByTag(3);

	if (!color)
	{
		temp->getChildByTag(5)->setColor(ccc3(0, 255, 0));
		color = true;
	}
	else
	{
		temp->getChildByTag(5)->setColor(ccc3(255, 0, 0));
		color = false;
	}
	log("menuItem!!");
}

void HelloWorld::menuCloseCallback(Ref* pSender)
{
    //Close the cocos2d-x game scene and quit the application
    Director::getInstance()->end();

    #if (CC_TARGET_PLATFORM == CC_PLATFORM_IOS)
    exit(0);
#endif

    /*To navigate back to native iOS screen(if present) without quitting the application  ,do not use Director::getInstance()->end() and exit(0) as given above,instead trigger a custom event created in RootViewController.mm as below*/

    //EventCustom customEndEvent("game_scene_close_event");
    //_eventDispatcher->dispatchEvent(&customEndEvent);


}
