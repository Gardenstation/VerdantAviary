using Content.Server._Omu.Objectives.Systems;

namespace Content.Server._Omu.Objectives.Components;

/// <summary>
///     Requires that a cybernetic mantle is present on station.
/// </summary>
[RegisterComponent, Access(typeof(CyberneticMantleExistsOnStationRequirementSystem))]
public sealed partial class CyberneticMantleExistsOnStationRequirementComponent : Component
{
}