﻿using AutoMapper;
using Entity.Models;
using Entity.ModelsDto;
using Microsoft.Extensions.Logging;
using Repositories.Contracts;
using RepositoryAppClient.Contracts;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.EFCore
{
    public class ApplicationService : IApplicationService
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryManager _repository;
        private readonly ILogger<ApplicationService> _logger;
        public ApplicationService(IRepositoryManager repository, IRepositoryAppClientManager repositoryAppClient, IMapper mapper, ILogger<ApplicationService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }
        public ApplicationDto CreateApplication(ApplicationDto applicationDto)
        {
            var mappedDto = _mapper.Map<Application>(applicationDto);
            _repository.Application.GenericCreate(mappedDto);
            _repository.Save();
            var createdDto = _mapper.Map<ApplicationDto>(mappedDto);
            return createdDto;
        }

        public void DeleteApplication(Guid id)
        {
            var deletedData = _repository.Application.GetApplication(id, false);

            if (deletedData is not null) 
            {
                _repository.Application.GenericDelete(deletedData);
                _repository.Save();
            }
        }

        public IEnumerable<ApplicationDto> GetAllApplication()
        {
            var applicationList = _repository.Application.GenericRead(false);
            var applicationDtoList = _mapper.Map<IEnumerable<ApplicationDto>>(applicationList);
            return applicationDtoList;
        }

        public IEnumerable<ApplicationDto> GetAllApplicationForLogin()
        {
            var applicationList = _repository.Application.GenericRead(false);
            var applicationDtoList = _mapper.Map<IEnumerable<ApplicationDto>>(applicationList);
            return applicationDtoList;
        }

        public IEnumerable<ApplicationDto> GetPaginatedApplication(RequestParameters parameters, bool trackChanges)
        {
            var serverApplications = _repository.Application.GetPagedApplications(parameters, trackChanges);
            var serverApplicationDto = _mapper.Map<IEnumerable<ApplicationDto>>(serverApplications);

            var applications = _repository.Application.GetPagedApplications(parameters, trackChanges);
            var applicationDto = _mapper.Map<IEnumerable<ApplicationDto>>(applications);

            return serverApplicationDto;
        }

        public ApplicationDto GetByIdApplication(Guid id)
        {
            var application = _repository.Application.GetApplication(id, false);
            var applicationDto = _mapper.Map<ApplicationDto>(application);
            return applicationDto;
        }

        public void UpdateApplication(ApplicationDto applicationDto)
        {
            var updatedData = _repository.Application.GetApplication(applicationDto.Id, false);

            if (updatedData is not null)
            {
                var mappedData = _mapper.Map(applicationDto, updatedData);
                _repository.Application.GenericUpdate(mappedData);
                _repository.Save();
            }
        }
    }
}
