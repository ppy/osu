// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;

namespace osu.Game.Overlays
{
    public class MainSettings : SettingsOverlay
    {
        protected override IEnumerable<SettingsSection> CreateSections() => new SettingsSection[]
        {
            new GeneralSection(),
            new GraphicsSection(),
            new GameplaySection(),
            new AudioSection(),
            new SkinSection(),
            new InputSection(),
            new OnlineSection(),
            new MaintenanceSection(),
            new DebugSection(),
        };

        protected override Drawable CreateHeader() => new SettingsHeader("settings", "Change the way osu! behaves");
        protected override Drawable CreateFooter() => new SettingsFooter();
    }
}