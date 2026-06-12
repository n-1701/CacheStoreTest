using NasNeuron.ClaimsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Engine services. RuleStore and ClaimStore are singletons so the in-memory
// ruleset and history survive across requests for the lifetime of the POC.
builder.Services.AddSingleton<JdmBuilder>();
builder.Services.AddSingleton<ZipPackager>();
builder.Services.AddSingleton<S3Uploader>();
builder.Services.AddSingleton<RuleStore>();
builder.Services.AddSingleton<ClaimStore>();

builder.Services.AddHttpClient<ZenAgentClient>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(30);
});

// Allow the Angular dev server to call the API. Tighten this in production.
const string CorsPolicy = "frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                      ?? ["http://localhost:4200"];
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve the compiled Angular app from wwwroot.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(CorsPolicy);
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Client-side routes (e.g. /members, /claims) fall back to the SPA entry point.
app.MapFallbackToFile("index.html");

app.Run();
