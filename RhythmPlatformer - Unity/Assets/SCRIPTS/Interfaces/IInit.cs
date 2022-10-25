namespace Interfaces
{
    public interface IInit
    {
        public void Init();
    }

    public interface IInit<in T>
    {
        public void Init(T in_T);
    }
}
