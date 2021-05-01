using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessProcessPipeline.Application.Commands;
using BusinessProcessPipeline.Infrastructure.Behaviours;
using BusinessProcessPipeline.Queries;
using BusinessProcessPipeline.Queries.Behaviours;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BusinessProcessPipeline
{
    public class UnitTest1
    {
        private readonly IServiceCollection services;
        private readonly IMediator mediator;

        public UnitTest1()
        {
            services = new ServiceCollection();
            services.AddMediatR(typeof(IBusinessRequest).Assembly);

            // Business process responders
            services.AddTransient<IBusinessProcessResponder<MyBusinessRequest>, MyBusinessRequestResponder>();
            
            // Command behaviours
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InputValidationBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DispatchDomainEventsBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationReportsBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EDIMessagingBehaviour<,>));
            
            // Query behaviours
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(QueryBehaviour1<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(QueryBehaviour2<,>));
            
            mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
            
            
        }
        
        [Fact]
        public async Task RunMyBusinessRequest()
        {
            var result = await mediator.Send(new MyBusinessRequest() {ReturnValue = false}) as BusinessProcessResult;
            if (result.Success == false)
            {
                
            }
            else
            {
                
            }
        }
        
        [Fact]
        public async Task RunMyInternalCommand()
        {
            var result = await mediator.Send(new MyInternalCommand());
        }
        
        [Fact]
        public async Task RunMyQuery()
        {
            var result = await mediator.Send(new MyQuery() {ReturnValue = true});
        }
    }
}