using AutoMapper;
using HospitalManagementCosmosDB.Application.DTO;
using HospitalManagementCosmosDB.Domain.Entities;

namespace HospitalManagementCosmosDB.Application.AutoMapping
{
    public class AutoMap : Profile
    {
        public AutoMap()
        {
            CreateMap<CreatePatientDTO, Patient>().ReverseMap();
            CreateMap<UpdatePatientDTO, Patient>().ReverseMap();
            CreateMap<Patient, PatientDTO>().ReverseMap();
        }
    }
}
