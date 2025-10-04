using Content.Shared._Omu.Item.ItemToggle.Components;
using Content.Shared.Flash;

namespace Content.Shared._Omu.Traits;

/// <summary>
///     Handles flashing the player when an item-toggle raycast is recieved.
/// </summary>
public abstract class SharedPhotophobiaSystem : EntitySystem
{

    [Dependency] private readonly SharedFlashSystem _flashSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotophobiaComponent, ItemToggleRayHitEvent>(OnLightRayHit);
    }

    private void OnLightRayHit(Entity<PhotophobiaComponent> ent, ref ItemToggleRayHitEvent args)
    {
        _flashSystem.Flash(ent.Owner, null, null, TimeSpan.FromSeconds(ent.Comp.FlashDuration), ent.Comp.FlashSlowdown);
    }

}