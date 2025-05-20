using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace TaskTracking.Pages;

public class Index_Tests : TaskTrackingWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
