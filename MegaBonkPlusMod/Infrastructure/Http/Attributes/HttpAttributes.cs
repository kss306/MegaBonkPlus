using System;

namespace MegaBonkPlusMod.Infrastructure.Http.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class HttpGetAttribute : Attribute
{
    public string Route { get; }

    public HttpGetAttribute(string route)
    {
        Route = route;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class HttpPostAttribute : Attribute
{
    public string Route { get; }

    public HttpPostAttribute(string route)
    {
        Route = route;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ApiControllerAttribute : Attribute
{
    public string BasePath { get; }

    public ApiControllerAttribute(string basePath = "")
    {
        BasePath = basePath;
    }
}