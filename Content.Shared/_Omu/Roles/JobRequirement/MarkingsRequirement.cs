using System.Diagnostics.CodeAnalysis;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using System.Text;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Content.Shared.Roles;
using Content.Shared.Humanoid.Markings;
using System.Linq;

namespace Content.Shared._Omu.Roles;

/// <summary>
/// Requires a character to have a certain fixture mass
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class MarkingsRequirement : JobRequirement
{
    [DataField(required: true)]
    public HashSet<ProtoId<MarkingPrototype>> Markings = new();

    public override bool Check(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        //the profile could be null if the player is a ghost. In this case we don't need to block the role selection for ghostrole
        if (profile is null)
            return true;

        // Prepare to make a list of all the markings that we want, so that we can tell tell the user what requirements they don't meet
        var sb = new StringBuilder();
        sb.Append("[color=yellow]");

        var containsMarking = false;

        // get a list of marking IDs, from the markings that the player has chosen to wear.
        List<String> profileMarkingIDs = new();
        foreach (var m in profile.Appearance.Markings)
        {
            profileMarkingIDs.Add(m.MarkingId);
        }

        // compare the required markings to the player's chosen markings.
        foreach (var marking in Markings)
        {
            // add this marking to the list of required markings
            var markingProto = protoManager.Index(marking);
            sb.Append(Loc.GetString(markingProto.Name) + " ");

            // check whether the player has one of the requisite markings, and don't bother looping again if they do
            if (profileMarkingIDs.Contains(marking.Id))
            {
                containsMarking = true;
                break;
            }
        }

        if (!Inverted)
        {
            reason = FormattedMessage.FromMarkupPermissive($"{Loc.GetString("role-timer-whitelisted-markings")}\n{sb}");
            return containsMarking;
        }
        else
        {
            reason = FormattedMessage.FromMarkupPermissive($"{Loc.GetString("role-timer-blacklisted-markings")}\n{sb}");
            return !containsMarking;
        }
    }

}