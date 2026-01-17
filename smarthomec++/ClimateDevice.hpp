#ifndef CLIMATEDEVICE_HPP
#define CLIMATEDEVICE_HPP

#include "device.hpp"
#include <string>
#include <memory>

class ClimateDevice : public Device {
private:
    double targetTemperature;
    double currentTemperature;
    double humidity;
    bool autoMode;

public:
    ClimateDevice(const std::string& id, const std::string& deviceName,
        const std::string& manuf, std::shared_ptr<Room> room,
        double power = 0.0, double targetTemp = 22.0);

    ClimateDevice(const ClimateDevice& other);
    ClimateDevice& operator=(const ClimateDevice& other);
    ClimateDevice& operator=(const Device& other);
    ~ClimateDevice();

    virtual void turnOn() override;
    virtual void turnOff() override;

    virtual std::string getDetails() const override;

    virtual void performSelfCheck() const override;
    virtual double calculateEfficiency() const override;

    virtual ClimateDevice* clone() const override;

    void setTargetTemperature(double temp);
    void setAutoMode(bool enabled);
    void adjustTemperature();
    double getTargetTemperature() const;
    double getCurrentTemperature() const;
    double getHumidity() const;
    bool isAutoMode() const;

    std::string serialize() const override;
    void displayInfo() const override;

    static std::shared_ptr<ClimateDevice> deserialize(const std::string& data,
        std::shared_ptr<Room> location);
};

#endif