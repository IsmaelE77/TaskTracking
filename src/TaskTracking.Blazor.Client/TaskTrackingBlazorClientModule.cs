using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using TaskTracking.Blazor.Client.Menus;
using OpenIddict.Abstractions;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity.Blazor.WebAssembly;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement.Blazor.WebAssembly;
using Volo.Abp.TenantManagement.Blazor.WebAssembly;
using Volo.Abp.UI.Navigation;
using Volo.Abp.AspNetCore.Components.WebAssembly.BasicTheme;

namespace TaskTracking.Blazor.Client;

[DependsOn(
    typeof(AbpAutofacWebAssemblyModule),
    typeof(TaskTrackingHttpApiClientModule),
    typeof(AbpAspNetCoreComponentsWebAssemblyBasicThemeModule),
    typeof(AbpIdentityBlazorWebAssemblyModule),
    typeof(AbpTenantManagementBlazorWebAssemblyModule),
    typeof(AbpSettingManagementBlazorWebAssemblyModule)
)]
public class TaskTrackingBlazorClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();
        var builder = context.Services.GetSingletonInstance<WebAssemblyHostBuilder>();

        ConfigureAuthentication(builder);
        ConfigureHttpClient(context, environment);
        ConfigureMudBlazor(context);
        ConfigureRouter(context);
        ConfigureMenu(context);
        ConfigureAutoMapper(context);
    }

    private void ConfigureRouter(ServiceConfigurationContext context)
    {
        Configure<AbpRouterOptions>(options =>
        {
            options.AppAssembly = typeof(TaskTrackingBlazorClientModule).Assembly;
        });
    }

    private void ConfigureMenu(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new TaskTrackingMenuContributor(context.Services.GetConfiguration()));
        });
    }

    private void ConfigureMudBlazor(ServiceConfigurationContext context)
    {
        context.Services.AddMudServices();
        context.Services.AddMudMarkdownServices();
    }

    private static void ConfigureAuthentication(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind("AuthServer", options.ProviderOptions);
            options.UserOptions.NameClaim = OpenIddictConstants.Claims.Name;
            options.UserOptions.RoleClaim = OpenIddictConstants.Claims.Role;
            options.ProviderOptions.DefaultScopes.Add("TaskTracking");
            options.ProviderOptions.DefaultScopes.Add("roles");
            options.ProviderOptions.DefaultScopes.Add("email");
            options.ProviderOptions.DefaultScopes.Add("phone");
        });
    }

    private static void ConfigureHttpClient(ServiceConfigurationContext context, IWebAssemblyHostEnvironment environment)
    {
        context.Services.AddTransient(sp => new HttpClient
        {
            BaseAddress = new Uri(environment.BaseAddress)
        });
    }

    private void ConfigureAutoMapper(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<TaskTrackingBlazorClientModule>();
        });
    }
}