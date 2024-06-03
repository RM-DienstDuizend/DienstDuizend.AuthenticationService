using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Vogen;

namespace DienstDuizend.AuthenticationService.Infrastructure.Filters;

public class VogenSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.GetCustomAttribute<ValueObjectAttribute>() is not { } attribute)
            return;

        // Since we don't hold the actual type, we ca only use the generic attribute
        var type = attribute.GetType();

        if (!type.IsGenericType || type.GenericTypeArguments.Length != 1)
        {
            return;
        }

        var schemaValueObject = context.SchemaGenerator.GenerateSchema(
            type.GenericTypeArguments[0],
            context.SchemaRepository,
            context.MemberInfo, context.ParameterInfo);

        CopyPublicProperties(schemaValueObject, schema);
    }

    private static void CopyPublicProperties<T>(T oldObject, T newObject) where T : class
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        if (ReferenceEquals(oldObject, newObject))
        {
            return;
        }

        var type = typeof(T);
        var propertyList = type.GetProperties(flags);
        if (propertyList.Length <= 0) return;

        foreach (var newObjProp in propertyList)
        {
            var oldProp = type.GetProperty(newObjProp.Name, flags)!;
            if (!oldProp.CanRead || !newObjProp.CanWrite) continue;

            var value = oldProp.GetValue(oldObject);
            newObjProp.SetValue(newObject, value);
        }
    }
}