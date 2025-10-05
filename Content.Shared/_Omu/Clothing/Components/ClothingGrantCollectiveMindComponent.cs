using Robust.Shared.Prototypes;
using Content.Shared._Starlight.CollectiveMind;

namespace Content.Shared._Omu.Clothing.Components
{
    [RegisterComponent, AutoGenerateComponentState]
    public sealed partial class ClothingGrantCollectiveMindComponent : Component
    {
        [DataField("minds", required: true), AutoNetworkedField]
        public List<ProtoId<CollectiveMindPrototype>> Minds = new();

        [DataField, AutoNetworkedField]
        public ProtoId<CollectiveMindPrototype>? defaultChannel = null;
    }
}
