using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ProjectServer.Client;
using Csom.Client.Abstractions;
using Csom.Client.Common;
using Csom.Client.Mapper;
using Csom.Client.Model;

namespace Csom.Client
{
    public class EnterpriseResourceClient : BaseClient, IEnterpriseResourceClient
    {
        private readonly IMapper _mapper;

        public EnterpriseResourceClient(ProjectContext projectContext, CsomClientOptions options) : base(projectContext, options)
        {
            _mapper = MapperFactory.BuildTaskMapper();
        }

        public async Task<IEnumerable<EnterpriseResource>> GetAllAsync()
        {
            IEnumerable<EnterpriseResource> enterpriseResources = new List<EnterpriseResource>();

            try
            {
                _projectContext.Load(_projectContext.EnterpriseResources);
                await _projectContext.ExecuteQueryAsync();

                // Force a new list to avoid returning same list than stored in context
                enterpriseResources = new List<EnterpriseResource>(_projectContext.EnterpriseResources.ToArray());
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting enterprise resources. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return enterpriseResources;
        }

        public async Task<IEnumerable<EnterpriseResource>> GetAllByTypeAsync(EnterpriseResourceType resourceType)
        {
            IEnumerable<EnterpriseResource> enterpriseResources = new List<EnterpriseResource>();

            try
            {
                var localEnterpriseResources = _projectContext.LoadQuery(_projectContext.EnterpriseResources.Where(p => p.ResourceType == resourceType));
                await _projectContext.ExecuteQueryAsync();

                // Force a new list to avoid returning same list than stored in context
                enterpriseResources = new List<EnterpriseResource>(localEnterpriseResources.ToArray());
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting enterprise resources by type. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return enterpriseResources;
        }

        public async Task<EnterpriseResource> GetByIdAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            EnterpriseResource enterpriseResource = null;

            try
            {
                IEnumerable<EnterpriseResource> enterpriseResources = _projectContext.LoadQuery(
                    _projectContext.EnterpriseResources.Where(p => p.Id == id));
                await _projectContext.ExecuteQueryAsync();
                if (enterpriseResources.Any())
                {
                    enterpriseResource = enterpriseResources.FirstOrDefault();                
                }
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting a enterprise resource by id. " +
                    $"id searched is {id}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return enterpriseResource;
        }

        public async Task<EnterpriseResource> GetByNameAsync(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            EnterpriseResource enterpriseResource = null;

            try
            {
                IEnumerable<EnterpriseResource> enterpriseResources =
                    _projectContext.LoadQuery(_projectContext.EnterpriseResources.Where(p => p.Name == name));
                await _projectContext.ExecuteQueryAsync();

                if (enterpriseResources.Any())
                {
                    enterpriseResource = enterpriseResources.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting a enterprise resource by name. " +
                    $"name searched is {name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return enterpriseResource;
        }

        public async Task<EnterpriseResource> AddAsync(EnterpriseResourceModel enterpriseResourceModel)
        {
            var result = await AddRangeAsync(new List<EnterpriseResourceModel>() { enterpriseResourceModel });

            return result?.FirstOrDefault();
        }

        public async Task<IEnumerable<EnterpriseResource>> AddRangeAsync(IEnumerable<EnterpriseResourceModel> enterpriseResourceModels)
        {
            if (enterpriseResourceModels == null)
            {
                throw new ArgumentNullException(nameof(enterpriseResourceModels));
            }
            
            IList<Guid> enterpriseResourceIds = new List<Guid>();

            _projectContext.Load(_projectContext.EnterpriseResources);
            await _projectContext.ExecuteQueryAsync();
            var exsistingEnterpriseResourceIds = _projectContext.EnterpriseResources.Select(er => er.Id).ToList();

            foreach (var enterpriseResourceModel in enterpriseResourceModels)
            {
                // Ensure no other enterprise resource with same id exist
                if (!exsistingEnterpriseResourceIds.Any(id => id == enterpriseResourceModel.Id))
                {
                    var enterpriseResource = _projectContext.EnterpriseResources
                        .Add(_mapper.Map<EnterpriseResourceCreationInformation>(enterpriseResourceModel));
                    enterpriseResource = _mapper.Map(enterpriseResourceModel, enterpriseResource);

                    _projectContext.EnterpriseResources.Update();
                    await _projectContext.ExecuteQueryAsync();

                    if (enterpriseResourceModel.CostRate != null)
                    {
                        await SetCostRate(enterpriseResource, enterpriseResourceModel.CostRate);
                    }

                    enterpriseResourceIds.Add(enterpriseResourceModel.Id);
                }
            }

            IList<EnterpriseResource> enterpriseResources = new List<EnterpriseResource>();
            if(enterpriseResourceIds.Any())
            {
                var enterpriseResourceTasks = enterpriseResourceIds.Select(async erId => await GetByIdAsync(erId));
                enterpriseResources = await System.Threading.Tasks.Task.WhenAll(enterpriseResourceTasks);
            }

            return enterpriseResources;
        }

        public async Task<bool> RemoveAsync(EnterpriseResource enterpriseResource)
        {
            if (enterpriseResource == null)
            {
                throw new ArgumentNullException(nameof(enterpriseResource));
            }

            bool result = false;

            try
            {
                var clientResult = _projectContext.EnterpriseResources.Remove(enterpriseResource);
                _projectContext.EnterpriseResources.Update();

                await _projectContext.ExecuteQueryAsync();

                result = clientResult.Value;
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error removing an enterprise resource. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return result;
        }

        private async System.Threading.Tasks.Task SetCostRate(EnterpriseResource enterpriseResource, CostRateCreationInformation creationInformation)
        {
            _projectContext.Load(enterpriseResource.CostRateTables);
            await _projectContext.ExecuteQueryAsync();

            var defaultCostRateTable = enterpriseResource.CostRateTables.GetByName(CostRateTableName.A);
            _projectContext.Load(defaultCostRateTable.CostRates);
            await _projectContext.ExecuteQueryAsync();

            // By default a line is added, we just need to modify the values in the line
            var costRate = defaultCostRateTable.CostRates.First();
            costRate = _mapper.Map(creationInformation, costRate);

            _projectContext.EnterpriseResources.Update();
            await _projectContext.ExecuteQueryAsync();
        }               
    }
}