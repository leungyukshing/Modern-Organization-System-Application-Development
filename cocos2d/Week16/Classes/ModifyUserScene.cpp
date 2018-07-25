#include "ModifyUserScene.h"
#include "Utils.h"
#include "network\HttpClient.h"
#include "json\document.h"


using namespace cocos2d::network;
using namespace rapidjson;
cocos2d::Scene * ModifyUserScene::createScene() {
  return ModifyUserScene::create();
}

bool ModifyUserScene::init() {
  if (!Scene::init()) return false;
  
  auto visibleSize = Director::getInstance()->getVisibleSize();
  Vec2 origin = Director::getInstance()->getVisibleOrigin();

  auto postDeckButton = MenuItemFont::create("Post Deck", CC_CALLBACK_1(ModifyUserScene::putDeckButtonCallback, this));
  if (postDeckButton) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + postDeckButton->getContentSize().height / 2;
    postDeckButton->setPosition(Vec2(x, y));
  }

  auto backButton = MenuItemFont::create("Back", [](Ref* pSender) {
    Director::getInstance()->popScene();
  });
  if (backButton) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + visibleSize.height - backButton->getContentSize().height / 2;
    backButton->setPosition(Vec2(x, y));
  }

  auto menu = Menu::create(postDeckButton, backButton, NULL);
  menu->setPosition(Vec2::ZERO);
  this->addChild(menu, 1);

  deckInput = TextField::create("Deck json here", "arial", 24);
  if (deckInput) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + visibleSize.height - 100.0f;
    deckInput->setPosition(Vec2(x, y));
    this->addChild(deckInput, 1);
  }

  messageBox = Label::create("", "arial", 30);
  if (messageBox) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + visibleSize.height / 2;
    messageBox->setPosition(Vec2(x, y));
    this->addChild(messageBox, 1);
  }

  return true;
}

void ModifyUserScene::putDeckButtonCallback(Ref * pSender) {
	// Your code here
	string deck = deckInput->getString();
	HttpRequest* request = new HttpRequest();
	request->setUrl("http://127.0.0.1:8000/users");
	request->setRequestType(HttpRequest::Type::PUT);
	request->setResponseCallback(CC_CALLBACK_2(ModifyUserScene::putDeckCompleted, this));

	string data = "{\"deck\":" + deck + "}";
	request->setRequestData(data.c_str(), data.length());
	request->setTag("Modify");
	HttpClient::getInstance()->send(request);
	request->release();
}

void ModifyUserScene::putDeckCompleted(HttpClient *sender, HttpResponse *response) {
	if (!response) {
		return;
	}
	if (!response->isSucceed()) {
		log("response failed");
		log("error buffer: %s", response->getErrorBuffer());
		return;
	}

	vector<char> *buffer = response->getResponseData();
	string result = "";
	for (unsigned int i = 0; i < buffer->size(); i++) {
		result += (*buffer)[i];
	}
	CCLOG(result.c_str());
	Document document;
	string msg;
	bool status;
	document.Parse<0>(result.c_str());
	if (document.HasParseError()) {
		CCLOG("GetParseError %s\n", document.GetParseError());
	}
	
	// get the message return by the server
	if (document.IsObject()) {
		if (document.HasMember("status")) {
			status = document["status"].GetBool();
		}
		if (document.HasMember("msg")) {
			msg = document["msg"].GetString();
		}
	}

	// Put succeed!
	if (status) {
		messageBox->setString("PUT OK");
	}
	else {
		messageBox->setString(msg);
	}
}
