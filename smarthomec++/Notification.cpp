#include "notification.hpp"
#include "device.hpp"
#include "automationScenario.hpp"
#include <iostream>
#include <sstream>
#include <iomanip>
#include <ctime>

Notification::Notification(const std::string& id, NotificationType notificationType,
    const std::string& msg)
    : notificationId(id),
    type(notificationType),
    message(msg),
    relatedDevice(nullptr),
    relatedScenario(nullptr),
    timestamp(std::time(nullptr)),
    isRead(false) {
}

Notification::~Notification() {
}

void Notification::send() {
    std::cout << "Отправка уведомления [" << notificationId << "]: " << message << std::endl;
    isRead = false;
}

void Notification::markAsRead() {
    isRead = true;
    std::cout << "Уведомление [" << notificationId << "] отмечено как прочитанное" << std::endl;
}

int Notification::getNotificationPriority() const {
    switch (type) {
    case NotificationType::ALERT:
        return 3;
    case NotificationType::WARNING:
        return 2;
    case NotificationType::INFO:
        return 1;
    default:
        return 0;
    }
}

void Notification::displayInfo() const {
    std::cout << "[" << notificationId << "] ";

    switch (type) {
    case NotificationType::ALERT:
        std::cout << "ТРЕВОГА: ";
        break;
    case NotificationType::WARNING:
        std::cout << "ПРЕДУПРЕЖДЕНИЕ: ";
        break;
    case NotificationType::INFO:
        std::cout << "ИНФОРМАЦИЯ: ";
        break;
    }

    std::cout << message << " (" << (isRead ? "Прочитано" : "Непрочитано") << ")" << std::endl;

    if (relatedDevice) {
        std::cout << "  Связанное устройство: " << relatedDevice->getName() << std::endl;
    }

    if (relatedScenario) {
        std::cout << "  Связанный сценарий: " << relatedScenario->getName() << std::endl;
    }

    char buffer[80];

    // Безопасная версия для Windows
#ifdef _WIN32
    struct tm timeInfo;
    localtime_s(&timeInfo, &timestamp);
    std::strftime(buffer, sizeof(buffer), "%Y-%m-%d %H:%M:%S", &timeInfo);
#else
    struct tm* timeInfo = std::localtime(&timestamp);
    std::strftime(buffer, sizeof(buffer), "%Y-%m-%d %H:%M:%S", timeInfo);
#endif

    std::cout << "  Время: " << buffer << std::endl;
    std::cout << "  Приоритет: " << getNotificationPriority() << std::endl;
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
    ss << notificationId << "|"
        << static_cast<int>(type) << "|"
        << message << "|"
        << (relatedDevice ? relatedDevice->getId() : "NULL") << "|"
        << (relatedScenario ? relatedScenario->getScenarioId() : "NULL") << "|"
        << timestamp << "|"
        << isRead;
    return ss.str();
}

std::unique_ptr<Notification> Notification::deserialize(const std::string& data) {
    try {
        std::stringstream ss(data);
        std::string id, typeStr, message, deviceId, scenarioId, timestampStr, isReadStr;

        std::getline(ss, id, '|');
        std::getline(ss, typeStr, '|');
        std::getline(ss, message, '|');
        std::getline(ss, deviceId, '|');
        std::getline(ss, scenarioId, '|');
        std::getline(ss, timestampStr, '|');
        std::getline(ss, isReadStr, '|');

        NotificationType type = static_cast<NotificationType>(std::stoi(typeStr));
        auto notification = std::make_unique<Notification>(id, type, message);

        notification->timestamp = std::stol(timestampStr);
        notification->isRead = (isReadStr == "1");

        // Примечание: связи с устройствами и сценариями должны быть установлены позже
        // после загрузки всех данных

        return notification;
    }
    catch (const std::exception& e) {
        std::cerr << "Ошибка десериализации уведомления: " << e.what() << std::endl;
        return nullptr;
    }
}