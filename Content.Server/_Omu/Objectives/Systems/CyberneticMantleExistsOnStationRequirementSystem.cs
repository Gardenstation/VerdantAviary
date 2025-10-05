using Content.Server._Omu.Objectives.Components;
using Content.Shared._Omu.Clothing.Components;
using Content.Shared.Objectives.Components;
using Robust.Shared.Utility;

namespace Content.Server._Omu.Objectives.Systems;

public sealed class CyberneticMantleExistsOnStationRequirementSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CyberneticMantleExistsOnStationRequirementComponent, RequirementCheckEvent>(OnCheck);
    }

    private void OnCheck(EntityUid uid, CyberneticMantleExistsOnStationRequirementComponent comp, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

        // if there exists no item on station with a CyberneticMantleComponent 
        if (EntityManager.GetAllComponents(typeof(CyberneticMantleComponent)).FirstOrNull() == null)
            args.Cancelled = true;
    }
}