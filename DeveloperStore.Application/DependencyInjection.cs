using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DeveloperStore.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    // Register MediatR
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

    // Register FluentValidation validators
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    // Register AutoMapper with profiles from this assembly
    services.AddAutoMapper(cfg =>
    {
      cfg.AddMaps(Assembly.GetExecutingAssembly());
    });

    return services;
  }
}
