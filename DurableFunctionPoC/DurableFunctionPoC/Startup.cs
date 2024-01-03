using DurableFunctionPoC;
using DurableFunctionPoC.Interfaces;
using DurableFunctionPoC.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace DurableFunctionPoC
{
    public class Startup: FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();
            builder.Services.AddTransient<IConcurRunbookProcessor, ConcurRunbookProcessor>();
            builder.Services.AddTransient<ISalesforceRunbookProcessor, SalesforceRunbookProcessor>();
            builder.Services.AddTransient<IIntactRunbookProcessor, IntactRunbookProcessor>();
            builder.Services.AddTransient<ISomeRunbookProcessor, SomeRunbookProcessor>();
        }
    }
}
