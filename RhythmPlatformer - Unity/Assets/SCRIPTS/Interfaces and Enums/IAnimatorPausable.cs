using UnityEngine;

namespace Interfaces_and_Enums
{
    public interface IAnimatorPausable
    {
        public Animator Animator { get; set; }

        public void ToggleAnimatorPause(bool in_paused) => Animator.speed = in_paused ? 0 : 1;
    }
}
