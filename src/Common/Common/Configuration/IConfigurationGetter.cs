namespace Common.Configuration
{
    public interface IConfigurationGetter
    {
        T GetOptions<T>();
    }
}
