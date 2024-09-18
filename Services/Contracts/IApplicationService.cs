using Entity.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IApplicationService
    {
        IEnumerable<ApplicationDto> GetAllApplication();
        ApplicationDto GetByIdApplication(Guid id);
        ApplicationDto CreateApplication(ApplicationDto applicationDto);
        IEnumerable<ApplicationDto> GetPaginatedApplication(RequestParameters parameters, bool trackChanges);
        void UpdateApplication(ApplicationDto applicationDto);
        void DeleteApplication(Guid id);
    }
}
