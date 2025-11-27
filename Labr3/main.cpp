#include <iostream>
#include <vector>
#include <ctime>
#include <limits>
#include <locale>
#include <windows.h>
#include <fstream>
#include <sstream>
#include <conio.h>
#include <stdio.h>
#include <string.h>
#include "room.hpp"
#include "device.hpp"
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

class DataManager {
private:
    static const string ROOMS_FILE;
    static const string DEVICES_FILE;
    static const string USERS_FILE;
    static const string SCENARIOS_FILE;
    static const string NOTIFICATIONS_FILE;
    static const string ACTIONS_FILE;

public:
    static void saveRooms(const vector<Room*>& rooms) {
        ofstream file(ROOMS_FILE);
        if (file.is_open()) {
            for (const auto& room : rooms) {
                file << room->serialize() << endl;
            }
            file.close();
        }
    }

    static vector<Room*> loadRooms() {
        vector<Room*> rooms;
        ifstream file(ROOMS_FILE);
        if (file.is_open()) {
            string line;
            while (getline(file, line)) {
                if (!line.empty()) {
                    rooms.push_back(Room::deserialize(line));
                }
            }
            file.close();
        }
        return rooms;
    }

    static void saveDevices(const vector<Device*>& devices) {
        ofstream file(DEVICES_FILE);
        if (file.is_open()) {
            for (const auto& device : devices) {
                file << device->serialize() << endl;
            }
            file.close();
        }
    }

    static vector<Device*> loadDevices(const vector<Room*>& rooms) {
        vector<Device*> devices;
        ifstream file(DEVICES_FILE);
        if (file.is_open()) {
            string line;
            while (getline(file, line)) {
                if (!line.empty()) {
                    // Находим комнату для устройства
                    stringstream ss(line);
                    string temp, locationId;
                    getline(ss, temp, '|'); // deviceId
                    getline(ss, temp, '|'); // name
                    getline(ss, temp, '|'); // manufacturer
                    getline(ss, temp, '|'); // type
                    getline(ss, locationId, '|'); // location

                    Room* location = nullptr;
                    for (auto room : rooms) {
                        if (room->getRoomId() == locationId) {
                            location = room;
                            break;
                        }
                    }

                    if (location) {
                        Device* device = Device::deserialize(line, location);
                        devices.push_back(device);
                        location->addDevice(device);
                    }
                }
            }
            file.close();
        }
        return devices;
    }

    static void saveUsers(const vector<User*>& users) {
        ofstream file(USERS_FILE);
        if (file.is_open()) {
            for (const auto& user : users) {
                file << user->serialize() << endl;
            }
            file.close();
        }
    }

    static vector<User*> loadUsers() {
        vector<User*> users;
        ifstream file(USERS_FILE);
        if (file.is_open()) {
            string line;
            while (getline(file, line)) {
                if (!line.empty()) {
                    users.push_back(User::deserialize(line));
                }
            }
            file.close();
        }
        return users;
    }

    static void saveScenarios(const vector<AutomationScenario*>& scenarios) {
        ofstream file(SCENARIOS_FILE);
        if (file.is_open()) {
            for (const auto& scenario : scenarios) {
                file << scenario->serialize() << endl;
            }
            file.close();
        }
    }

    static vector<AutomationScenario*> loadScenarios() {
        vector<AutomationScenario*> scenarios;
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

    static void saveNotifications(const vector<Notification*>& notifications) {
        ofstream file(NOTIFICATIONS_FILE);
        if (file.is_open()) {
            for (const auto& notification : notifications) {
                file << notification->serialize() << endl;
            }
            file.close();
        }
    }

    static vector<Notification*> loadNotifications() {
        vector<Notification*> notifications;
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

    static void initializeDefaultData(vector<Room*>& rooms, vector<Device*>& devices,
        vector<User*>& users, vector<Notification*>& notifications,
        vector<AutomationScenario*>& scenarios) {
        // Создание комнат
        rooms.push_back(new Room("LR001", "Гостиная", 25.5));
        rooms.push_back(new Room("KIT001", "Кухня", 15.0));
        rooms.push_back(new Room("BR001", "Спальня", 18.0));

        // Создание устройств с указанием энергопотребления
        devices.push_back(new Device("DEV001", "Умный телевизор", "Samsung", DeviceType::MULTIMEDIA, rooms[0], 120.0));
        devices.push_back(new Device("DEV002", "Кондиционер", "LG", DeviceType::CLIMATE_CONTROL, rooms[0], 1500.0));
        devices.push_back(new Device("DEV003", "Умный свет", "Philips", DeviceType::ACTUATOR, rooms[1], 15.0));
        devices.push_back(new Device("DEV004", "Камера безопасности", "Xiaomi", DeviceType::SECURITY, rooms[2], 8.0));
        devices.push_back(new Device("DEV005", "Датчик температуры", "Bosch", DeviceType::SENSOR, rooms[1], 2.0));

        // Добавление устройств в комнаты
        for (auto device : devices) {
            if (device->getLocation()) {
                device->getLocation()->addDevice(device);
            }
        }

        // Создание пользователей 
        users.push_back(new User("USR001", "admin", "123", AccessLevel::ADMIN, "", ""));
        users.push_back(new User("USR002", "user", "123", AccessLevel::USER, "", ""));

        // Создание уведомлений
        notifications.push_back(new Notification("NOT001", NotificationType::INFO, "Система успешно запущена"));

        // Создание тестового сценария
        AutomationScenario* morningScenario = new AutomationScenario("SCN001", "Утренний режим", "07:00");
        morningScenario->addAction(new ScenarioAction("ACT001", devices[2], "turnOn"));
        morningScenario->addAction(new ScenarioAction("ACT002", devices[0], "turnOn"));
        scenarios.push_back(morningScenario);
    }
};

// Статические члены класса DataManager
const string DataManager::ROOMS_FILE = "rooms.dat";
const string DataManager::DEVICES_FILE = "devices.dat";
const string DataManager::USERS_FILE = "users.dat";
const string DataManager::SCENARIOS_FILE = "scenarios.dat";
const string DataManager::NOTIFICATIONS_FILE = "notifications.dat";
const string DataManager::ACTIONS_FILE = "actions.dat";

class SmartHomeSystem {
private:
    vector<Room*> rooms;
    vector<Device*> devices;
    vector<User*> users;
    vector<AutomationScenario*> scenarios;
    vector<Notification*> notifications;
    vector<EnergyReport*> reports;
    User* currentUser;

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

        // Загрузка комнат
        rooms = DataManager::loadRooms();

        // Загрузка устройств (требует загруженные комнаты)
        devices = DataManager::loadDevices(rooms);

        // Загрузка пользователей
        users = DataManager::loadUsers();

        // Загрузка сценариев
        scenarios = DataManager::loadScenarios();

        // Загрузка уведомлений
        notifications = DataManager::loadNotifications();

        // Если данных нет, создаем начальные данные
        if (rooms.empty() && devices.empty() && users.empty()) {
            cout << "Создание начальных данных..." << endl;
            DataManager::initializeDefaultData(rooms, devices, users, notifications, scenarios);
            saveData();
        }

        cout << "Данные загружены: " << rooms.size() << " комнат, "
            << devices.size() << " устройств, " << users.size() << " пользователей" << endl;
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
        for (auto room : rooms) delete room;
        for (auto device : devices) delete device;
        for (auto user : users) delete user;
        for (auto scenario : scenarios) delete scenario;
        for (auto notification : notifications) delete notification;
        for (auto report : reports) delete report;
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
            cout << " (" << (currentUser->getAccessLevel() == AccessLevel::ADMIN ? "Администратор" : "Пользователь") << ")" << endl;
        }
        cout << "1. Управление устройствами" << endl;
        cout << "2. Управление комнатами" << endl;
        cout << "3. Управление пользователями" << (currentUser && currentUser->getAccessLevel() == AccessLevel::ADMIN ? " (Админ)" : "") << endl;
        cout << "4. Сценарии автоматизации" << endl;
        cout << "5. Уведомления" << endl;
        cout << "6. Энергоотчеты" << endl;
        cout << "7. Статус системы" << endl;
        if (!currentUser) {
            cout << "8. Войти в систему" << endl;
        }
        cout << "9. Сохранить данные" << endl;
        cout << "0. Выход" << endl;
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

    void displayUsersMenu() {
        cout << "\n=== УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ (АДМИН) ===" << endl;
        cout << "1. Список пользователей" << endl;
        cout << "2. Добавить пользователя" << endl;
        cout << "3. Удалить пользователя" << endl;
        cout << "4. Изменить права доступа" << endl;
        cout << "5. Назад в главное меню" << endl;
        cout << "Выберите опцию: ";
    }

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

    void displayNotificationsMenu() {
        cout << "\n=== УВЕДОМЛЕНИЯ ===" << endl;
        cout << "1. Показать все уведомления" << endl;
        cout << "2. Показать непрочитанные" << endl;
        cout << "3. Отметить как прочитанное" << endl;
        cout << "4. Назад в главное меню" << endl;
        cout << "Выберите опцию: ";
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
            Device* device = devices[choice - 1];
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

        cout << "Добавление нового устройства:" << endl;
        cout << "Название: ";
        string name;
        cin.ignore();
        getline(cin, name);

        cout << "Производитель: ";
        string manufacturer;
        getline(cin, manufacturer);

        cout << "Тип устройства (1-Датчик, 2-Исполнительное, 3-Климат, 4-Безопасность, 5-Мультимедиа): ";
        int typeChoice;
        cin >> typeChoice;
        DeviceType type = static_cast<DeviceType>(typeChoice - 1);

        cout << "Энергопотребление (Вт): ";
        double power;
        cin >> power;

        listAllRooms();
        cout << "Выберите номер комнаты: ";
        int roomChoice;
        cin >> roomChoice;

        if (roomChoice > 0 && roomChoice <= rooms.size()) {
            Room* room = rooms[roomChoice - 1];
            Device* newDevice = new Device(
                "DEV" + to_string(devices.size() + 1),
                name,
                manufacturer,
                type,
                room,
                power
            );
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
            Device* device = devices[choice - 1];
            // Удаляем устройство из комнаты
            if (device->getLocation()) {
                device->getLocation()->removeDevice(device);
            }
            // Удаляем устройство из списка
            devices.erase(devices.begin() + choice - 1);
            delete device;
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

            // Показываем какие устройства включены и их потребление
            auto roomDevices = rooms[choice - 1]->getDevices();
            int activeDevices = 0;
            for (auto device : roomDevices) {
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

        Room* newRoom = new Room(
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
            // Проверяем, есть ли устройства в комнате
            if (!rooms[choice - 1]->getDevices().empty()) {
                cout << "Ошибка: В комнате есть устройства! Сначала удалите или переместите их." << endl;
                return;
            }

            Room* room = rooms[choice - 1];
            rooms.erase(rooms.begin() + choice - 1);
            delete room;
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

        User* newUser = new User(
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
            // Нельзя удалить текущего пользователя
            if (users[choice - 1] == currentUser) {
                cout << "Ошибка: Нельзя удалить текущего пользователя!" << endl;
                return;
            }

            User* user = users[choice - 1];
            users.erase(users.begin() + choice - 1);
            delete user;
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
        EnergyReport* report = new EnergyReport("ОТЧЕТ" + to_string(reports.size() + 1),
            time(nullptr) - 86400, time(nullptr));

        // Добавляем потребление только включенных устройств
        for (auto device : devices) {
            if (device->getIsOn()) {
                report->addDeviceConsumption(device, device->getPowerConsumption() * 24);
            }
        }

        report->generateReport();
        reports.push_back(report);
        report->displayReport();
    }

    void systemStatus() {
        cout << "\n=== СТАТУС СИСТЕМЫ ===" << endl;
        cout << "Комнат: " << rooms.size() << endl;
        cout << "Устройств: " << devices.size() << endl;
        cout << "Пользователей: " << users.size() << endl;
        cout << "Сценариев: " << scenarios.size() << endl;
        cout << "Уведомлений: " << notifications.size() << endl;

        int activeDevices = 0;
        double totalPower = 0.0;
        for (auto device : devices) {
            if (device->getIsOn()) {
                activeDevices++;
                totalPower += device->getPowerConsumption();
            }
        }

        cout << "Активных устройств: " << activeDevices << "/" << devices.size() << endl;
        cout << "Текущее потребление: " << totalPower << " Вт" << endl;
    }

    void login() {
        cout << "\n=== ВХОД В СИСТЕМУ ===" << endl;
        cout << "Имя пользователя: ";
        string username;
        cin >> username;

        cout << "Пароль: ";
        string password;
        cin >> password;

        for (auto user : users) {
            if (user->getUsername() == username && user->login(password)) {
                currentUser = user;
                cout << "Вход выполнен успешно! Добро пожаловать, " << username << "!" << endl;
                return;
            }
        }
        cout << "Ошибка входа!" << endl;
    }

    void run() {
        // Требуем вход в систему перед началом работы
        while (!currentUser) {
            login();
            if (!currentUser) {
                cout << "Для работы с системой необходимо войти в аккаунт!" << endl;
            }
        }

        int choice;
        do {
            displayMainMenu();
            cin >> choice;

            switch (choice) {
            case 1: // Управление устройствами
                deviceManagement();
                break;
            case 2: // Управление комнатами
                roomManagement();
                break;
            case 3: // Управление пользователями - ТОЛЬКО АДМИН
                if (hasAdminAccess()) userManagement();
                break;
            case 4: // Сценарии автоматизации
                if (hasUserAccess()) scenarioManagement();
                break;
            case 5: // Уведомления
                if (hasUserAccess()) notificationManagement();
                break;
            case 6: // Энергоотчеты
                if (hasUserAccess()) createEnergyReport();
                break;
            case 7: // Статус системы - доступно всем
                systemStatus();
                break;
            case 8: // Войти в систему
                if (!currentUser) {
                    login();
                }
                break;
            case 9: // Сохранить данные
                saveData();
                break;
            case 0: // Выход
                cout << "До свидания!" << endl;
                break;
            default:
                cout << "Неверная опция!" << endl;
            }
        } while (choice != 0);
    }

    void deviceManagement() {
        int choice;
        do {
            displayDevicesMenu();
            cin >> choice;

            switch (choice) {
            case 1:
                listAllDevices();
                break;
            case 2:
                toggleDevice();
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
                    deleteDevice();
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
                    for (auto scenario : scenarios) {
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

        AutomationScenario* scenario = new AutomationScenario(
            "SCN" + to_string(scenarios.size() + 1), name, time
        );
        scenarios.push_back(scenario);
        cout << "Сценарий создан! Теперь добавьте действия." << endl;
        addActionToScenario(scenario);
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
                scenario = scenarios[choice - 1];
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
            ScenarioAction* action = new ScenarioAction(
                "ACT" + to_string(scenario->getActionCount() + 1),
                devices[deviceChoice - 1],
                command
            );
            scenario->addAction(action);
            cout << "Действие добавлено в сценарий!" << endl;
            saveData();
        }
        else {
            cout << "Неверный номер устройства!" << endl;
        }
    }
};

int main() {
    setRussianLocale();
    cout << "Запуск системы Умный Дом..." << endl;
    SmartHomeSystem system;
    system.run();
    return 0;
}