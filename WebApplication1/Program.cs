using MassTransit;
using Microsoft.AspNetCore.Mvc;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer()
    .AddSwaggerGen();

builder.Services.AddMediator(cfg => {
    cfg.AddConsumersFromNamespaceContaining<PersonCommandHandler>();
    cfg.SetKebabCaseEndpointNameFormatter();
});

builder.Services.AddScoped<CommandSender>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", async (CommandSender commandSender, [FromQuery] int id) => {
    var result = await commandSender.Send<PersonCommand, int>(new PersonCommand(id));
    return Results.Ok(new
    {
        returnType = result
    });
});

app.Run();