namespace Interfaces_and_Enums
{
    public interface IUpdatable
    {
        public UpdateType UpdateType { get; }

        public void CustomUpdate();
    }
}
