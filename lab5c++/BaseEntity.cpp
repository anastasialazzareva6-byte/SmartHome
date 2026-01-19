#include "BaseEntity.hpp"
#include <iostream>
#include <sstream>

BaseEntity::BaseEntity(const std::string& id, const std::string& name)
    : id(id), name(name), createdDate(std::time(nullptr)) {}

std::string BaseEntity::getId() const {
    return this->id;
}

std::string BaseEntity::getName() const {
    return this->name;
}

std::time_t BaseEntity::getCreatedDate() const {
    return this->createdDate;
}

bool BaseEntity::operator==(const BaseEntity& other) const {
    return this->id == other.id;
}

bool BaseEntity::operator!=(const BaseEntity& other) const {
    return !(*this == other);
}

std::ostream& operator<<(std::ostream& os, const BaseEntity& entity) {
    os << entity.name << " (ID: " << entity.id << ")";
    return os;
}