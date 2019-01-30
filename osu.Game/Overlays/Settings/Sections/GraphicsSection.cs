// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.Graphics;

namespace osu.Game.Overlays.Settings.Sections
{
    public class GraphicsSection : SettingsSection
    {
        public override string Header => "Graphics";
        public override FontAwesome Icon => FontAwesome.fa_laptop;

        public GraphicsSection()
        {
            Children = new Drawable[]
            {
                new RendererSettings(),
                new LayoutSettings(),
                new DetailSettings(),
                new MainMenuSettings(),
            };
        }
    }
}
