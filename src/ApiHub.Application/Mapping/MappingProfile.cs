using ApiHub.Application.Features.Connectors.Queries;
using ApiHub.Application.Features.Records.Queries;
using ApiHub.Domain.Entities;
using AutoMapper;

namespace ApiHub.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Connector, ConnectorDto>()
            .ForMember(d => d.EndpointCount, opt => opt.MapFrom(s => s.Endpoints.Count));

        CreateMap<ApiRecord, ApiRecordDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.FirstName + " " + s.User.LastName))
            .ForMember(d => d.ConnectorName, opt => opt.MapFrom(s => s.Connector.Name));
    }
}
