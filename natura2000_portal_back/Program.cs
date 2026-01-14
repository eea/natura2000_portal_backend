using DotNetEnv;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using natura2000_portal_back.Data;
using natura2000_portal_back.Helpers;
using natura2000_portal_back.Hubs;
using natura2000_portal_back.Models;
using natura2000_portal_back.Services;
using System.IO.Compression;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;


// Load local .env file (ignored in Git)
Env.Load("env");

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IDownloadService, DownloadService>();
builder.Services.AddScoped<IInfoService, InfoService>();
builder.Services.AddScoped<ISDFService, SDFService>();

//builder.Services.AddHostedService<FMELongRunningService>();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[] { "text/plain", "application/json", "text/json", "application/octet-stream", "Content-Disposition" };
});
builder.Services.Configure<GzipCompressionProviderOptions>
   (opt =>
   {
       opt.Level = CompressionLevel.SmallestSize;
   }
);

// Add environment variables to Configuration
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json");

var connStr =
    builder.Configuration["BackboneDB_ConnectionString"]
    ?? builder.Configuration.GetConnectionString("N2K_BackboneBackEndContext");

builder.Services.AddDbContext<N2KBackboneContext>(options =>
{
    options.UseSqlServer(connStr);
});

connStr =
    builder.Configuration["ReleasesDB_ConnectionString"]
    ?? builder.Configuration.GetConnectionString("N2K_ReleasesBackEndContext");

builder.Services.AddDbContext<N2KReleasesContext>(options =>
{
    options.UseSqlServer(connStr);
});


builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});


builder.Services.AddControllersWithViews();
builder.Services.Configure<ConfigSettings>(builder.Configuration.GetSection("GeneralSettings"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "N2KBacboneAPI", Version = "v1", Description="This is a REST API developed for Natura Internal Portal" });
});

builder.Services.AddMemoryCache();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddRouting(options =>
{
    options.ConstraintMap.Add("string", typeof(RouteAlphaNumericConstraint));
    options.ConstraintMap.Add("Status", typeof(RouteStatusConstraint));
    options.ConstraintMap.Add("level", typeof(RouteLevelConstraint));
});

var app = builder.Build();
// <snippet_UseWebSockets>
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
app.UseWebSockets(webSocketOptions);

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .WithExposedHeaders("Content-Disposition")
    .AllowCredentials()); // allow credentials

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}
app.UseResponseCompression();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/ws");

app.Run();
