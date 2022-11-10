namespace Interfaces_and_Enums
{
    public interface IInit
    {
        public void Init();
    }

    public interface IInit<in T>
    {
        public void Init(T in_T);
    }
    
    public interface IInit<in T, in U>
    {
        public void Init(T in_T, U in_U);
    }
}
