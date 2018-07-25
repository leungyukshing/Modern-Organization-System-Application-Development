#include "LoginRegisterScene.h"
#include "ui\CocosGUI.h"
#include "json\rapidjson.h"
#include "json\document.h"
#include "json\writer.h"
#include "json\stringbuffer.h"
#include "Utils.h"
#include <vector>

USING_NS_CC;
using namespace cocos2d::network;
using namespace cocos2d::ui;
using namespace std;
using namespace rapidjson;

cocos2d::Scene * LoginRegisterScene::createScene() {
  return LoginRegisterScene::create();
}

bool LoginRegisterScene::init() {
  if (!Scene::init()) {
    return false;
  }

  auto visibleSize = Director::getInstance()->getVisibleSize();
  Vec2 origin = Director::getInstance()->getVisibleOrigin();

  auto loginButton = MenuItemFont::create("Login", CC_CALLBACK_1(LoginRegisterScene::loginButtonCallback, this));
  if (loginButton) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + loginButton->getContentSize().height / 2;
    loginButton->setPosition(Vec2(x, y));
  }

  auto registerButton = MenuItemFont::create("Register", CC_CALLBACK_1(LoginRegisterScene::registerButtonCallback, this));
  if (registerButton) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + registerButton->getContentSize().height / 2 + 100;
    registerButton->setPosition(Vec2(x, y));
  }

  auto backButton = MenuItemFont::create("Back", [] (Ref* pSender) {
    Director::getInstance()->popScene();
  });
  if (backButton) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + visibleSize.height - backButton->getContentSize().height / 2;
    backButton->setPosition(Vec2(x, y));
  }

  auto menu = Menu::create(loginButton, registerButton, backButton, NULL);
  menu->setPosition(Vec2::ZERO);
  this->addChild(menu, 1);

  usernameInput = TextField::create("username", "arial", 24);
  if (usernameInput) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + visibleSize.height - 100.0f;
    usernameInput->setPosition(Vec2(x, y));
    this->addChild(usernameInput, 1);
  }

  passwordInput = TextField::create("password", "arial", 24);
  if (passwordInput) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + visibleSize.height - 130.0f;
    passwordInput->setPosition(Vec2(x, y));
    this->addChild(passwordInput, 1);
  }

  messageBox = Label::create("", "arial", 30);
  if (messageBox) {
    float x = origin.x + visibleSize.width / 2;
    float y = origin.y + visibleSize.height - 200.0f;
    messageBox->setPosition(Vec2(x, y));
    this->addChild(messageBox, 1);
  }

  return true;
}

void LoginRegisterScene::loginButtonCallback(cocos2d::Ref * pSender) {
  // Your code here
	string username = usernameInput->getString();
	string password = passwordInput->getString();
	HttpRequest* request = new HttpRequest();
	request->setUrl("http://127.0.0.1:8000/auth");
	request->setRequestType(HttpRequest::Type::POST);
	request->setResponseCallback(CC_CALLBACK_2(LoginRegisterScene::onCompleted, this));

	// write the post data
	string data = toJson(username, password);
	request->setRequestData(data.c_str(), data.length());
	request->setTag("Login");
	HttpClient::getInstance()->enableCookies("co.txt");
	HttpClient::getInstance()->send(request);
	request->release();
}

void LoginRegisterScene::registerButtonCallback(Ref * pSender) {
	// Your code here
	string username = usernameInput->getString();
	string password = passwordInput->getString();
	HttpRequest* request = new HttpRequest();
	request->setUrl("http://127.0.0.1:8000/users");
	request->setRequestType(HttpRequest::Type::POST);
	request->setResponseCallback(CC_CALLBACK_2(LoginRegisterScene::onCompleted, this));

	// write the post data
	string data = toJson(username, password);
	request->setRequestData(data.c_str(), data.length());
	request->setTag("Register");
	HttpClient::getInstance()->send(request);
	HttpClient::getInstance()->enableCookies(NULL);
	request->release();
}

void LoginRegisterScene::onCompleted(HttpClient *sender, HttpResponse *response) {
	if (!response) {
		return;
	}
	if (!response->isSucceed()) {
		log("response failed");
		log("error buffer: %s", response->getErrorBuffer());
		return;
	}
	std::vector<char> *buffer = response->getResponseData();
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
	CCLOG(msg.c_str());
	// Post succeed!
	if (status) {
		// Login return
		if (strcmp(response->getHttpRequest()->getTag(), "Login") == 0) {
			messageBox->setString("LoginOK\n" + msg);
		}
		// Register return
		else {
			messageBox->setString("RegisterOK");
		}
	}
	// Fail
	else {
		// Login return
		if (strcmp(response->getHttpRequest()->getTag(), "Login") == 0) {
			messageBox->setString("LoginFailed\n"+ msg);
		}
		// Register return
		else {
			messageBox->setString("RegisterFailed\n" + msg);
		}
	}
}

string LoginRegisterScene::toJson(string username, string password) {
	Document document;
	document.SetObject();
	Document::AllocatorType& allocator = document.GetAllocator();

	document.AddMember("username", rapidjson::Value(username.c_str(), allocator), allocator);
	document.AddMember("password", rapidjson::Value(password.c_str(), allocator), allocator);

	StringBuffer buffer;
	Writer<StringBuffer> writer(buffer);
	document.Accept(writer);

	return buffer.GetString();
}

