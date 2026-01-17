#ifndef USER_HPP
#define USER_HPP

#include <string>
#include <vector>
#include <memory>
#include <stdexcept>
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
    std::vector<std::unique_ptr<Activity>> activityLog;

public:
    User(const std::string& id, const std::string& uname,
        const std::string& pwdHash, AccessLevel level,
        const std::string& userEmail, const std::string& userPhone);
    User(const User& other);
    ~User();

    User& operator=(const User& other);
    bool operator==(const User& other) const;
    explicit operator bool() const;

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

    std::string getFullInfo() const;
    bool containsInInfo(const std::string& search) const;
    std::string getFormattedContactInfo() const;

    std::string serialize() const;
    static std::shared_ptr<User> deserialize(const std::string& data);
};

#endif