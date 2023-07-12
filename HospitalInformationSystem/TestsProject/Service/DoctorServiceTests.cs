using AutoMapper;
using DtoEntityProject;
using DtoEntityProject.Constants;
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
    public class DoctorServiceTests : IClassFixture<PatientFixture>, IClassFixture<PatientResponseFixture>, IClassFixture<DoctorFixture>, IClassFixture<DoctorResponseFixture>
    {
        private static readonly IServiceProvider _serviceProvider = BuildServiceProviderTest();

        private readonly Mock<IMapper> _mapperMock;
        private readonly HisContext _contextTest;
        private readonly PatientFixture _patientFixture;
        private readonly DoctorFixture _doctorFixture;
        private readonly DoctorResponseFixture _doctorResponseFixture;
        private readonly Mock<JwtParser> _jwtParserMock;
        private readonly UserManager<ApiUser> _userManager;

        public DoctorServiceTests(PatientFixture patientFixture, DoctorResponseFixture doctorResponseFixture, DoctorFixture doctorFixture)
        {
            var serviceProvider = BuildTestServiceProvider();
            _mapperMock = new Mock<IMapper>();
            _contextTest = TestContextFactory.CreateInMemoryHisContext();
            _jwtParserMock = new Mock<JwtParser>();
            _contextTest = _serviceProvider.GetRequiredService<HisContext>();
            _patientFixture = patientFixture;
            _doctorResponseFixture = doctorResponseFixture;
            _doctorFixture = doctorFixture;
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
        public async Task DoctorServiceTests_UpdateAsync_ShouldReturnDoctorResponseApiResponse()
        {
            //arrange
            var doctor = _doctorFixture.Generate();
            DoctorUpdate request = new DoctorUpdate
            {
                Email = "email@gmail.com",
                Address = "aaa",
                Phone = "sdasddsad",
                DateOfBirth = DateTime.Now,
                Specialization = "spec1",
                HoursPerDay = 30,
                FirstName = "das",
                LastName = "dsa"
            };
            var doctorResponse = _doctorResponseFixture.Generate();
            request.Id = doctor.Id;
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(Constants.Technician);
            _mapperMock.Setup(x => x.Map<DoctorUpdate, ApiUser>(It.IsAny<DoctorUpdate>(), It.IsAny<ApiUser>()))
                .Returns(doctor);
            _mapperMock.Setup(x => x.Map<DoctorResponse>(It.IsAny<Doctor>()))
                .Returns(doctorResponse);
            var sut = GenerateSut();
            _contextTest.Add(doctor);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.UpdateAsync(request);

            //assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(ApiResponse));
            result.Result.Should().BeOfType(typeof(DoctorResponse));
            result.Result.Should().BeSameAs(doctorResponse);

        }

        [Fact]
        public async Task DoctorServiceTests_UpdateAsync_ShouldReturnErrorResponseApiResponse()
        {
            //arrange
            var doctor = _doctorFixture.Generate();
            DoctorUpdate request = new DoctorUpdate
            {
                Email = "email@gmail.com",
                Address = "aaa",
                Phone = "sdasddsad",
                DateOfBirth = DateTime.Now,
                Specialization = "spec1",
                HoursPerDay = 30,
                FirstName = "das",
                LastName = "dsa"
            };
            var doctorResponse = _doctorResponseFixture.Generate();
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(Constants.Technician);
            _mapperMock.Setup(x => x.Map<DoctorUpdate, ApiUser>(It.IsAny<DoctorUpdate>(), It.IsAny<ApiUser>()))
                .Returns(doctor);
            _mapperMock.Setup(x => x.Map<DoctorResponse>(It.IsAny<Doctor>()))
                .Returns(doctorResponse);
            var sut = GenerateSut();
            _contextTest.Add(doctor);
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
        public async Task DoctorServiceTests_GetAllAsync_ShouldReturnListOfDoctorsResponseInApiResponse()
        {
            //arrange
            var doctorsResponse = Enumerable.Range(0, 5)
                .Select(p => _doctorResponseFixture.Generate())
                .ToList();
            var doctors = Enumerable.Range(0, 5)
                .Select(p => _doctorFixture.Generate())
                .ToList();
            _mapperMock.Setup(x => x.Map<IEnumerable<DoctorResponse>>(It.IsAny<IEnumerable<Doctor>>()))
                .Returns(doctorsResponse);
            var sut = GenerateSut();
            doctors.ForEach(p => _contextTest.Add(p));
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetAllAsync();

            //assert
            result.Result.Should().NotBeNull();
            result.Result.Should().BeSameAs(doctorsResponse);
        }

        [Fact]
        public async Task DoctorServiceTests_GetAsyncForTechnicianRole_ShouldReturnDoctorsResponseInApiResponse()
        {
            //arrange
            var doctor = _doctorFixture.Generate();
            var doctorResponse = _doctorResponseFixture.Generate();
            _mapperMock.Setup(x => x.Map<DoctorResponse>(It.IsAny<Doctor>()))
                .Returns(doctorResponse);
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(Constants.Technician);
            var sut = GenerateSut();
            _contextTest.Add(doctor);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetAsync(doctor.Id);

            //assert
            result.Result.Should().NotBeNull();
            result.Result.Should().BeSameAs(doctorResponse);
        }

        [Fact]
        public async Task DoctorServiceTests_GetAsyncForPatientRole_ShouldReturnDoctorsResponseInApiResponse()
        {
            //arrange
            var doctor = _doctorFixture.Generate();
            var patientId = _patientFixture.Generate().Id;
            var doctorResponse = _doctorResponseFixture.Generate();
            _mapperMock.Setup(x => x.Map<DoctorResponse>(It.IsAny<Doctor>()))
                .Returns(doctorResponse);
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(Constants.Patient);
            _contextTest.Appointments.AddRange(new Appointment { Id = 10, Note = "Note1", DoctorId = doctor.Id, PatientId = patientId });
            var sut = GenerateSut();
            _contextTest.Add(doctor);
            _jwtParserMock.Setup(x => x.GetIdFromJWT())
                .Returns(patientId);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetAsync(doctor.Id);

            //assert
            result.Result.Should().NotBeNull();
            result.Result.Should().BeSameAs(doctorResponse);
        }

        [Fact]
        public async Task DoctorServiceTests_GetDoctorsByPatientAsync_ShouldReturnDoctorsResponseInApiResponse()
        {
            //arrange
            var patientId = _patientFixture.Generate().Id;
            var doctorsResponse = Enumerable.Range(0, 5)
                .Select(p => _doctorResponseFixture.Generate())
                .ToList();
            var doctors = Enumerable.Range(0, 5)
                .Select(p => _doctorFixture.Generate())
                .ToList();
            _contextTest.Appointments.AddRange(new Appointment { Id = 10, Note = "Note1", PatientId = patientId });
            _mapperMock.Setup(x => x.Map<IEnumerable<DoctorResponse>>(It.IsAny<IEnumerable<Doctor>>()))
                .Returns(doctorsResponse);
            _jwtParserMock.Setup(x => x.GetIdFromJWT())
                .Returns(patientId);
            var sut = GenerateSut();
            doctors.ForEach(p => _contextTest.Add(p));
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetDoctorsByPatientAsync();

            //assert
            result.Result.Should().NotBeNull();
            result.Roles.Should().NotBeNull();
            result.Result.Should().BeSameAs(doctorsResponse);
        }

        [Fact]
        public async Task DoctorServiceTests_GetAsync_ShouldReturnErrorInApiResponse()
        {
            //arrange
            var doctor = _doctorFixture.Generate();
            var doctorResponse = _doctorResponseFixture.Generate();
            _mapperMock.Setup(x => x.Map<DoctorResponse>(It.IsAny<Doctor>()))
                .Returns(doctorResponse);
            var sut = GenerateSut();

            //act
            var result = await sut.GetAsync(doctor.Id);

            //assert
            result.Result.Should().BeNull();
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task DoctorServiceTests_DeleteAsync_ShouldReturnMessageInApiRespone()
        {
            //arrange
            var doctor = _doctorFixture.Generate();
            var sut = GenerateSut();
            _contextTest.Add(doctor);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.DeleteAsync(doctor.Id);

            //assert
            result.Result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task DoctorServiceTests_DeleteAsync_ShouldReturnErrorInApiResponse()
        {
            //arrange
            var doctor = _doctorFixture.Generate();
            var sut = GenerateSut();
            _contextTest.Add(doctor);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.DeleteAsync(It.IsAny<string>());

            //assert
            result.Result.Should().BeNull();
            result.Errors.Should().NotBeEmpty();
        }
        private DoctorService GenerateSut()
        {
            return new DoctorService(_contextTest, _mapperMock.Object, _userManager, _jwtParserMock.Object);
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
