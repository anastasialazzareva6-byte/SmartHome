#ifndef BASEENTITY_HPP
#define BASEENTITY_HPP

#include <string>
#include <memory>
#include <ctime>

class BaseEntity {
protected:
    std::string id;
    std::string name;
    std::time_t createdDate;

public:
    BaseEntity(const std::string& id, const std::string& name);
    virtual ~BaseEntity() = default;  // Виртуальный деструктор

    std::string getId() const;
    std::string getName() const;
    std::time_t getCreatedDate() const;

    // Виртуальные функции
    virtual std::string serialize() const = 0;  // Чисто виртуальная - делаем класс абстрактным
    virtual void displayInfo() const = 0;
    virtual std::string getDetails() const { return ""; }  // Виртуальная с реализацией по умолчанию

    // Перегрузка операторов
    bool operator==(const BaseEntity& other) const;
    bool operator!=(const BaseEntity& other) const;

    // Метод для клонирования (прототип)
    virtual BaseEntity* clone() const = 0;

    friend std::ostream& operator<<(std::ostream& os, const BaseEntity& entity);
};

#endif