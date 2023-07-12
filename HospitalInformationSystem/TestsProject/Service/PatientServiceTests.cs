using AutoMapper;
using DtoEntityProject;
using DtoEntityProject.Constants;
using DtoEntityProject.Configuration;
using EntityProject;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RepositoryProject.Context;
using ServiceProject.Implementation;
using ServiceProject.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestsProject.Common;
using TestsProject.Fixture;
using Xunit;

namespace TestsProject.Service
{
    public class PatientServiceTests : IClassFixture<PatientFixture>, IClassFixture<PatientResponseFixture>, IClassFixture<DoctorFixture>, IClassFixture<AppointmentFixture>
    {
        private static readonly IServiceProvider _serviceProvider = BuildServiceProviderTest();
        private readonly Mock<IMapper> _mapperMock;
        private readonly HisContext _contextTest;
        private readonly PatientFixture _patientFixture;
        private readonly DoctorFixture _doctorFixture;
        private readonly PatientResponseFixture _patientResponseFixture;
        private readonly AppointmentFixture _appointmentFixture;
        private readonly Mock<JwtParser> _jwtParserMock;
        private readonly UserManager<ApiUser> _userManager;

        public PatientServiceTests(PatientFixture patientFixture, PatientResponseFixture patientResponseFixture, DoctorFixture doctorFixture, AppointmentFixture appointmentFixture)
        {
            _mapperMock = new Mock<IMapper>();
            _contextTest = TestContextFactory.CreateInMemoryHisContext();
            _jwtParserMock = new Mock<JwtParser>();
            _contextTest = _serviceProvider.GetRequiredService<HisContext>();
            _patientFixture = patientFixture;
            _patientResponseFixture = patientResponseFixture;
            _doctorFixture = doctorFixture;
            _appointmentFixture = appointmentFixture;
            _userManager = new UserManager<ApiUser>(new UserStore<ApiUser>(_contextTest), null, null, null, null, null, null, null, null);
        }

        private static IServiceProvider BuildServiceProviderTest()
        {
            var services = new ServiceCollection();
            var configuration = TestingConfigurationBuilder.BuildConfiguration();

            services.Configure<HisConfiguration>(configuration.GetSection("HISConnection"));

            services.AddDbContext<HisContext>(opt => opt.UseInMemoryDatabase(databaseName: "InMemoryDb"),
                ServiceLifetime.Scoped,
                ServiceLifetime.Scoped);

            return services.BuildServiceProvider();
        }
        
        [Fact]
        public async Task PatientServiceTests_AddAsync_ShouldReturnPatientResponseInApiResponse()
        {
            //arrange
            PatientRequest request = new PatientRequest
            {
                Email = "email@gmail.com",
                Address = "aaa",
                Phone = "sdasddsad",
                Password = "P@ssw0rd",
                DateOfBirth = DateTime.Now,
                SSN = "sadsadsad",
                FirstName = "das",
                LastName = "dsa"
            };
            var response = _patientResponseFixture.Generate();
            var patient = _patientFixture.Generate();
            _mapperMock.Setup(x => x.Map<Patient>(It.IsAny<PatientRequest>()))
                 .Returns(patient);
            _mapperMock.Setup(x => x.Map<PatientResponse>(It.IsAny<Patient>()))
                 .Returns(response);
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(Constants.Technician);

            var sut = GenerateSut();

            //act
            var result = await sut.AddAsync(request);

            //assert
            result.Should().BeOfType(typeof(ApiResponse));
            result.Result.Should().BeOfType(typeof(PatientResponse));
            result.Result.Should().BeSameAs(response);
        }
        
        [Fact]
        public async Task PatientServiceTests_UpdatePatientAsync_ShouldReturnPatientResponseInApiResponse()
        {
            //arrange
            var patient = _patientFixture.Generate();
            PatientUpdate request = new PatientUpdate
            {
                Email = "email@gmail.com",
                Address = "aaa",
                Phone = "sdasddsad",
                DateOfBirth = DateTime.Now,
                SSN = "sadsadsad",
                FirstName = "das",
                LastName = "dsa"
            };
            var patientResponse = _patientResponseFixture.Generate();
            request.Id = patient.Id;
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(Constants.Technician);
            _mapperMock.Setup(x => x.Map<PatientUpdate, ApiUser>(It.IsAny<PatientUpdate>(), It.IsAny<ApiUser>()))
                .Returns(patient);
            _mapperMock.Setup(x => x.Map<PatientResponse>(It.IsAny<Patient>()))
                .Returns(patientResponse);
            var sut = GenerateSut();
            _contextTest.Add(patient);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.UpdateAsync(request);

            //assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(ApiResponse));
            result.Result.Should().BeOfType(typeof(PatientResponse));
            result.Result.Should().BeSameAs(patientResponse);
        }

        [Fact]
        public async Task PatientServiceTests_UpdateAsync_ShouldReturnErrorInApiResponse()
        {
            //arrange
            var patient = _patientFixture.Generate();
            PatientUpdate request = new PatientUpdate
            {
                Email = "email@gmail.com",
                Address = "aaa",
                Phone = "sdasddsad",
                DateOfBirth = DateTime.Now,
                SSN = "sadsadsad",
                FirstName = "das",
                LastName = "dsa"
            };
            var patientResponse = _patientResponseFixture.Generate();
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(Constants.Technician);
            _mapperMock.Setup(x => x.Map<PatientUpdate, ApiUser>(It.IsAny<PatientUpdate>(), It.IsAny<ApiUser>()))
                .Returns(patient);
            _mapperMock.Setup(x => x.Map<PatientResponse>(It.IsAny<Patient>()))
                .Returns(patientResponse);
            var sut = GenerateSut();
            _contextTest.Add(patient);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.UpdateAsync(request);

            //assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(ApiResponse));
            result.Result.Should().BeNull();
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task PatientServiceTests_GetAllAsync_ShouldReturnListOfPatientsResponseInApiResponse()
        {
            //arrange
            var patientsResponse = Enumerable.Range(0, 5)
                .Select(p => _patientResponseFixture.Generate())
                .ToList();
            var patients = Enumerable.Range(0, 5)
                .Select(p => _patientFixture.Generate())
                .ToList();
            _mapperMock.Setup(x => x.Map<IEnumerable<PatientResponse>>(It.IsAny<IEnumerable<Patient>>()))
                .Returns(patientsResponse);
            var sut = GenerateSut();
            patients.ForEach(p => _contextTest.Add(p));
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetAllAsync();

            //assert
            result.Result.Should().NotBeNull();
            result.Result.Should().BeSameAs(patientsResponse);
        }

        [Fact]
        public async Task PatientServiceTests_GetAsyncForTechnicianRole_ShouldReturnPatientsResponseInApiResponse()
        {
            //arrange

            var patient = _patientFixture.Generate();
            var patientResponse = _patientResponseFixture.Generate();
            _mapperMock.Setup(x => x.Map<PatientResponse>(It.IsAny<Patient>()))
                .Returns(patientResponse);
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(Constants.Technician);
            var sut = GenerateSut();
            _contextTest.Add(patient);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetAsync(patient.Id);

            //assert
            result.Result.Should().NotBeNull();
            result.Result.Should().BeSameAs(patientResponse);
        }

        [Fact]
        public async Task PatientServiceTests_GetAsyncForDoctorRole_ShouldReturnPatientsResponseInApiResponse()
        {
            //arrange
            var patient = _patientFixture.Generate();
            var doctorId = _doctorFixture.Generate().Id;
            var patientResponse = _patientResponseFixture.Generate();
            _mapperMock.Setup(x => x.Map<PatientResponse>(It.IsAny<Patient>()))
                .Returns(patientResponse);
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(Constants.Doctor);
            _contextTest.Appointments.AddRange(new Appointment { Id = 10, Note = "Note1", PatientId = patient.Id, DoctorId = doctorId });
            _jwtParserMock.Setup(x => x.GetIdFromJWT())
                .Returns(doctorId);
            var sut = GenerateSut();
            _contextTest.Add(patient);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetAsync(patient.Id);

            //assert
            result.Result.Should().NotBeNull();
            result.Result.Should().BeSameAs(patientResponse);
        }

        [Fact]
        public async Task PatientServiceTests_GetPatientsByDoctorAsync_ShouldReturnPatientsResponseInApiResponse()
        {
            //arrange
            var doctorId = _doctorFixture.Generate().Id;
            var patientsResponse = Enumerable.Range(0, 5)
                .Select(p => _patientResponseFixture.Generate())
                .ToList();
            var patients = Enumerable.Range(0, 5)
                .Select(p => _patientFixture.Generate())
                .ToList();
            _contextTest.Appointments.AddRange(new Appointment { Id = 10, Note = "Note1", DoctorId = doctorId });
            _mapperMock.Setup(x => x.Map<IEnumerable<PatientResponse>>(It.IsAny<IEnumerable<Patient>>()))
                .Returns(patientsResponse);
            _jwtParserMock.Setup(x => x.GetIdFromJWT())
                .Returns(doctorId);
            var sut = GenerateSut();
            patients.ForEach(p => _contextTest.Add(p));
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetPatientsByDoctorAsync();

            //assert
            result.Result.Should().NotBeNull();
            result.Roles.Should().NotBeNull();
            result.Result.Should().BeSameAs(patientsResponse);
        }

        [Fact]
        public async Task PatientServiceTests_GetAsync_ShouldReturnErrorInApiResponse()
        {
            //arrange
            var patient = _patientFixture.Generate();
            var patientResponse = _patientResponseFixture.Generate();
            _mapperMock.Setup(x => x.Map<PatientResponse>(It.IsAny<Patient>()))
                .Returns(patientResponse);
            var sut = GenerateSut();

            //act
            var result = await sut.GetAsync(patient.Id);
            //assert
            result.Result.Should().BeNull();
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task PatientServiceTests_DeleteAsync_ShouldReturnMessageInApiRespone()
        {
            //arrange
            var patient = _patientFixture.Generate();
            var sut = GenerateSut();
            _contextTest.Add(patient);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.DeleteAsync(patient.Id);

            //assert
            result.Result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task PatientServiceTests_DeleteAsync_ShouldReturnErrorInApiResponse()
        {
            //arrange
            var patient = _patientFixture.Generate();
            var sut = GenerateSut();
            _contextTest.Add(patient);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.DeleteAsync(It.IsAny<string>());

            //assert
            result.Result.Should().BeNull();
            result.Errors.Should().NotBeEmpty();
        }
        private PatientService GenerateSut()
        {
            return new PatientService(_userManager, _contextTest, _mapperMock.Object, _jwtParserMock.Object);
        }
        private IServiceProvider BuildTestServiceProvider()
        {
            var services = new ServiceCollection();

            var testingConfiguration = TestingConfigurationBuilder.GetTestConfiguration();

            services.Configure<HisConfiguration>(testingConfiguration.GetSection("HISConnection"));

            services.AddDbContext<HisContext>(opt => opt.UseInMemoryDatabase(databaseName: "InMemoryDb"),
                ServiceLifetime.Scoped,
                ServiceLifetime.Scoped);

            return services.BuildServiceProvider();
        }
    }

}
