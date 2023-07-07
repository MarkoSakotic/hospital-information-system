using AutoMapper;
using DtoEntityProject;
using DtoEntityProject.Constants;
using EntityProject;
using FluentAssertions;
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
    public class AppointmentServiceTests : IClassFixture<AppointmentFixture>, IClassFixture<AppointmentResponseFixture>, IClassFixture<AppointmentFilterFixture>
    {
        private static readonly IServiceProvider _serviceProvider = BuildServiceProviderTest();
        private readonly Mock<IMapper> _mapperMock;
        private readonly HisContext _contextTest;
        private readonly AppointmentFixture _appointmentFixture;
        private readonly AppointmentFilterFixture _appointmentFilterFixture;
        private readonly AppointmentResponseFixture _appointmentResponseFixture;
        private readonly Mock<JwtParser> _jwtParserMock;

        public AppointmentServiceTests(AppointmentFixture appointmentFixture, AppointmentResponseFixture appointmentResponseFixture, AppointmentFilterFixture appointmentFilterFixture)
        {
            var serviceProvider = BuildTestServiceProvider();
            _mapperMock = new Mock<IMapper>();
            _contextTest = TestContextFactory.CreateInMemoryHisContext();
            _jwtParserMock = new Mock<JwtParser>();
            _contextTest = _serviceProvider.GetRequiredService<HisContext>();
            _appointmentFixture = appointmentFixture;
            _appointmentResponseFixture = appointmentResponseFixture;
            _appointmentFilterFixture = appointmentFilterFixture;
        }

        [Fact]
        public async Task AppointmentServiceTests_CreateAppointmentsAsync_ShouldReturnListOfAppointmentsInApiResponse()
        {
            //arrange
            var request = new AppointmentRequest()
            {
                DoctorId = "dsadwdas",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(10),
                HoursPerDay = 2,
                Duration = 30
            };
            var appointmentResponse = Enumerable.Range(0, 3)
                .Select(a => _appointmentResponseFixture.Generate())
                .ToList()
                .AsEnumerable();
            var appointments = Enumerable.Range(0, 3)
                .Select(a => _appointmentFixture.Generate())
                .ToList();
            _mapperMock.Setup(x => x.Map<IEnumerable<AppointmentResponse>>(It.IsAny<IEnumerable<Appointment>>()))
                 .Returns((IEnumerable<AppointmentResponse>)appointmentResponse);
            var sut = GenerateSut();

            //act
            var result = await sut.CreateAppointmentsAsync(request);

            //assert
            result.Should().BeOfType(typeof(ApiResponse));
            result.Result.Should().BeOfType(typeof(List<AppointmentResponse>));
            result.Result.Should().BeSameAs(appointmentResponse);
        }

        [Fact]
        public async Task AppointmentServiceTests_GetAppointmentAsync_ShouldReturnListOfAppointmentsInApiResponse()
        {
            //arrange
            var appointment = _appointmentFixture.Generate();
            var appointmentResponse = _appointmentResponseFixture.Generate();
            appointment.Patient = new Patient();
            appointment.Doctor = new Doctor();
            _mapperMock.Setup(x => x.Map<AppointmentResponse>(It.IsAny<Appointment>()))
                .Returns(appointmentResponse);
            var sut = GenerateSut();
            _contextTest.Add(appointment);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetAppointmentAsync(appointment.Id);

            //assert
            result.Should().NotBeNull();
            result.Result.Should().BeSameAs(appointmentResponse);
        }

        [Fact]
        public async Task AppointmentServiceTests_GetAppointmentAsync_ShouldReturnErrorInApiResponse()
        {
            //arrange
            var appointment = _appointmentFixture.Generate();
            var appointmentResponse = _appointmentResponseFixture.Generate();
            appointment.Patient = new Patient();
            appointment.Doctor = new Doctor();
            _mapperMock.Setup(x => x.Map<AppointmentResponse>(It.IsAny<Appointment>()))
                .Returns(appointmentResponse);
            var sut = GenerateSut();

            //act
            var result = await sut.GetAppointmentAsync(appointment.Id);

            //assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain("Unable to get appointment, because it doesn't exists.");
        }

        [Fact]
        public async Task AppointmentServiceTests_GetAllAppointmentsAsync_ShouldReturnListOfAppointmentsInApiResponse()
        {
            //arrange
            var appointmentsResponse = Enumerable.Range(0, 5)
                .Select(p => _appointmentResponseFixture.Generate())
                .ToList();
            var appointments = Enumerable.Range(0, 5)
                .Select(p => _appointmentFixture.Generate())
                .ToList();
            _mapperMock.Setup(x => x.Map<IEnumerable<AppointmentResponse>>(It.IsAny<IEnumerable<Appointment>>()))
                .Returns(appointmentsResponse);
            var sut = GenerateSut();
            appointments.ForEach(p => _contextTest.Add(p));
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetAllAppointmentsAsync();

            //assert
            Assert.NotNull(result);
            result.Result.Should().BeSameAs(appointmentsResponse);
        }

        [Fact]
        public async Task AppointmentServiceTests_GetAllAppointmentsAsync_ShouldReturnErrorInApiResponse()
        {
            //arrange
            var appointmentsResponse = Enumerable.Range(0, 5)
                .Select(p => _appointmentResponseFixture.Generate())
                .ToList();
            var appointments = Enumerable.Range(0, 5)
                .Select(p => _appointmentFixture.Generate())
                .ToList();
            _mapperMock.Setup(x => x.Map<IEnumerable<AppointmentResponse>>(It.IsAny<IEnumerable<Appointment>>()))
                .Returns(appointmentsResponse);
            var sut = GenerateSut();

            //act
            var result = await sut.GetAllAppointmentsAsync();

            //assert
            Assert.NotNull(result);
            result.Errors.Should().Contain("There is no appointments in databse.");
        }

        [Fact]
        public async Task AppointmentServiceTests_DeleteAsync_ShouldReturnMessageInApiRespone()
        {
            //arrange
            var appointment = _appointmentFixture.Generate();
            var sut = GenerateSut();
            _contextTest.Add(appointment);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.DeleteAppointmentAsync(appointment.Id);

            //assert
            result.Result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task AppointmentServiceTests_DeleteAsync_ShouldReturnErrorInApiResponse()
        {
            //arrange
            var appointment = _appointmentFixture.Generate();
            var sut = GenerateSut();
            _contextTest.Add(appointment);
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.DeleteAppointmentAsync(It.IsAny<int>());

            //assert
            result.Result.Should().BeNull();
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task AppointmentServiceTests_ScheduleAppointmentAsync_ShouldReturnAppointmentInApiResponse()
        {
            //arrange
            var appointment = _appointmentFixture.Generate();
            appointment.PatientId = null;
            var appointmentResponse = _appointmentResponseFixture.Generate();
            _mapperMock.Setup(x => x.Map<AppointmentResponse>(It.IsAny<Appointment>()))
                .Returns(appointmentResponse);
            var sut = GenerateSut();
            _contextTest.Add(appointment);
            await _contextTest.SaveChangesAsync();
            var appointmentSchedule = new AppointmentSchedule()
            {
                AppointmentId = appointment.Id,
                PatientId = "dskfg_skd253fds_dsakpf"
            };

            //act
            var result = await sut.ScheduleAppointmentAsync(appointmentSchedule);

            //assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType(typeof(AppointmentResponse));
            result.Result.Should().BeSameAs(appointmentResponse);
        }

        [Fact]
        public async Task AppointmentServiceTests_ScheduleAppointmentAsync_ShouldReturnErrorForAppointmentObjInApiResponse()
        {
            //arrange
            var appointment = _appointmentFixture.Generate();
            var appointmentResponse = _appointmentResponseFixture.Generate();
            _mapperMock.Setup(x => x.Map<AppointmentResponse>(It.IsAny<Appointment>()))
                .Returns(appointmentResponse);
            var sut = GenerateSut();
            var appointmentSchedule = new AppointmentSchedule()
            {
                AppointmentId = appointment.Id,
                PatientId = "dskfg_skd253fds_dsakpf"
            };

            //act
            var result = await sut.ScheduleAppointmentAsync(appointmentSchedule);

            //assert
            result.Should().NotBeNull();
            result.Errors.Should().Contain("Unable to schedule appointment because it doesn't exists.");
        }

        [Fact]
        public async Task AppointmentServiceTests_ScheduleAppointmentAsync_ShouldReturnErrorForPatientIdInApiResponse()
        {
            //arrange
            var appointment = _appointmentFixture.Generate();
            var appointmentResponse = _appointmentResponseFixture.Generate();
            _mapperMock.Setup(x => x.Map<AppointmentResponse>(It.IsAny<Appointment>()))
                .Returns(appointmentResponse);
            var sut = GenerateSut();
            _contextTest.Add(appointment);
            await _contextTest.SaveChangesAsync();
            var appointmentSchedule = new AppointmentSchedule()
            {
                AppointmentId = appointment.Id,
                PatientId = "dskfg_skd253fds_dsakpf"
            };

            //act
            var result = await sut.ScheduleAppointmentAsync(appointmentSchedule);

            //assert
            result.Should().NotBeNull();
            result.Errors.Should().Contain("Choosen appointment is already scheduled.");
        }

        [Fact]
        public async Task AppointmentServiceTests_GetAllAppointmentsForUserWithinGivenPeriodAsyncForPatient_ShouldReturnAppointmensInApiResponse()
        {
            //arrange
            var appointment = _appointmentFixture.Generate();
            var appointmentFilter = _appointmentFilterFixture.Generate();

            _contextTest.Appointments.AddRange(new Appointment { PatientId = appointmentFilter.UserId, StartTime = appointmentFilter.StartDate, EndTime = appointmentFilter.EndDate });

            var appointmentsResponse = Enumerable.Range(0, 10)
                                                 .Select(p => _appointmentResponseFixture.Generate())
                                                 .ToList();//_appointmentResponseFixture.Generate();
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(Constants.Patient);

            _mapperMock.Setup(x => x.Map<IEnumerable<AppointmentResponse>>(It.IsAny<IEnumerable<Appointment>>()))
                .Returns(appointmentsResponse);
            var sut = GenerateSut();
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetAllAppointmentsForUserWithinGivenPeriodAsync(appointmentFilter);

            //assert
            result.Result.Should().BeOfType(typeof(List<AppointmentResponse>));
            result.Result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();

        }

        [Fact]
        public async Task AppointmentServiceTests_GetAllAppointmentsForUserWithinGivenPeriodAsyncForDoctor_ShouldReturnAppointmensInApiResponse()
        {
            //arrange
            var appointment = _appointmentFixture.Generate();
            var appointmentFilter = _appointmentFilterFixture.Generate();

            _contextTest.Appointments.AddRange(new Appointment { DoctorId = appointmentFilter.UserId, StartTime = appointmentFilter.StartDate, EndTime = appointmentFilter.EndDate });

            var appointmentsResponse = Enumerable.Range(0, 10)
                                                 .Select(p => _appointmentResponseFixture.Generate())
                                                 .ToList();
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(Constants.Doctor);

            _mapperMock.Setup(x => x.Map<IEnumerable<AppointmentResponse>>(It.IsAny<IEnumerable<Appointment>>()))
                .Returns(appointmentsResponse);
            var sut = GenerateSut();
            await _contextTest.SaveChangesAsync();

            //act
            var result = await sut.GetAllAppointmentsForUserWithinGivenPeriodAsync(appointmentFilter);

            //assert
            result.Result.Should().BeOfType(typeof(List<AppointmentResponse>));
            result.Result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();

        }

        [Fact]
        public async Task AppointmentServiceTests_GetAllAppointmentsForUserWithinGivenPeriodAsync_ShouldReturnErrorInApiResponse()
        {
            //arrange
            var appointment = _appointmentFilterFixture.Generate();
            var appointmentsResponse = Enumerable.Range(0, 10)
                                                 .Select(p => _appointmentResponseFixture.Generate())
                                                 .ToList();
            //_mapperMock.Setup(x => x.Map<IEnumerable<AppointmentResponse>>(It.IsAny<IEnumerable<Appointment>>()))
            //    .Returns(appointmentsResponse);
            var sut = GenerateSut();

            //act
            var result = await sut.GetAllAppointmentsForUserWithinGivenPeriodAsync(appointment);
            //assert
            result.Result.Should().BeNull();
            result.Errors.Should().NotBeEmpty();

        }

        private static IServiceProvider BuildServiceProviderTest()
        {
            var services = new ServiceCollection();
            var configuration = Common.TestingConfigurationBuilder.BuildConfiguration();

            //services.Configure<HisConfiguration>(configuration.GetSection("HISConnection"));

            services.AddDbContext<HisContext>(opt => opt.UseInMemoryDatabase(databaseName: "InMemoryDb"),
                ServiceLifetime.Scoped,
                ServiceLifetime.Scoped);

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task AppointmentServiceTests_GetAllAppointmentsForUserWithinGivenPeriodAsyncForNoRole_ShouldReturnErrorsInApiResponse()
        {
            //arrange
            var appointmentFilter = _appointmentFilterFixture.Generate();
            _jwtParserMock.Setup(x => x.GetRoleFromJWT())
                .Returns(" ");

            var sut = GenerateSut();
            //act
            var result = await sut.GetAllAppointmentsForUserWithinGivenPeriodAsync(appointmentFilter);
            //assert
            result.Result.Should().BeNull();
            result.Errors.Should().NotBeEmpty();
        }

        private AppointmentService GenerateSut()
        {
            return new AppointmentService(_contextTest, _mapperMock.Object, _jwtParserMock.Object);
        }

        private IServiceProvider BuildTestServiceProvider()
        {
            var services = new ServiceCollection();

            var testingConfiguration = Common.TestingConfigurationBuilder.GetTestConfiguration();

            //services.Configure<HisConfiguration>(testingConfiguration.GetSection("HISConnection"));

            services.AddDbContext<HisContext>(opt => opt.UseInMemoryDatabase(databaseName: "InMemoryDb"),
                ServiceLifetime.Scoped,
                ServiceLifetime.Scoped);

            return services.BuildServiceProvider();
        }
    }

}
