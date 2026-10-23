using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: Kaynak tablosu ve organizasyonda tekil ad indeksini tanimlar.
// sistemdeki gorevi: Manager'in ad tekilligi kuralini veritabani duzeyinde korur.
public sealed class RealtimeSourceConfiguration : IEntityTypeConfiguration<RealtimeSource>
{
    // EF modeline tablo, sema ve tekillik kuralini uygular.
    public void Configure(EntityTypeBuilder<RealtimeSource> builder)
    {
        builder.ToTable(RealtimeSourceConsts.TableName, GtfsRealtimeManagementDbProperties.DbSchema);
        builder.ConfigureByConvention();
        builder.HasIndex(source => new { source.TenantId, source.OrganizationId, source.Name })
            .IsUnique().HasDatabaseName(RealtimeSourceConsts.NameIndex);
        builder.HasIndex(source => new { source.TenantId, source.OrganizationId })
            .IsUnique().HasFilter("is_active = true")
            .HasDatabaseName(RealtimeSourceConsts.ActiveOrganizationIndex);
    }
}
