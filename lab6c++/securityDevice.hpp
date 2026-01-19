#ifndef SECURITYDEVICE_HPP
#define SECURITYDEVICE_HPP

#include "device.hpp"
#include <string>
#include <memory>
#include <vector>

class SecurityDevice : public Device {
private:
    bool isArmed;
    std::vector<std::string> accessCodes;
    int sensitivityLevel;
    bool motionDetected;

public:
    SecurityDevice(const std::string& id, const std::string& deviceName,
        const std::string& manuf, std::shared_ptr<Room> room,
        double power = 0.0);

    SecurityDevice(const SecurityDevice& other);

    ~SecurityDevice();

    virtual void turnOn() override;
    virtual void turnOff() override;

    virtual std::string getDetails() const override;

    virtual void performSelfCheck() const override;
    virtual double calculateEfficiency() const override;

    virtual SecurityDevice* clone() const override;

    void arm();
    void disarm();
    void addAccessCode(const std::string& code);
    bool checkAccessCode(const std::string& code) const;
    void setSensitivity(int level);
    void motionDetection(bool detected);

    bool getIsArmed() const;
    int getSensitivityLevel() const;
    bool getMotionDetected() const;

    std::string serialize() const override;
    void displayInfo() const override;

    static std::shared_ptr<SecurityDevice> deserialize(const std::string& data,
        std::shared_ptr<Room> location);
};

#endif