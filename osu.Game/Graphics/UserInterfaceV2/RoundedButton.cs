// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class RoundedButton : OsuButton, IFilterable, IHasTooltip
    {
        protected TrianglesV2? Triangles { get; private set; }

        protected override float HoverLayerFinalAlpha => 0;

        private Color4? triangleGradientSecondColour;

        public override Color4 BackgroundColour
        {
            get => base.BackgroundColour;
            set
            {
                base.BackgroundColour = value;
                triangleGradientSecondColour = BackgroundColour.Lighten(0.2f);
                updateColours();
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? overlayColourProvider, OsuColour colours)
        {
            // Many buttons have local colours, but this provides a sane default for all other cases.
            DefaultBackgroundColour = overlayColourProvider?.Colour3 ?? colours.Blue3;
            triangleGradientSecondColour ??= DefaultBackgroundColour.Lighten(0.2f);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // This doesn't match the latest design spec (should be 5) but is an in-between that feels right to the eye
            // until we move everything over to Form controls.
            Content.CornerRadius = 10;
            Content.CornerExponent = 2.5f;

            Add(Triangles = new TrianglesV2
            {
                Thickness = 0.02f,
                SpawnRatio = 0.6f,
                RelativeSizeAxes = Axes.Both,
                Depth = float.MaxValue,
            });

            updateColours();
        }

        private void updateColours()
        {
            if (Triangles == null)
                return;

            Debug.Assert(triangleGradientSecondColour != null);

            Triangles.Colour = ColourInfo.GradientVertical(triangleGradientSecondColour.Value, BackgroundColour);
        }

        protected override bool OnHover(HoverEvent e)
        {
            Debug.Assert(triangleGradientSecondColour != null);

            Background.FadeColour(triangleGradientSecondColour.Value, 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Background.FadeColour(BackgroundColour, 300, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        public virtual IEnumerable<LocalisableString> FilterTerms => new[] { Text };

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }

        public virtual LocalisableString TooltipText { get; set; }
    }
}
