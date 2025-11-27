#ifndef ENERGYREPORT_HPP
#define ENERGYREPORT_HPP

#include <string>
#include <vector>
#include <map>
#include <ctime>

class Device;

class EnergyReport {
private:
    std::string reportId;
    std::time_t periodStart;
    std::time_t periodEnd;
    double totalConsumption;
    double peakLoad;
    std::map<Device*, double> deviceConsumptions;

public:
    EnergyReport(const std::string& id, std::time_t start, std::time_t end);
    ~EnergyReport();

    void generateReport();
    std::vector<Device*> getTopConsumingDevices(int count) const;
    void addDeviceConsumption(Device* device, double consumption);
    void displayReport() const;

    std::string getReportId() const;
    double getTotalConsumption() const;
    double getPeakLoad() const;

    // Методы для сериализации
    std::string serialize() const;
    static EnergyReport* deserialize(const std::string& data);
};

#endif