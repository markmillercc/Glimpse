using Glimpse.Db;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using MediatR.Endpoints;
using Glimpse.Operations;
using Glimpse.Plumbing;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

var regex = new Regex("[^a-zA-Z0-9 -]");
builder.Services.AddSwaggerGen(options => { options.CustomSchemaIds(type => regex.Replace(type.ToString(), "")); });

builder.Services.AddDbContext<GlimpseDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddLogging(config => config.AddDebug());

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddMediatREndoints(cfg => 
{
    cfg.AddRouteGroupBuilder(a => a.AddEndpointFilter<ModelStateValidationEndpointFilter>());
    cfg.AddRouteGroupBuilder(a => a.AddEndpointFilter<OperationValidationEndpointFilter>());
    //cfg.UseEndpointHandlerDelegateFactory<HandlerFactory>();
});// cfg => 
//{
//    cfg.AddRouteGroupBuilder(a => a.AddEndpointFilter<ModelStateValidationEndpointFilter2>().AddEndpointFilter<ModelStateValidationEndpointFilter>());
//    cfg.MapGet<GetProfile.Query>(handler: async (IMediator mediator, [AsParameters] GetProfile.Query request) =>
//    {
//        var response = await mediator.Send(request);
//        return Results.Ok(response.Result);
//    });
//});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>(app.Logger, app.Environment.IsDevelopment());
app.MapMediatREndpoints();

app.Run();

public partial class Program { }