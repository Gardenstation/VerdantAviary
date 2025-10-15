// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Gip-Kip <hargifarspam2@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.GameTicking;

[RegisterComponent]
public sealed partial class AddGameRuleOnUseComponent : Component
{
    /// <summary>
    /// The rule to add.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Rule;

    /// <summary>
    /// Indicates whether the item has been used at least once.
    /// </summary>
    [DataField]
    public bool Used;

    /// <summary>
    /// If set to true, allows you to spam gamerules.
    /// </summary>
    [DataField]
    public bool AllowMultipleUses;
}

/// <summary>
/// Raised when a gamerule gets added from the item
/// </summary>
[ByRefEvent]
public record struct AddGameRuleItemEvent(EntityUid? Initiator);
