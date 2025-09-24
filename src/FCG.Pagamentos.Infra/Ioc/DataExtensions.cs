using FCG.Pagamentos.Business.Interfaces;
using FCG.Pagamentos.Business.Services;
using FCG.Pagamentos.Business.Services.Interface;
using FCG.Pagamentos.Infra.Data;
using FCG.Pagamentos.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Pagamentos.Infra.Ioc
{
    public static class DataExtensions
    {
        public static IServiceCollection AddDataService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentEventRepository, PaymentEventRepository>();
            return services;
        }
    }
}
