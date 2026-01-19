#include "room.hpp"
#include "device.hpp"
#include <iostream>
#include <sstream>
#include <memory>
#include <stdexcept>

Room::Room(const std::string& id, const std::string& roomName, double roomArea)
    : BaseEntity(id, roomName), area(roomArea) {}

Room::Room(const Room& other)
    : BaseEntity(other.id + "_copy", other.name + " (Copy)"),
    area(other.area) {
    for (const auto& device : other.devices) {
        devices.push_back(std::shared_ptr<Device>(device->clone()));
    }
}

Room::~Room() {
    devices.clear();
}

Room& Room::operator+=(std::shared_ptr<Device> device) {
    devices.push_back(device);
    return *this;
}

Room& Room::operator-=(std::shared_ptr<Device> device) {
    for (auto it = devices.begin(); it != devices.end(); ++it) {
        if (*it == device) {
            devices.erase(it);
            break;
        }
    }
    return *this;
}

std::shared_ptr<Device> Room::operator[](size_t index) const {
    if (index >= devices.size()) {
        throw std::out_of_range("Индекс выходит за пределы диапазона");
    }
    return devices[index];
}

std::ostream& operator<<(std::ostream& os, const Room& room) {
    os << "Комната: " << room.name << " (ID: " << room.id << ")\n";
    os << "  Площадь: " << room.area << " м²\n";
    os << "  Устройств: " << room.devices.size();
    return os;
}

void Room::addDevice(std::shared_ptr<Device> device) {
    devices.push_back(device);
}

void Room::removeDevice(std::shared_ptr<Device> device) {
    for (auto it = devices.begin(); it != devices.end(); ++it) {
        if (*it == device) {
            devices.erase(it);
            break;
        }
    }
}

std::vector<std::shared_ptr<Device>> Room::getDevices() const {
    return devices;
}

double Room::calculateRoomPowerConsumption() const {
    double totalPower = 0.0;
    for (const auto& device : devices) {
        if (device->getIsOn()) {
            totalPower += device->getPowerConsumption();
        }
    }
    return totalPower;
}

void Room::displayDevices() const {
    std::cout << "Устройства в " << name << ":\n";
    for (size_t i = 0; i < devices.size(); i++) {
        std::cout << "  " << i + 1 << ". " << devices[i]->getName()
            << " (" << devices[i]->getStatus() << ")\n";
    }
}

double Room::getArea() const {
    return area;
}

std::string Room::serialize() const {
    std::stringstream ss;
    ss << id << "|" << name << "|" << area;
    return ss.str();
}

void Room::displayInfo() const {
    std::cout << "Комната: " << name << " (ID: " << id << ")" << std::endl;
    std::cout << "  Площадь: " << area << " м²" << std::endl;
    std::cout << "  Устройств: " << devices.size() << std::endl;
    std::cout << "  Текущее потребление: " << calculateRoomPowerConsumption() << " Вт" << std::endl;
}

BaseEntity* Room::clone() const {
    return new Room(*this);
}

std::shared_ptr<Room> Room::deserialize(const std::string& data) {
    try {
        std::stringstream ss(data);
        std::string id, name, areaStr;

        std::getline(ss, id, '|');
        std::getline(ss, name, '|');
        std::getline(ss, areaStr, '|');

        return std::make_shared<Room>(id, name, std::stod(areaStr));
    }
    catch (const std::exception& e) {
        std::cerr << "Ошибка десериализации комнаты: " << e.what() << std::endl;
        return nullptr;
    }
}