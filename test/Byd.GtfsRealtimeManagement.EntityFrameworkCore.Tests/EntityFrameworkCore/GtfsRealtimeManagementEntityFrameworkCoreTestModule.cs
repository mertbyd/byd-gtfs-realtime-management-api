using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Byd.CoreExtensions.Options;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.Uow;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

[DependsOn(
    typeof(GtfsRealtimeManagementApplicationTestModule),
    typeof(GtfsRealtimeManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class GtfsRealtimeManagementEntityFrameworkCoreTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<CoreExtensionsModuleOptions>(options =>
        {
            options.ModuleName = "GtfsRealtimeManagement";
            options.EnvironmentName = "Test";
        });
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();
        // Testler tek SQLite baglantisinda calisir; tenant degistiren testlerde baglanti dizesi icin tenant deposu
        // (auth'un PostgreSQL tablolari) aranmaz.
        context.Services.Replace(ServiceDescriptor.Transient<IConnectionStringResolver, DefaultConnectionStringResolver>());
        // Ayar tablolari da SQLite'ta yoktur; ayarlar tanimdaki varsayilan degerleriyle okunur.
        context.Services.Replace(ServiceDescriptor.Singleton<ISettingStore, NullSettingStore>());
        // Gercek akis adresi ve ortak veritabanindaki GTFS plani testte yoktur: okuyucu bos akis, plan deposu bos plan dondurur.
        // Akis ya da plan isteyen test siniflari kendi sahtelerini kurar.
        var reader = Substitute.For<IRealtimeFeedReader>();
        reader.ReadAsync(default!, default!, default).ReturnsForAnyArgs(
            new FeedMessageModel { Header = new FeedHeaderModel { GtfsRealtimeVersion = "2.0" } });
        context.Services.Replace(ServiceDescriptor.Singleton(reader));
        var snapshots = Substitute.For<IRealtimeSnapshotRepository>();
        snapshots.GetVehiclesAsync(default, default, default).ReturnsForAnyArgs([]);
        snapshots.GetActivePlanAsync(default, default, default).ReturnsForAnyArgs(new RealtimePlanSnapshotModel());
        context.Services.Replace(ServiceDescriptor.Singleton(snapshots));

        var sqliteConnection = CreateDatabaseAndGetConnection();

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(abpDbContextConfigurationContext =>
            {
                abpDbContextConfigurationContext.DbContextOptions.UseSqlite(sqliteConnection);
            });
        });
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new AbpUnitTestSqliteConnection("Data Source=:memory:");
        connection.Open();

        new GtfsRealtimeManagementDbContext(
            new DbContextOptionsBuilder<GtfsRealtimeManagementDbContext>().UseSqlite(connection).Options
        ).GetService<IRelationalDatabaseCreator>().CreateTables();

        return connection;
    }
}
