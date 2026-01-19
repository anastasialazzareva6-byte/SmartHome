#include "device.hpp"
#include "room.hpp"
#include <sstream>

Device::Device(const std::string& id, const std::string& deviceName,
    const std::string& manuf, DeviceType type, Room* room, double power)
    : deviceId(id), name(deviceName), manufacturer(manuf),
    deviceType(type), location(room), isOn(false), isOnline(true), powerConsumption(power) {}

Device::~Device() {}

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
    powerConsumption = value;
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

std::string Device::getDeviceId() const {
    return deviceId;
}

std::string Device::getName() const {
    return name;
}

std::string Device::getManufacturer() const {
    return manufacturer;
}

DeviceType Device::getDeviceType() const {
    return deviceType;
}

Room* Device::getLocation() const {
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

void Device::setLocation(Room* newLocation) {
    location = newLocation;
}

std::string Device::serialize() const {
    std::stringstream ss;
    ss << deviceId << "|" << name << "|" << manufacturer << "|"
        << static_cast<int>(deviceType) << "|" << (location ? location->getRoomId() : "NULL") << "|"
        << isOn << "|" << isOnline << "|" << powerConsumption;
    return ss.str();
}

Device* Device::deserialize(const std::string& data, Room* location) {
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
    Device* device = new Device(id, name, manufacturer, type, location, std::stod(powerStr));
    device->setIsOnline(isOnlineStr == "1");
    if (isOnStr == "1") device->turnOn();

    return device;
}