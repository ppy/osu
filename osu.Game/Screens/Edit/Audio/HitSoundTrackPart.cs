// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Overlays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;

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

        public new Bindable<Colour4> Colour = new();

        protected const float CIRCLE_WIDTH = 15;

        [Resolved]
        private SoundTrackObjectsDisplay soundTrackObjectsDisplay { get; set; } = null!;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public HitSoundTrackPart(HitObject hitObject)
        {
            HitObject = hitObject;

            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Origin = Anchor.TopCentre;

            Spacing = new Vector2(12.5f);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            string buildTarget(string sample, string bank) => soundTrackObjectsDisplay.Mode == HitSoundTrackMode.Sample ? sample : bank;

            Children = new[]
            {
                new HitSoundTrackObjectToggle(buildTarget(HitSampleInfo.HIT_WHISTLE, HitSampleInfo.BANK_NORMAL)),
                new HitSoundTrackObjectToggle(buildTarget(HitSampleInfo.HIT_FINISH, HitSampleInfo.BANK_SOFT)),
                new HitSoundTrackObjectToggle(buildTarget(HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_DRUM)),
            };

            HitObject.StartTimeBindable.BindValueChanged(v =>
            {
                UpdateWidthAndPosition();
            }, true);

            if (HitObject is IHasDisplayColour displayColour)
                displayColour.DisplayColour.BindValueChanged(_ => updateColour(), true);

            if (HitObject is IHasComboInformation comboInfo)
                comboInfo.IndexInCurrentComboBindable.BindValueChanged(_ => updateColour(), true);

            skin.SourceChanged += updateColour;

            updateColour();
        }

        private void updateColour()
        {
            Colour4 colour;

            switch (HitObject)
            {
                case IHasDisplayColour displayColour:
                    colour = displayColour.DisplayColour.Value;
                    break;

                case IHasComboInformation combo:
                    colour = combo.GetComboColour(skin);
                    break;

                default:
                    colour = colourProvider.Highlight1;
                    break;
            }

            Colour.Value = colour;
        }

        protected void UpdateWidthAndPosition()
        {
            Width = (float)GetWidth();
            X = (float)GetStartTime();
        }

        protected virtual double GetStartTime()
        {
            return HitObject.StartTime;
        }
        protected virtual double GetWidth()
        {
            return CIRCLE_WIDTH;
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

            Width = CIRCLE_WIDTH;
        }

        protected override double GetStartTime()
        {
            return HitObject.StartTime + HasRepeat.Duration * NodeIndex / HasRepeat.SpanCount();
        }

        protected override double GetWidth()
        {
            return CIRCLE_WIDTH;
        }
    }

    public partial class ExtendableHitSoundTrackPart : HitSoundTrackPart
    {
        public ExtendableHitSoundTrackPart(HitObject hitObject) : base(hitObject)
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override void Update()
        {
            base.Update();
            UpdateWidthAndPosition();
        }

        protected override double GetStartTime()
        {
            return base.GetStartTime() + Width / 2 - CIRCLE_WIDTH / 2;
        }

        protected override double GetWidth()
        {
            if (HitObject is IHasDuration duration)
            {
                return duration.Duration + CIRCLE_WIDTH;
            }
            return base.GetWidth();
        }
    }
}
