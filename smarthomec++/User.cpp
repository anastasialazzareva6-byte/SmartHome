#include "user.hpp"
#include "activity.hpp"
#include <iostream>
#include <sstream>
#include <memory>
#include <algorithm>

User::User(const std::string& id, const std::string& uname,
    const std::string& pwdHash, AccessLevel level,
    const std::string& userEmail, const std::string& userPhone)
    : userId(id), username(uname), passwordHash(pwdHash),
    accessLevel(level), email(userEmail), phone(userPhone) {}

User::User(const User& other)
    : userId(other.userId + "_copy"),
    username(other.username + " (Copy)"),
    passwordHash(other.passwordHash),
    accessLevel(other.accessLevel),
    email(other.email),
    phone(other.phone) {
    for (const auto& activity : other.activityLog) {
        activityLog.push_back(std::make_unique<Activity>(
            activity->getActivityId() + "_copy",
            this,
            activity->getAction()
        ));
    }
}

User::~User() {
    activityLog.clear();
}

User& User::operator=(const User& other) {
    if (this != &other) {
        userId = other.userId + "_assigned";
        username = other.username + " (Assigned)";
        passwordHash = other.passwordHash;
        accessLevel = other.accessLevel;
        email = other.email;
        phone = other.phone;

        activityLog.clear();
        for (const auto& activity : other.activityLog) {
            activityLog.push_back(std::make_unique<Activity>(
                activity->getActivityId() + "_assigned",
                this,
                activity->getAction()
            ));
        }
    }
    return *this;
}

bool User::operator==(const User& other) const {
    return this->username == other.username && this->userId == other.userId;
}

User::operator bool() const {
    return !this->username.empty() && !this->userId.empty();
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
    try {
        if (newPassword.length() < 3) {
            throw std::invalid_argument("Пароль должен содержать минимум 3 символа");
        }
        if (newPassword.find(' ') != std::string::npos) {
            throw std::invalid_argument("Пароль не должен содержать пробелов");
        }

        passwordHash = newPassword;
        addActivity("Пароль изменен");
        return true;
    }
    catch (const std::invalid_argument& e) {
        std::cerr << "Ошибка изменения пароля: " << e.what() << std::endl;
        return false;
    }
}

std::vector<Activity*> User::getActivityHistory() const {
    std::vector<Activity*> activities;
    for (const auto& activity : activityLog) {
        activities.push_back(activity.get());
    }
    return activities;
}

void User::addActivity(const std::string& action) {
    activityLog.push_back(std::make_unique<Activity>(
        std::to_string(activityLog.size() + 1),
        this,
        action
    ));
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

std::string User::getFullInfo() const {
    std::string info = "ID: " + userId + "\n";
    info += "Имя: " + username + "\n";

    info += "Роль: " + (accessLevel == AccessLevel::ADMIN ? std::string("Администратор") :
        std::string("Пользователь")) + "\n";

    if (!email.empty()) {
        info += "Email: " + email + "\n";
    }
    if (!phone.empty()) {
        info += "Телефон: " + phone + "\n";
    }

    std::string lowerUsername = username;
    std::transform(lowerUsername.begin(), lowerUsername.end(), lowerUsername.begin(), ::tolower);
    if (lowerUsername.find("admin") != std::string::npos) {
        info += "Примечание: Имя содержит 'admin'\n";
    }

    return info;
}

bool User::containsInInfo(const std::string& search) const {
    std::string allInfo = getFullInfo();
    std::string lowerAllInfo = allInfo;
    std::string lowerSearch = search;

    std::transform(lowerAllInfo.begin(), lowerAllInfo.end(), lowerAllInfo.begin(), ::tolower);
    std::transform(lowerSearch.begin(), lowerSearch.end(), lowerSearch.begin(), ::tolower);

    return lowerAllInfo.find(lowerSearch) != std::string::npos;
}

std::string User::getFormattedContactInfo() const {
    std::string contacts;
    if (!email.empty() && !phone.empty()) {
        contacts = "Контакты: " + email + ", " + phone;
    }
    else if (!email.empty()) {
        contacts = "Email: " + email;
    }
    else if (!phone.empty()) {
        contacts = "Телефон: " + phone;
    }
    else {
        contacts = "Контакты не указаны";
    }
    return contacts;
}

std::string User::serialize() const {
    std::stringstream ss;
    ss << userId << "|" << username << "|" << passwordHash << "|"
        << static_cast<int>(accessLevel) << "|" << email << "|" << phone;
    return ss.str();
}

std::shared_ptr<User> User::deserialize(const std::string& data) {
    try {
        std::stringstream ss(data);
        std::string id, username, password, levelStr, email, phone;

        std::getline(ss, id, '|');
        std::getline(ss, username, '|');
        std::getline(ss, password, '|');
        std::getline(ss, levelStr, '|');
        std::getline(ss, email, '|');
        std::getline(ss, phone, '|');

        AccessLevel level = static_cast<AccessLevel>(std::stoi(levelStr));
        return std::make_shared<User>(id, username, password, level, email, phone);
    }
    catch (const std::exception& e) {
        std::cerr << "Ошибка десериализации пользователя: " << e.what() << std::endl;
        return nullptr;
    }
}