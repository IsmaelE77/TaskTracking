using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskTracking.Blazor.Client;
using Volo.Abp;
using Volo.Abp.AspNetCore.Components.WebAssembly.LeptonXLiteTheme.Bundling;
using Volo.Abp.AspNetCore.Components.WebAssembly.WebApp;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace TaskTracking.Blazor;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcUiBundlingModule),
    typeof(AbpAspNetCoreComponentsWebAssemblyLeptonXLiteThemeBundlingModule)
)]
public class TaskTrackingBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //https://github.com/dotnet/aspnetcore/issues/52530
        Configure<RouteOptions>(options =>
        {
            options.SuppressCheckForUnhandledSecurityMetadata = true;
        });
        
        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });

        context.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([
                "application/octet-stream", "application/wasm"
            ]);
        });

        // Add services to the container.
        context.Services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var env = context.GetEnvironment();
        var app = context.GetApplicationBuilder();

        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.MapAbpStaticAssets();
        app.UseRouting();
        app.UseAntiforgery();
        app.UseResponseCompression();

        app.UseConfiguredEndpoints(builder =>
        {
            builder.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(WebAppAdditionalAssembliesHelper.GetAssemblies<TaskTrackingBlazorClientModule>());
        });
    }
}