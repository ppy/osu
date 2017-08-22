// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.ReplaySettings;
using OpenTK;

namespace osu.Game.Screens.Play
{
    public class ReplaySettingsOverlay : FillFlowContainer
    {
        public ReplaySettingsOverlay()
        {
            Direction = FillDirection.Vertical;
            AutoSizeAxes = Axes.Both;
            Spacing = new Vector2(0, 20);

            Add(new CollectionSettings());
            Add(new DiscussionSettings());
            Add(new PlaybackSettings());
        }
    }
}
