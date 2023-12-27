using Microsoft.EntityFrameworkCore;
using mvcMovie.Data;
using mvcMovie.Services;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.Extensions.NETCore.Setup;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using mvcMovie;

var builder = WebApplication.CreateBuilder(args);

// Configure AWS options
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);

// Register AWS Simple Systems Management Service
builder.Services.AddAWSService<IAmazonSimpleSystemsManagement>(awsOptions);

// Register the custom configuration as a singleton
builder.Services.AddSingleton<MyCustomConfiguration>();

// Register the ParameterStoreHostedService as a hosted service
builder.Services.AddHostedService<ParameterStoreHostedService>();

// Register other AWS services
builder.Services.AddAWSService<IAmazonS3>(awsOptions);
builder.Services.AddAWSService<IAmazonDynamoDB>(awsOptions);

// Register Entity Framework context for SQL Server
// The actual connection string will be set by the ParameterStoreHostedService
builder.Services.AddDbContext<MovieDbContext>((serviceProvider, options) =>
{
    var customConfig = serviceProvider.GetRequiredService<MyCustomConfiguration>();
    if (string.IsNullOrWhiteSpace(customConfig.Connection2RDS))
    {
        throw new InvalidOperationException("Connection string is not set.");
    }
    options.UseSqlServer(customConfig.Connection2RDS);
});

// Register DynamoDB context
builder.Services.AddSingleton(serviceProvider =>
{
    var dynamoDbClient = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
    return new DynamoDbContext(dynamoDbClient);
});

// Add controllers and views with Razor runtime compilation
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Configure AWSConfig section
builder.Services.Configure<AWSConfig>(builder.Configuration.GetSection("AWS"));

// Register S3Service with DI container
builder.Services.AddTransient<S3Service>();
builder.Services.AddTransient<DynamoDbContext>();


// Remaining service registrations
// ...

// Configure authentication with cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
    });

// Add cookie policy configuration
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Define the default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
