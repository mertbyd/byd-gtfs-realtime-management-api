using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeSources;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSources;
using Byd.GtfsRealtimeManagement.Mappers.RealtimeSources;
using Volo.Abp.Application.Dtos;

namespace Byd.GtfsRealtimeManagement.Services.RealtimeSources;

// islevi: Kaynak olustur, oku, filtreli listele, guncelle ve sil akisini yurutur.
// sistemdeki gorevi: Controller ile Manager/Repository zinciri arasindaki public giristir.
public class RealtimeSourceAppService : GtfsRealtimeManagementAppService, IRealtimeSourceAppService
{
    private static readonly RealtimeSourceMapper Mapper = new();
    private RealtimeSourceManager Manager => LazyGetRequiredService<RealtimeSourceManager>();
    private IRealtimeSourceRepository Repository => LazyGetRequiredService<IRealtimeSourceRepository>();
    private IValidator<TInput> Validator<TInput>() => LazyGetRequiredService<IValidator<TInput>>();

    // Girdiyi dogrular, kapsamdaki kaynagi kurar ve kaydeder.
    public virtual async Task<RealtimeSourceDto> CreateAsync(
        CreateRealtimeSourceDto input, CancellationToken cancellationToken = default)
    {
        await Validator<CreateRealtimeSourceDto>().ValidateAndThrowAsync(input, cancellationToken);
        var entity = await Manager.CreateAsync(Mapper.Map(input), cancellationToken);
        await Repository.InsertAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }

    // Kaynagi id ile istegin kapsaminda getirir.
    public virtual async Task<RealtimeSourceDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Mapper.Map((await Manager.TryGetByIdAsync(id, cancellationToken: cancellationToken))!);

    // Filtreli sayfayi ve toplam kayit sayisini getirir.
    public virtual async Task<PagedResultDto<RealtimeSourceDto>> GetFilteredListAsync(
        DataTableListInput<RealtimeSourceFilter> input, CancellationToken cancellationToken = default)
    {
        await Validator<DataTableListInput<RealtimeSourceFilter>>().ValidateAndThrowAsync(input, cancellationToken);
        var entities = await Repository.GetFilteredListAsync(input, cancellationToken);
        var totalCount = await Repository.GetFilteredCountAsync(input, cancellationToken);
        return new PagedResultDto<RealtimeSourceDto>(totalCount, entities.Select(Mapper.Map).ToList());
    }

    // Girdiyi dogrular, kapsamdaki kaynagi Manager ile gunceller ve kaydeder.
    public virtual async Task<RealtimeSourceDto> UpdateAsync(
        Guid id, UpdateRealtimeSourceDto input, CancellationToken cancellationToken = default)
    {
        await Validator<UpdateRealtimeSourceDto>().ValidateAndThrowAsync(input, cancellationToken);
        var entity = (await Manager.TryGetByIdAsync(id, cancellationToken: cancellationToken))!;
        await Manager.UpdateAsync(entity, Mapper.Map(input), cancellationToken);
        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }

    // Kaynagi bagli kayit yoksa siler.
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = (await Manager.TryGetByIdAsync(id, cancellationToken: cancellationToken))!;
        await Manager.EnsureCanDeleteAsync(entity, cancellationToken);
        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
    }
}
