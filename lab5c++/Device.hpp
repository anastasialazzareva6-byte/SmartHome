#ifndef DEVICE_HPP
#define DEVICE_HPP

#include <string>
#include <memory>
#include <stdexcept>
#include "deviceType.hpp"
#include "BaseEntity.hpp"

class Room;

class Device : public BaseEntity {
private:
    std::string manufacturer;
    DeviceType deviceType;
    std::shared_ptr<Room> location;
    bool isOn;
    bool isOnline;
    double powerConsumption;

    static int deviceCount;  // Статическое поле

public:
    Device(const std::string& id, const std::string& deviceName,
        const std::string& manuf, DeviceType type, std::shared_ptr<Room> room, double power = 0.0);
    Device(const Device& other);  // Конструктор копирования
    ~Device();

    // Перегрузка операторов
    Device& operator=(const Device& other);
    Device operator+(double additionalPower) const;
    bool operator>(const Device& other) const;
    explicit operator std::string() const;

    void turnOn();
    void turnOff();
    std::string getStatus() const;
    void updateCurrentValue(double value);
    std::string getDeviceTypeString() const;

    std::string getManufacturer() const;
    DeviceType getDeviceType() const;
    std::shared_ptr<Room> getLocation() const;
    bool getIsOn() const;
    bool getIsOnline() const;
    double getPowerConsumption() const;
    void setPowerConsumption(double power);
    void setIsOnline(bool online);
    void setLocation(std::shared_ptr<Room> newLocation);

    // Методы для сериализации
    std::string serialize() const override;
    void displayInfo() const override;

    // Статический метод
    static int getDeviceCount();

    // Дружественная функция
    friend void validateDevice(const Device& device);

    static std::shared_ptr<Device> deserialize(const std::string& data, std::shared_ptr<Room> location);
};

#endif