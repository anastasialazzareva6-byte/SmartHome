#ifndef DEVICE_HPP
#define DEVICE_HPP

#include <string>
#include "deviceType.hpp"

class Room;

class Device {
private:
    std::string deviceId;
    std::string name;
    std::string manufacturer;
    DeviceType deviceType;
    Room* location;
    bool isOn;
    bool isOnline;
    double powerConsumption;

public:
    Device(const std::string& id, const std::string& deviceName,
        const std::string& manuf, DeviceType type, Room* room, double power = 0.0);
    ~Device();

    void turnOn();
    void turnOff();
    std::string getStatus() const;
    void updateCurrentValue(double value);
    std::string getDeviceTypeString() const;

    std::string getDeviceId() const;
    std::string getName() const;
    std::string getManufacturer() const;
    DeviceType getDeviceType() const;
    Room* getLocation() const;
    bool getIsOn() const;
    bool getIsOnline() const;
    double getPowerConsumption() const;
    void setPowerConsumption(double power);
    void setIsOnline(bool online);
    void setLocation(Room* newLocation);

    // Методы для сериализации
    std::string serialize() const;
    static Device* deserialize(const std::string& data, Room* location);
};

#endif