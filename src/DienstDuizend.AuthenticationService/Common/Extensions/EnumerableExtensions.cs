namespace DienstDuizend.AuthenticationService.Common.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate) {
        if (condition) return source.Where(predicate);
        return source;
    }
    
    public static string Join<T>(this IEnumerable<T> enumerable, string separator)
    {
        return string.Join(separator, enumerable);
    }
}