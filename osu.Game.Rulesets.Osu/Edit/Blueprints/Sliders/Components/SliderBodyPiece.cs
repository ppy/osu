// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public partial class SliderBodyPiece : BlueprintPiece<Slider>
    {
        private readonly ManualSliderBody body;

        /// <summary>
        /// Offset in absolute (local) coordinates from the start of the curve.
        /// </summary>
        public Vector2 PathStartLocation => body.PathOffset;

        /// <summary>
        /// Offset in absolute (local) coordinates from the end of the curve.
        /// </summary>
        public Vector2 PathEndLocation => body.PathEndOffset;

        public SliderBodyPiece()
        {
            AutoSizeAxes = Axes.Both;

            // SliderSelectionBlueprint relies on calling ReceivePositionalInputAt on this drawable to determine whether selection should occur.
            // Without AlwaysPresent, a movement in a parent container (ie. the editor composer area resizing) could cause incorrect input handling.
            AlwaysPresent = true;

            InternalChild = body = new ManualSliderBody
            {
                AccentColour = Color4.Transparent
            };
        }

        private readonly Bindable<float> sliderPathRadius = new BindableFloat(OsuHitObject.OBJECT_RADIUS);

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            body.BorderColour = colours.Yellow;

            if (skin != null)
            {
                skin.SourceChanged += skinChanged;
                skinChanged();
            }
        }

        private void skinChanged()
        {
            sliderPathRadius.Value = skin?.GetConfig<OsuSkinConfiguration, float>(OsuSkinConfiguration.EditorBlueprintRadius)?.Value ?? OsuHitObject.OBJECT_RADIUS;
        }

        private int? lastVersion;

        [Resolved(canBeNull: true)]
        private ISkinSource? skin { get; set; }

        public override void UpdateFrom(Slider hitObject)
        {
            base.UpdateFrom(hitObject);

            body.PathRadius = hitObject.Scale * sliderPathRadius.Value;

            if (lastVersion != hitObject.Path.Version.Value)
            {
                lastVersion = hitObject.Path.Version.Value;

                var vertices = new List<Vector2>();
                hitObject.Path.GetPathToProgress(vertices, 0, 1);

                body.SetVertices(vertices);
            }

            OriginPosition = body.PathOffset;
        }

        public void RecyclePath() => body.RecyclePath();

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => body.ReceivePositionalInputAt(screenSpacePos);

        protected override void Dispose(bool isDisposing)
        {
            if (skin != null)
                skin.SourceChanged -= skinChanged;

            base.Dispose(isDisposing);
        }
    }
}
