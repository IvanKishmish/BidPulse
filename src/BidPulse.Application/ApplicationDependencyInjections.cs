using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BidPulse.Application;

public static class ApplicationDependencyInjections
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddBidPulseApplicationHandlers();
        services.AddBidPulseApplicationBehaviors();
        services.AddValidatorsFromAssembly(typeof(ApplicationDependencyInjections).Assembly);
        
        return services;
    }
}