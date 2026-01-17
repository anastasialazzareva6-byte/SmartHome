#include "scenarioAction.hpp"
#include "device.hpp"
#include <iostream>
#include <sstream>
#include <memory>

ScenarioAction::ScenarioAction(const std::string& id, std::shared_ptr<Device> device, const std::string& cmd)
    : actionId(id), targetDevice(device), command(cmd) {}

ScenarioAction::~ScenarioAction() {
    parameters.clear();
}

void ScenarioAction::execute() {
    std::cout << "Выполнение действия: " << command << " на устройстве: ";
    if (targetDevice) {
        std::cout << targetDevice->getName() << std::endl;

        if (command == "turnOn") {
            targetDevice->turnOn();
        }
        else if (command == "turnOff") {
            targetDevice->turnOff();
        }
    }
    else {
        std::cout << "Устройство не найдено" << std::endl;
    }
}

void ScenarioAction::addParameter(const std::string& key, const std::string& value) {
    parameters[key] = value;
}

void ScenarioAction::displayInfo() const {
    std::cout << "Действие " << actionId << ": " << command;
    if (targetDevice) {
        std::cout << " на " << targetDevice->getName();
    }
    std::cout << std::endl;
}

std::string ScenarioAction::getActionId() const {
    return actionId;
}

std::string ScenarioAction::getCommand() const {
    return command;
}

std::shared_ptr<Device> ScenarioAction::getTargetDevice() const {
    return targetDevice;
}

std::string ScenarioAction::getDescription() const {
    std::string desc = command + " ";
    if (targetDevice) {
        desc += targetDevice->getName();
    }
    return desc;
}

std::string ScenarioAction::serialize() const {
    std::stringstream ss;
    ss << actionId << "|" << (targetDevice ? targetDevice->getId() : "NULL") << "|" << command;
    return ss.str();
}

std::unique_ptr<ScenarioAction> ScenarioAction::deserialize(const std::string& data, std::shared_ptr<Device> device) {
    std::stringstream ss(data);
    std::string id, deviceId, command;
    std::getline(ss, id, '|');
    std::getline(ss, deviceId, '|');
    std::getline(ss, command, '|');
    return std::make_unique<ScenarioAction>(id, device, command);
}