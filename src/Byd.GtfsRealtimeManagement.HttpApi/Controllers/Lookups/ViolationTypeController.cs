using Microsoft.AspNetCore.Mvc;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Services.Lookups;

namespace Byd.GtfsRealtimeManagement.Controllers.Lookups;

// mirastan gelen: Create, Get, GetList, Update ve Delete uclari ile yetki ve activity log metadatasi.
// burada yazilan: ihlal turu rotasi ve AppService baglantisi.
[ApiController]
[Route(RealtimeLookupRoutes.ViolationTypesPath)]
public class ViolationTypeController : RealtimeLookupControllerBase<IViolationTypeAppService>
{
}
