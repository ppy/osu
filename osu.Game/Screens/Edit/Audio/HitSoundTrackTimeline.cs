// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class HitSoundTrackTimeline : Timeline
    {
        public HitSoundTrackTimeline(Drawable userContent) : base(userContent)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            RelativeSizeAxes = Axes.X;
        }

        protected override void UpdateAfterAutoSize()
        {
            base.UpdateAfterAutoSize();
            Height = Parent.DrawHeight;
        }
    }
}
