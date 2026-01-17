#include "securityDevice.hpp"
#include "room.hpp"
#include <sstream>
#include <iostream>
#include <algorithm>

SecurityDevice::SecurityDevice(const std::string& id, const std::string& deviceName,
    const std::string& manuf, std::shared_ptr<Room> room,
    double power)
    : Device(id, deviceName, manuf, DeviceType::SECURITY, room, power),
    isArmed(false),
    sensitivityLevel(5),
    motionDetected(false) {
    accessCodes.push_back("0000");
}

SecurityDevice::SecurityDevice(const SecurityDevice& other)
    : Device(other.id + "_copy", other.name + " (Copy)", other.manufacturer,
        other.deviceType, other.location, other.powerConsumption),
    isArmed(other.isArmed),
    accessCodes(other.accessCodes),
    sensitivityLevel(other.sensitivityLevel),
    motionDetected(other.motionDetected) {
    this->isOn = other.isOn;
    this->isOnline = other.isOnline;
}

SecurityDevice::~SecurityDevice() {
}

void SecurityDevice::turnOn() {
    isOn = true;
    std::cout << "Устройство безопасности " << name << " активировано" << std::endl;
}

void SecurityDevice::turnOff() {
    isOn = false;
    isArmed = false;
    std::cout << "Устройство безопасности " << name << " деактивировано" << std::endl;
}

std::string SecurityDevice::getDetails() const {
    std::string baseInfo = Device::getDetails();
    std::stringstream ss;
    ss << baseInfo
        << "Состояние охраны: " << (isArmed ? "АКТИВИРОВАНА" : "ОТКЛЮЧЕНА") << "\n"
        << "Чувствительность: " << sensitivityLevel << "\n"
        << "Обнаружено движение: " << (motionDetected ? "ДА" : "НЕТ") << "\n"
        << "Кодов доступа: " << accessCodes.size() << "\n";
    return ss.str();
}

void SecurityDevice::performSelfCheck() const {
    std::cout << "Проверка устройства безопасности: " << name << std::endl;
    std::cout << "  Статус: " << (isOn ? "ВКЛ" : "ВЫКЛ") << std::endl;
    std::cout << "  Охрана: " << (isArmed ? "АКТИВИРОВАНА" : "ОТКЛЮЧЕНА") << std::endl;
    std::cout << "  Чувствительность: " << sensitivityLevel << "/10" << std::endl;
    std::cout << "  Обнаружено движение: " << (motionDetected ? "ДА" : "НЕТ") << std::endl;
}

double SecurityDevice::calculateEfficiency() const {
    if (!isOn) return 0.0;

    double efficiency = 1.0;
    if (isArmed) efficiency *= 1.5;
    efficiency *= (sensitivityLevel / 10.0);

    return powerConsumption * efficiency;
}

SecurityDevice* SecurityDevice::clone() const {
    return new SecurityDevice(*this);
}

void SecurityDevice::arm() {
    if (isOn) {
        isArmed = true;
        std::cout << "Охрана активирована для устройства " << name << std::endl;
    }
    else {
        std::cout << "Нельзя активировать охрану: устройство выключено" << std::endl;
    }
}

void SecurityDevice::disarm() {
    isArmed = false;
    motionDetected = false;
    std::cout << "Охрана отключена для устройства " << name << std::endl;
}

void SecurityDevice::addAccessCode(const std::string& code) {
    if (code.length() >= 4) {
        accessCodes.push_back(code);
        std::cout << "Код доступа добавлен" << std::endl;
    }
    else {
        std::cout << "Код должен содержать не менее 4 символов" << std::endl;
    }
}

bool SecurityDevice::checkAccessCode(const std::string& code) const {
    return std::find(accessCodes.begin(), accessCodes.end(), code) != accessCodes.end();
}

void SecurityDevice::setSensitivity(int level) {
    if (level >= 1 && level <= 10) {
        sensitivityLevel = level;
        std::cout << "Чувствительность установлена на уровень " << level << std::endl;
    }
    else {
        std::cout << "Чувствительность должна быть от 1 до 10" << std::endl;
    }
}

void SecurityDevice::motionDetection(bool detected) {
    motionDetected = detected;
    if (detected && isArmed) {
        std::cout << "ВНИМАНИЕ! Обнаружено движение в охраняемой зоне!" << std::endl;
    }
}

bool SecurityDevice::getIsArmed() const {
    return isArmed;
}

int SecurityDevice::getSensitivityLevel() const {
    return sensitivityLevel;
}

bool SecurityDevice::getMotionDetected() const {
    return motionDetected;
}

std::string SecurityDevice::serialize() const {
    std::stringstream ss;
    ss << id << "|" << name << "|" << manufacturer << "|"
        << static_cast<int>(deviceType) << "|" << (location ? location->getId() : "NULL") << "|"
        << isOn << "|" << isOnline << "|" << powerConsumption << "|"
        << isArmed << "|" << sensitivityLevel << "|" << motionDetected;

    for (const auto& code : accessCodes) {
        ss << "|" << code;
    }

    return ss.str();
}

void SecurityDevice::displayInfo() const {
    std::cout << "Устройство безопасности: " << name << " (ID: " << id << ")" << std::endl;
    std::cout << "  Производитель: " << manufacturer << std::endl;
    std::cout << "  Статус: " << getStatus() << std::endl;
    std::cout << "  Потребление: " << powerConsumption << " Вт" << std::endl;
    std::cout << "  Охрана: " << (isArmed ? "АКТИВИРОВАНА" : "ОТКЛЮЧЕНА") << std::endl;
    std::cout << "  Чувствительность: " << sensitivityLevel << "/10" << std::endl;
    std::cout << "  Обнаружено движение: " << (motionDetected ? "ДА" : "НЕТ") << std::endl;
    std::cout << "  Кодов доступа: " << accessCodes.size() << std::endl;
}

std::shared_ptr<SecurityDevice> SecurityDevice::deserialize(const std::string& data,
    std::shared_ptr<Room> location) {
    try {
        std::stringstream ss(data);
        std::string id, name, manufacturer, typeStr, locationId, isOnStr, isOnlineStr,
            powerStr, isArmedStr, sensitivityStr, motionStr;

        std::getline(ss, id, '|');
        std::getline(ss, name, '|');
        std::getline(ss, manufacturer, '|');
        std::getline(ss, typeStr, '|');
        std::getline(ss, locationId, '|');
        std::getline(ss, isOnStr, '|');
        std::getline(ss, isOnlineStr, '|');
        std::getline(ss, powerStr, '|');
        std::getline(ss, isArmedStr, '|');
        std::getline(ss, sensitivityStr, '|');
        std::getline(ss, motionStr, '|');

        auto device = std::make_shared<SecurityDevice>(id, name, manufacturer, location,
            std::stod(powerStr));

        device->setIsOnline(isOnlineStr == "1");
        if (isOnStr == "1") device->turnOn();
        if (isArmedStr == "1") device->arm();
        device->setSensitivity(std::stoi(sensitivityStr));
        if (motionStr == "1") device->motionDetection(true);

        std::string code;
        while (std::getline(ss, code, '|')) {
            if (!code.empty()) {
                device->addAccessCode(code);
            }
        }

        return device;
    }
    catch (const std::exception& e) {
        std::cerr << "Ошибка десериализации устройства безопасности: " << e.what() << std::endl;
        return nullptr;
    }
}