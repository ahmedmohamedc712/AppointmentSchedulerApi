using AppointmentScheduler.Data;
using AppointmentScheduler.Data.DTOs;
using AppointmentScheduler.Exceptions;
using AppointmentScheduler.Models;
using AppointmentScheduler.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using TaskScheduler.Services;

namespace AppointmentScheduler.Tests
{
    public class AppointmentServiceTests
    {
        [Fact]
        public async Task Get_WhenThereIsData_ReturnsUserAppointments()
        {
            // Arrange
            var mockCurrentUser = new Mock<ICurrentUserAccessor>();
            var mockConverter = new Mock<IUtcLocalConverter>();
            var mockJobProvider = new Mock<IBackgroundJobProvider>();
            var mockClock = new Mock<IClock>();

            int userId = 100;
            mockCurrentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var context = await GetContext(
            [
                new Appointment
                {
                    Id = 1,
                    Title = "A1",
                    Description = "Desc1",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 2,
                    Title = "A2",
                    Description = "Desc2",
                    UserId = 999
                }
            ]);
            string timeZone = "Africa/Cairo";
            mockConverter
                .Setup(x => x.ConvertUtcToLocal(It.IsAny<Instant>(), timeZone))
                .Returns(DateTime.Now);

            var sut = new AppointmentService
            (
                context,
                mockCurrentUser.Object,
                mockConverter.Object,
                mockJobProvider.Object,
                mockClock.Object
            );

            // Act
            var result = await sut.Get(timeZone);

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("A1", list[0].Title);

            mockConverter.Verify(
                x => x.ConvertUtcToLocal(It.IsAny<Instant>(), timeZone),
                Times.Exactly(3)
            );

            Assert.True(result.Count() == 1);
            Assert.True(result.ElementAt(0).Title == "A1");
        }

        [Fact]
        public async Task Get_WhenNoData_ReturnsEmptyList()
        {
            // Arrange
            var mockCurrentUser = new Mock<ICurrentUserAccessor>();
            var mockConverter = new Mock<IUtcLocalConverter>();
            var mockJobProvider = new Mock<IBackgroundJobProvider>();
            var mockClock = new Mock<IClock>();

            int userId = 100;
            mockCurrentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var context = await GetContext([
                new Appointment
                {
                    Id = 1,
                    Title = "A1",
                    Description = "Desc1",
                    UserId = 500
                },
                new Appointment
                {
                    Id = 2,
                    Title = "A2",
                    Description = "Desc2",
                    UserId = 999
                }
            ]);

            var sut = new AppointmentService
            (
                context,
                mockCurrentUser.Object,
                mockConverter.Object,
                mockJobProvider.Object,
                mockClock.Object
            );

            var timeZone = "Africa/Cairo";
            var result = await sut.Get(timeZone);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetById_WhenUserDoesntExist_ThrowsNotFoundException()
        {
            // Arrange
            var mockCurrentUser = new Mock<ICurrentUserAccessor>();
            var mockConverter = new Mock<IUtcLocalConverter>();
            var mockJobProvider = new Mock<IBackgroundJobProvider>();
            var mockClock = new Mock<IClock>();

            var appointmentId = 1;
            var userId = 200;
            mockCurrentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var context = await GetContext([
                new Appointment
                {
                    Id = 1,
                    Title = "A1",
                    Description = "Desc1",
                    UserId = 999
                },
                new Appointment
                {
                    Id = 2,
                    Title = "A2",
                    Description = "Desc2",
                    UserId = 999
                }
            ]);

            var sut = new AppointmentService(
                context,
                mockCurrentUser.Object,
                mockConverter.Object,
                mockJobProvider.Object,
                mockClock.Object
            );

            // Act
            var timeZone = "Africa/Cairo";
            await Assert.ThrowsAsync<NotFoundException>(async () => await sut.GetById(appointmentId, timeZone));
        }

        [Fact]
        public async Task GetById_WhenAppointmentNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var mockCurrentUser = new Mock<ICurrentUserAccessor>();
            var mockConverter = new Mock<IUtcLocalConverter>();
            var mockJobProvider = new Mock<IBackgroundJobProvider>();
            var mockClock = new Mock<IClock>();

            var appointmentId = 500;
            var userId = 200;
            mockCurrentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var context = await GetContext([
                new Appointment
                {
                    Id = 1,
                    Title = "A1",
                    Description = "Desc1",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 3,
                    Title = "A3",
                    Description = "Desc3",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 2,
                    Title = "A2",
                    Description = "Desc2",
                    UserId = 999
                }
            ]);

            var sut = new AppointmentService(
                context,
                mockCurrentUser.Object,
                mockConverter.Object,
                mockJobProvider.Object,
                mockClock.Object
            );

            // Act
            var timeZone = "Africa/Cairo";
            await Assert.ThrowsAsync<NotFoundException>(async () => await sut.GetById(appointmentId, timeZone));
        }

        [Fact]
        public async Task GetById_WhenAppointmentFound_ReturnsAppointment()
        {
            // Arrange
            var mockCurrentUser = new Mock<ICurrentUserAccessor>();
            var mockConverter = new Mock<IUtcLocalConverter>();
            var mockJobProvider = new Mock<IBackgroundJobProvider>();
            var mockClock = new Mock<IClock>();

            var appointmentId = 3;
            var userId = 200;
            mockCurrentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var context = await GetContext([
                new Appointment
                {
                    Id = 1,
                    Title = "A1",
                    Description = "Desc1",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 3,
                    Title = "A3",
                    Description = "Desc3",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 2,
                    Title = "A2",
                    Description = "Desc2",
                    UserId = 999
                }
            ]);

            var sut = new AppointmentService(
                context,
                mockCurrentUser.Object,
                mockConverter.Object,
                mockJobProvider.Object,
                mockClock.Object
            );

            // Act
            var timeZone = "Africa/Cairo";
            var appointment = await sut.GetById(appointmentId, timeZone);

            Assert.NotNull(appointment);
            Assert.True(appointment.Title == "A3" && appointment.Description == "Desc3");
        }

        [Fact]
        public async Task Create_WhenTitleIsNullOrWhitespace_ThrowsBadRequestException()
        {
            // Arrange
            var mockCurrentUser = new Mock<ICurrentUserAccessor>();
            var mockConverter = new Mock<IUtcLocalConverter>();
            var mockJobProvider = new Mock<IBackgroundJobProvider>();
            var mockClock = new Mock<IClock>();

            var userId = 100;
            mockCurrentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var context = await GetContext([
                new Appointment
                {
                    Id = 1,
                    Title = "A1",
                    Description = "Desc1",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 3,
                    Title = "A3",
                    Description = "Desc3",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 2,
                    Title = "A2",
                    Description = "Desc2",
                    UserId = 999
                }
            ]);

            var timeZone = "Africa/Cairo";
            var request = new CreateAppointmentRequest(
                "", "Des", new DateTime(2026, 4, 1), new DateTime(2026, 3, 30), false
            );

            var sut = new AppointmentService(
                context,
                mockCurrentUser.Object,
                mockConverter.Object,
                mockJobProvider.Object,
                mockClock.Object
            );

            await Assert.ThrowsAsync<BadRequestException>(async () => await sut.Create(request, timeZone));
        }
        [Fact]
        public async Task Create_WhenDescriptionIsNullOrWhitespace_ThrowsBadRequestException()
        {
            // Arrange
            var mockCurrentUser = new Mock<ICurrentUserAccessor>();
            var mockConverter = new Mock<IUtcLocalConverter>();
            var mockJobProvider = new Mock<IBackgroundJobProvider>();
            var mockClock = new Mock<IClock>();

            var userId = 100;
            mockCurrentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var context = await GetContext([
                new Appointment
                {
                    Id = 1,
                    Title = "A1",
                    Description = "Desc1",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 3,
                    Title = "A3",
                    Description = "Desc3",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 2,
                    Title = "A2",
                    Description = "Desc2",
                    UserId = 999
                }
            ]);

            var timeZone = "Africa/Cairo";

            var request = new CreateAppointmentRequest(
                "test title", "", new DateTime(2026, 4, 1), new DateTime(2026, 3, 30), false
            );

            var sut = new AppointmentService(
                context,
                mockCurrentUser.Object,
                mockConverter.Object,
                mockJobProvider.Object,
                mockClock.Object
            );

            await Assert.ThrowsAsync<BadRequestException>(async () => await sut.Create(request, timeZone));
        }
        [Fact]
        public async Task Create_WhenAppointmentDateIsInPast_ThrowsBadRequestException()
        {
            // Arrange
            var mockCurrentUser = new Mock<ICurrentUserAccessor>();
            var mockJobProvider = new Mock<IBackgroundJobProvider>();
            var mockClock = new Mock<IClock>();

            var userId = 100;
            mockCurrentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var context = await GetContext([
                new Appointment
                {
                    Id = 1,
                    Title = "A1",
                    Description = "Desc1",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 3,
                    Title = "A3",
                    Description = "Desc3",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 2,
                    Title = "A2",
                    Description = "Desc2",
                    UserId = 999
                }
            ]);

            IUtcLocalConverter utcConverter = new UtcLocalConverter();
            var timeZone = "Africa/Cairo";

            var currentUtc = DateTime.UtcNow;
            mockClock.Setup(x => x.GetCurrentInstant())
                .Returns(Instant.FromUtc(
                    currentUtc.Year, currentUtc.Month, currentUtc.Day, currentUtc.Hour, currentUtc.Minute));

            var request = new CreateAppointmentRequest(
                "test title", "test desc", new DateTime(2026, 3, 1), new DateTime(2026, 3, 30), false
            );

            var sut = new AppointmentService(
                context,
                mockCurrentUser.Object,
                utcConverter,
                mockJobProvider.Object,
                mockClock.Object
            );

            await Assert.ThrowsAsync<BadRequestException>(async () => await sut.Create(request, timeZone));
        }
        [Fact]
        public async Task Create_WhenReminderDateIsInPast_ThrowsBadRequestException()
        {
            // Arrange
            var mockCurrentUser = new Mock<ICurrentUserAccessor>();
            var mockJobProvider = new Mock<IBackgroundJobProvider>();
            var mockClock = new Mock<IClock>();

            var userId = 100;
            mockCurrentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var context = await GetContext([
                new Appointment
                {
                    Id = 1,
                    Title = "A1",
                    Description = "Desc1",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 3,
                    Title = "A3",
                    Description = "Desc3",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 2,
                    Title = "A2",
                    Description = "Desc2",
                    UserId = 999
                }
            ]);

            var timeZone = "Africa/Cairo";

            IUtcLocalConverter utcConverter = new UtcLocalConverter();
            var currentUtc = DateTime.UtcNow;
            mockClock.Setup(x => x.GetCurrentInstant())
                .Returns(Instant.FromUtc(
                    currentUtc.Year, currentUtc.Month, currentUtc.Day, currentUtc.Hour, currentUtc.Minute));

            var request = new CreateAppointmentRequest(
                "test title", "test desc", new DateTime(2026, 4, 1), new DateTime(2026, 2, 15), false
            );

            var sut = new AppointmentService(
                context,
                mockCurrentUser.Object,
                utcConverter,
                mockJobProvider.Object,
                mockClock.Object
            );

            await Assert.ThrowsAsync<BadRequestException>(async () => await sut.Create(request, timeZone));
        }
        [Fact]
        public async Task Create_WhenAppointmentDateIsBeforeReminderDate_ThrowsBadRequestException()
        {
            // Arrange
            var mockCurrentUser = new Mock<ICurrentUserAccessor>();
            var mockJobProvider = new Mock<IBackgroundJobProvider>();
            var mockClock = new Mock<IClock>();

            var userId = 100;
            mockCurrentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);


            var context = await GetContext([
                new Appointment
                {
                    Id = 1,
                    Title = "A1",
                    Description = "Desc1",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 3,
                    Title = "A3",
                    Description = "Desc3",
                    UserId = userId
                },
                new Appointment
                {
                    Id = 2,
                    Title = "A2",
                    Description = "Desc2",
                    UserId = 999
                }
            ]);
            var timeZone = "Africa/Cairo";

            var currentUtc = DateTime.UtcNow;
            mockClock.Setup(x => x.GetCurrentInstant())
                .Returns(Instant.FromUtc(
                    currentUtc.Year, currentUtc.Month, currentUtc.Day, currentUtc.Hour, currentUtc.Minute));

            IUtcLocalConverter utcConverter = new UtcLocalConverter();
            var request = new CreateAppointmentRequest(
                "test title", "test desc", new DateTime(2026, 4, 1), new DateTime(2026, 4, 2), false
            );

            var sut = new AppointmentService(
                context,
                mockCurrentUser.Object,
                utcConverter,
                mockJobProvider.Object,
                mockClock.Object
            );

            await Assert.ThrowsAsync<BadRequestException>(async () => await sut.Create(request, timeZone));
        }
        [Fact]
        public async Task Update_WhenPathIdDoesntMatchRequestId_ThrowsBadRequestException()
        {
            int id = 5;
            var request = new UpdateAppointmentRequest(60, "", "", DateTime.Now, DateTime.Now, false);

            var context = await GetContext([]);
            var userAccsesorMock = new Mock<ICurrentUserAccessor>();
            var clockMock = new Mock<IClock>();
            var converterMock = new Mock<IUtcLocalConverter>();
            var jobProviderMock = new Mock<IBackgroundJobProvider>();

            var sut = new AppointmentService(
                context,
                userAccsesorMock.Object,
                converterMock.Object,
                jobProviderMock.Object,
                clockMock.Object);

            await Assert.ThrowsAsync<BadRequestException>(async () => await sut.Update(id, request, "Africa/Cairo"));
        }

        [Fact]
        public async Task Update_WhenTitleIsNullOrWhitespace_ThrowsBadRequestException()
        {
            var userAccsesorMock = new Mock<ICurrentUserAccessor>();
            var clockMock = new Mock<IClock>();
            var converterMock = new Mock<IUtcLocalConverter>();
            var jobProviderMock = new Mock<IBackgroundJobProvider>();

            int userId = 5;
            userAccsesorMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var appointmentId = 1;
            var request = new UpdateAppointmentRequest(appointmentId, "", "updated description", DateTime.Now, DateTime.Now, false);

            var context = await GetContext([new Appointment() {
                Id = 1,
                Title = "title",
                Description = "Descirption",
                UserId = userId
            }]);

            var sut = new AppointmentService(
                context,
                userAccsesorMock.Object,
                converterMock.Object,
                jobProviderMock.Object,
                clockMock.Object);

            var ex = await Assert.ThrowsAsync<BadRequestException>(async () => await sut.Update(appointmentId, request, "Africa/Cairo"));
            Assert.Contains("provide", ex.Message);
        }

        [Fact]
        public async Task Update_WhenAppointmentDateIsInPast_ThrowsBadRequestException()
        {
            var userAccsesorMock = new Mock<ICurrentUserAccessor>();
            var clockMock = new Mock<IClock>();
            var timeConverterMock = new Mock<IUtcLocalConverter>();
            var jobProviderMock = new Mock<IBackgroundJobProvider>();

            int userId = 5;
            userAccsesorMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            clockMock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2026, 3, 20, 1, 30)); // Invalid Appointmnet Date that will trigger the exceptoin

            timeConverterMock.Setup(x => x.ConvertLocalToUtc(It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(Instant.FromUtc(2000, 1, 1, 1, 1));

            var appointmentId = 20000;
            var request = new UpdateAppointmentRequest(appointmentId, "updated title", "updated description", DateTime.Now, DateTime.Now, false);

            var context = await GetContext([]);
            var sut = new AppointmentService(
                context,
                userAccsesorMock.Object,
                timeConverterMock.Object,
                jobProviderMock.Object,
                clockMock.Object);

            var ex = await Assert.ThrowsAsync<BadRequestException>(async () => await sut.Update(appointmentId, request, "Africa/Cairo"));
            Assert.Contains("Appointment date", ex.Message);
        }

        [Fact]
        public async Task Update_WhenReminderDateIsInPast_ThrowsBadRequestException()
        {
            var userAccsesorMock = new Mock<ICurrentUserAccessor>();
            var clockMock = new Mock<IClock>();
            var timeConverterMock = new Mock<IUtcLocalConverter>();
            var jobProviderMock = new Mock<IBackgroundJobProvider>();

            int userId = 5;
            userAccsesorMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            clockMock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2026, 3, 20, 1, 30));

            var utc = DateTime.UtcNow;
            DateTime currentUtc;
            if (utc.Day > 20)
            {
                if (utc.Month == 12)
                {
                    currentUtc = new DateTime(utc.Year + 1, 1, 1);
                }
                else
                    currentUtc = new DateTime(utc.Year, utc.Month + 1, 1);
            }
            currentUtc = new DateTime(utc.Year, utc.Month, utc.Day);

            Instant appointmentDate = Instant.FromUtc(currentUtc.Year, utc.Month, currentUtc.Day + 5, 1, 1);
            Instant reminderDate = Instant.FromUtc(2000, utc.Month, currentUtc.Day + 3, 1, 1); // Invalid Reminder that will trigger the exception

            timeConverterMock.SetupSequence(x => x.ConvertLocalToUtc(It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(appointmentDate)
                .Returns(reminderDate);

            var appointmentId = 20000;
            var request = new UpdateAppointmentRequest(appointmentId, "updated title", "updated description", DateTime.Now, DateTime.Now, false);

            var context = await GetContext([]);
            var sut = new AppointmentService(
                context,
                userAccsesorMock.Object,
                timeConverterMock.Object,
                jobProviderMock.Object,
                clockMock.Object);

            var ex = await Assert.ThrowsAsync<BadRequestException>(async () => await sut.Update(appointmentId, request, "Africa/Cairo"));
            Assert.Contains("Reminder date", ex.Message);
            Assert.Contains("future", ex.Message);
        }

        [Fact]
        public async Task Update_WhenReminderDateIsAfterAppointmentDate_ThrowsBadRequestException()
        {
            var userAccsesorMock = new Mock<ICurrentUserAccessor>();
            var clockMock = new Mock<IClock>();
            var timeConverterMock = new Mock<IUtcLocalConverter>();
            var jobProviderMock = new Mock<IBackgroundJobProvider>();

            int userId = 5;
            userAccsesorMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            clockMock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2026, 3, 20, 1, 30));

            var utc = DateTime.UtcNow;
            DateTime currentUtc;
            if (utc.Day > 20)
            {
                if (utc.Month == 12)
                {
                    currentUtc = new DateTime(utc.Year + 1, 1, 1);
                }
                else
                    currentUtc = new DateTime(utc.Year, utc.Month + 1, 1);
            }
            currentUtc = new DateTime(utc.Year, utc.Month, utc.Day);

            Instant appointmentDate = Instant.FromUtc(currentUtc.Year, utc.Month, currentUtc.Day + 5, 1, 1);
            Instant reminderDate = Instant.FromUtc(currentUtc.Year, utc.Month, currentUtc.Day + 6, 1, 1);
            // Reminder date is after the appointment date. Therefore, An exception will be thrown

            timeConverterMock.SetupSequence(x => x.ConvertLocalToUtc(It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(appointmentDate)
                .Returns(reminderDate);

            var appointmentId = 20000;
            var request = new UpdateAppointmentRequest(appointmentId, "updated title", "updated description", DateTime.Now, DateTime.Now, false);

            var context = await GetContext([]);
            var sut = new AppointmentService(
                context,
                userAccsesorMock.Object,
                timeConverterMock.Object,
                jobProviderMock.Object,
                clockMock.Object);

            var ex = await Assert.ThrowsAsync<BadRequestException>(async () => await sut.Update(appointmentId, request, "Africa/Cairo"));
            Assert.Contains("Reminder date", ex.Message);
            Assert.Contains("before", ex.Message);
        }

        [Fact]
        public async Task Update_WhenAppointmentNotFound_ThrowsNotFoundException()
        {
            var userAccsesorMock = new Mock<ICurrentUserAccessor>();
            var clockMock = new Mock<IClock>();
            var timeConverterMock = new Mock<IUtcLocalConverter>();
            var jobProviderMock = new Mock<IBackgroundJobProvider>();

            int userId = 5;
            userAccsesorMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            clockMock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2026, 3, 20, 1, 30));

            var utc = DateTime.UtcNow;
            DateTime currentUtc;
            if (utc.Day > 20)
            {
                if (utc.Month == 12)
                {
                    currentUtc = new DateTime(utc.Year + 1, 1, 1);
                }
                else
                    currentUtc = new DateTime(utc.Year, utc.Month + 1, 1);
            }
            currentUtc = new DateTime(utc.Year, utc.Month, utc.Day);

            Instant appointmentDate = Instant.FromUtc(currentUtc.Year, utc.Month, currentUtc.Day + 5, 1, 1);
            Instant reminderDate = Instant.FromUtc(currentUtc.Year, utc.Month, currentUtc.Day + 3, 1, 1);

            timeConverterMock.SetupSequence(x => x.ConvertLocalToUtc(It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(appointmentDate)
                .Returns(reminderDate);

            var appointmentId = 20000;
            var request = new UpdateAppointmentRequest(appointmentId, "updated title", "updated description", DateTime.Now, DateTime.Now, false);

            var context = await GetContext([
                new Appointment {
                    Id = 1,
                    Title = "title",
                    Description = "description",
                    UserId = userId
                }]);

            var sut = new AppointmentService(
                context,
                userAccsesorMock.Object,
                timeConverterMock.Object,
                jobProviderMock.Object,
                clockMock.Object);

            await Assert.ThrowsAsync<NotFoundException>(async () => await sut.Update(appointmentId, request, "Africa/Cairo"));
        }

        [Fact]
        public async Task Update_WhenAppointmentFound_UpdatesRecordCorrectly()
        {
            var userAccsesorMock = new Mock<ICurrentUserAccessor>();
            var clockMock = new Mock<IClock>();
            var timeConverterMock = new Mock<IUtcLocalConverter>();
            var jobProviderMock = new Mock<IBackgroundJobProvider>();

            int userId = 5;
            userAccsesorMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            clockMock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2026, 3, 20, 1, 30));

            var utc = DateTime.UtcNow;
            DateTime currentUtc;
            if (utc.Day > 20)
            {
                if (utc.Month == 12)
                {
                    currentUtc = new DateTime(utc.Year + 1, 1, 1);
                }
                else
                    currentUtc = new DateTime(utc.Year, utc.Month + 1, 1);
            }
            currentUtc = new DateTime(utc.Year, utc.Month, utc.Day);

            Instant appointmentDate = Instant.FromUtc(currentUtc.Year, utc.Month, currentUtc.Day + 5, 1, 1);
            Instant reminderDate = Instant.FromUtc(currentUtc.Year, utc.Month, currentUtc.Day + 3, 1, 1);

            timeConverterMock.SetupSequence(x => x.ConvertLocalToUtc(It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(appointmentDate)
                .Returns(reminderDate);

            var appointmentId = 1;
            var request = new UpdateAppointmentRequest(appointmentId, "updated title", "updated description", DateTime.Now, DateTime.Now, false);

            var context = await GetContext([
                new Appointment {
                    Id = 1,
                    Title = "title",
                    Description = "description",
                    UserId = userId
                }]);

            var sut = new AppointmentService(
                context,
                userAccsesorMock.Object,
                timeConverterMock.Object,
                jobProviderMock.Object,
                clockMock.Object);

            await sut.Update(appointmentId, request, "Africa/Cairo");

            Appointment updatedAppointment = await context.Appointments
                .FirstAsync(x => x.Id == appointmentId && x.UserId == userId);

            Assert.Equal(request.Title, updatedAppointment.Title);
            Assert.Equal(request.Description, updatedAppointment.Description);
            Assert.Equal(appointmentDate, updatedAppointment.Date);
            Assert.Equal(reminderDate, updatedAppointment.ReminderDate);

            jobProviderMock.Verify(x =>
            x.RescheduleAppointmentsJobs(
                It.IsAny<Appointment>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                request.WantAutoDelete),
            Times.Once);
        }

        [Fact]
        public async Task Delete_WhenAppointmentNotFound_ThrowsNotFoundException()
        {
            var userAccsesorMock = new Mock<ICurrentUserAccessor>();
            var clockMock = new Mock<IClock>();
            var timeConverterMock = new Mock<IUtcLocalConverter>();
            var jobProviderMock = new Mock<IBackgroundJobProvider>();

            int userId = 5;
            userAccsesorMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var context = await GetContext([
                new Appointment {
                    Id = 1,
                    Title = "title",
                    Description = "description",
                    UserId = userId
                }]);

            var sut = new AppointmentService(
                context,
                userAccsesorMock.Object,
                timeConverterMock.Object,
                jobProviderMock.Object,
                clockMock.Object
            );

            var appointmentId = 2000;
            await Assert.ThrowsAsync<NotFoundException>(async () => await sut.Delete(appointmentId));
        }

        [Fact]
        public async Task Delete_WhenAppointmentFound_DeletesCorrectly()
        {
            var userAccsesorMock = new Mock<ICurrentUserAccessor>();
            var clockMock = new Mock<IClock>();
            var timeConverterMock = new Mock<IUtcLocalConverter>();
            var jobProviderMock = new Mock<IBackgroundJobProvider>();

            int userId = 5;
            userAccsesorMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var context = await GetContext([
                new Appointment {
                    Id = 1,
                    Title = "title",
                    Description = "description",
                    UserId = userId
                }]);

            var sut = new AppointmentService(
                context,
                userAccsesorMock.Object,
                timeConverterMock.Object,
                jobProviderMock.Object,
                clockMock.Object
            );

            var appointmentId = 1;
            await sut.Delete(appointmentId);

            var shouldBeNull = await context.Appointments
                .FirstOrDefaultAsync(x => x.Id == appointmentId);

            Assert.Null(shouldBeNull);
            jobProviderMock.Verify(x => x.DeleteJobs(appointmentId), Times.Once);
        }
        private async Task<AppDbContext> GetContext(Appointment[] appointments)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"AppointmentScheduler-{Guid.NewGuid()}")
                .Options;

            var context = new AppDbContext(options);

            var instant = Instant.FromUtc(2027, 1, 1, 0, 0);
            var createdAt = Instant.FromUtc(2026, 12, 25, 0, 0);

            foreach (var appointment in appointments)
            {
                appointment.CreatedAt = createdAt;
                appointment.Date = instant;
                appointment.ReminderDate = instant;
            }

            await context.Appointments.AddRangeAsync(appointments);

            await context.SaveChangesAsync();

            return context;
        }
    }
}
