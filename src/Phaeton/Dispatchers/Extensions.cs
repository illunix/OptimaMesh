﻿using Microsoft.Extensions.DependencyInjection;
using Phaeton.Abstractions;

namespace Phaeton.Dispatchers;

public static class Extensions
{
    public static IServiceCollection AddHandlers(
        this IServiceCollection services,
        string project
    )
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(q =>
                q.FullName is not null &&
                q.FullName.Contains(project)
            )
            .ToArray();

        services.Scan(q => q.FromAssemblies(assemblies)
            .AddClasses(q => q.AssignableTo(typeof(ICommandHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Scan(q => q.FromAssemblies(assemblies)
            .AddClasses(q => q.AssignableTo(typeof(IEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Scan(q => q.FromAssemblies(assemblies)
            .AddClasses(q => q.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    public static IServiceCollection AddDispatchers(this IServiceCollection services)
        => services
            .AddSingleton<IMediator, Mediator>()
            .AddSingleton<ICommandDispatcher, InMemoryCommandDispatcher>()
            .AddSingleton<IEventDispatcher, InMemoryEventDispatcher>()
            .AddSingleton<IQueryDispatcher, InMemoryQueryDIspatcher>();
}