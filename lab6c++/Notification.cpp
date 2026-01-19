#include "notification.hpp"
#include "device.hpp"
#include "automationScenario.hpp"
#include <iostream>
#include <ctime>
#include <sstream>

Notification::Notification(const std::string& id, NotificationType notificationType,
    const std::string& msg)
    : notificationId(id), type(notificationType), message(msg),
    relatedDevice(nullptr), relatedScenario(nullptr),
    timestamp(std::time(nullptr)), isRead(false) {}

Notification::~Notification() {
}

void Notification::send() {
    std::cout << "Уведомление отправлено: " << message << std::endl;
}

void Notification::markAsRead() {
    isRead = true;
    std::cout << "Уведомление отмечено как прочитанное" << std::endl;
}

int Notification::getNotificationPriority() const {
    switch (type) {
    case NotificationType::ALERT: return 3;
    case NotificationType::WARNING: return 2;
    case NotificationType::INFO: return 1;
    default: return 0;
    }
}

void Notification::displayInfo() const {
    std::cout << "[" << notificationId << "] ";
    switch (type) {
    case NotificationType::ALERT: std::cout << "[ТРЕВОГА] "; break;
    case NotificationType::WARNING: std::cout << "[ПРЕДУПРЕЖДЕНИЕ] "; break;
    case NotificationType::INFO: std::cout << "[ИНФО] "; break;
    }
    std::cout << message;
    if (relatedDevice) {
        std::cout << " (Устройство: " << relatedDevice->getName() << ")";
    }
    if (relatedScenario) {
        std::cout << " (Сценарий: " << relatedScenario->getName() << ")";
    }
    std::cout << " [" << (isRead ? "Прочитано" : "Новое") << "]" << std::endl;
}

std::string Notification::getNotificationId() const {
    return notificationId;
}

NotificationType Notification::getType() const {
    return type;
}

std::string Notification::getMessage() const {
    return message;
}

bool Notification::getIsRead() const {
    return isRead;
}

void Notification::setRelatedDevice(std::shared_ptr<Device> device) {
    relatedDevice = device;
}

void Notification::setRelatedScenario(std::shared_ptr<AutomationScenario> scenario) {
    relatedScenario = scenario;
}

std::string Notification::serialize() const {
    std::stringstream ss;
    ss << notificationId << "|" << static_cast<int>(type) << "|" << message << "|"
        << timestamp << "|" << isRead;
    return ss.str();
}

std::unique_ptr<Notification> Notification::deserialize(const std::string& data) {
    std::stringstream ss(data);
    std::string id, typeStr, message, timestampStr, isReadStr;

    std::getline(ss, id, '|');
    std::getline(ss, typeStr, '|');
    std::getline(ss, message, '|');
    std::getline(ss, timestampStr, '|');
    std::getline(ss, isReadStr, '|');

    auto notification = std::make_unique<Notification>(
        id,
        static_cast<NotificationType>(std::stoi(typeStr)),
        message
    );

    if (isReadStr == "1") notification->markAsRead();
    return notification;
}