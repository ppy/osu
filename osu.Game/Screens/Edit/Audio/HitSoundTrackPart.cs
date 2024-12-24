// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Audio
{
    public enum HitSoundTrackMode
    {
        Sample,
        NormalBank,
        AdditionBank,
    }

    [Cached]
    public partial class HitSoundTrackPart : FillFlowContainer
    {
        public HitObject HitObject;

        [Resolved]
        private SoundTrackObjectsDisplay soundTrackObjectsDisplay { get; set; } = null!;

        public HitSoundTrackPart(HitObject hitObject)
        {
            HitObject = hitObject;

            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
            Direction = FillDirection.Vertical;
            Origin = Anchor.TopCentre;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            HitObject.StartTimeBindable.BindValueChanged(v =>
            {
                X = (float)GetStartTime();
            }, true);

            string buildTarget(string sample, string bank) => soundTrackObjectsDisplay.Mode == HitSoundTrackMode.Sample ? sample : bank;

            Children = new[]
            {
                new HitSoundTrackObjectToggle(buildTarget(HitSampleInfo.HIT_WHISTLE, HitSampleInfo.BANK_NORMAL)),
                new HitSoundTrackObjectToggle(buildTarget(HitSampleInfo.HIT_FINISH, HitSampleInfo.BANK_SOFT)),
                new HitSoundTrackObjectToggle(buildTarget(HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_DRUM)),
            };
        }

        protected virtual double GetStartTime()
        {
            return HitObject.StartTime;
        }
    }

    public partial class NodeHitSoundTrackPart : HitSoundTrackPart
    {
        public IHasRepeats HasRepeat;
        public int NodeIndex;

        public NodeHitSoundTrackPart(HitObject hitObject, IHasRepeats hasRepeat, int nodeIndex) : base(hitObject)
        {
            HasRepeat = hasRepeat;
            NodeIndex = nodeIndex;
        }

        protected override double GetStartTime()
        {
            return HitObject.StartTime + HasRepeat.Duration * NodeIndex / HasRepeat.SpanCount();
        }
    }
}
