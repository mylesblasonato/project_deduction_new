using System;
using QFSW.QC.Utilities;

namespace QFSW.QC.Serializers
{
    public class TypeSerialiazer : PolymorphicQcSerializer<Type>
    {
        public override string SerializeFormatted(Type value, QuantumTheme theme)
        {
            return value.GetDisplayName();
        }
    }
}
