using FCG.Pagamentos.Business.Services;
using FCG.Pagamentos.Business.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Infra.Ioc
{
    public static class BusinessExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // Serviços de negócio
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentEventService, PaymentEventService>();
            return services;
        }
    }
}
