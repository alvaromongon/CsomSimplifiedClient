using System;
using AutoMapper;
using Microsoft.ProjectServer.Client;
using Csom.Client.Model;

namespace Csom.Client.Mapper
{
    internal static class MapperFactory
    {
        /// <summary>
        /// Build an instance of a automapper IMapper interface
        /// definining the needed mappings for task object related
        /// </summary>
        /// <returns></returns>
        public static IMapper BuildTaskMapper()
        {
            var config = new MapperConfiguration(cfg => {
                // Project mappings
                cfg.CreateMap<ProjectModel, ProjectCreationInformation>()
                    .ForAllMembers(o => o.Condition((src, dest, srcMember) =>
                    {
                        return IsMappeable(srcMember);
                    }));
                cfg.CreateMap<ProjectModel, DraftProject>()
                    .ForAllMembers(o => o.Condition((src, dest, srcMember) =>
                    {
                        return IsMappeable(srcMember);
                    }));

                // Task mappings
                cfg.CreateMap<TaskModel, TaskCreationInformation>()
                    .ForAllMembers(o => o.Condition((src, dest, srcMember) =>
                    {
                        return IsMappeable(srcMember);
                    }));
                cfg.CreateMap<TaskModel, DraftTask>()
                    .ForAllMembers(o => o.Condition((src, dest, srcMember) =>
                    {
                        return IsMappeable(srcMember);
                    }));

                // Assigment mappings
                cfg.CreateMap<AssignmentModel, AssignmentCreationInformation>()
                    .ForAllMembers(o => o.Condition((src, dest, srcMember) =>
                    {
                        return IsMappeable(srcMember);
                    }));
                cfg.CreateMap<AssignmentModel, DraftAssignment>()
                    .ForAllMembers(o => o.Condition((src, dest, srcMember) =>
                    {
                        return IsMappeable(srcMember);
                    }));

                // Enterprise Resource mappings
                cfg.CreateMap<EnterpriseResourceModel, EnterpriseResourceCreationInformation>()
                    .ForAllMembers(o => o.Condition((src, dest, srcMember) =>
                    {
                        return IsMappeable(srcMember);
                    }));
                cfg.CreateMap<EnterpriseResourceModel, EnterpriseResource>()
                    .ForAllMembers(o => o.Condition((src, dest, srcMember) =>
                    {
                        return IsMappeable(srcMember);
                    }));

                // Cost rate mappings
                cfg.CreateMap<CostRateCreationInformation, EnterpriseResourceCostRate>()
                    .ForAllMembers(o => o.Condition((src, dest, srcMember) =>
                    {
                        return IsMappeable(srcMember);
                    }));                
            });

            return config.CreateMapper();
        }

        private static bool IsMappeable(object srcMember)
        {
            if (srcMember is DateTime && ((DateTime)srcMember).Equals(DateTime.MinValue))
            {
                return false;
            }

            if (srcMember is TimeSpan && ((TimeSpan)srcMember).Equals(TimeSpan.MinValue))
            {
                return false;
            }

            return true;
        }
    }
}
