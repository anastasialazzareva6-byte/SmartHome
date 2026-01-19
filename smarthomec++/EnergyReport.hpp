#ifndef ENERGYREPORT_HPP
#define ENERGYREPORT_HPP

#include <string>
#include <vector>
#include <map>
#include <ctime>
#include <memory>

class Device;

class EnergyReport {
private:
    std::string reportId;
    std::time_t periodStart;
    std::time_t periodEnd;
    double totalConsumption;
    double peakLoad;
    std::map<std::shared_ptr<Device>, double> deviceConsumptions;

public:
    EnergyReport(const std::string& id, std::time_t start, std::time_t end);
    ~EnergyReport();

    void generateReport();
    std::vector<std::shared_ptr<Device>> getTopConsumingDevices(int count) const;
    void addDeviceConsumption(std::shared_ptr<Device> device, double consumption);
    void displayReport() const;

    std::string getReportId() const;
    double getTotalConsumption() const;
    double getPeakLoad() const;

    std::string serialize() const;
    static std::unique_ptr<EnergyReport> deserialize(const std::string& data);
};

#endif