using System;
using System.Text;
using NLua;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Screens.LLin.Plugins.Internal.LuaSupport.Graphics;
using osu.Game.Screens.LLin.Plugins.Internal.LuaSupport.LuaFunctions;
using osu.Game.Screens.LLin.Plugins.Types;

namespace osu.Game.Screens.LLin.Plugins.Internal.LuaSupport
{
    public partial class LuaPlugin : BindableControlledPlugin, IKeyBindingHandler<GlobalAction>
    {
        protected override Drawable CreateContent()
        {
            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Name = "Initial Content"
            };
        }

        public override int Version { get; } = 10;

        protected override bool OnContentLoaded(Drawable content) => true;

        protected override bool PostInit() => true;

        public Lua? Lua;
        private OsuTextBox? textBox;

        public override TargetLayer Target => TargetLayer.Foreground;

        public LuaPlugin()
        {
            RelativeSizeAxes = Axes.Both;

            Author = "mfosu";
            Name = "Lua控制台";

            Flags.Add(PluginFlags.CanDisable);

            Depth = -2;
        }

        private FillFlowContainer loggingTextFlow = null!;
        private OsuScrollContainer loggingScroll = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            this.Alpha = 0;

            InternalChildren = new Drawable[]
            {
                textBox = new OsuTextBox
                {
                    PlaceholderText = "在此输入Lua命令",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    RelativePositionAxes = Axes.Y,
                    Width = 0.8f,
                    Y = -0.2f
                },
                loggingScroll = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 0.8f,
                    Height = 0.72f,
                    ScrollContent = { Anchor = Anchor.BottomLeft, Origin = Anchor.BottomLeft },
                    Child = new OsuContextMenuContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y
                    }.WithChild(loggingTextFlow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical
                    })
                }
            };

            try
            {
                //Initialize Lua
                Log($"Initializing NLua...");

                Lua?.Dispose();
                Lua = new Lua();

                Lua.State.Encoding = Encoding.UTF8;

                Lua.LoadCLRPackage();
                registerLuaFunctions();
            }
            catch (Exception e)
            {
                Log($"Unable to initialize Lua: {e} -> {e.Message} :: {e.InnerException}", LogLevel.Error);
                Logger.Log(e.StackTrace);

                if (e.InnerException is Exception innerException)
                {
                    Log($"With inner exception: {innerException} -> {innerException.Message}", LogLevel.Error);
                }

                return;
            }

            textBox.OnCommit += (sender, isNewText) =>
            {
                try
                {
                    RunLuaCommand(sender.Text);
                }
                catch (Exception e)
                {
                    Log($"Unable to execute command: {e.Message} -> {e}", LogLevel.Error);
                    Logger.Log(e.StackTrace);

                    if (e.InnerException is Exception innerException)
                        Log($"\n\nAnd probably caused by: {innerException.Message} -> {innerException}", LogLevel.Error);
                }
            };

            Log("Done!");
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (haveUnreadMessage && shouldTrack && !loggingScroll.IsScrolledToEnd())
            {
                haveUnreadMessage = false;
                loggingScroll.ScrollToEnd();
            }
        }

        private bool haveUnreadMessage;
        private bool shouldTrack;

        public void RunLuaCommand(string cmd)
        {
            if (Lua == null) throw new NullDependencyException("Lua not initialized!");

            if (string.IsNullOrEmpty(cmd)) return;

            Log(cmd, newSegment: true);

            Lua.UseTraceback = true;
            object[]? retValues = Lua.DoString(cmd);

            if (retValues != null)
            {
                string values = "";

                foreach (object value in retValues)
                    values += $"'{value}', ";

                if (!string.IsNullOrEmpty(values))
                    Log($"Command returned with value(s): {values}");
            }
        }

        internal void Log(string? text, LogLevel level = LogLevel.Verbose, bool newSegment = false)
        {
            if (text == null) return;

            Logger.Log(text);

            loggingTextFlow.Add(new OutputLine(text, level == LogLevel.Error, newSegment));
            haveUnreadMessage = true;
            shouldTrack = loggingScroll.IsScrolledToEnd(10);
        }

        internal void ClearConsole()
        {
            loggingTextFlow.Clear();
        }

        [Resolved]
        private IImplementLLin lLin { get; set; } = null!;

        private void registerLuaFunctions()
        {
            if (Lua == null) throw new NullDependencyException("Lua not initialized!");

            var baseFunction = new BaseFunctions(this);
            var utilFunction = new SubContainerManager();
            const string base_function_name = "BaseFunctions";

            Add(baseFunction);
            Add(utilFunction);

            Lua[base_function_name] = baseFunction;
            Lua["Containers"] = utilFunction;
            Lua["DrawableUtil"] = new DrawableUtils();
            Lua["Player"] = lLin;

            Lua.DoString(@$"
                print = function (str) {base_function_name}:Print(str) end
                clear = function (str) {base_function_name}:ClearConsole() end
            ");

            string generatedImports = DefaultImportGenerator.Generate(new[]
            {
                "osu.Framework",
                "osu.Game"
            }, true);

            Log("Importing commons...");
            Lua.DoString(generatedImports);
        }

        protected override void Dispose(bool isDisposing)
        {
            Lua?.Dispose();
            Lua = null;

            base.Dispose(isDisposing);
        }

        public override bool Enable()
        {
            if (!base.Enable()) return false;

            this.Alpha = 0.01f;
            this.FadeIn(300, Easing.OutQuint);
            return true;
        }

        public override bool Disable()
        {
            if (!base.Disable()) return false;

            this.FadeOut(300, Easing.OutQuint);
            return true;
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            return textBox?.HasFocus ?? false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
