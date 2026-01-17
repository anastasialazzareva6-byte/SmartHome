#include "energyReport.hpp"
#include "device.hpp"
#include <iostream>
#include <algorithm>
#include <vector>
#include <iomanip>
#include <sstream>

EnergyReport::EnergyReport(const std::string& id, std::time_t start, std::time_t end)
    : reportId(id), periodStart(start), periodEnd(end),
    totalConsumption(0.0), peakLoad(0.0) {
}

EnergyReport::~EnergyReport() {
    deviceConsumptions.clear();
}

void EnergyReport::generateReport() {
    totalConsumption = 0.0;
    peakLoad = 0.0;

    for (const auto& pair : deviceConsumptions) {
        totalConsumption += pair.second;
        if (pair.second > peakLoad) {
            peakLoad = pair.second;
        }
    }
}

std::vector<std::shared_ptr<Device>> EnergyReport::getTopConsumingDevices(int count) const {
    std::vector<std::pair<std::shared_ptr<Device>, double>> devices;
    for (const auto& pair : deviceConsumptions) {
        devices.push_back(pair);
    }

    std::sort(devices.begin(), devices.end(),
        [](const auto& a, const auto& b) { return a.second > b.second; });

    std::vector<std::shared_ptr<Device>> topDevices;
    for (int i = 0; i < std::min(count, static_cast<int>(devices.size())); ++i) {
        topDevices.push_back(devices[i].first);
    }

    return topDevices;
}

void EnergyReport::addDeviceConsumption(std::shared_ptr<Device> device, double consumption) {
    deviceConsumptions[device] = consumption;
}

void EnergyReport::displayReport() const {
    std::cout << "\n=== ќ“„≈“ ѕќ ЁЌ≈–√ќѕќ“–≈ЅЋ≈Ќ»ё " << reportId << " ===" << std::endl;

    char startBuffer[80], endBuffer[80];

    // Ѕезопасна€ верси€ дл€ Windows
#ifdef _WIN32
    struct tm startInfo, endInfo;
    localtime_s(&startInfo, &periodStart);
    localtime_s(&endInfo, &periodEnd);
    std::strftime(startBuffer, sizeof(startBuffer), "%Y-%m-%d %H:%M:%S", &startInfo);
    std::strftime(endBuffer, sizeof(endBuffer), "%Y-%m-%d %H:%M:%S", &endInfo);
#else
    struct tm* startInfo = std::localtime(&periodStart);
    struct tm* endInfo = std::localtime(&periodEnd);
    std::strftime(startBuffer, sizeof(startBuffer), "%Y-%m-%d %H:%M:%S", startInfo);
    std::strftime(endBuffer, sizeof(endBuffer), "%Y-%m-%d %H:%M:%S", endInfo);
#endif

    std::cout << "ѕериод: с " << startBuffer << " по " << endBuffer << std::endl;
    std::cout << "ќбщее потребление: " << std::fixed << std::setprecision(2)
        << totalConsumption << " к¬тЈч" << std::endl;
    std::cout << "ѕикова€ нагрузка: " << peakLoad << " к¬т" << std::endl;
    std::cout << "¬сего устройств в отчете: " << deviceConsumptions.size() << std::endl;

    auto topDevices = getTopConsumingDevices(3);
    if (!topDevices.empty()) {
        std::cout << "\n“оп-3 потребителей энергии:" << std::endl;
        for (size_t i = 0; i < topDevices.size(); i++) {
            const auto& device = topDevices[i];
            double consumption = deviceConsumptions.at(device);
            std::cout << "  " << i + 1 << ". " << device->getName()
                << " (" << device->getDeviceTypeString() << "): "
                << consumption << " к¬тЈч" << std::endl;
        }
    }

    std::cout << "\nƒетализаци€ по устройствам:" << std::endl;
    int counter = 1;
    for (const auto& pair : deviceConsumptions) {
        std::cout << "  " << counter++ << ". " << pair.first->getName()
            << ": " << pair.second << " к¬тЈч" << std::endl;
    }

    double averageLoad = deviceConsumptions.empty() ? 0.0 : totalConsumption / deviceConsumptions.size();
    std::cout << "\n—редн€€ нагрузка на устройство: " << averageLoad << " к¬тЈч" << std::endl;

    std::cout << "======================================" << std::endl;
}

std::string EnergyReport::getReportId() const {
    return reportId;
}

double EnergyReport::getTotalConsumption() const {
    return totalConsumption;
}

double EnergyReport::getPeakLoad() const {
    return peakLoad;
}

std::string EnergyReport::serialize() const {
    std::stringstream ss;
    ss << reportId << "|"
        << periodStart << "|"
        << periodEnd << "|"
        << totalConsumption << "|"
        << peakLoad << "|"
        << deviceConsumptions.size();

    for (const auto& pair : deviceConsumptions) {
        ss << "|" << pair.first->getId() << ":" << pair.second;
    }

    return ss.str();
}

std::unique_ptr<EnergyReport> EnergyReport::deserialize(const std::string& data) {
    try {
        std::stringstream ss(data);
        std::string id, startStr, endStr, totalStr, peakStr, countStr;

        std::getline(ss, id, '|');
        std::getline(ss, startStr, '|');
        std::getline(ss, endStr, '|');
        std::getline(ss, totalStr, '|');
        std::getline(ss, peakStr, '|');
        std::getline(ss, countStr, '|');

        auto report = std::make_unique<EnergyReport>(
            id,
            std::stol(startStr),
            std::stol(endStr)
        );

        report->totalConsumption = std::stod(totalStr);
        report->peakLoad = std::stod(peakStr);

        int deviceCount = std::stoi(countStr);

        // ѕримечание: устройства должны быть добавлены позже
        // после загрузки всех данных

        return report;
    }
    catch (const std::exception& e) {
        std::cerr << "ќшибка десериализации энергоотчета: " << e.what() << std::endl;
        return nullptr;
    }
}