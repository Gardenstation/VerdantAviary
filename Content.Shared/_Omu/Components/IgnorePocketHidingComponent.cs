using Robust.Shared.GameStates;
namespace Content.Shared._Omu.Components;

/// <summary>
/// Makes the entity able to see into pockets while stripping.
/// Used for the Thieving trait.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IgnorePocketHidingComponent : Component;
