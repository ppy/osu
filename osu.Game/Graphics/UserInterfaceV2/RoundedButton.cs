// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class RoundedButton : OsuButton, IFilterable
    {
        protected TrianglesV2? Triangles { get; private set; }

        protected override float HoverLayerFinalAlpha => 0;

        private Color4? triangleGradientSecondColour;

        public override float Height
        {
            get => base.Height;
            set
            {
                base.Height = value;

                if (IsLoaded)
                    updateCornerRadius();
            }
        }

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
            triangleGradientSecondColour ??= overlayColourProvider?.Colour1 ?? colours.Blue3.Lighten(0.2f);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateCornerRadius();

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

        private void updateCornerRadius() => Content.CornerRadius = DrawHeight / 2;

        public virtual IEnumerable<LocalisableString> FilterTerms => new[] { Text };

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }
    }
}
