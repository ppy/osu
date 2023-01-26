// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// Indicates that this hit object consists of multiple elements which each require separate combo informations.
    /// </summary>
    public interface IHasMultipleComboInformation
    {
        IEnumerable<IHasComboInformation> ComboObjects { get; }
    }
}
