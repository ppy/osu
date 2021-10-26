// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Edit.Timing
{
    public class ControlPointSettings : EditorRoundedScreenSettings
    {
        protected override IReadOnlyList<Drawable> CreateSections() => new Drawable[]
        {
            new GroupSection(),
            new TimingSection(),
            new EffectSection(),
        };
    }
}
