using Content.Shared._Omu.Clothing.EntitySystems;
using Content.Shared._Omu.Traits;
using Content.Shared.Speech;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Omu.Traits;

/// <summary>
///     This component indicates that a player is a Cybernetic Beast.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(CyberneticMantleSystem), typeof(CyberneticBeastSystem))]
public sealed partial class CyberneticBeastComponent : Component
{

    public ProtoId<SpeechSoundsPrototype> SpeechSoundWhileWearingMantle = "Pai";

}