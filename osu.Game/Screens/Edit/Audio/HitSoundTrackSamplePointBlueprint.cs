// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public partial class HitSoundTrackSamplePointBlueprint : FillFlowContainer
    {
        public HitObject HitObject;

        public new Bindable<Colour4> Colour = new Bindable<Colour4>();

        protected const float CIRCLE_WIDTH = 15;

        [Resolved]
        private HitSoundTrackSamplePointBlueprintContainer samplePointsContainer { get; set; } = null!;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public HitSoundTrackSamplePointBlueprint(HitObject hitObject)
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

            string buildTarget(string sample, string bank) => samplePointsContainer.Mode == HitSoundTrackMode.Sample ? sample : bank;

            Children = new[]
            {
                new HitSoundTrackSamplePointToggle(buildTarget(HitSampleInfo.HIT_WHISTLE, HitSampleInfo.BANK_NORMAL)),
                new HitSoundTrackSamplePointToggle(buildTarget(HitSampleInfo.HIT_FINISH, HitSampleInfo.BANK_SOFT)),
                new HitSoundTrackSamplePointToggle(buildTarget(HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_DRUM)),
            };

            HitObject.StartTimeBindable.BindValueChanged(v =>
            {
                UpdateWidthAndPosition();
            }, true);

            if (HitObject is IHasDisplayColour displayColour)
                displayColour.DisplayColour.BindValueChanged(_ => updateColour());

            if (HitObject is IHasComboInformation comboInfo)
            {
                comboInfo.IndexInCurrentComboBindable.BindValueChanged(_ => updateColour());
                comboInfo.ComboIndexBindable.BindValueChanged(_ => updateColour());
                comboInfo.ComboIndexWithOffsetsBindable.BindValueChanged(_ => updateColour());
            }

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

        public void UpdateWidthAndPosition()
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

    public partial class NodeHitSoundTrackSamplePointBlueprint : HitSoundTrackSamplePointBlueprint
    {
        public IHasRepeats HasRepeat;
        public int NodeIndex;

        public NodeHitSoundTrackSamplePointBlueprint(HitObject hitObject, IHasRepeats hasRepeat, int nodeIndex)
            : base(hitObject)
        {
            HasRepeat = hasRepeat;
            NodeIndex = nodeIndex;

            Width = CIRCLE_WIDTH;
        }

        protected override void Update()
        {
            base.Update();

            if (HasRepeat.RepeatCount < NodeIndex - 1)
                Expire();
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

    public partial class ExtendableHitSoundTrackSamplePointBlueprint : HitSoundTrackSamplePointBlueprint
    {
        public ExtendableHitSoundTrackSamplePointBlueprint(HitObject hitObject)
            : base(hitObject)
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
                return duration.Duration + CIRCLE_WIDTH;

            return base.GetWidth();
        }
    }
}
