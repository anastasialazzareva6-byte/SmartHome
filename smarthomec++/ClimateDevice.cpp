#include "climateDevice.hpp"
#include "room.hpp"
#include <sstream>
#include <iostream>
#include <cmath>

ClimateDevice::ClimateDevice(const std::string& id, const std::string& deviceName,
    const std::string& manuf, std::shared_ptr<Room> room,
    double power, double targetTemp)
    : Device(id, deviceName, manuf, DeviceType::CLIMATE_CONTROL, room, power),
    targetTemperature(targetTemp),
    currentTemperature(20.0),
    humidity(50.0),
    autoMode(true) {
}

ClimateDevice::ClimateDevice(const ClimateDevice& other)
    : Device(other.id + "_copy", other.name + " (Copy)", other.manufacturer,
        other.deviceType, other.location, other.powerConsumption),
    targetTemperature(other.targetTemperature),
    currentTemperature(other.currentTemperature),
    humidity(other.humidity),
    autoMode(other.autoMode) {
    this->isOn = other.isOn;
    this->isOnline = other.isOnline;
}

ClimateDevice::~ClimateDevice() {
}

ClimateDevice& ClimateDevice::operator=(const ClimateDevice& other) {
    if (this != &other) {
        Device::operator=(other);

        this->targetTemperature = other.targetTemperature;
        this->currentTemperature = other.currentTemperature;
        this->humidity = other.humidity;
        this->autoMode = other.autoMode;
    }
    return *this;
}

ClimateDevice& ClimateDevice::operator=(const Device& other) {
    if (this != &other) {
        this->id = other.getId() + "_from_base";
        this->name = other.getName() + " (From Base)";

        const Device& deviceRef = static_cast<const Device&>(other);
        this->manufacturer = deviceRef.getManufacturer();
        this->deviceType = deviceRef.getDeviceType();
        this->location = deviceRef.getLocation();
        this->isOn = deviceRef.getIsOn();
        this->isOnline = deviceRef.getIsOnline();
        this->powerConsumption = deviceRef.getPowerConsumption();

        this->targetTemperature = 22.0;
        this->currentTemperature = 20.0;
        this->humidity = 50.0;
        this->autoMode = true;
    }
    return *this;
}

void ClimateDevice::turnOn() {
    isOn = true;
    if (autoMode) {
        adjustTemperature();
    }
    std::cout << "Климатическое устройство " << name << " включено" << std::endl;
}

void ClimateDevice::turnOff() {
    isOn = false;
    std::cout << "Климатическое устройство " << name << " выключено" << std::endl;
}

std::string ClimateDevice::getDetails() const {
    std::stringstream ss;
    ss << "Климатическое устройство: " << name << "\n"
        << "Целевая температура: " << targetTemperature << "°C\n"
        << "Текущая температура: " << currentTemperature << "°C\n"
        << "Влажность: " << humidity << "%\n"
        << "Автоматический режим: " << (autoMode ? "ВКЛ" : "ВЫКЛ") << "\n";
    return ss.str();
}

void ClimateDevice::performSelfCheck() const {
    std::cout << "Проверка климатического устройства: " << name << std::endl;
    std::cout << "  Температура: " << currentTemperature << "/" << targetTemperature << "°C" << std::endl;
    std::cout << "  Влажность: " << humidity << "%" << std::endl;
    std::cout << "  Авторежим: " << (autoMode ? "ВКЛ" : "ВЫКЛ") << std::endl;
}

double ClimateDevice::calculateEfficiency() const {
    if (!isOn) return 0.0;

    double tempDiff = std::abs(currentTemperature - targetTemperature);
    double efficiency = (tempDiff < 2.0) ? 0.9 : (tempDiff < 5.0) ? 0.7 : 0.5;

    return powerConsumption * efficiency;
}

ClimateDevice* ClimateDevice::clone() const {
    return new ClimateDevice(*this);
}

void ClimateDevice::setTargetTemperature(double temp) {
    targetTemperature = temp;
    if (isOn && autoMode) {
        adjustTemperature();
    }
}

void ClimateDevice::setAutoMode(bool enabled) {
    autoMode = enabled;
}

void ClimateDevice::adjustTemperature() {
    if (!isOn) return;

    if (currentTemperature < targetTemperature) {
        currentTemperature += 0.5;
    }
    else if (currentTemperature > targetTemperature) {
        currentTemperature -= 0.5;
    }

    if (humidity < 45) humidity += 1;
    else if (humidity > 55) humidity -= 1;

    std::cout << "Температура отрегулирована: " << currentTemperature << "°C" << std::endl;
}

double ClimateDevice::getTargetTemperature() const {
    return targetTemperature;
}

double ClimateDevice::getCurrentTemperature() const {
    return currentTemperature;
}

double ClimateDevice::getHumidity() const {
    return humidity;
}

bool ClimateDevice::isAutoMode() const {
    return autoMode;
}

std::string ClimateDevice::serialize() const {
    std::stringstream ss;
    ss << id << "|" << name << "|" << manufacturer << "|"
        << static_cast<int>(deviceType) << "|" << (location ? location->getId() : "NULL") << "|"
        << isOn << "|" << isOnline << "|" << powerConsumption << "|"
        << targetTemperature << "|" << currentTemperature << "|" << humidity << "|" << autoMode;
    return ss.str();
}

void ClimateDevice::displayInfo() const {
    std::cout << "Климатическое устройство: " << name << " (ID: " << id << ")" << std::endl;
    std::cout << "  Производитель: " << manufacturer << std::endl;
    std::cout << "  Статус: " << getStatus() << std::endl;
    std::cout << "  Потребление: " << powerConsumption << " Вт" << std::endl;
    std::cout << "  Температура: " << currentTemperature << "°C (цель: " << targetTemperature << "°C)" << std::endl;
    std::cout << "  Влажность: " << humidity << "%" << std::endl;
    std::cout << "  Авторежим: " << (autoMode ? "ВКЛ" : "ВЫКЛ") << std::endl;
}

std::shared_ptr<ClimateDevice> ClimateDevice::deserialize(const std::string& data,
    std::shared_ptr<Room> location) {
    try {
        std::stringstream ss(data);
        std::string id, name, manufacturer, typeStr, locationId, isOnStr, isOnlineStr,
            powerStr, targetTempStr, currentTempStr, humidityStr, autoModeStr;

        std::getline(ss, id, '|');
        std::getline(ss, name, '|');
        std::getline(ss, manufacturer, '|');
        std::getline(ss, typeStr, '|');
        std::getline(ss, locationId, '|');
        std::getline(ss, isOnStr, '|');
        std::getline(ss, isOnlineStr, '|');
        std::getline(ss, powerStr, '|');
        std::getline(ss, targetTempStr, '|');
        std::getline(ss, currentTempStr, '|');
        std::getline(ss, humidityStr, '|');
        std::getline(ss, autoModeStr, '|');

        auto device = std::make_shared<ClimateDevice>(id, name, manufacturer, location,
            std::stod(powerStr), std::stod(targetTempStr));

        device->setIsOnline(isOnlineStr == "1");
        if (isOnStr == "1") device->turnOn();
        device->setAutoMode(autoModeStr == "1");

        return device;
    }
    catch (const std::exception& e) {
        std::cerr << "Ошибка десериализации климатического устройства: " << e.what() << std::endl;
        return nullptr;
    }
}