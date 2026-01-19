#include "automationScenario.hpp"
#include "scenarioAction.hpp"
#include <iostream>
#include <iomanip>
#include <sstream>

AutomationScenario::AutomationScenario(const std::string& id, const std::string& scenarioName,
    const std::string& time)
    : scenarioId(id), name(scenarioName), triggerTime(time),
    isActive(false), createdDate(std::time(nullptr)) {}

AutomationScenario::~AutomationScenario() {
    actions.clear();
}

void AutomationScenario::activate() {
    isActive = true;
    std::cout << "Сценарий '" << name << "' активирован" << std::endl;
}

void AutomationScenario::deactivate() {
    isActive = false;
    std::cout << "Сценарий '" << name << "' деактивирован" << std::endl;
}

void AutomationScenario::execute() {
    if (!isActive) {
        std::cout << "Сценарий '" << name << "' не активен" << std::endl;
        return;
    }
    std::cout << "Выполнение сценария: " << name << std::endl;
    for (auto& action : actions) {
        action->execute();
    }
}

void AutomationScenario::addAction(std::unique_ptr<ScenarioAction> action) {
    actions.push_back(std::move(action));
}

void AutomationScenario::removeAction(ScenarioAction* action) {
    for (auto it = actions.begin(); it != actions.end(); ++it) {
        if (it->get() == action) {
            actions.erase(it);
            break;
        }
    }
}

void AutomationScenario::displayInfo() const {
    std::cout << "Сценарий: " << name << " (" << (isActive ? "Активен" : "Неактивен") << ")\n";
    std::cout << "Время запуска: " << triggerTime << "\n";
    std::cout << "Действий: " << actions.size() << "\n";
}

void AutomationScenario::displayActions() const {
    std::cout << "Действия сценария '" << name << "':\n";
    for (size_t i = 0; i < actions.size(); i++) {
        std::cout << "  " << i + 1 << ". ";
        actions[i]->displayInfo();
    }
}

std::string AutomationScenario::getScenarioId() const {
    return scenarioId;
}

std::string AutomationScenario::getName() const {
    return name;
}

std::string AutomationScenario::getTriggerTime() const {
    return triggerTime;
}

bool AutomationScenario::getIsActive() const {
    return isActive;
}

int AutomationScenario::getActionCount() const {
    return actions.size();
}

std::string AutomationScenario::serialize() const {
    std::stringstream ss;
    ss << scenarioId << "|" << name << "|" << triggerTime << "|" << isActive << "|" << createdDate;
    return ss.str();
}

std::unique_ptr<AutomationScenario> AutomationScenario::deserialize(const std::string& data) {
    std::stringstream ss(data);
    std::string id, name, time, activeStr, dateStr;
    std::getline(ss, id, '|');
    std::getline(ss, name, '|');
    std::getline(ss, time, '|');
    std::getline(ss, activeStr, '|');
    std::getline(ss, dateStr, '|');
    auto scenario = std::make_unique<AutomationScenario>(id, name, time);
    if (activeStr == "1") scenario->activate();
    return scenario;
}