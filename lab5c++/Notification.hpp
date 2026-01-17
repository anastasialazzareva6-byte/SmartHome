#ifndef NOTIFICATION_HPP
#define NOTIFICATION_HPP

#include <string>
#include <ctime>
#include <memory>
#include "notificationType.hpp"

class Device;
class AutomationScenario;

class Notification {
private:
    std::string notificationId;
    NotificationType type;
    std::string message;
    std::shared_ptr<Device> relatedDevice;
    std::shared_ptr<AutomationScenario> relatedScenario;
    std::time_t timestamp;
    bool isRead;

public:
    Notification(const std::string& id, NotificationType notificationType,
        const std::string& msg);
    ~Notification();

    void send();
    void markAsRead();
    int getNotificationPriority() const;
    void displayInfo() const;

    std::string getNotificationId() const;
    NotificationType getType() const;
    std::string getMessage() const;
    bool getIsRead() const;
    void setRelatedDevice(std::shared_ptr<Device> device);
    void setRelatedScenario(std::shared_ptr<AutomationScenario> scenario);

    // Методы для сериализации
    std::string serialize() const;
    static std::unique_ptr<Notification> deserialize(const std::string& data);
};

#endif