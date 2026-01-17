using System;

namespace SmartHomeSystem
{
    public interface ICloneableDevice
    {
        Device ShallowClone();
        Device DeepClone();
        object Clone();
    }
}