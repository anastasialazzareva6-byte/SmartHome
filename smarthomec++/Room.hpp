#ifndef ROOM_HPP
#define ROOM_HPP

#include <string>
#include <vector>
#include <memory>
#include "BaseEntity.hpp"
#include "deviceType.hpp"

class Device;

class Room : public BaseEntity {
private:
    double area;
    std::vector<std::shared_ptr<Device>> devices;

public:
    Room(const std::string& id, const std::string& roomName, double roomArea);
    Room(const Room& other);
    ~Room();

    Room& operator+=(std::shared_ptr<Device> device);
    Room& operator-=(std::shared_ptr<Device> device);
    std::shared_ptr<Device> operator[](size_t index) const;
    friend std::ostream& operator<<(std::ostream& os, const Room& room);

    void addDevice(std::shared_ptr<Device> device);
    void removeDevice(std::shared_ptr<Device> device);
    std::vector<std::shared_ptr<Device>> getDevices() const;
    double calculateRoomPowerConsumption() const;
    void displayDevices() const;
    double getArea() const;

    // Новые методы для поиска и сортировки
    std::shared_ptr<Device> findDeviceByName(const std::string& name) const;
    std::vector<std::shared_ptr<Device>> findDevicesByType(DeviceType type) const;
    void sortDevicesByPower(bool ascending = true);
    void sortDevicesByName();

    std::string serialize() const override;
    void displayInfo() const override;
    BaseEntity* clone() const override;

    static std::shared_ptr<Room> deserialize(const std::string& data);
};

#endif