using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Byd.CoreExtensions;
using Byd.CoreExtensions.Extensions;
using Byd.CoreExtensions.Options;
using Byd.FileManagement;
using Byd.GtfsRealtimeManagement.EntityFrameworkCore;
using Byd.GtfsRealtimeManagement.MultiTenancy;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Swashbuckle;

namespace Byd.GtfsRealtimeManagement;

[DependsOn(
    typeof(GtfsRealtimeManagementApplicationModule),
    typeof(GtfsRealtimeManagementEntityFrameworkCoreModule),
    typeof(GtfsRealtimeManagementHttpApiModule),
    typeof(AbpAspNetCoreMvcUiMultiTenancyModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpAutofacModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
)]
public class GtfsRealtimeManagementHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        ServiceConfigurationContextExtension.ResolveSchemaAndPrefix();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        Configure<CoreExtensionsModuleOptions>(options =>
        {
            options.ModuleName = Assembly.GetEntryAssembly()?.GetName().Name?
                .Replace(".HttpApi.Host", string.Empty);
            options.EnvironmentName = hostingEnvironment.EnvironmentName;
        });

        Configure<AbpDbContextOptions>(options => { options.UseNpgsql(); });

        Configure<AbpMultiTenancyOptions>(options => { options.IsEnabled = MultiTenancyConsts.IsEnabled; });

        ConfigureSwaggerServices(context);

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            options.Languages.Add(new LanguageInfo("tr", "tr", "Türkçe"));
        });

        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddAbpJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata");
                options.Audience = configuration["AuthServer:Audience"];
            });
    }

    private static void ConfigureSwaggerServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "GtfsRealtimeManagement API", Version = "v1" });
            options.DocInclusionPredicate((docName, description) =>
                ConfigureSwaggerNotVisibleApis(description));
            options.CustomSchemaIds(type => type.FullName);
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer", document), [] }
            });

            var filePathHttpApi = Path.Combine(AppContext.BaseDirectory, "Byd.GtfsRealtimeManagement.HttpApi.xml");
            options.IncludeXmlComments(filePathHttpApi, true);
        });
    }

    private static bool ConfigureSwaggerNotVisibleApis(ApiDescription apiDescription)
    {
        return !apiDescription.RelativePath!.StartsWith("api/abp/");
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseAbpRequestLocalization();
        app.UseRateLimiter();
        app.UseCoreExtensions();
        app.UseFileManagement();
        app.UseAuthorization();
        if (!env.IsProduction())
        {
            app.UseSwagger();
            app.UseAbpSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "GtfsRealtimeManagement API");
            });
        }

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
