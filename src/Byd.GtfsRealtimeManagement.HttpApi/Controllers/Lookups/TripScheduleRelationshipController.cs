using Microsoft.AspNetCore.Mvc;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Services.Lookups;

namespace Byd.GtfsRealtimeManagement.Controllers.Lookups;

// mirastan gelen: Create, Get, GetList, Update ve Delete uclari ile yetki ve activity log metadatasi.
// burada yazilan: sefer planlama durumu rotasi ve AppService baglantisi.
[ApiController]
[Route(RealtimeLookupRoutes.TripScheduleRelationshipsPath)]
public class TripScheduleRelationshipController : RealtimeLookupControllerBase<ITripScheduleRelationshipAppService>
{
}
