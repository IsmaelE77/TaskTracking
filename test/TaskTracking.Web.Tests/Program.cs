using Microsoft.AspNetCore.Builder;
using TaskTracking;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();

builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("TaskTracking.Web.csproj");
await builder.RunAbpModuleAsync<TaskTrackingWebTestModule>(applicationName: "TaskTracking.Web" );

public partial class Program
{
}
