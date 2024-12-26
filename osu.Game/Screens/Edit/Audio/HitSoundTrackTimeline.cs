// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class HitSoundTrackTimeline : Timeline
    {
        public HitSoundTrackTimeline(Drawable userContent)
            : base(userContent)
        {
        }

        public bool HideCentreMarker { get; set; } = false;

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            RelativeSizeAxes = Axes.X;

            WaveformOpacity = new Bindable<float>(0f);

            if (HideCentreMarker)
                InternalChildren.FirstOrDefault(c => c.GetType() == typeof(CentreMarker))?.Expire();
        }

        protected override void UpdateAfterAutoSize()
        {
            base.UpdateAfterAutoSize();

            if (Parent != null)
                Height = Parent.DrawHeight;
        }
    }
}
