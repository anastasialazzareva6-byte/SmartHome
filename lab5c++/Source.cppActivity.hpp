#ifndef ACTIVITY_HPP
#define ACTIVITY_HPP

#include <string>
#include <ctime>
#include <memory>

class User;

class Activity {
private:
    std::string activityId;
    User* user;  // Не владеющий указатель
    std::string action;
    std::time_t timestamp;
    std::string relatedObject;

public:
    Activity(const std::string& id, User* activityUser, const std::string& act);
    Activity(const Activity& other);  // Конструктор копирования
    ~Activity();

    void logActivity();
    void displayInfo() const;

    std::string getActivityId() const;
    User* getUser() const;
    std::string getAction() const;
    std::time_t getTimestamp() const;
    void setRelatedObject(const std::string& object);

    // Методы для сериализации
    std::string serialize() const;
    static std::shared_ptr<Activity> deserialize(const std::string& data, User* user);
};

#endif