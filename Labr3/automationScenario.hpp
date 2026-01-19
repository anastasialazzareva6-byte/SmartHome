#ifndef AUTOMATIONSCENARIO_HPP
#define AUTOMATIONSCENARIO_HPP

#include <string>
#include <vector>
#include <ctime>

class ScenarioAction;

class AutomationScenario {
private:
    std::string scenarioId;
    std::string name;
    std::string triggerTime;
    std::vector<ScenarioAction*> actions;
    bool isActive;
    std::time_t createdDate;

public:
    AutomationScenario(const std::string& id, const std::string& scenarioName,
        const std::string& time);
    ~AutomationScenario();

    void activate();
    void deactivate();
    void execute();
    void addAction(ScenarioAction* action);
    void removeAction(ScenarioAction* action);
    void displayInfo() const;
    void displayActions() const;

    std::string getScenarioId() const;
    std::string getName() const;
    std::string getTriggerTime() const;
    bool getIsActive() const;
    int getActionCount() const;

    // Методы для сериализации
    std::string serialize() const;
    static AutomationScenario* deserialize(const std::string& data);
};

#endif