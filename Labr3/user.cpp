#include "user.hpp"
#include "activity.hpp"
#include <iostream>
#include <sstream>

User::User(const std::string& id, const std::string& uname,
    const std::string& pwdHash, AccessLevel level,
    const std::string& userEmail, const std::string& userPhone)
    : userId(id), username(uname), passwordHash(pwdHash),
    accessLevel(level), email(userEmail), phone(userPhone) {}

User::~User() {
    for (auto activity : activityLog) {
        delete activity;
    }
    activityLog.clear();
}

bool User::login(const std::string& password) {
    bool success = (password == passwordHash);
    if (success) {
        addActivity("Пользователь вошел в систему");
    }
    return success;
}

void User::logout() {
    addActivity("Пользователь вышел из системы");
}

bool User::changePassword(const std::string& newPassword) {
    passwordHash = newPassword;
    addActivity("Пароль изменен");
    return true;
}

std::vector<Activity*> User::getActivityHistory() const {
    return activityLog;
}

void User::addActivity(const std::string& action) {
    Activity* activity = new Activity(std::to_string(activityLog.size() + 1), this, action);
    activityLog.push_back(activity);
}

void User::displayInfo() const {
    std::cout << "Пользователь: " << username << " ("
        << (accessLevel == AccessLevel::ADMIN ? "Администратор" : "Пользователь") << ")\n";
}

std::string User::getUserId() const {
    return userId;
}

std::string User::getUsername() const {
    return username;
}

AccessLevel User::getAccessLevel() const {
    return accessLevel;
}

std::string User::getEmail() const {
    return email;
}

std::string User::getPhone() const {
    return phone;
}

void User::setAccessLevel(AccessLevel level) {
    accessLevel = level;
}

std::string User::serialize() const {
    std::stringstream ss;
    ss << userId << "|" << username << "|" << passwordHash << "|"
        << static_cast<int>(accessLevel) << "|" << email << "|" << phone;
    return ss.str();
}

User* User::deserialize(const std::string& data) {
    std::stringstream ss(data);
    std::string id, username, password, levelStr, email, phone;

    std::getline(ss, id, '|');
    std::getline(ss, username, '|');
    std::getline(ss, password, '|');
    std::getline(ss, levelStr, '|');
    std::getline(ss, email, '|');
    std::getline(ss, phone, '|');

    AccessLevel level = static_cast<AccessLevel>(std::stoi(levelStr));
    return new User(id, username, password, level, email, phone);
}