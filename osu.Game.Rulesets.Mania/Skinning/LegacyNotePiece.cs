// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyNotePiece : LegacyManiaColumnElement
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container directionContainer;
        private Sprite noteSprite;

        public LegacyNotePiece()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            InternalChild = directionContainer = new Container
            {
                Anchor = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = noteSprite = new Sprite { Texture = GetTexture(skin) }
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(OnDirectionChanged, true);
        }

        protected override void Update()
        {
            base.Update();

            if (noteSprite.Texture != null)
            {
                var scale = DrawWidth / noteSprite.Texture.DisplayWidth;
                noteSprite.Scale = new Vector2(scale);
            }
        }

        protected virtual void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                directionContainer.Origin = Anchor.BottomCentre;
                directionContainer.Scale = new Vector2(1, -1);
            }
            else
            {
                directionContainer.Origin = Anchor.TopCentre;
                directionContainer.Scale = Vector2.One;
            }
        }

        protected virtual Texture GetTexture(ISkinSource skin) => GetTextureFromLookup(skin, LegacyManiaSkinConfigurationLookups.NoteImage);

        protected Texture GetTextureFromLookup(ISkin skin, LegacyManiaSkinConfigurationLookups lookup)
        {
            string suffix = string.Empty;

            switch (lookup)
            {
                case LegacyManiaSkinConfigurationLookups.HoldNoteHeadImage:
                    suffix = "H";
                    break;

                case LegacyManiaSkinConfigurationLookups.HoldNoteTailImage:
                    suffix = "T";
                    break;
            }

            string noteImage = skin.GetConfig<LegacyManiaSkinConfigurationLookup, string>(
                                   new LegacyManiaSkinConfigurationLookup(Stage?.Columns.Count ?? 4, lookup, Column.Index))?.Value
                               ?? $"mania-note{FallbackColumnIndex}{suffix}";

            return skin.GetTexture(noteImage);
        }
    }
}
