#include "UsersInfoScene.h"
#include "json\document.h"
#include "Utils.h"

using namespace cocos2d::network;
using namespace rapidjson;
using namespace std;


cocos2d::Scene * UsersInfoScene::createScene() {
  return UsersInfoScene::create();
}

bool UsersInfoScene::init() {
  if (!Scene::init()) return false;

  auto visibleSize = Director::getInstance()->getVisibleSize();
  Vec2 origin = Director::getInstance()->getVisibleOrigin();

  auto getUserButton = MenuItemFont::create("Get User", CC_CALLBACK_1(UsersInfoScene::getUserButtonCallback, this));
  if (getUserButton) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + getUserButton->getContentSize().height / 2;
    getUserButton->setPosition(Vec2(x, y));
  }

  auto backButton = MenuItemFont::create("Back", [](Ref* pSender) {
    Director::getInstance()->popScene();
  });
  if (backButton) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + visibleSize.height - backButton->getContentSize().height / 2;
    backButton->setPosition(Vec2(x, y));
  }

  auto menu = Menu::create(getUserButton, backButton, NULL);
  menu->setPosition(Vec2::ZERO);
  this->addChild(menu, 1);

  limitInput = TextField::create("limit", "arial", 24);
  if (limitInput) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + visibleSize.height - 100.0f;
    limitInput->setPosition(Vec2(x, y));
    this->addChild(limitInput, 1);
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

void UsersInfoScene::getUserButtonCallback(Ref * pSender) {
  // Your code here
	string limit = limitInput->getString();
	HttpRequest* request = new HttpRequest();
	request->setUrl("http://127.0.0.1:8000/users?limit=" + limit);
	request->setRequestType(HttpRequest::Type::GET);
	request->setResponseCallback(CC_CALLBACK_2(UsersInfoScene::onRequestCompleted, this));
	request->setTag("Get");
	HttpClient::getInstance()->send(request);
	request->release();
}

void UsersInfoScene::onRequestCompleted(HttpClient *sender, HttpResponse *response) {
	if (!response) {
		return;
	}
	if (!response->isSucceed()) {
		log("response failed");
		log("error buffer: &s", response->getErrorBuffer());
		return;
	}
	string result = "";
	vector<char> *buffer = response->getResponseData();
	for (unsigned int i = 0; i < buffer->size(); i++) {
		result += (*buffer)[i];
	}
	CCLOG(result.c_str());
	Document document;
	string msg;
	bool status;
	string text;
	document.Parse<0>(result.c_str());
	if (document.HasParseError()) {
		CCLOG("GetParseError %s\n", document.GetParseError());
	}

	if (document.IsObject()) {
		if (document.HasMember("status")) {
			status = document["status"].GetBool();
		}
		if (document.HasMember("msg")) {
			msg = document["msg"].GetString();
		}
	}
	
	// Get Failed!
	if (!status) {
		// Return Error Information
		messageBox->setString("Get User Failed\n" + msg);
	}
	else {
		rapidjson::Value& data = document["data"];
		for (int i = 0; i < data.Size(); i++) {
			text += "Username: ";
			text += data[i]["username"].GetString();
			text += "\n";

			text += "Deck:\n";
			rapidjson::Value& deckTemp = data[i]["deck"];
			for (int j = 0; j < deckTemp.Size(); j++) {
				// һ���ֶ�
				rapidjson::Value& ob = deckTemp[j];

				for (auto it = ob.MemberBegin(); it != ob.MemberEnd(); it++) {
					text += " ";
					text += it->name.GetString();
					text += ": ";
					text += " ";
					text += to_string(it->value.GetInt());
					text += "\n";
				}
				text += " ---\n";
			}
			text += "---\n";
		}
		messageBox->setString(text);
	}
}
