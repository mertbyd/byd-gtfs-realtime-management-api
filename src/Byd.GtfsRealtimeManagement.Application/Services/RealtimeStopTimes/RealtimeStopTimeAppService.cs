using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Interfaces;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Managers.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Mappers.RealtimeStopTimes;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;

namespace Byd.GtfsRealtimeManagement.Services.RealtimeStopTimes;

// islevi: Durak satiri CRUD ve sayfali okuma akislarini yurutur.
// sistemdeki gorevi: Controller ile Manager/Repository zincirinin public girisidir.
public class RealtimeStopTimeAppService : GtfsRealtimeManagementAppService, IRealtimeStopTimeAppService
{
    private static readonly RealtimeStopTimeMapper Mapper = new();
    private RealtimeStopTimeManager Manager => LazyGetRequiredService<RealtimeStopTimeManager>();
    private IRealtimeStopTimeRepository Repository => LazyGetRequiredService<IRealtimeStopTimeRepository>();
    private IDataFilter ActiveFilter => LazyGetRequiredService<IDataFilter>();
    private IValidator<TInput> Validator<TInput>() => LazyGetRequiredService<IValidator<TInput>>();

    // Girdiyi dogrular, aktif kaynakta satir kurar ve kaydeder.
    public virtual async Task<RealtimeStopTimeDto> CreateAsync(
        CreateRealtimeStopTimeDto input, CancellationToken cancellationToken = default)
    {
        await Validator<CreateRealtimeStopTimeDto>().ValidateAndThrowAsync(input, cancellationToken);
        var entity = await Manager.CreateAsync(Mapper.Map(input), cancellationToken);
        await Repository.InsertAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }

    // Aktif kaynak altindaki satiri getirir.
    public virtual async Task<RealtimeStopTimeDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Mapper.Map(await Manager.EnsureExistsInScopeAsync(id, cancellationToken));

    // Pasif kaynak gecmisindeki satiri da kapsam icinde getirir.
    public virtual async Task<RealtimeStopTimeDto> GetIncludingInactiveAsync(
        Guid id, CancellationToken cancellationToken = default)
        => Mapper.Map(await Manager.EnsureExistsIncludingInactiveInScopeAsync(id, cancellationToken));

    // Aktif kaynak satirlarinin filtreli sayfasini getirir.
    public virtual async Task<PagedResultDto<RealtimeStopTimeDto>> GetFilteredListAsync(
        DataTableListInput<RealtimeStopTimeFilter> input, CancellationToken cancellationToken = default)
    {
        await Validator<DataTableListInput<RealtimeStopTimeFilter>>().ValidateAndThrowAsync(input, cancellationToken);
        var entities = await Repository.GetFilteredListAsync(input, cancellationToken);
        var totalCount = await Repository.GetFilteredCountAsync(input, cancellationToken);
        return new PagedResultDto<RealtimeStopTimeDto>(totalCount, entities.Select(Mapper.Map).ToList());
    }

    // Pasif kaynak gecmisini dahil ederek filtreli sayfa getirir.
    public virtual async Task<PagedResultDto<RealtimeStopTimeDto>> GetFilteredListIncludingInactiveAsync(
        DataTableListInput<RealtimeStopTimeFilter> input, CancellationToken cancellationToken = default)
    {
        using var inactiveIncluded = ActiveFilter.Disable<ActiveRealtimeSourceFilter>();
        return await GetFilteredListAsync(input, cancellationToken);
    }

    // Aktif kaynak satirini dogrular, gunceller ve kaydeder.
    public virtual async Task<RealtimeStopTimeDto> UpdateAsync(
        Guid id, UpdateRealtimeStopTimeDto input, CancellationToken cancellationToken = default)
    {
        await Validator<UpdateRealtimeStopTimeDto>().ValidateAndThrowAsync(input, cancellationToken);
        var entity = await Manager.EnsureExistsInScopeAsync(id, cancellationToken);
        await Manager.UpdateAsync(entity, Mapper.Map(input), cancellationToken);
        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }

    // Aktif kaynak satirini kalici olarak siler.
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Manager.EnsureExistsInScopeAsync(id, cancellationToken);
        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
    }
}
