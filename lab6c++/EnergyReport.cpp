#include "energyReport.hpp"
#include "device.hpp"
#include <iostream>
#include <algorithm>
#include <vector>
#include <iomanip>
#include <sstream>

EnergyReport::EnergyReport(const std::string& id, std::time_t start, std::time_t end)
    : reportId(id), periodStart(start), periodEnd(end),
    totalConsumption(0.0), peakLoad(0.0) {}

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
    std::cout << "=== ÎÒ×ÅÒ ÏÎ ÝÍÅÐÃÎÏÎÒÐÅÁËÅÍÈÞ " << reportId << " ===\n";
    std::cout << "Îáùåå ïîòðåáëåíèå: " << std::fixed << std::setprecision(2)
        << totalConsumption << " êÂò·÷\n";
    std::cout << "Ïèêîâàÿ íàãðóçêà: " << peakLoad << " êÂò\n";
    auto topDevices = getTopConsumingDevices(3);
    if (!topDevices.empty()) {
        std::cout << "Òîï ïîòðåáèòåëåé:\n";
        for (const auto& device : topDevices) {
            std::cout << "  - " << device->getName() << ": "
                << deviceConsumptions.at(device) << " êÂò·÷\n";
        }
    }
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
    ss << reportId << "|" << periodStart << "|" << periodEnd << "|"
        << totalConsumption << "|" << peakLoad;
    return ss.str();
}

std::unique_ptr<EnergyReport> EnergyReport::deserialize(const std::string& data) {
    std::stringstream ss(data);
    std::string id, startStr, endStr, totalStr, peakStr;

    std::getline(ss, id, '|');
    std::getline(ss, startStr, '|');
    std::getline(ss, endStr, '|');
    std::getline(ss, totalStr, '|');
    std::getline(ss, peakStr, '|');

    auto report = std::make_unique<EnergyReport>(id, std::stol(startStr), std::stol(endStr));
    report->totalConsumption = std::stod(totalStr);
    report->peakLoad = std::stod(peakStr);
    return report;
}