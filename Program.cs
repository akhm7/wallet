using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Primitives;
using wallet;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped((_) => new HMACSHA1(Encoding.UTF8.GetBytes("01234567890123456789012345678901")));
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton(new WalletRepository(new Wallet[]{
    new Wallet("1", false),
    new Wallet("2", true),
    new Wallet("3", false),
    new Wallet("4", true),
    new Wallet("5", false),
    new Wallet("6", false),
}));
builder.Services.AddSingleton(new TransactionLogger());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.Use(async(context, next) =>
{
    var hasher = context.RequestServices.GetService<HMACSHA1>();

    if (!context.Request.Headers.TryGetValue("X-Digest", out var xdigest) || hasher == null)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
        await context.Response.CompleteAsync();
        return;
    }

    using var memory = new MemoryStream();
    await context.Request.Body.CopyToAsync(memory);
    memory.Seek(0, SeekOrigin.Begin);

    var hash = System.Convert.ToHexString(
        await hasher.ComputeHashAsync(
            memory
            )
        );

    if (string.Compare(xdigest.First(), hash, true) != 0)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        await context.Response.CompleteAsync();
        return;
    }

    memory.Seek(0, SeekOrigin.Begin);
    context.Request.Body = memory;
    await next(context);
});

app.MapControllers();

app.Run();
