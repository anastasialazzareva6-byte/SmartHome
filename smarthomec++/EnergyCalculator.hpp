#ifndef ENERGYCALCULATOR_HPP
#define ENERGYCALCULATOR_HPP

#include <type_traits>
#include <iostream>
#include <cmath>

// Шаблонная функция для расчета энергии
// Ограничение на уровне компиляции: только арифметические типы
template<typename T>
typename std::enable_if<std::is_arithmetic<T>::value, T>::type
calculateEnergyCost(T powerConsumption, T hours, T costPerKWh) {
    // Расчет стоимости энергии: (потребление * часы / 1000) * стоимость за кВт·ч
    T energyKWh = (powerConsumption * hours) / 1000.0;
    T cost = energyKWh * costPerKWh;

    // Округляем до 2 знаков после запятой
    return std::round(cost * 100.0) / 100.0;
}

// Перегрузка для устройств
template<typename DeviceType>
double calculateDeviceEnergyCost(const DeviceType& device, double hours, double costPerKWh) {
    if (!device.getIsOn()) {
        return 0.0;
    }
    return calculateEnergyCost(device.getPowerConsumption(), hours, costPerKWh);
}

#endif