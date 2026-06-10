// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Commands;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyStoryboardEncoder
    {
        private readonly Storyboard storyboard;

        public LegacyStoryboardEncoder(Storyboard storyboard)
        {
            this.storyboard = storyboard;
        }

        #region Storyboards embedded in beatmaps

        public void EncodeGeneralToBeatmap(TextWriter writer)
        {
            writer.WriteLine(FormattableString.Invariant($@"UseSkinSprites: {(storyboard.UseSkinSprites ? '1' : '0')}"));
            writer.WriteLine(FormattableString.Invariant($@"WidescreenStoryboard: {(storyboard.Beatmap.WidescreenStoryboard ? '1' : '0')}"));
        }

        public void EncodeEventsToBeatmap(TextWriter writer)
            => encodeEvents(writer, StoryboardElementSource.Beatmap);

        #endregion

        #region Standalone storyboards

        public void EncodeStandaloneStoryboard(TextWriter writer)
        {
            writer.WriteLine(@"[Events]");
            encodeEvents(writer, StoryboardElementSource.Shared);
        }

        #endregion

        private void encodeEvents(TextWriter writer, StoryboardElementSource target)
        {
            // https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/GameModes/Edit/Modes/EditorModeDesign.cs#L189
            // https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/GameplayElements/Events/EventManager.cs#L368
            writer.WriteLine(@"// Background and Video events");

            if (target == StoryboardElementSource.Beatmap)
            {
                // https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/GameplayElements/HitObjectManager_LoadSave.cs#L1499
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    @"{0},{1},""{2}"",{3},{4}",
                    (int)LegacyEventType.Background, 0, storyboard.BeatmapInfo.Metadata.BackgroundFile, storyboard.BackgroundOffset.X, storyboard.BackgroundOffset.Y));
            }

            // https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/GameplayElements/HitObjectManager_LoadSave.cs#L1496
            foreach (var video in storyboard.GetLayer(@"Video").Elements.OfType<StoryboardVideo>().Where(v => v.Source == target))
            {
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    @"{0},{1},""{2}""",
                    nameof(LegacyEventType.Video), video.StartTime, video.Path));
                encodeCommands(writer, video);
            }

            foreach (var legacyLayer in Enum.GetValues<LegacyStoryLayer>().Except(LegacyStoryLayer.Video.Yield()))
            {
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    @"// Storyboard Layer {0} ({1})",
                    (int)legacyLayer,
                    legacyLayer));
                string layerName = legacyLayer.ToString();
                var layer = storyboard.GetLayer(layerName);
                encodeSpritesFromLayer(writer, layer, target);
            }

            // https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/GameplayElements/HitObjectManager_LoadSave.cs#L1478-L1481
            writer.WriteLine(@"// Storyboard Sound Samples");

            foreach (var legacyLayer in Enum.GetValues<LegacyStoryLayer>().Except(LegacyStoryLayer.Video.Yield()))
            {
                string layerName = legacyLayer.ToString();
                var layer = storyboard.GetLayer(layerName);

                foreach (var sample in layer.Elements.OfType<StoryboardSampleInfo>().Where(s => s.Source == target))
                {
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        @"{0},{1},{2},""{3}"",{4}",
                        nameof(LegacyEventType.Sample),
                        sample.StartTime,
                        (int)legacyLayer,
                        sample.Path,
                        sample.Volume));
                }
            }
        }

        private void encodeSpritesFromLayer(TextWriter writer, StoryboardLayer layer, StoryboardElementSource target)
        {
            foreach (var element in layer.Elements.Where(elem => elem.Source == target))
            {
                LegacyOrigins origin;

                switch (element)
                {
                    case StoryboardAnimation animation:
                    {
                        origin = convertOrigin(animation.Origin);
                        // https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/GameplayElements/HitObjectManager_LoadSave.cs#L1505-L1507
                        writer.WriteLine(string.Format(
                            CultureInfo.InvariantCulture,
                            @"{0},{1},{2},""{3}"",{4},{5},{6},{7},{8}",
                            nameof(LegacyEventType.Animation),
                            layer.Name,
                            origin,
                            animation.Path,
                            animation.InitialPosition.X,
                            animation.InitialPosition.Y,
                            animation.FrameCount,
                            animation.FrameDelay,
                            animation.LoopType));

                        encodeCommands(writer, animation);
                        break;
                    }

                    case StoryboardSprite sprite:
                    {
                        origin = convertOrigin(sprite.Origin);
                        // https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/GameplayElements/HitObjectManager_LoadSave.cs#L1502
                        writer.WriteLine(string.Format(
                            CultureInfo.InvariantCulture,
                            @"{0},{1},{2},""{3}"",{4},{5}",
                            nameof(LegacyEventType.Sprite),
                            layer.Name,
                            origin,
                            sprite.Path,
                            sprite.InitialPosition.X,
                            sprite.InitialPosition.Y));

                        encodeCommands(writer, sprite);
                        break;
                    }
                }
            }
        }

        private void encodeCommands(TextWriter writer, StoryboardSprite sprite)
        {
            foreach (var loopingGroup in sprite.LoopingGroups)
            {
                writer.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    @" L,{0},{1}",
                    loopingGroup.StartTime, loopingGroup.TotalIterations));
                foreach (var command in loopingGroup.AllCommands)
                    // see `StoryboardLoopingCommand` ctor for why `relativeToTime` is passed
                    encodeCommand(writer, command, 2, relativeToTime: loopingGroup.StartTime);
            }

            foreach (var command in sprite.Commands.AllCommands)
                encodeCommand(writer, command, 1);

            foreach (var triggerGroup in sprite.TriggerGroups)
            {
                // https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/GameplayElements/HitObjectManager_LoadSave.cs#L1564-L1572
                writer.Write(string.Format(
                    CultureInfo.InvariantCulture,
                    @" T,{0}",
                    triggerGroup.TriggerName));

                if (triggerGroup.TriggerEndTime != 0)
                {
                    writer.Write(string.Format(
                        CultureInfo.InvariantCulture,
                        @",{0},{1}",
                        triggerGroup.TriggerStartTime, triggerGroup.TriggerEndTime));
                }

                if (triggerGroup.GroupNumber != 0)
                {
                    writer.Write(string.Format(CultureInfo.InvariantCulture, @",{0}", -triggerGroup.GroupNumber));
                }

                writer.WriteLine();

                foreach (var command in triggerGroup.AllCommands)
                    encodeCommand(writer, command, 2);
            }
        }

        private void encodeCommand(TextWriter writer, IStoryboardCommand command, int depth, double relativeToTime = 0)
        {
            for (int i = 0; i < depth; ++i)
                writer.Write(' ');

            string typeAcronym;
            string details;

            if (command is IStoryboardLoopingCommand loopingCommand)
                command = loopingCommand.OriginalCommand;

            // https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/GameplayElements/HitObjectManager_LoadSave.cs#L1546-L1550
            // https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/GameplayElements/HitObjectManager_LoadSave.cs#L1690-L1730
            switch (command)
            {
                case StoryboardVectorScaleCommand vectorScale:
                    typeAcronym = @"V";
                    details = vectorScale.StartValue == vectorScale.EndValue
                        ? string.Format(CultureInfo.InvariantCulture, @"{0},{1}", vectorScale.StartValue.X, vectorScale.StartValue.Y)
                        : string.Format(CultureInfo.InvariantCulture,
                            @"{0},{1},{2},{3}",
                            vectorScale.StartValue.X, vectorScale.StartValue.Y, vectorScale.EndValue.X, vectorScale.EndValue.Y);
                    break;

                case StoryboardAlphaCommand fade:
                    typeAcronym = @"F";
                    details = fade.StartValue == fade.EndValue
                        ? fade.StartValue.ToString(CultureInfo.InvariantCulture)
                        : string.Format(CultureInfo.InvariantCulture,
                            @"{0},{1}",
                            fade.StartValue,
                            fade.EndValue);
                    break;

                case StoryboardRotationCommand rotation:
                    typeAcronym = @"R";
                    details = rotation.StartValue == rotation.EndValue
                        ? rotation.StartValue.ToString(CultureInfo.InvariantCulture)
                        : string.Format(CultureInfo.InvariantCulture,
                            @"{0},{1}",
                            float.DegreesToRadians(rotation.StartValue),
                            float.DegreesToRadians(rotation.EndValue));
                    break;

                case StoryboardScaleCommand scale:
                    typeAcronym = @"S";
                    details = scale.StartValue == scale.EndValue
                        ? scale.StartValue.ToString(CultureInfo.InvariantCulture)
                        : string.Format(CultureInfo.InvariantCulture,
                            @"{0},{1}",
                            scale.StartValue,
                            scale.EndValue);
                    break;

                // stable has M commands that combine X and Y movement, but we decompose those into X/Y with no way to undo

                case StoryboardXCommand movementX:
                    typeAcronym = @"MX";
                    details = movementX.StartValue == movementX.EndValue
                        ? movementX.StartValue.ToString(CultureInfo.InvariantCulture)
                        : string.Format(CultureInfo.InvariantCulture,
                            @"{0},{1}",
                            movementX.StartValue,
                            movementX.EndValue);
                    break;

                case StoryboardYCommand movementY:
                    typeAcronym = @"MY";
                    details = movementY.StartValue == movementY.EndValue
                        ? movementY.StartValue.ToString(CultureInfo.InvariantCulture)
                        : string.Format(CultureInfo.InvariantCulture,
                            @"{0},{1}",
                            movementY.StartValue,
                            movementY.EndValue);
                    break;

                case StoryboardColourCommand colour:
                    typeAcronym = @"C";
                    details = colour.StartValue == colour.EndValue
                        ? string.Format(CultureInfo.InvariantCulture,
                            @"{0},{1},{2}",
                            (int)(colour.StartValue.R * 255), (int)(colour.StartValue.G * 255), (int)(colour.StartValue.B * 255))
                        : string.Format(CultureInfo.InvariantCulture,
                            @"{0},{1},{2},{3},{4},{5}",
                            (int)(colour.StartValue.R * 255), (int)(colour.StartValue.G * 255), (int)(colour.StartValue.B * 255),
                            (int)(colour.EndValue.R * 255), (int)(colour.EndValue.G * 255), (int)(colour.EndValue.B * 255));
                    break;

                case StoryboardFlipHCommand:
                    typeAcronym = @"P";
                    details = @"H";
                    break;

                case StoryboardFlipVCommand:
                    typeAcronym = @"P";
                    details = @"V";
                    break;

                case StoryboardBlendingParametersCommand:
                    typeAcronym = @"P";
                    details = @"A";
                    break;

                default:
                    return;
            }

            writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                @"{0},{1},{2},{3},{4}",
                typeAcronym,
                (int)command.Easing,
                command.StartTime - relativeToTime,
                command.StartTime == command.EndTime ? null : command.EndTime - relativeToTime,
                details));
        }

        private LegacyOrigins convertOrigin(Anchor anchor)
        {
            switch (anchor)
            {
                case Anchor.TopLeft:
                    return LegacyOrigins.TopLeft;

                case Anchor.TopCentre:
                    return LegacyOrigins.TopCentre;

                case Anchor.TopRight:
                    return LegacyOrigins.TopRight;

                case Anchor.CentreLeft:
                    return LegacyOrigins.CentreLeft;

                case Anchor.Centre:
                    return LegacyOrigins.Centre;

                case Anchor.CentreRight:
                    return LegacyOrigins.CentreRight;

                case Anchor.BottomLeft:
                    return LegacyOrigins.BottomLeft;

                case Anchor.BottomCentre:
                    return LegacyOrigins.BottomCentre;

                case Anchor.BottomRight:
                    return LegacyOrigins.BottomRight;

                default:
                    return LegacyOrigins.TopLeft;
            }
        }
    }
}
