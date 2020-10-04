using System;

namespace JiiLib.Constraints.Analyzers
{
    internal enum InterfaceConstraintDiagnosticChoice
    {
        Valid = 0,
        Struct,
        New,
        BaseClass,
        TypeParam
    }
}
