using Byd.GtfsRealtimeManagement.Dtos.Lookups;
using Byd.GtfsRealtimeManagement.Models.Lookups;

namespace Byd.GtfsRealtimeManagement.Mappers.Lookups;

// islevi: Lookup CRUD akisinin ihtiyac duydugu DTO, model ve entity eslemelerini tanimlar.
// sistemdeki gorevi: Ortak lookup AppService'inin her turun Mapperly sinifini ayni sozlesmeyle kullanmasini saglar.
public interface IRealtimeLookupCrudMapper<TEntity>
{
    RealtimeLookupCreateModel Map(CreateRealtimeLookupDto input);
    RealtimeLookupUpdateModel Map(UpdateRealtimeLookupDto input);
    RealtimeLookupDto Map(TEntity entity);
}
