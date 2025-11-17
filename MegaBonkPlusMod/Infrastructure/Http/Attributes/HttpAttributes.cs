using System;

namespace MegaBonkPlusMod.Infrastructure.Http.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class HttpGetAttribute : Attribute
{
    public HttpGetAttribute(string route)
    {
        Route = route;
    }

    public string Route { get; }
}

[AttributeUsage(AttributeTargets.Method)]
public class HttpPostAttribute : Attribute
{
    public HttpPostAttribute(string route)
    {
        Route = route;
    }

    public string Route { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public class ApiControllerAttribute : Attribute
{
    public ApiControllerAttribute(string basePath = "")
    {
        BasePath = basePath;
    }

    public string BasePath { get; }
}