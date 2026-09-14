using System;
using System.Collections.Generic;
using PowerLab.Domain;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Explicit, dependency-light mappings between API DTOs and versioned Domain contracts.
    /// </summary>
    public static class WorkflowContractMappings
    {
        public static WorkflowValueDto ToDto(this WorkflowValue value)
        {
            return WorkflowValueDto.FromDomain(value);
        }

        public static WorkflowValue ToDomain(this WorkflowValueDto value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value.ToDomain();
        }

        public static NodeDescriptorDto ToDto(this NodeDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            return new NodeDescriptorDto
            {
                DescriptorVersion = descriptor.DescriptorVersion,
                NodeType = descriptor.NodeType,
                DisplayName = descriptor.DisplayName,
                Description = descriptor.Description,
                Category = descriptor.Category,
                Icon = descriptor.Icon,
                Parameters = descriptor.Parameters,
                Outputs = descriptor.Outputs,
                DeviceSlots = descriptor.DeviceSlots,
                PauseMode = descriptor.PauseMode
            };
        }

        public static NodeDescriptor ToDomain(this NodeDescriptorDto descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            return new NodeDescriptor
            {
                DescriptorVersion = descriptor.DescriptorVersion,
                NodeType = descriptor.NodeType,
                DisplayName = descriptor.DisplayName,
                Description = descriptor.Description,
                Category = descriptor.Category,
                Icon = descriptor.Icon,
                Parameters = descriptor.Parameters,
                Outputs = descriptor.Outputs,
                DeviceSlots = descriptor.DeviceSlots,
                PauseMode = descriptor.PauseMode
            };
        }

        public static ValidationIssueDto ToDto(this ValidationIssue issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            return new ValidationIssueDto
            {
                Severity = (ValidationIssueSeverity)issue.Severity,
                Code = issue.Code,
                Message = issue.Message,
                Location = issue.Location == null
                    ? new ValidationIssueLocationDto()
                    : new ValidationIssueLocationDto
                    {
                        JsonPointer = issue.Location.JsonPointer,
                        ScopeId = issue.Location.ScopeId,
                        NodeId = issue.Location.NodeId,
                        ParameterId = issue.Location.ParameterId,
                        OutputId = issue.Location.OutputId,
                        DeviceSlotId = issue.Location.DeviceSlotId,
                        Path = issue.Location.Path
                    }
            };
        }

        public static ValidationIssue ToDomain(this ValidationIssueDto issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            ValidationIssueLocationDto location = issue.Location ?? new ValidationIssueLocationDto();
            return new ValidationIssue
            {
                Severity = (ValidationSeverity)issue.Severity,
                Code = issue.Code,
                Message = issue.Message,
                Location = new ValidationIssueLocation
                {
                    JsonPointer = location.JsonPointer ?? string.Empty,
                    ScopeId = location.ScopeId,
                    NodeId = location.NodeId,
                    ParameterId = location.ParameterId,
                    OutputId = location.OutputId,
                    DeviceSlotId = location.DeviceSlotId,
                    Path = location.Path
                }
            };
        }

        public static WorkflowReleaseDto ToDto(this WorkflowRelease release)
        {
            if (release == null)
            {
                throw new ArgumentNullException(nameof(release));
            }

            List<RequiredPluginDto> plugins = new List<RequiredPluginDto>();
            foreach (RequiredPluginReference plugin in release.RequiredPlugins)
            {
                plugins.Add(new RequiredPluginDto
                {
                    PluginId = plugin.PluginId,
                    PluginVersion = plugin.PluginVersion
                });
            }

            List<PythonScriptHashDto> scripts = new List<PythonScriptHashDto>();
            foreach (PythonScriptHash script in release.PythonScriptHashes)
            {
                scripts.Add(new PythonScriptHashDto { NodeId = script.NodeId, Hash = script.Hash });
            }

            return new WorkflowReleaseDto
            {
                ReleaseId = release.ReleaseId,
                WorkflowId = release.WorkflowId,
                ReleaseNumber = release.ReleaseNumber,
                PublishedAt = release.PublishedAt,
                ExecutionHash = release.ExecutionHash,
                RequiredPlugins = plugins,
                PythonScriptHashes = scripts,
                Document = release.Document
            };
        }

        public static WorkflowRelease ToDomain(this WorkflowReleaseDto release)
        {
            if (release == null)
            {
                throw new ArgumentNullException(nameof(release));
            }

            List<RequiredPluginReference> plugins = new List<RequiredPluginReference>();
            foreach (RequiredPluginDto plugin in release.RequiredPlugins)
            {
                plugins.Add(new RequiredPluginReference
                {
                    PluginId = plugin.PluginId,
                    PluginVersion = plugin.PluginVersion
                });
            }

            List<PythonScriptHash> scripts = new List<PythonScriptHash>();
            foreach (PythonScriptHashDto script in release.PythonScriptHashes)
            {
                scripts.Add(new PythonScriptHash { NodeId = script.NodeId, Hash = script.Hash });
            }

            return new WorkflowRelease
            {
                ReleaseId = release.ReleaseId,
                WorkflowId = release.WorkflowId,
                ReleaseNumber = release.ReleaseNumber,
                PublishedAt = release.PublishedAt,
                ExecutionHash = release.ExecutionHash,
                RequiredPlugins = plugins,
                PythonScriptHashes = scripts,
                Document = release.Document
            };
        }
    }
}
