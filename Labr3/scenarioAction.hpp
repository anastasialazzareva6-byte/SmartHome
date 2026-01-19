#ifndef SCENARIOACTION_HPP
#define SCENARIOACTION_HPP

#include <string>
#include <map>

class Device;

class ScenarioAction {
private:
    std::string actionId;
    Device* targetDevice;
    std::string command;
    std::map<std::string, std::string> parameters;

public:
    ScenarioAction(const std::string& id, Device* device, const std::string& cmd);
    ~ScenarioAction();

    void execute();
    void addParameter(const std::string& key, const std::string& value);
    void displayInfo() const;

    std::string getActionId() const;
    std::string getCommand() const;
    Device* getTargetDevice() const;
    std::string getDescription() const;

    // Методы для сериализации
    std::string serialize() const;
    static ScenarioAction* deserialize(const std::string& data, Device* device);
};

#endif