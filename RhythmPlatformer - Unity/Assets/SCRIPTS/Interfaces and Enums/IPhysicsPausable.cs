using UnityEngine;

public interface IPhysicsPausable
{
    public Rigidbody2D PausableRigidbody { get; }
    public Vector2 Velocity { get; }

    public void TogglePausePhysics(bool in_paused) => PausableRigidbody.velocity = in_paused ? Vector2.zero : Velocity;

    public void RegisterPhysicsPausable();
    public void DeregisterPhysicsPausable();
}
