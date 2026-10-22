using System;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Byd.GtfsRealtimeManagement.Entities.RealtimeSources;

// islevi: Organizasyonun GTFS-RT kaynak adreslerini ve etkinlik durumunu saklar.
// sistemdeki gorevi: Kaynak yonetimi icin tenant ve organizasyon kapsamli kalici kayittir.
public class RealtimeSource : AuditedAggregateRoot<Guid>, IMultiTenant, IMultiOrganization
{
    // Kaydin ait oldugu tenant.
    public Guid? TenantId { get; internal set; }

    // Kaydin ait oldugu organizasyon.
    public Guid? OrganizationId { get; internal set; }

    // Organizasyonda tekil gorunen kaynak adi.
    public string Name { get; internal set; } = default!;

    // VehiclePositions akisi adresi.
    public string? VehiclePositionsUrl { get; internal set; }

    // TripUpdates akisi adresi.
    public string? TripUpdatesUrl { get; internal set; }

    // ServiceAlerts akisi adresi.
    public string? ServiceAlertsUrl { get; internal set; }

    // Kaynagin cekim icin acik olup olmadigi; CRUD okumalarini filtrelemez.
    public bool IsActive { get; internal set; }

    // EF materialization icin bos kabuk kurar.
    protected RealtimeSource()
    {
    }

    // Manager'in dogruladigi kapsama yeni kayit kurar.
    internal RealtimeSource(Guid id, Guid tenantId, Guid organizationId) : base(id)
    {
        TenantId = tenantId;
        OrganizationId = organizationId;
    }
}
