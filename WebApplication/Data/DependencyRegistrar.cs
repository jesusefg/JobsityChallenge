using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WebApplication.Data.Entities;
using WebApplication.Data.Interfaces;

namespace WebApplication.Data
{
    public static class DependencyRegistrar
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<ISQLRepository<ChatHistory>, SQLRepository<ChatHistory>>();
            services.AddTransient<ISQLRepository<IdentityUser>, SQLRepository<IdentityUser>>();
            services.AddTransient<ISQLRepository<ChatRoom>, SQLRepository<ChatRoom>>();
        }
    }
}
