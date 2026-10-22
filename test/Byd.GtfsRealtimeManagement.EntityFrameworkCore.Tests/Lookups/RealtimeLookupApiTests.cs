using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Controllers.Lookups;
using Byd.GtfsRealtimeManagement.Permissions;
using Byd.GtfsRealtimeManagement.Services.Lookups;
using Xunit;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore.Lookups;

public class RealtimeLookupApiTests
{
    [Fact]
    public void EachLookupHasItsOwnControllerAndServiceWithFiveCrudActions()
    {
        var endpoints = new (Type Controller, Type Service, string Path)[]
        {
            (typeof(TripScheduleRelationshipController), typeof(ITripScheduleRelationshipAppService), RealtimeLookupRoutes.TripScheduleRelationships),
            (typeof(StopScheduleRelationshipController), typeof(IStopScheduleRelationshipAppService), RealtimeLookupRoutes.StopScheduleRelationships),
            (typeof(VehicleStopStatusController), typeof(IVehicleStopStatusAppService), RealtimeLookupRoutes.VehicleStopStatuses),
            (typeof(ViolationTypeController), typeof(IViolationTypeAppService), RealtimeLookupRoutes.ViolationTypes)
        };

        foreach (var (controller, service, path) in endpoints)
        {
            Assert.Equal(RealtimeLookupRoutes.Root + "/" + path, controller.GetCustomAttribute<RouteAttribute>()?.Template);
            Assert.True(typeof(IRealtimeLookupCrudAppService).IsAssignableFrom(service));
            Assert.Equal(5, controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Count(method => method.DeclaringType?.IsGenericType == true &&
                    method.DeclaringType.GetGenericTypeDefinition() == typeof(RealtimeLookupControllerBase<>)));
            AssertAction(controller, nameof(IRealtimeLookupCrudAppService.CreateAsync), typeof(HttpPostAttribute),
                GtfsRealtimeManagementPermissions.Lookups.Manage);
            AssertAction(controller, nameof(IRealtimeLookupCrudAppService.GetAsync), typeof(HttpGetAttribute),
                GtfsRealtimeManagementPermissions.Lookups.View, RealtimeLookupRoutes.ById);
            AssertAction(controller, nameof(IRealtimeLookupCrudAppService.GetListAsync), typeof(HttpGetAttribute),
                GtfsRealtimeManagementPermissions.Lookups.View);
            AssertAction(controller, nameof(IRealtimeLookupCrudAppService.UpdateAsync), typeof(HttpPutAttribute),
                GtfsRealtimeManagementPermissions.Lookups.Manage, RealtimeLookupRoutes.ById);
            AssertAction(controller, nameof(IRealtimeLookupCrudAppService.DeleteAsync), typeof(HttpDeleteAttribute),
                GtfsRealtimeManagementPermissions.Lookups.Manage, RealtimeLookupRoutes.ById);
        }
    }

    private static void AssertAction(Type controller, string name, Type verb, string permission, string? path = null)
    {
        var action = controller.GetMethod(name) ?? throw new InvalidOperationException(name);
        Assert.Equal(path, ((HttpMethodAttribute?)action.GetCustomAttribute(verb))?.Template);
        Assert.Equal(permission, action.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
    }
}
