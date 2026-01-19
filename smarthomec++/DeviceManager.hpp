#ifndef DEVICEMANAGER_HPP
#define DEVICEMANAGER_HPP
#include <vector>
#include <memory>
#include <algorithm>
#include <iostream>
#include <functional>
#include <iterator>
#include "device.hpp"
#include "BaseEntity.hpp"

// Шаблонный класс для управления коллекцией устройств
template<typename T>
class DeviceManager {
private:
    std::vector<std::shared_ptr<T>> devices;
    std::string managerName;

public:
    DeviceManager(const std::string& name) : managerName(name) {}

    // Шаблонный метод для добавления устройства
    template<typename U>
    void addDevice(U&& device) {
        devices.push_back(std::forward<U>(device));
        // УБРАЛИ ВЫВОД: std::cout << "Устройство добавлено в менеджер: " << managerName << std::endl;
    }

    // Не шаблонный метод
    void removeDevice(const std::string& deviceId) {
        auto it = std::remove_if(devices.begin(), devices.end(),
            [&deviceId](const std::shared_ptr<T>& device) {
                return device->getId() == deviceId;
            });
        if (it != devices.end()) {
            devices.erase(it, devices.end());
            std::cout << "Устройство " << deviceId << " удалено" << std::endl;
        }
        else {
            std::cout << "Устройство не найдено" << std::endl;
        }
    }

    // Метод для сортировки устройств по потреблению (STL алгоритм sort)
    void sortByPowerConsumption() {
        std::sort(devices.begin(), devices.end(),
            [](const std::shared_ptr<T>& a, const std::shared_ptr<T>& b) {
                return a->getPowerConsumption() < b->getPowerConsumption();
            });
        // УБРАЛИ ВЫВОД: std::cout << "Устройства отсортированы по потреблению энергии" << std::endl;
    }

    // Метод для поиска устройства по имени (STL алгоритм find_if)
    std::shared_ptr<T> findDeviceByName(const std::string& name) const {
        auto it = std::find_if(devices.begin(), devices.end(),
            [&name](const std::shared_ptr<T>& device) {
                return device->getName() == name;
            });
        return (it != devices.end()) ? *it : nullptr;
    }

    // Метод для поиска устройств по типу (STL алгоритм copy_if)
    std::vector<std::shared_ptr<T>> findDevicesByType(DeviceType type) const {
        std::vector<std::shared_ptr<T>> result;
        std::copy_if(devices.begin(), devices.end(), std::back_inserter(result),
            [type](const std::shared_ptr<T>& device) {
                return device->getDeviceType() == type;
            });
        return result;
    }

    // Метод для получения устройства с максимальным потреблением (STL алгоритм max_element)
    std::shared_ptr<T> getMaxPowerDevice() const {
        if (devices.empty()) return nullptr;
        auto it = std::max_element(devices.begin(), devices.end(),
            [](const std::shared_ptr<T>& a, const std::shared_ptr<T>& b) {
                return a->getPowerConsumption() < b->getPowerConsumption();
            });
        return *it;
    }

    // Метод для получения устройства с минимальным потреблением (STL алгоритм min_element)
    std::shared_ptr<T> getMinPowerDevice() const {
        if (devices.empty()) return nullptr;
        auto it = std::min_element(devices.begin(), devices.end(),
            [](const std::shared_ptr<T>& a, const std::shared_ptr<T>& b) {
                return a->getPowerConsumption() < b->getPowerConsumption();
            });
        return *it;
    }

    // Метод для фильтрации включенных устройств (STL алгоритм copy_if)
    std::vector<std::shared_ptr<T>> getActiveDevices() const {
        std::vector<std::shared_ptr<T>> activeDevices;
        std::copy_if(devices.begin(), devices.end(), std::back_inserter(activeDevices),
            [](const std::shared_ptr<T>& device) {
                return device->getIsOn();
            });
        return activeDevices;
    }

    // Проверка наличия онлайн устройств (STL алгоритм any_of)
    bool hasOnlineDevices() const {
        return std::any_of(devices.begin(), devices.end(),
            [](const std::shared_ptr<T>& device) {
                return device->getIsOnline();
            });
    }

    // Удаление отключенных устройств (STL алгоритм remove_if)
    void removeOfflineDevices() {
        auto new_end = std::remove_if(devices.begin(), devices.end(),
            [](const std::shared_ptr<T>& device) {
                return !device->getIsOnline();
            });
        if (new_end != devices.end()) {
            devices.erase(new_end, devices.end());
            std::cout << "Офлайн устройства удалены" << std::endl;
        }
    }

    // Применение функции ко всем устройствам (STL алгоритм for_each) - ДОБАВЛЕНО
    template<typename Func>
    void forEachDevice(Func func) const {
        std::for_each(devices.begin(), devices.end(), func);
    }

    // Поиск устройства по производителю (STL алгоритм find_if)
    std::shared_ptr<T> findDeviceByManufacturer(const std::string& manufacturer) const {
        auto it = std::find_if(devices.begin(), devices.end(),
            [&manufacturer](const std::shared_ptr<T>& device) {
                return device->getManufacturer() == manufacturer;
            });
        return (it != devices.end()) ? *it : nullptr;
    }

    // Подсчет устройств определенного типа (STL алгоритм count_if)
    int countDevicesByType(DeviceType type) const {
        return std::count_if(devices.begin(), devices.end(),
            [type](const std::shared_ptr<T>& device) {
                return device->getDeviceType() == type;
            });
    }

    // Вывод информации о всех устройствах
    void displayAllDevices() const {
        std::cout << "\n=== Устройства в менеджере: " << managerName << " ===" << std::endl;
        for (size_t i = 0; i < devices.size(); ++i) {
            std::cout << i + 1 << ". ";
            devices[i]->displayInfo();
            std::cout << "---" << std::endl;
        }
        std::cout << "Всего устройств: " << devices.size() << std::endl;
    }

    // Вывод краткой информации об устройствах
    void displayDevicesSummary() const {
        std::cout << "\n=== Краткая информация об устройствах ===" << std::endl;
        this->forEachDevice([](const std::shared_ptr<T>& device) {
            std::cout << "  - " << device->getName() << ": "
                << device->getPowerConsumption() << " Вт ("
                << device->getStatus() << ")" << std::endl;
            });
    }

    // Получение общего потребления энергии
    double getTotalPowerConsumption() const {
        double total = 0.0;
        for (const auto& device : devices) {
            if (device->getIsOn()) {
                total += device->getPowerConsumption();
            }
        }
        return total;
    }

    // Получение среднего потребления
    double getAveragePowerConsumption() const {
        if (devices.empty()) return 0.0;
        double total = 0.0;
        for (const auto& device : devices) {
            total += device->getPowerConsumption();
        }
        return total / devices.size();
    }

    // Получение количества устройств
    size_t getDeviceCount() const {
        return devices.size();
    }

    // Получение всех устройств
    const std::vector<std::shared_ptr<T>>& getAllDevices() const {
        return devices;
    }

    // Получение имени менеджера
    std::string getManagerName() const {
        return managerName;
    }

    // Проверка пустоты
    bool isEmpty() const {
        return devices.empty();
    }

    // Очистка всех устройств
    void clear() {
        devices.clear();
        std::cout << "Все устройства удалены из менеджера" << std::endl;
    }

    // Получение устройства по индексу
    std::shared_ptr<T> getDeviceByIndex(size_t index) const {
        if (index < devices.size()) {
            return devices[index];
        }
        return nullptr;
    }
};

#endif