using BidPulse.Application.Common.Interfaces;
using BidPulse.Persistence.Context;
using BidPulse.Persistence.Interceptors;
using BidPulse.Persistence.UOF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BidPulse.Persistence;

public static class PersistenceDependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<UpdateAuditableEntitiesInterceptor>();
        
        // services.AddDbContext<AppDbContext>(options => 
        //     options.UseNpgsql(configuration.GetConnectionString("DB_CONNECTION_STRING")));

        services.AddPooledDbContextFactory<AppDbContext>((serviceProvider, options) =>
        {
            var auditingInterceptor = serviceProvider.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
            
            options.UseNpgsql(configuration.GetConnectionString("DB_CONNECTION_STRING"))
                .AddInterceptors(auditingInterceptor);
        });
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}