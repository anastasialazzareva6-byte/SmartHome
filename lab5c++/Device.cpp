#include "device.hpp"
#include "room.hpp"
#include <sstream>
#include <memory>
#include <stdexcept>
#include <iostream>

// Инициализация статического поля
int Device::deviceCount = 0;

Device::Device(const std::string& id, const std::string& deviceName,
    const std::string& manuf, DeviceType type, std::shared_ptr<Room> room, double power)
    : BaseEntity(id, deviceName), manufacturer(manuf),
    deviceType(type), location(room), isOn(false), isOnline(true),
    powerConsumption(power) {
    deviceCount++;
}

Device::Device(const Device& other)
    : BaseEntity(other.id + "_copy", other.name + " (Copy)"),
    manufacturer(other.manufacturer),
    deviceType(other.deviceType),
    location(other.location),
    isOn(other.isOn),
    isOnline(other.isOnline),
    powerConsumption(other.powerConsumption) {
    deviceCount++;
}

Device::~Device() {
    deviceCount--;
}

Device& Device::operator=(const Device& other) {
    if (this != &other) {
        this->id = other.id + "_assigned";
        this->name = other.name + " (Assigned)";
        this->manufacturer = other.manufacturer;
        this->deviceType = other.deviceType;
        this->location = other.location;
        this->isOn = other.isOn;
        this->isOnline = other.isOnline;
        this->powerConsumption = other.powerConsumption;
    }
    return *this;
}

Device Device::operator+(double additionalPower) const {
    Device newDevice(*this);
    newDevice.powerConsumption += additionalPower;
    return newDevice;
}

bool Device::operator>(const Device& other) const {
    return this->powerConsumption > other.powerConsumption;
}

Device::operator std::string() const {
    return this->name + " [" + this->manufacturer + "] - " + std::to_string(this->powerConsumption) + "W";
}

void Device::turnOn() {
    isOn = true;
}

void Device::turnOff() {
    isOn = false;
}

std::string Device::getStatus() const {
    return isOn ? "ВКЛ" : "ВЫКЛ";
}

void Device::updateCurrentValue(double value) {
    try {
        if (value < 0) {
            throw std::invalid_argument("Потребляемая мощность не может быть отрицательной");
        }
        if (value > 10000) {
            throw std::out_of_range("Слишком высокая мощность");
        }
        powerConsumption = value;
    }
    catch (const std::invalid_argument& e) {
        std::cerr << "Ошибка: " << e.what() << ". Установлено значение 0." << std::endl;
        powerConsumption = 0;
    }
    catch (const std::out_of_range& e) {
        std::cerr << "Ошибка: " << e.what() << ". Установлено максимальное значение 10000." << std::endl;
        powerConsumption = 10000;
    }
}

std::string Device::getDeviceTypeString() const {
    switch (deviceType) {
    case DeviceType::SENSOR: return "Датчик";
    case DeviceType::ACTUATOR: return "Исполнительное устройство";
    case DeviceType::CLIMATE_CONTROL: return "Климат-контроль";
    case DeviceType::SECURITY: return "Безопасность";
    case DeviceType::MULTIMEDIA: return "Мультимедиа";
    default: return "Неизвестно";
    }
}

std::string Device::getManufacturer() const {
    return manufacturer;
}

DeviceType Device::getDeviceType() const {
    return deviceType;
}

std::shared_ptr<Room> Device::getLocation() const {
    return location;
}

bool Device::getIsOn() const {
    return isOn;
}

bool Device::getIsOnline() const {
    return isOnline;
}

double Device::getPowerConsumption() const {
    return powerConsumption;
}

void Device::setPowerConsumption(double power) {
    powerConsumption = power;
}

void Device::setIsOnline(bool online) {
    isOnline = online;
}

void Device::setLocation(std::shared_ptr<Room> newLocation) {
    location = newLocation;
}

std::string Device::serialize() const {
    std::stringstream ss;
    ss << id << "|" << name << "|" << manufacturer << "|"
        << static_cast<int>(deviceType) << "|" << (location ? location->getId() : "NULL") << "|"
        << isOn << "|" << isOnline << "|" << powerConsumption;
    return ss.str();
}

void Device::displayInfo() const {
    std::cout << "Устройство: " << name << " (ID: " << id << ")" << std::endl;
    std::cout << "  Производитель: " << manufacturer << std::endl;
    std::cout << "  Тип: " << getDeviceTypeString() << std::endl;
    std::cout << "  Статус: " << getStatus() << std::endl;
    std::cout << "  Потребление: " << powerConsumption << " Вт" << std::endl;
}

int Device::getDeviceCount() {
    return deviceCount;
}

void validateDevice(const Device& device) {
    std::cout << "Проверка устройства:" << std::endl;
    std::cout << "  ID: " << device.id << std::endl;
    std::cout << "  Название: " << device.name << std::endl;

    // Проверка имени устройства
    if (device.name.find("Тест") != std::string::npos) {
        std::cout << "  ВНИМАНИЕ: Устройство может быть тестовым!" << std::endl;
    }

    // Проверка мощности
    if (device.powerConsumption > 1000) {
        std::cout << "  ВНИМАНИЕ: Высокое энергопотребление!" << std::endl;
    }

    std::cout << "  Проверка завершена." << std::endl;
}

std::shared_ptr<Device> Device::deserialize(const std::string& data, std::shared_ptr<Room> location) {
    try {
        std::stringstream ss(data);
        std::string id, name, manufacturer, typeStr, locationId, isOnStr, isOnlineStr, powerStr;

        std::getline(ss, id, '|');
        std::getline(ss, name, '|');
        std::getline(ss, manufacturer, '|');
        std::getline(ss, typeStr, '|');
        std::getline(ss, locationId, '|');
        std::getline(ss, isOnStr, '|');
        std::getline(ss, isOnlineStr, '|');
        std::getline(ss, powerStr, '|');

        DeviceType type = static_cast<DeviceType>(std::stoi(typeStr));
        auto device = std::make_shared<Device>(id, name, manufacturer, type, location, std::stod(powerStr));

        device->setIsOnline(isOnlineStr == "1");
        if (isOnStr == "1") device->turnOn();

        return device;
    }
    catch (const std::exception& e) {
        std::cerr << "Ошибка десериализации устройства: " << e.what() << std::endl;
        return nullptr;
    }
}