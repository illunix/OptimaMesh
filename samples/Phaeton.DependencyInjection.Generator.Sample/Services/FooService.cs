﻿using Phaeton.DependencyInjection.Generator.Sample.Abstractions.Services;

namespace Phaeton.DependencyInjection.Generator.Sample.Services;

[GenerateInterfaceAndRegisterIt(ServiceLifetime.Singleton)]
public sealed partial class FooService : IFooService
{
    private readonly IBarService _barService;

    public void Bar()
        => _barService.DoSomething();
}