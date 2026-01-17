#include "activity.hpp"
#include "user.hpp"
#include <iostream>
#include <iomanip>
#include <ctime>
#include <sstream>
#include <memory>

Activity::Activity(const std::string& id, User* activityUser, const std::string& act)
    : activityId(id), user(activityUser), action(act),
    timestamp(std::time(nullptr)), relatedObject("") {}

Activity::Activity(const Activity& other)
    : activityId(other.activityId + "_copy"),
    user(other.user),
    action(other.action + " (Copy)"),
    timestamp(std::time(nullptr)),  // Новая временная метка для копии
    relatedObject(other.relatedObject) {}

Activity::~Activity() {}

void Activity::logActivity() {
    std::cout << " Активность: " << action;
    if (user) {
        std::cout << " пользователем " << user->getUsername();
    }
    if (!relatedObject.empty()) {
        std::cout << " на " << relatedObject;
    }
    std::cout << std::endl;
}

void Activity::displayInfo() const {
    std::cout << "Активность: " << action;
    if (user) {
        std::cout << " (Пользователь: " << user->getUsername() << ")";
    }
    std::cout << std::endl;
}

std::string Activity::getActivityId() const {
    return activityId;
}

User* Activity::getUser() const {
    return user;
}

std::string Activity::getAction() const {
    return action;
}

std::time_t Activity::getTimestamp() const {
    return timestamp;
}

void Activity::setRelatedObject(const std::string& object) {
    relatedObject = object;
}

std::string Activity::serialize() const {
    std::stringstream ss;
    ss << activityId << "|" << (user ? user->getUserId() : "NULL") << "|"
        << action << "|" << timestamp << "|" << relatedObject;
    return ss.str();
}

std::shared_ptr<Activity> Activity::deserialize(const std::string& data, User* user) {
    try {
        std::stringstream ss(data);
        std::string id, userId, action, timestampStr, relatedObject;

        std::getline(ss, id, '|');
        std::getline(ss, userId, '|');
        std::getline(ss, action, '|');
        std::getline(ss, timestampStr, '|');
        std::getline(ss, relatedObject, '|');

        auto activity = std::make_shared<Activity>(id, user, action);
        activity->setRelatedObject(relatedObject);
        return activity;
    }
    catch (const std::exception& e) {
        std::cerr << "Ошибка десериализации активности: " << e.what() << std::endl;
        return nullptr;
    }
}