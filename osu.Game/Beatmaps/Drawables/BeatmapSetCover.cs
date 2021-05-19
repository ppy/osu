// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Online;

namespace osu.Game.Beatmaps.Drawables
{
    [LongRunningLoad]
    public class BeatmapSetCover : CompositeDrawable
    {
        private readonly BeatmapSetInfo set;
        private readonly BeatmapSetCoverType type;

        private readonly Sprite coverSprite;

        public BeatmapSetCover(BeatmapSetInfo set, BeatmapSetCoverType type = BeatmapSetCoverType.Cover)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            this.set = set;
            this.type = type;

            InternalChild = coverSprite = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
            };
        }

        private IBindable<bool> userAllowedExplicitContent;

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            string resource = null;

            switch (type)
            {
                case BeatmapSetCoverType.Cover:
                    resource = set.OnlineInfo.Covers.Cover;
                    break;

                case BeatmapSetCoverType.Card:
                    resource = set.OnlineInfo.Covers.Card;
                    break;

                case BeatmapSetCoverType.List:
                    resource = set.OnlineInfo.Covers.List;
                    break;
            }

            if (resource != null)
            {
                coverSprite.Texture = textures.Get(resource);

                // update fill aspect ratio with sprite's for FillMode to work on usages.
                FillAspectRatio = coverSprite.FillAspectRatio;
            }
        }

        [Resolved]
        private IExplicitContentPermission explicitPermission { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (set.OnlineInfo.HasExplicitContent)
            {
                userAllowedExplicitContent = explicitPermission.UserAllowed.GetBoundCopy();
                userAllowedExplicitContent.BindValueChanged(allowed =>
                {
                    if (allowed.NewValue)
                        coverSprite.FadeIn(300, Easing.OutQuint);
                    else
                        coverSprite.FadeOut(200, Easing.OutQuint);
                }, true);

                coverSprite.FinishTransforms();
            }
        }
    }

    public enum BeatmapSetCoverType
    {
        Cover,
        Card,
        List,
    }
}
