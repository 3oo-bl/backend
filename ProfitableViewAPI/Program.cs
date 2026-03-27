using ProfitableViewCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

if (args.Contains("--fake"))
{
    app.MapFakeEndpoints(app.Logger);
}
else
{
    app.MapEndpoints();
}

app.Run();
