using AutoMapper;
using HospitalManagement.Application.DTO;
using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.AutoMapping
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
