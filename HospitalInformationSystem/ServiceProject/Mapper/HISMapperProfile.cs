using AutoMapper;
using DtoEntityProject;
using EntityProject;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceProject.Mapper
{
    public class HisMapperProfile : Profile
    {
        public HisMapperProfile()
        {
            CreateMap<Patient, PatientResponse>()
                .ForMember(d => d.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<PatientRequest, Patient>();

            CreateMap<Appointment, AppointmentResponse>()
                .ForMember(d => d.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                    .ForPath(s => s.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<Doctor, DoctorResponse>()
                .ForMember(d => d.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<DoctorRequest, Doctor>();

        }
    }
}
