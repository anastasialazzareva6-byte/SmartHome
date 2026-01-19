#include <iostream>
#include <vector>
#include <ctime>
#include <limits>
#include <locale>
#include <windows.h>
#include <fstream>
#include <sstream>
#include <conio.h>
#include <memory>
#include <algorithm>
#include <stdexcept>
#include <functional>
#include "room.hpp"
#include "device.hpp"
#include "climateDevice.hpp"
#include "securityDevice.hpp"
#include "deviceType.hpp"
#include "user.hpp"
#include "accessLevel.hpp"
#include "scenarioAction.hpp"
#include "automationScenario.hpp"
#include "notification.hpp"
#include "notificationType.hpp"
#include "energyReport.hpp"
#include "activity.hpp"
using namespace std;

void setRussianLocale() {
    SetConsoleCP(1251);
    SetConsoleOutputCP(1251);
    setlocale(LC_ALL, "Russian");
}

// Функции для сортировки
bool compareByPowerAsc(const shared_ptr<Device>& a, const shared_ptr<Device>& b) {
    return a->getPowerConsumption() < b->getPowerConsumption();
}

bool compareByPowerDesc(const shared_ptr<Device>& a, const shared_ptr<Device>& b) {
    return a->getPowerConsumption() > b->getPowerConsumption();
}

bool compareByNameAsc(const shared_ptr<Device>& a, const shared_ptr<Device>& b) {
    return a->getName() < b->getName();
}

// Функция-предикат для поиска устройств в комнате
bool deviceInRoom(const shared_ptr<Device>& device, const string& roomName) {
    auto location = device->getLocation();
    return location && location->getName() == roomName;
}

// Функция-предикат для поиска по производителю
bool deviceByManufacturer(const shared_ptr<Device>& device, const string& manufacturer) {
    return device->getManufacturer() == manufacturer;
}

class DataManager {
private:
    static const string ROOMS_FILE;
    static const string DEVICES_FILE;
    static const string USERS_FILE;
    static const string SCENARIOS_FILE;
    static const string NOTIFICATIONS_FILE;

public:
    static void saveRooms(const vector<shared_ptr<Room>>& rooms) {
        ofstream file(ROOMS_FILE);
        if (file.is_open()) {
            for (const auto& room : rooms) {
                file << room->serialize() << endl;
            }
            file.close();
        }
    }

    static vector<shared_ptr<Room>> loadRooms() {
        vector<shared_ptr<Room>> rooms;
        ifstream file(ROOMS_FILE);
        if (file.is_open()) {
            string line;
            while (getline(file, line)) {
                if (!line.empty()) {
                    auto room = Room::deserialize(line);
                    if (room) {
                        rooms.push_back(room);
                    }
                }
            }
            file.close();
        }
        return rooms;
    }

    static void saveDevices(const vector<shared_ptr<Device>>& devices) {
        ofstream file(DEVICES_FILE);
        if (file.is_open()) {
            for (const auto& device : devices) {
                file << device->serialize() << endl;
            }
            file.close();
        }
    }

    static vector<shared_ptr<Device>> loadDevices(const vector<shared_ptr<Room>>& rooms) {
        vector<shared_ptr<Device>> devices;
        ifstream file(DEVICES_FILE);
        if (file.is_open()) {
            string line;
            while (getline(file, line)) {
                if (!line.empty()) {
                    stringstream ss(line);
                    string temp, locationId;
                    getline(ss, temp, '|');
                    getline(ss, temp, '|');
                    getline(ss, temp, '|');
                    getline(ss, temp, '|');
                    getline(ss, locationId, '|');

                    shared_ptr<Room> location = nullptr;
                    for (auto& room : rooms) {
                        if (room->getId() == locationId) {
                            location = room;
                            break;
                        }
                    }

                    if (location) {
                        auto device = Device::deserialize(line, location);
                        if (device) {
                            devices.push_back(device);
                            location->addDevice(device);
                        }
                    }
                }
            }
            file.close();
        }
        return devices;
    }

    static void saveUsers(const vector<shared_ptr<User>>& users) {
        ofstream file(USERS_FILE);
        if (file.is_open()) {
            for (const auto& user : users) {
                file << user->serialize() << endl;
            }
            file.close();
        }
    }

    static vector<shared_ptr<User>> loadUsers() {
        vector<shared_ptr<User>> users;
        ifstream file(USERS_FILE);
        if (file.is_open()) {
            string line;
            while (getline(file, line)) {
                if (!line.empty()) {
                    auto user = User::deserialize(line);
                    if (user) {
                        users.push_back(user);
                    }
                }
            }
            file.close();
        }
        return users;
    }

    static void saveScenarios(const vector<unique_ptr<AutomationScenario>>& scenarios) {
        ofstream file(SCENARIOS_FILE);
        if (file.is_open()) {
            for (const auto& scenario : scenarios) {
                file << scenario->serialize() << endl;
            }
            file.close();
        }
    }

    static vector<unique_ptr<AutomationScenario>> loadScenarios() {
        vector<unique_ptr<AutomationScenario>> scenarios;
        ifstream file(SCENARIOS_FILE);
        if (file.is_open()) {
            string line;
            while (getline(file, line)) {
                if (!line.empty()) {
                    scenarios.push_back(AutomationScenario::deserialize(line));
                }
            }
            file.close();
        }
        return scenarios;
    }

    static void saveNotifications(const vector<unique_ptr<Notification>>& notifications) {
        ofstream file(NOTIFICATIONS_FILE);
        if (file.is_open()) {
            for (const auto& notification : notifications) {
                file << notification->serialize() << endl;
            }
            file.close();
        }
    }

    static vector<unique_ptr<Notification>> loadNotifications() {
        vector<unique_ptr<Notification>> notifications;
        ifstream file(NOTIFICATIONS_FILE);
        if (file.is_open()) {
            string line;
            while (getline(file, line)) {
                if (!line.empty()) {
                    notifications.push_back(Notification::deserialize(line));
                }
            }
            file.close();
        }
        return notifications;
    }

    static void initializeDefaultData(vector<shared_ptr<Room>>& rooms,
        vector<shared_ptr<Device>>& devices,
        vector<shared_ptr<User>>& users,
        vector<unique_ptr<Notification>>& notifications,
        vector<unique_ptr<AutomationScenario>>& scenarios) {
        try {
            // Полностью пустая инициализация - не создаем НИЧЕГО
            cout << "Система инициализирована без данных." << endl;
            cout << "Для использования системы:" << endl;
            cout << "1. Добавьте пользователей через меню 'Управление пользователями'" << endl;
            cout << "2. Добавьте комнаты через меню 'Управление комнатами'" << endl;
            cout << "3. Добавьте устройства через меню 'Управление устройствами'" << endl;
        }
        catch (const exception& e) {
            cerr << "Ошибка инициализации данных: " << e.what() << endl;
        }
    }
};

const string DataManager::ROOMS_FILE = "rooms.dat";
const string DataManager::DEVICES_FILE = "devices.dat";
const string DataManager::USERS_FILE = "users.dat";
const string DataManager::SCENARIOS_FILE = "scenarios.dat";
const string DataManager::NOTIFICATIONS_FILE = "notifications.dat";

class SmartHomeSystem {
private:
    vector<shared_ptr<Room>> rooms;
    vector<shared_ptr<Device>> devices;
    vector<shared_ptr<User>> users;
    vector<unique_ptr<AutomationScenario>> scenarios;
    vector<unique_ptr<Notification>> notifications;
    vector<unique_ptr<EnergyReport>> reports;
    shared_ptr<User> currentUser;

    void displayScenariosMenu() {
        cout << "\n=== СЦЕНАРИИ АВТОМАТИЗАЦИИ ===" << endl;
        cout << "1. Список сценариев" << endl;
        cout << "2. Создать сценарий" << endl;
        cout << "3. Выполнить сценарий" << endl;
        cout << "4. Показать действия сценария" << endl;
        cout << "5. Добавить действие в сценарий" << endl;
        cout << "6. Назад в главное меню" << endl;
        cout << "Выберите опцию: ";
    }

    void displayUsersMenu() {
        cout << "\n=== УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ (АДМИН) ===" << endl;
        cout << "1. Список пользователей" << endl;
        cout << "2. Добавить пользователя" << endl;
        cout << "3. Удалить пользователя" << endl;
        cout << "4. Изменить права доступа" << endl;
        cout << "5. Назад в главное меню" << endl;
        cout << "Выберите опцию: ";
    }

    void displayRoomsMenu() {
        cout << "\n=== УПРАВЛЕНИЕ КОМНАТАМИ ===" << endl;
        cout << "1. Список всех комнат" << endl;
        cout << "2. Показать устройства в комнате" << endl;
        cout << "3. Потребление энергии комнаты" << endl;
        if (currentUser && currentUser->getAccessLevel() == AccessLevel::ADMIN) {
            cout << "4. Добавить комнату" << endl;
            cout << "5. Удалить комнату" << endl;
            cout << "6. Назад в главное меню" << endl;
        }
        else {
            cout << "4. Назад в главное меню" << endl;
        }
        cout << "Выберите опцию: ";
    }

    void displayDevicesMenu() {
        cout << "\n=== УПРАВЛЕНИЕ УСТРОЙСТВАМИ ===" << endl;
        cout << "1. Список всех устройств" << endl;
        cout << "2. Включить/Выключить устройство" << endl;
        if (currentUser && currentUser->getAccessLevel() == AccessLevel::ADMIN) {
            cout << "3. Добавить устройство" << endl;
            cout << "4. Удалить устройство" << endl;
            cout << "5. Назад в главное меню" << endl;
        }
        else {
            cout << "3. Назад в главное меню" << endl;
        }
        cout << "Выберите опцию: ";
    }

    void displaySearchMenu() {
        cout << "\n=== СОРТИРОВКА И ПОИСК УСТРОЙСТВ ===" << endl;
        cout << "1. Сортировка устройств по потреблению (возрастание)" << endl;
        cout << "2. Сортировка устройств по потреблению (убывание)" << endl;
        cout << "3. Сортировка устройств по названию" << endl;
        cout << "4. Поиск устройств в комнате" << endl;
        cout << "5. Поиск устройств по производителю" << endl;
        cout << "6. Поиск устройства по названию" << endl;
        cout << "7. Поиск устройств с потреблением выше порога" << endl;
        cout << "8. Назад в главное меню" << endl;
        cout << "Выберите опцию: ";
    }

    void displayNotificationsMenu() {
        cout << "\n=== УВЕДОМЛЕНИЯ ===" << endl;
        cout << "1. Показать все уведомления" << endl;
        cout << "2. Показать непрочитанные" << endl;
        cout << "3. Отметить как прочитанное" << endl;
        cout << "4. Назад в главное меню" << endl;
        cout << "Выберите опцию: ";
    }

public:
    SmartHomeSystem() : currentUser(nullptr) {
        loadData();
    }

    ~SmartHomeSystem() {
        saveData();
        cleanup();
    }

    void loadData() {
        cout << "Загрузка данных..." << endl;

        try {
            auto loadedRooms = DataManager::loadRooms();
            for (auto& room : loadedRooms) {
                rooms.push_back(move(room));
            }

            devices = DataManager::loadDevices(rooms);

            auto loadedUsers = DataManager::loadUsers();
            for (auto& user : loadedUsers) {
                users.push_back(move(user));
            }

            auto loadedScenarios = DataManager::loadScenarios();
            for (auto& scenario : loadedScenarios) {
                scenarios.push_back(move(scenario));
            }

            auto loadedNotifications = DataManager::loadNotifications();
            for (auto& notification : loadedNotifications) {
                notifications.push_back(move(notification));
            }

            // Если все файлы пустые, это первый запуск
            if (users.empty() && rooms.empty() && devices.empty()) {
                // Первый запуск - ничего не делаем, не создаем файлы
                // Не вызываем initializeDefaultData вообще
            }
            else {
                // Данные загружены из файлов
                cout << "Данные загружены из файлов." << endl;
            }
        }
        catch (const exception& e) {
            cerr << "Ошибка загрузки данных: " << e.what() << endl;
        }
    }

    void saveData() {
        cout << "Сохранение данных..." << endl;
        DataManager::saveRooms(rooms);
        DataManager::saveDevices(devices);
        DataManager::saveUsers(users);
        DataManager::saveScenarios(scenarios);
        DataManager::saveNotifications(notifications);
        cout << "Данные сохранены!" << endl;
    }

    void cleanup() {
        rooms.clear();
        devices.clear();
        users.clear();
        scenarios.clear();
        notifications.clear();
        reports.clear();
    }

    bool hasAdminAccess() const {
        if (!currentUser) {
            cout << "Ошибка: Необходимо войти в систему!" << endl;
            return false;
        }
        if (currentUser->getAccessLevel() != AccessLevel::ADMIN) {
            cout << "Ошибка: Недостаточно прав! Требуются права администратора." << endl;
            return false;
        }
        return true;
    }

    bool hasUserAccess() const {
        if (!currentUser) {
            cout << "Ошибка: Необходимо войти в систему!" << endl;
            return false;
        }
        return true;
    }

    void displayMainMenu() {
        cout << "\n=== УМНЫЙ ДОМ - ГЛАВНОЕ МЕНЮ ===" << endl;
        if (currentUser) {
            cout << "Текущий пользователь: " << currentUser->getUsername();
            cout << " (" << (currentUser->getAccessLevel() == AccessLevel::ADMIN ?
                "Администратор" : "Пользователь") << ")" << endl;
        }
        cout << "1. Управление устройствами" << endl;
        cout << "2. Управление комнатами" << endl;
        cout << "3. Управление пользователями" << (currentUser && currentUser->getAccessLevel() == AccessLevel::ADMIN ? " (Админ)" : "") << endl;
        cout << "4. Сценарии автоматизации" << endl;
        cout << "5. Уведомления" << endl;
        cout << "6. Энергоотчеты" << endl;
        cout << "7. Статус системы" << endl;
        cout << "8. Сортировка и поиск устройств" << endl;
        cout << "9. Сохранить данные" << endl;
        cout << "0. Выход" << endl;
        cout << "Выберите опцию: ";
    }

    // НОВЫЙ РАЗДЕЛ: ФУНКЦИИ СОРТИРОВКИ И ПОИСКА

    void sortDevicesByPowerAsc() {
        vector<shared_ptr<Device>> sortedDevices = devices;
        sort(sortedDevices.begin(), sortedDevices.end(), compareByPowerAsc);

        cout << "\n=== УСТРОЙСТВА ОТСОРТИРОВАНЫ ПО ПОТРЕБЛЕНИЮ (ВОЗРАСТАНИЕ) ===" << endl;
        for (size_t i = 0; i < sortedDevices.size(); i++) {
            cout << i + 1 << ". " << sortedDevices[i]->getName()
                << " - " << sortedDevices[i]->getPowerConsumption() << " Вт"
                << " - Комната: " << (sortedDevices[i]->getLocation() ?
                    sortedDevices[i]->getLocation()->getName() : "Нет") << endl;
        }
    }

    void sortDevicesByPowerDesc() {
        vector<shared_ptr<Device>> sortedDevices = devices;
        sort(sortedDevices.begin(), sortedDevices.end(), compareByPowerDesc);

        cout << "\n=== УСТРОЙСТВА ОТСОРТИРОВАНЫ ПО ПОТРЕБЛЕНИЮ (УБЫВАНИЕ) ===" << endl;
        for (size_t i = 0; i < sortedDevices.size(); i++) {
            cout << i + 1 << ". " << sortedDevices[i]->getName()
                << " - " << sortedDevices[i]->getPowerConsumption() << " Вт"
                << " - Комната: " << (sortedDevices[i]->getLocation() ?
                    sortedDevices[i]->getLocation()->getName() : "Нет") << endl;
        }
    }

    void sortDevicesByName() {
        vector<shared_ptr<Device>> sortedDevices = devices;
        sort(sortedDevices.begin(), sortedDevices.end(), compareByNameAsc);

        cout << "\n=== УСТРОЙСТВА ОТСОРТИРОВАНЫ ПО НАЗВАНИЮ ===" << endl;
        for (size_t i = 0; i < sortedDevices.size(); i++) {
            cout << i + 1 << ". " << sortedDevices[i]->getName()
                << " - " << sortedDevices[i]->getPowerConsumption() << " Вт"
                << " - Комната: " << (sortedDevices[i]->getLocation() ?
                    sortedDevices[i]->getLocation()->getName() : "Нет") << endl;
        }
    }


    void findDevicesByManufacturer() {
        cout << "Введите название производителя для поиска: ";
        string manufacturer;
        cin.ignore();
        getline(cin, manufacturer);

        cout << "\n=== УСТРОЙСТВА ПРОИЗВОДИТЕЛЯ: " << manufacturer << " ===" << endl;

        // Использование std::find_if 
        vector<shared_ptr<Device>> foundDevices;
        for (const auto& device : devices) {
            if (device->getManufacturer() == manufacturer) {
                foundDevices.push_back(device);
            }
        }

        if (foundDevices.empty()) {
            cout << "Устройства не найдены для производителя: " << manufacturer << endl;
        }
        else {
            sort(foundDevices.begin(), foundDevices.end(), compareByNameAsc);
            for (size_t i = 0; i < foundDevices.size(); i++) {
                cout << i + 1 << ". " << foundDevices[i]->getName()
                    << " - " << foundDevices[i]->getPowerConsumption() << " Вт"
                    << " - Комната: " << (foundDevices[i]->getLocation() ?
                        foundDevices[i]->getLocation()->getName() : "Нет") << endl;
            }
        }
    }

    void findDeviceByName() {
        cout << "Введите название устройства для поиска: ";
        string deviceName;
        cin.ignore();
        getline(cin, deviceName);

        // Использование std::find_if с пользовательским предикатором
        auto it = find_if(devices.begin(), devices.end(),
            [&deviceName](const shared_ptr<Device>& device) {
                return device->getName() == deviceName;
            });

        if (it != devices.end()) {
            cout << "\n=== УСТРОЙСТВО НАЙДЕНО ===" << endl;
            (*it)->displayInfo();
            cout << "Комната: " << ((*it)->getLocation() ? (*it)->getLocation()->getName() : "Нет") << endl;
            cout << "Производитель: " << (*it)->getManufacturer() << endl;
            cout << "Потребление: " << (*it)->getPowerConsumption() << " Вт" << endl;
        }
        else {
            cout << "Устройство с названием '" << deviceName << "' не найдено." << endl;
        }
    }

    void findDevicesByPowerThreshold() {
        cout << "Введите минимальное потребление (Вт): ";
        double threshold;
        cin >> threshold;

        cout << "\n=== УСТРОЙСТВА С ПОТРЕБЛЕНИЕМ > " << threshold << " Вт ===" << endl;

        // Использование std::find_if 
        vector<shared_ptr<Device>> highPowerDevices;
        copy_if(devices.begin(), devices.end(), back_inserter(highPowerDevices),
            [threshold](const shared_ptr<Device>& device) {
                return device->getPowerConsumption() > threshold;
            });

        if (highPowerDevices.empty()) {
            cout << "Устройства с потреблением выше " << threshold << " Вт не найдены." << endl;
        }
        else {
            // Сортировка по потреблению
            sort(highPowerDevices.begin(), highPowerDevices.end(), compareByPowerDesc);

            for (size_t i = 0; i < highPowerDevices.size(); i++) {
                cout << i + 1 << ". " << highPowerDevices[i]->getName()
                    << " - " << highPowerDevices[i]->getPowerConsumption() << " Вт"
                    << " - Комната: " << (highPowerDevices[i]->getLocation() ?
                        highPowerDevices[i]->getLocation()->getName() : "Нет") << endl;
            }
        }
    }

    void listAllDevices() {
        cout << "\n=== ВСЕ УСТРОЙСТВА ===" << endl;
        for (size_t i = 0; i < devices.size(); i++) {
            cout << i + 1 << ". " << devices[i]->getName()
                << " (" << devices[i]->getDeviceTypeString() << ")"
                << " - " << devices[i]->getStatus()
                << " - " << devices[i]->getPowerConsumption() << " Вт"
                << " - Комната: " << (devices[i]->getLocation() ? devices[i]->getLocation()->getName() : "Нет") << endl;
        }
    }

    void toggleDevice() {
        listAllDevices();
        cout << "Выберите номер устройства: ";
        int choice;
        cin >> choice;

        if (choice > 0 && choice <= devices.size()) {
            auto& device = devices[choice - 1];
            if (device->getIsOn()) {
                device->turnOff();
                cout << device->getName() << " выключен" << endl;
            }
            else {
                device->turnOn();
                cout << device->getName() << " включен" << endl;
            }
            saveData();
        }
        else {
            cout << "Неверный номер устройства!" << endl;
        }
    }

    void addDevice() {
        if (!hasAdminAccess()) return;

        if (rooms.empty()) {
            cout << "Нельзя добавить устройство: в системе нет комнат." << endl;
            cout << "Сначала добавьте комнату через меню 'Управление комнатами'." << endl;
            return;
        }

        cout << "Добавление нового устройства:" << endl;
        if (!hasAdminAccess()) return;

        cout << "Добавление нового устройства:" << endl;
        cout << "Тип устройства (1-Базовое, 2-Климатическое, 3-Безопасности): ";
        int typeChoice;
        cin >> typeChoice;

        cout << "Название: ";
        string name;
        cin.ignore();
        getline(cin, name);

        cout << "Производитель: ";
        string manufacturer;
        getline(cin, manufacturer);

        cout << "Энергопотребление (Вт): ";
        double power;
        cin >> power;

        listAllRooms();
        cout << "Выберите номер комнаты: ";
        int roomChoice;
        cin >> roomChoice;

        if (roomChoice > 0 && roomChoice <= rooms.size()) {
            auto room = rooms[roomChoice - 1];
            shared_ptr<Device> newDevice;

            switch (typeChoice) {
            case 1: {
                cout << "Тип базового устройства (1-Датчик, 2-Исполнительное, 3-Климат, 4-Безопасность, 5-Мультимедиа): ";
                int deviceTypeChoice;
                cin >> deviceTypeChoice;
                DeviceType type = static_cast<DeviceType>(deviceTypeChoice - 1);

                newDevice = make_shared<Device>(
                    "DEV" + to_string(devices.size() + 1),
                    name,
                    manufacturer,
                    type,
                    room,
                    power
                );
                break;
            }
            case 2: {
                cout << "Целевая температура: ";
                double targetTemp;
                cin >> targetTemp;

                newDevice = make_shared<ClimateDevice>(
                    "CLIM" + to_string(devices.size() + 1),
                    name,
                    manufacturer,
                    room,
                    power,
                    targetTemp
                );
                break;
            }
            case 3: {
                newDevice = make_shared<SecurityDevice>(
                    "SEC" + to_string(devices.size() + 1),
                    name,
                    manufacturer,
                    room,
                    power
                );
                break;
            }
            default:
                cout << "Неверный тип устройства!" << endl;
                return;
            }

            devices.push_back(newDevice);
            room->addDevice(newDevice);
            cout << "Устройство добавлено в комнату '" << room->getName() << "'!" << endl;
            saveData();
        }
        else {
            cout << "Неверный номер комнаты!" << endl;
        }
    }

    void deleteDevice() {
        if (!hasAdminAccess()) return;

        listAllDevices();
        cout << "Выберите номер устройства для удаления: ";
        int choice;
        cin >> choice;

        if (choice > 0 && choice <= devices.size()) {
            auto device = devices[choice - 1];
            if (device->getLocation()) {
                device->getLocation()->removeDevice(device);
            }
            devices.erase(devices.begin() + choice - 1);
            cout << "Устройство удалено!" << endl;
            saveData();
        }
        else {
            cout << "Неверный номер устройства!" << endl;
        }
    }

    void listAllRooms() {
        cout << "\n=== ВСЕ КОМНАТЫ ===" << endl;
        for (size_t i = 0; i < rooms.size(); i++) {
            cout << i + 1 << ". " << rooms[i]->getName()
                << " (" << rooms[i]->getArea() << " м^2)"
                << " - Устройств: " << rooms[i]->getDevices().size() << endl;
        }
    }

    void showRoomDevices() {
        if (rooms.empty()) {
            cout << "В системе нет комнат. Сначала добавьте комнату." << endl;
            return;
        }

        listAllRooms();
        cout << "Выберите номер комнаты: ";
        int choice;
        cin >> choice;

        if (choice > 0 && choice <= rooms.size()) {
            rooms[choice - 1]->displayDevices();
        }
        else {
            cout << "Неверный номер комнаты!" << endl;
        }
    }

    void findDevicesInRoom() {
        if (rooms.empty()) {
            cout << "В системе нет комнат. Сначала добавьте комнату через меню 'Управление комнатами'." << endl;
            return;
        }

        cout << "\n=== ПОИСК УСТРОЙСТВ В КОМНАТЕ ===" << endl;

        // Показать список комнат с номерами
        cout << "Доступные комнаты:" << endl;
        for (size_t i = 0; i < rooms.size(); i++) {
            auto roomDevices = rooms[i]->getDevices();
            int activeDevices = 0;
            for (const auto& device : roomDevices) {
                if (device->getIsOn()) {
                    activeDevices++;
                }
            }

            cout << i + 1 << ". " << rooms[i]->getName()
                << " (" << rooms[i]->getArea() << " м^2)"
                << " - Устройств: " << roomDevices.size()
                << " (включено: " << activeDevices << ")" << endl;
        }
        listAllRooms();
        cout << "Выберите номер комнаты: ";
        int choice;
        cin >> choice;

        if (choice > 0 && choice <= rooms.size()) {
            rooms[choice - 1]->displayDevices();
        }
        else {
            cout << "Неверный номер комнаты!" << endl;
        }
    }

    void showRoomPower() {
        listAllRooms();
        cout << "Выберите номер комнаты: ";
        int choice;
        cin >> choice;

        if (choice > 0 && choice <= rooms.size()) {
            double power = rooms[choice - 1]->calculateRoomPowerConsumption();
            cout << "Текущее потребление энергии: " << power << " Вт" << endl;

            auto roomDevices = rooms[choice - 1]->getDevices();
            int activeDevices = 0;
            for (auto& device : roomDevices) {
                if (device->getIsOn()) {
                    activeDevices++;
                    cout << "  - " << device->getName() << ": " << device->getPowerConsumption() << " Вт" << endl;
                }
            }
            cout << "Включено устройств: " << activeDevices << " из " << roomDevices.size() << endl;
        }
        else {
            cout << "Неверный номер комнаты!" << endl;
        }
    }

    void addRoom() {
        if (!hasAdminAccess()) return;

        cout << "Добавление новой комнаты:" << endl;
        cout << "Название: ";
        string name;
        cin.ignore();
        getline(cin, name);

        cout << "Площадь (м^2): ";
        double area;
        cin >> area;

        auto newRoom = make_shared<Room>(
            "ROOM" + to_string(rooms.size() + 1),
            name,
            area
        );
        rooms.push_back(newRoom);
        cout << "Комната добавлена!" << endl;
        saveData();
    }

    void deleteRoom() {
        if (!hasAdminAccess()) return;

        listAllRooms();
        cout << "Выберите номер комнаты для удаления: ";
        int choice;
        cin >> choice;

        if (choice > 0 && choice <= rooms.size()) {
            if (!rooms[choice - 1]->getDevices().empty()) {
                cout << "Ошибка: В комнате есть устройства! Сначала удалите или переместите их." << endl;
                return;
            }

            rooms.erase(rooms.begin() + choice - 1);
            cout << "Комната удалена!" << endl;
            saveData();
        }
        else {
            cout << "Неверный номер комнаты!" << endl;
        }
    }

    void listUsers() {
        cout << "\n=== ВСЕ ПОЛЬЗОВАТЕЛИ ===" << endl;
        for (size_t i = 0; i < users.size(); i++) {
            cout << i + 1 << ". ";
            users[i]->displayInfo();
            cout << "---" << endl;
        }
    }

    void addUser() {
        cout << "Добавление нового пользователя:" << endl;
        cout << "Логин: ";
        string username;
        cin >> username;
        cout << "Пароль: ";
        string password;
        cin >> password;
        cout << "Уровень доступа (1 - User, 2 - Admin): ";
        int level;
        cin >> level;

        auto newUser = make_shared<User>(
            "USR" + to_string(users.size() + 1),
            username,
            password,
            (level == 2 ? AccessLevel::ADMIN : AccessLevel::USER),
            "",
            ""
        );
        users.push_back(newUser);
        cout << "Пользователь добавлен!" << endl;
        saveData();
    }

    void deleteUser() {
        listUsers();
        cout << "Выберите номер пользователя для удаления: ";
        int choice;
        cin >> choice;

        if (choice > 0 && choice <= users.size()) {
            if (users[choice - 1] == currentUser) {
                cout << "Ошибка: Нельзя удалить текущего пользователя!" << endl;
                return;
            }

            users.erase(users.begin() + choice - 1);
            cout << "Пользователь удален!" << endl;
            saveData();
        }
        else {
            cout << "Неверный номер пользователя!" << endl;
        }
    }

    void changeUserAccess() {
        listUsers();
        cout << "Выберите номер пользователя: ";
        int choice;
        cin >> choice;

        if (choice > 0 && choice <= users.size()) {
            cout << "Новый уровень доступа (1 - User, 2 - Admin): ";
            int level;
            cin >> level;

            users[choice - 1]->setAccessLevel(level == 2 ? AccessLevel::ADMIN : AccessLevel::USER);
            cout << "Права доступа изменены!" << endl;
            saveData();
        }
        else {
            cout << "Неверный номер пользователя!" << endl;
        }
    }

    void showAllNotifications() {
        cout << "\n=== ВСЕ УВЕДОМЛЕНИЯ ===" << endl;
        for (size_t i = 0; i < notifications.size(); i++) {
            cout << i + 1 << ". ";
            notifications[i]->displayInfo();
        }
    }

    void showUnreadNotifications() {
        cout << "\n=== НЕПРОЧИТАННЫЕ УВЕДОМЛЕНИЯ ===" << endl;
        bool hasUnread = false;
        for (size_t i = 0; i < notifications.size(); i++) {
            if (!notifications[i]->getIsRead()) {
                cout << i + 1 << ". ";
                notifications[i]->displayInfo();
                hasUnread = true;
            }
        }
        if (!hasUnread) {
            cout << "Нет непрочитанных уведомлений." << endl;
        }
    }

    void markNotificationAsRead() {
        showAllNotifications();
        cout << "Выберите номер уведомления: ";
        int choice;
        cin >> choice;

        if (choice > 0 && choice <= notifications.size()) {
            notifications[choice - 1]->markAsRead();
            saveData();
        }
        else {
            cout << "Неверный номер уведомления!" << endl;
        }
    }

    void createEnergyReport() {
        auto report = make_unique<EnergyReport>("ОТЧЕТ" + to_string(reports.size() + 1),
            time(nullptr) - 86400, time(nullptr));

        for (auto& device : devices) {
            if (device->getIsOn()) {
                report->addDeviceConsumption(device, device->getPowerConsumption() * 24);
            }
        }

        report->generateReport();
        reports.push_back(move(report));
        reports.back()->displayReport();
    }


    void login() {
        cout << "\n=== ВХОД В СИСТЕМУ ===" << endl;
        cout << "Имя пользователя: ";
        string username;
        cin >> username;

        cout << "Пароль: ";
        string password;
        cin >> password;

        for (auto& user : users) {
            if (user->getUsername() == username && user->login(password)) {
                currentUser = user;
                cout << "Вход выполнен успешно! Добро пожаловать, " << username << "!" << endl;
                return;
            }
        }
        cout << "Ошибка входа!" << endl;
    }

    void run() {
        // Если нет пользователей, предлагаем создать первого
        if (users.empty()) {
            cout << "\n=== РЕГИСТРАЦИЯ ПЕРВОГО ПОЛЬЗОВАТЕЛЯ ===" << endl;
            cout << "В системе нет пользователей. Создайте первого пользователя:" << endl;

            cout << "Логин: ";
            string username;
            cin >> username;

            cout << "Пароль: ";
            string password;
            cin >> password;

            cout << "Уровень доступа (1 - Обычный пользователь, 2 - Администратор): ";
            int level;
            cin >> level;

            auto firstUser = make_shared<User>(
                "USR001",
                username,
                password,
                (level == 2 ? AccessLevel::ADMIN : AccessLevel::USER),
                "",
                ""
            );
            users.push_back(firstUser);
            currentUser = firstUser;

            cout << "Пользователь создан! Добро пожаловать, " << username << "!" << endl;
            saveData();
        }
        else {
            // Обычный вход
            while (!currentUser) {
                login();
                if (!currentUser) {
                    cout << "Для работы с системой необходимо войти в аккаунт!" << endl;
                }
            }
        }

        int choice;
        do {
            displayMainMenu();
            cin >> choice;

            switch (choice) {
            case 1:
                deviceManagement();
                break;
            case 2:
                roomManagement();
                break;
            case 3:
                if (hasAdminAccess()) userManagement();
                break;
            case 4:
                if (hasUserAccess()) scenarioManagement();
                break;
            case 5:
                if (hasUserAccess()) notificationManagement();
                break;
            case 6:
                if (hasUserAccess()) createEnergyReport();
                break;
            case 7:
                systemStatus();
                break;
            case 8:
                if (hasUserAccess()) searchManagement();
                break;
            case 9:
                saveData();
                break;
            case 0:
                cout << "До свидания!" << endl;
                break;
            default:
                cout << "Неверная опция!" << endl;
            }
        } while (choice != 0);
    }

    void deviceManagement() {
        if (rooms.empty()) {
            cout << "\nВ системе нет комнат. Сначала добавьте комнату через меню 'Управление комнатами'." << endl;
            return;
        }

        int choice;
        do {
            displayDevicesMenu();
            cin >> choice;

            switch (choice) {
            case 1:
                listAllDevices();
                break;
            case 2:
                if (devices.empty()) {
                    cout << "В системе нет устройств для управления." << endl;
                }
                else {
                    toggleDevice();
                }
                break;
            case 3:
                if (currentUser && currentUser->getAccessLevel() == AccessLevel::ADMIN) {
                    addDevice();
                }
                else {
                    return;
                }
                break;
            case 4:
                if (currentUser && currentUser->getAccessLevel() == AccessLevel::ADMIN) {
                    if (devices.empty()) {
                        cout << "В системе нет устройств для удаления." << endl;
                    }
                    else {
                        deleteDevice();
                    }
                }
                else {
                    return;
                }
                break;
            case 5:
                return;
            default:
                cout << "Неверная опция!" << endl;
            }
        } while (choice != (currentUser && currentUser->getAccessLevel() == AccessLevel::ADMIN ? 5 : 3));
    }

   
    void systemStatus() {
        cout << "\n=== СТАТУС СИСТЕМЫ ===" << endl;
        cout << "Комнат: " << rooms.size() << endl;
        cout << "Устройств: " << devices.size() << endl;
        cout << "Пользователей: " << users.size() << endl;
        cout << "Сценариев: " << scenarios.size() << endl;
        cout << "Уведомлений: " << notifications.size() << endl;

        if (devices.empty()) {
            cout << "\nСистема пуста. Добавьте устройства для мониторинга потребления." << endl;
        }
        else {
            int activeDevices = 0;
            double totalPower = 0.0;
            for (auto& device : devices) {
                if (device->getIsOn()) {
                    activeDevices++;
                    totalPower += device->getPowerConsumption();
                }
            }

            cout << "\nАктивных устройств: " << activeDevices << "/" << devices.size() << endl;
            cout << "Текущее потребление: " << totalPower << " Вт" << endl;
        }
    }

    void searchManagement() {
        int choice;
        do {
            displaySearchMenu();
            cin >> choice;

            switch (choice) {
            case 1:
                sortDevicesByPowerAsc();
                break;
            case 2:
                sortDevicesByPowerDesc();
                break;
            case 3:
                sortDevicesByName();
                break;
            case 4:
                findDevicesInRoom();
                break;
            case 5:
                findDevicesByManufacturer();
                break;
            case 6:
                findDeviceByName();
                break;
            case 7:
                findDevicesByPowerThreshold();
                break;
            case 8:
                return;
            default:
                cout << "Неверная опция!" << endl;
            }
        } while (choice != 8);
    }

    void roomManagement() {
        int choice;
        do {
            displayRoomsMenu();
            cin >> choice;

            switch (choice) {
            case 1:
                listAllRooms();
                break;
            case 2:
                showRoomDevices();
                break;
            case 3:
                showRoomPower();
                break;
            case 4:
                if (currentUser && currentUser->getAccessLevel() == AccessLevel::ADMIN) {
                    addRoom();
                }
                else {
                    return;
                }
                break;
            case 5:
                if (currentUser && currentUser->getAccessLevel() == AccessLevel::ADMIN) {
                    deleteRoom();
                }
                else {
                    return;
                }
                break;
            case 6:
                return;
            default:
                cout << "Неверная опция!" << endl;
            }
        } while (choice != (currentUser && currentUser->getAccessLevel() == AccessLevel::ADMIN ? 6 : 4));
    }

    void userManagement() {
        int choice;
        do {
            displayUsersMenu();
            cin >> choice;

            switch (choice) {
            case 1:
                listUsers();
                break;
            case 2:
                addUser();
                break;
            case 3:
                deleteUser();
                break;
            case 4:
                changeUserAccess();
                break;
            case 5:
                return;
            default:
                cout << "Неверная опция!" << endl;
            }
        } while (choice != 5);
    }

    void scenarioManagement() {
        int choice;
        do {
            displayScenariosMenu();
            cin >> choice;

            switch (choice) {
            case 1:
                if (scenarios.empty()) {
                    cout << "Сценарии еще не созданы!" << endl;
                }
                else {
                    for (auto& scenario : scenarios) {
                        scenario->displayInfo();
                    }
                }
                break;
            case 2:
                createScenario();
                break;
            case 3:
                executeScenario();
                break;
            case 4:
                showScenarioActions();
                break;
            case 5:
                addActionToScenario();
                break;
            case 6:
                return;
            default:
                cout << "Неверная опция!" << endl;
            }
        } while (choice != 6);
    }

    void notificationManagement() {
        int choice;
        do {
            displayNotificationsMenu();
            cin >> choice;

            switch (choice) {
            case 1:
                showAllNotifications();
                break;
            case 2:
                showUnreadNotifications();
                break;
            case 3:
                markNotificationAsRead();
                break;
            case 4:
                return;
            default:
                cout << "Неверная опция!" << endl;
            }
        } while (choice != 4);
    }

    void createScenario() {
        cout << "Создание нового сценария:" << endl;
        cout << "Название: ";
        string name;
        cin.ignore();
        getline(cin, name);

        cout << "Время запуска (например, 07:00): ";
        string time;
        getline(cin, time);

        auto scenario = make_unique<AutomationScenario>(
            "SCN" + to_string(scenarios.size() + 1), name, time
        );
        scenarios.push_back(move(scenario));
        cout << "Сценарий создан! Теперь добавьте действия." << endl;
        addActionToScenario(scenarios.back().get());
        saveData();
    }

    void executeScenario() {
        if (scenarios.empty()) {
            cout << "Нет доступных сценариев!" << endl;
            return;
        }

        cout << "Доступные сценарии:" << endl;
        for (size_t i = 0; i < scenarios.size(); i++) {
            cout << i + 1 << ". ";
            scenarios[i]->displayInfo();
        }

        cout << "Выберите номер сценария: ";
        int choice;
        cin >> choice;

        if (choice > 0 && choice <= scenarios.size()) {
            scenarios[choice - 1]->activate();
            scenarios[choice - 1]->execute();
            saveData();
        }
        else {
            cout << "Неверный номер сценария!" << endl;
        }
    }

    void showScenarioActions() {
        if (scenarios.empty()) {
            cout << "Нет доступных сценариев!" << endl;
            return;
        }

        cout << "Доступные сценарии:" << endl;
        for (size_t i = 0; i < scenarios.size(); i++) {
            cout << i + 1 << ". ";
            scenarios[i]->displayInfo();
        }

        cout << "Выберите номер сценария: ";
        int choice;
        cin >> choice;

        if (choice > 0 && choice <= scenarios.size()) {
            scenarios[choice - 1]->displayActions();
        }
        else {
            cout << "Неверный номер сценария!" << endl;
        }
    }

    void addActionToScenario(AutomationScenario* specificScenario = nullptr) {
        if (scenarios.empty()) {
            cout << "Нет доступных сценариев!" << endl;
            return;
        }

        AutomationScenario* scenario = specificScenario;
        if (!scenario) {
            cout << "Доступные сценарии:" << endl;
            for (size_t i = 0; i < scenarios.size(); i++) {
                cout << i + 1 << ". ";
                scenarios[i]->displayInfo();
            }

            cout << "Выберите номер сценария: ";
            int choice;
            cin >> choice;

            if (choice > 0 && choice <= scenarios.size()) {
                scenario = scenarios[choice - 1].get();
            }
            else {
                cout << "Неверный номер сценария!" << endl;
                return;
            }
        }

        cout << "Добавление действия в сценарий '" << scenario->getName() << "':" << endl;
        listAllDevices();
        cout << "Выберите номер устройства: ";
        int deviceChoice;
        cin >> deviceChoice;

        if (deviceChoice > 0 && deviceChoice <= devices.size()) {
            cout << "Действие (1 - Включить, 2 - Выключить): ";
            int actionChoice;
            cin >> actionChoice;

            string command = (actionChoice == 1) ? "turnOn" : "turnOff";
            auto action = make_unique<ScenarioAction>(
                "ACT" + to_string(scenario->getActionCount() + 1),
                devices[deviceChoice - 1],
                command
            );
            scenario->addAction(move(action));
            cout << "Действие добавлено в сценарий!" << endl;
            saveData();
        }
        else {
            cout << "Неверный номер устройства!" << endl;
        }
    }
};

int main() {
    try {
        setRussianLocale();
        cout << "Запуск системы Умный Дом..." << endl;
        SmartHomeSystem system;
        system.run();
        return 0;
    }
    catch (const exception& e) {
        cerr << "Критическая ошибка: " << e.what() << endl;
        return 1;
    }
}