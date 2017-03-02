using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Graphics.Containers;
using OpenTK.Input;
using osu.Framework.Input;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableBash : DrawableTaikoHitObject
    {
        private const float outer_scale = 3f;

        protected virtual List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        private HitCirclePiece bodyPiece;

        private Container bashOuterRingContainer;
        private Sprite bashOuterRing;

        private Container bashInnerRingContainer;
        private Sprite bashInnerRing;

        private Container explodeContainer;
        private Container flashContainer;

        private int userHits;
        private bool ringsVisible;

        public DrawableBash(Bash spinner)
            : base(spinner)
        {
            Size = new Vector2(128);

            Children = new Drawable[]
            {
                explodeContainer = new Container()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(237, 171, 0, 255)
                },
                bashOuterRingContainer = new Container()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    AutoSizeAxes = Axes.Both,

                    Scale = new Vector2(outer_scale),

                    Alpha = 0f,

                    Children = new Drawable[]
                    {
                        bashOuterRing = new Sprite()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            Size = new Vector2(151)
                        }
                    }
                },
                bashInnerRingContainer = new Container()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    AutoSizeAxes = Axes.Both,

                    Alpha = 0f,

                    Children = new Drawable[]
                    {
                        bashInnerRing = new Sprite()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            Size = new Vector2(151)
                        }
                    }
                },
                flashContainer = new CircularContainer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Size = new Vector2(141),
                    Scale = new Vector2(outer_scale),

                    Alpha = 0,

                    Children = new[]
                    {
                        new Box()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,

                            Colour = Color4.White
                        }
                    }
                },
                bodyPiece = new SpinnerPiece(),
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            bashOuterRing.Texture = textures.Get(@"Play/Taiko/bash-outer-ring");
            bashInnerRing.Texture = textures.Get(@"Play/Taiko/bash-inner-ring");
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (Judgement.Result.HasValue)
                return false;

            if (!Keys.Contains(args.Key))
                return false;

            if (Time.Current < HitObject.StartTime)
                return false;

            UpdateJudgement(true);

            return true;
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            Bash spinner = HitObject as Bash;
            TaikoJudgementInfo taikoJudgement = Judgement as TaikoJudgementInfo;

            if (userTriggered)
            {
                if (Time.Current < HitObject.StartTime)
                    return;

                userHits++;

                ExplodePiece ep;
                explodeContainer.Add(ep = new ExplodePiece());

                ep.FadeIn();
                ep.ScaleTo(3f, 200);
                ep.FadeOut(200);
                ep.Expire();

                bashInnerRingContainer.ScaleTo(1f + (outer_scale - 1) * userHits / spinner.RequiredHits, 50);

                if (userHits == spinner.RequiredHits)
                {
                    Judgement.Result = HitResult.Hit;
                    taikoJudgement.Score = TaikoScoreResult.Great;
                }

            }
            else
            {
                if (Judgement.TimeOffset < 0)
                    return;

                if (userHits > spinner.RequiredHits / 2)
                {
                    Judgement.Result = HitResult.Hit;
                    taikoJudgement.Score = TaikoScoreResult.Good;
                }
                else
                    Judgement.Result = HitResult.Miss;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded)
                return;

            base.UpdateState(state);

            const double flash_in = 200;

            switch (State)
            {
                case ArmedState.Idle:
                    break;
                case ArmedState.Miss:
                    break;
                case ArmedState.Hit:
                    bodyPiece.ScaleTo(1.5f, flash_in);
                    bodyPiece.FadeOut(flash_in);

                    bashOuterRingContainer.FadeOut(flash_in);
                    bashInnerRingContainer.FadeOut(flash_in);

                    explodeContainer.FadeOut(flash_in);

                    flashContainer.FadeIn();
                    flashContainer.FadeOut(flash_in);

                    Delay(flash_in * 2);
                    break;
            }
        }

        protected override void Update()
        {
            MoveToOffset(Math.Min(Time.Current, HitObject.StartTime));

            if (Time.Current >= HitObject.StartTime && !ringsVisible)
            {
                bashOuterRingContainer.FadeIn(200);
                bashInnerRingContainer.FadeIn(200);
                ringsVisible = true;
            }
        }
    }
}
