#ifndef USER_HPP
#define USER_HPP

#include <string>
#include <vector>
#include "accessLevel.hpp"

class Activity;

class User {
private:
    std::string userId;
    std::string username;
    std::string passwordHash;
    AccessLevel accessLevel;
    std::string email;
    std::string phone;
    std::vector<Activity*> activityLog;

public:
    User(const std::string& id, const std::string& uname,
        const std::string& pwdHash, AccessLevel level,
        const std::string& userEmail, const std::string& userPhone);
    ~User();

    bool login(const std::string& password);
    void logout();
    bool changePassword(const std::string& newPassword);
    std::vector<Activity*> getActivityHistory() const;
    void addActivity(const std::string& action);
    void displayInfo() const;

    std::string getUserId() const;
    std::string getUsername() const;
    AccessLevel getAccessLevel() const;
    std::string getEmail() const;
    std::string getPhone() const;
    void setAccessLevel(AccessLevel level);

    // Методы для сериализации
    std::string serialize() const;
    static User* deserialize(const std::string& data);
};

#endif