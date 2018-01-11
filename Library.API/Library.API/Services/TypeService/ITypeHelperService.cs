namespace Library.API.Services.TypeService
{
    public interface ITypeHelperService
    {
        bool TypeHasProperties<T>(string fields);
    }
}