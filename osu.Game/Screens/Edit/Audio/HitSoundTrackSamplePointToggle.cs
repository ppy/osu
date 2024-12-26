// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class HitSoundTrackSamplePointToggleButton : ClickableContainer
    {
        public IBindable<bool>? Active;

        private readonly Circle circle;

        public new IBindable<Colour4> Colour = new Bindable<Colour4>();

        public HitSoundTrackSamplePointToggleButton()
        {
            RelativeSizeAxes = Axes.Both;
            Child = circle = new Circle
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Colour.BindValueChanged(v => updateColour(), true);
            Active?.BindValueChanged(v => updateColour(), true);
        }

        private void updateColour()
        {
            circle.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Active?.Value == true ? Colour.Value.Darken(0.2f) : Colour4.Black.Opacity(0.2f),
                Radius = Active?.Value == true ? 3f : 2f,
                Hollow = true,
            };
            circle.FadeColour(Active?.Value == true ? Colour.Value : Colour.Value.Darken(0.8f));
            circle.FadeTo(Active?.Value == true ? 1f : 0.5f);
        }

        protected override bool OnHover(HoverEvent e)
        {
            circle.ResizeHeightTo(1.5f, 50);
            base.OnHover(e);
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            circle.ResizeHeightTo(1, 50);
        }
    }

    public interface IHasTarget
    {
        string Target { get; }
    }

    public partial class HitSoundTrackSamplePointToggle : Container, IHasTarget
    {
        public string Target { get; }

        private readonly Bindable<bool> active = new Bindable<bool>(false);

        [Resolved]
        private HitSoundTrackSamplePointBlueprint samplePoint { get; set; } = null!;

        public HitSoundTrackSamplePointToggle(string target)
        {
            Target = target;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            RelativeSizeAxes = Axes.X;

            Height = 15;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Child = new HitSoundTrackSamplePointToggleButton
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Action = () => samplePoint.Toggle(Target),
                Active = active,
                Colour = samplePoint.Colour,
            };

            samplePoint.OnSampleChange += () => active.Value = samplePoint.GetActiveState(Target);
            active.Value = samplePoint.GetActiveState(Target);
        }
    }
}
