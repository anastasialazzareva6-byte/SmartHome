#ifndef DEVICE_HPP
#define DEVICE_HPP

#include <string>
#include <memory>
#include <stdexcept>
#include "deviceType.hpp"
#include "BaseEntity.hpp"

class Room;

class Device : public BaseEntity {
protected:
    std::string manufacturer;
    DeviceType deviceType;
    std::shared_ptr<Room> location;
    bool isOn;
    bool isOnline;
    double powerConsumption;

    static int deviceCount;

public:
    Device(const std::string& id, const std::string& deviceName,
        const std::string& manuf, DeviceType type,
        std::shared_ptr<Room> room, double power = 0.0);

    // Запрещаем конструктор копирования
    Device(const Device& other) = delete;

    // Добавляем конструктор перемещения
    Device(Device&& other) noexcept;

    virtual ~Device();

    Device& operator=(const Device& other);

    // Добавляем оператор присваивания перемещением
    Device& operator=(Device&& other) noexcept;

    // Операторы
    Device operator+(double additionalPower) const;
    bool operator>(const Device& other) const;
    explicit operator std::string() const;

    // Виртуальные методы
    virtual void turnOn();
    virtual void turnOff();
    virtual std::string getStatus() const;

    virtual std::string getDetails() const override;

    virtual void performSelfCheck() const;
    virtual double calculateEfficiency() const;

    virtual Device* clone() const override;

    // Другие методы...
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

    std::string serialize() const override;
    void displayInfo() const override;

    static int getDeviceCount();
    friend void validateDevice(const Device& device);

    static std::shared_ptr<Device> deserialize(const std::string& data,
        std::shared_ptr<Room> location);
};

#endif