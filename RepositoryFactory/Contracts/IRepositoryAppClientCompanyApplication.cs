﻿using Entity.Models;
using Entity.ModelsDto;
using RepositoryAppClient.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.Contracts
{
    public interface IRepositoryAppClientCompanyApplication : IRepositoryAppClientBase<CompanyApplication>
    {
        CompanyApplication GetCompanyApplication(Guid id, bool trackChanges);
        IEnumerable<CompanyApplication> GetCompanyApplicationByCompanyId(Guid companyId, bool trackChanges);
        IEnumerable<CompanyApplication> GetCompanyApplicationByApplicationId(Guid applicationId, bool trackChanges);
        public IEnumerable<CompanyApplication> GetPagedCompanyApplications(RequestParameters parameters, bool trackChanges);

    }
}