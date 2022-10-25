using System;
using Systems;

namespace Interfaces
{
    public interface IUpdatable
    {
        public UpdateType UpdateType { get; }

        public void CustomUpdate();
    }

    [Flags]
    public enum UpdateType
    {
        GamePlay = 1,
        Paused = 2,
        Always = ~0
    }
}
