using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectServer.Client;
using Csom.Client.Abstractions;
using Csom.Client.Common;

namespace Csom.Client
{
    public class CustomFieldClient : BaseClient, ICustomFieldClient
    {
        public CustomFieldClient(ProjectContext projectContext, CsomClientOptions options) : base(projectContext, options)
        {}

        public async Task<IEnumerable<CustomField>> GetAllAsync()
        {
            IEnumerable<CustomField> customFields = new List<CustomField>();

            try
            {
                _projectContext.Load(_projectContext.CustomFields);
                _projectContext.Load(_projectContext.EntityTypes);
                await _projectContext.ExecuteQueryAsync();

                // Force a new list to avoid returning same list than stored in context
                customFields = new List<CustomField>(_projectContext.CustomFields.ToArray());
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting custom fields. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return customFields;
        }

        /// <summary>
        /// Get custom fields by entityType.
        /// Example: GetAllByEntityTypeAsync(entityTypesClient.GetAllAsync().ProjectEntity);
        /// </summary>
        /// <param name="entityType">entity type to look for</param>
        /// <returns></returns>
        public async Task<IEnumerable<CustomField>> GetAllByEntityTypeAsync(EntityType entityType)
        {
            IEnumerable<CustomField> customFields = new List<CustomField>();

            try
            {
                var internalCustomFields = _projectContext.LoadQuery(_projectContext.CustomFields.Where(p => p.EntityType.ID == entityType.ID));
                await _projectContext.ExecuteQueryAsync();

                foreach (var customField in internalCustomFields)
                {
                    _projectContext.Load(customField.EntityType); // Might be not always needed but it is used in tests
                }                
                await _projectContext.ExecuteQueryAsync();

                // Force a new list to avoid returning same list than stored in context
                customFields = new List<CustomField>(internalCustomFields.ToArray());
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting custom fields by entity type. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return customFields;
        }

        public async Task<CustomField> GetByIdAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            CustomField customField = null;

            try
            {
                IEnumerable<CustomField> customFields =
                    _projectContext.LoadQuery(_projectContext.CustomFields.Where(p => p.Id == id));
                await _projectContext.ExecuteQueryAsync();

                if (customFields.Any())
                {
                    customField = customFields.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting a custom field by id. " +
                    $"id searched is {id}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return customField;
        }

        public async Task<CustomField> GetByNameAsync(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            CustomField customField = null;

            try
            {
                IEnumerable<CustomField> customFields =
                    _projectContext.LoadQuery(_projectContext.CustomFields.Where(p => p.Name == name));
                await _projectContext.ExecuteQueryAsync();

                if (customFields.Any())
                {
                    customField = customFields.FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error getting a custom field by name. " +
                    $"Name searched is {name}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return customField;
        }

        public async Task<CustomField> AddAsync(CustomFieldCreationInformation creationInformation)
        {
            if (creationInformation == null)
            {
                throw new ArgumentNullException(nameof(creationInformation));
            }            

            if (!Enum.IsDefined(typeof(CustomFieldType), creationInformation.FieldType))
            {
                throw new ArgumentNullException(nameof(creationInformation.FieldType));
            }

            CustomField customField = null;

            try
            {
                // Ensure no other custom field with same id or name exist
                _projectContext.Load(_projectContext.CustomFields);                
                var customFields = _projectContext.LoadQuery(_projectContext.CustomFields.Where(pr =>
                pr.Id == creationInformation.Id || pr.Name == creationInformation.Name));
                await _projectContext.ExecuteQueryAsync();

                if (!customFields.Any())
                {
                    _projectContext.CustomFields.Add(creationInformation);

                    _projectContext.CustomFields.Update();

                    customField = await GetByIdAsync(creationInformation.Id);
                }                
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error adding a new custom field. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return customField;
        }

        public async Task<bool> RemoveAsync(CustomField customField)
        {
            if (customField == null)
            {
                throw new ArgumentNullException(nameof(customField));
            }

            bool result = false;

            try
            {
                var clientResult = _projectContext.CustomFields.Remove(customField);
                _projectContext.CustomFields.Update();

                await _projectContext.ExecuteQueryAsync();
                result = clientResult.Value;
            }
            catch (Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error removing a custom field. " +
                    $"custom field id is {customField.Id}. " +
                    $"Project context url is {_projectContext.Url}.", ex);
            }

            return result;
        }
    }
}
