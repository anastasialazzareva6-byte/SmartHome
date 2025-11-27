#ifndef ROOM_HPP
#define ROOM_HPP

#include <string>
#include <vector>

class Device;

class Room {
private:
    std::string roomId;
    std::string name;
    double area;
    std::vector<Device*> devices;

public:
    Room(const std::string& id, const std::string& roomName, double roomArea);
    ~Room();

    void addDevice(Device* device);
    void removeDevice(Device* device);
    std::vector<Device*> getDevices() const;
    double calculateRoomPowerConsumption() const;
    void displayDevices() const;

    std::string getRoomId() const;
    std::string getName() const;
    double getArea() const;

    // Методы для сериализации
    std::string serialize() const;
    static Room* deserialize(const std::string& data);
};

#endif