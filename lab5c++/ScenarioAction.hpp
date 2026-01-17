#ifndef SCENARIOACTION_HPP
#define SCENARIOACTION_HPP

#include <string>
#include <map>
#include <memory>

class Device;

class ScenarioAction {
private:
    std::string actionId;
    std::shared_ptr<Device> targetDevice;
    std::string command;
    std::map<std::string, std::string> parameters;

public:
    ScenarioAction(const std::string& id, std::shared_ptr<Device> device, const std::string& cmd);
    ~ScenarioAction();

    void execute();
    void addParameter(const std::string& key, const std::string& value);
    void displayInfo() const;

    std::string getActionId() const;
    std::string getCommand() const;
    std::shared_ptr<Device> getTargetDevice() const;
    std::string getDescription() const;

    // Методы для сериализации
    std::string serialize() const;
    static std::unique_ptr<ScenarioAction> deserialize(const std::string& data, std::shared_ptr<Device> device);
};

#endif