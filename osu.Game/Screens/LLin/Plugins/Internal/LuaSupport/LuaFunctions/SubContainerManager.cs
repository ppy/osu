using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.LLin.Plugins.Internal.LuaSupport.LuaFunctions
{
    public partial class SubContainerManager : Container<SubContainer>
    {
        //region LuaAPI

        public SubContainer GetContainer(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new InvalidOperationException("Container name cannot be null or empty");

            var child = this.Children.FirstOrDefault(c => c != null && c.ContainerName == name, null);

            if (child != null && (child.LifetimeEnd == LatestTransformEndTime || child.LifetimeEnd == double.MinValue))
            {
                child.Expire();
                child = null;
            }

            if (child != null) return child;

            child = new SubContainer(this, name)
            {
                Name = name
            };

            Add(child);
            return child;
        }

        //endregion

        private SubContainerManager instance;

        public SubContainerManager()
        {
            RelativeSizeAxes = Axes.Both;
            instance = this;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
        }
    }

    public partial class SubContainer : Container
    {
        public string ContainerName { get; internal set; }

        public SubContainer(SubContainerManager manager, string name)
        {
            ContainerName = Name = name;
        }

        public Action? PostUpdate;

        protected override void Update()
        {
            base.Update();

            PostUpdate?.Invoke();
        }
    }
}
