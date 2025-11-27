#include "room.hpp"
#include "device.hpp"
#include <iostream>
#include <sstream>

Room::Room(const std::string& id, const std::string& roomName, double roomArea)
    : roomId(id), name(roomName), area(roomArea) {}

Room::~Room() {
    devices.clear();
}

void Room::addDevice(Device* device) {
    devices.push_back(device);
}

void Room::removeDevice(Device* device) {
    for (auto it = devices.begin(); it != devices.end(); ++it) {
        if (*it == device) {
            devices.erase(it);
            break;
        }
    }
}

std::vector<Device*> Room::getDevices() const {
    return devices;
}

double Room::calculateRoomPowerConsumption() const {
    double totalPower = 0.0;
    for (const auto& device : devices) {
        if (device->getIsOn()) {  // Только включенные устройства
            totalPower += device->getPowerConsumption();
        }
    }
    return totalPower;
}

void Room::displayDevices() const {
    std::cout << "Устройства в " << name << ":\n";
    for (const auto& device : devices) {
        std::cout << "  - " << device->getName() << " (" << device->getStatus() << ")\n";
    }
}

std::string Room::getRoomId() const {
    return roomId;
}

std::string Room::getName() const {
    return name;
}

double Room::getArea() const {
    return area;
}

std::string Room::serialize() const {
    std::stringstream ss;
    ss << roomId << "|" << name << "|" << area;
    return ss.str();
}

Room* Room::deserialize(const std::string& data) {
    std::stringstream ss(data);
    std::string id, name, areaStr;

    std::getline(ss, id, '|');
    std::getline(ss, name, '|');
    std::getline(ss, areaStr, '|');

    return new Room(id, name, std::stod(areaStr));
}