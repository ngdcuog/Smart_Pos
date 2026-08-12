using Microsoft.EntityFrameworkCore;
using SmartPOS.Data;
using SmartPOS.Models.Enums;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Tests;

public sealed class EmployeeAndAttendanceServiceTests : IAsyncLifetime
{
    private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SmartPOSPhase6Tests;Trusted_Connection=True;TrustServerCertificate=True";
    private readonly DbContextOptions<AppDbContext> _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(ConnectionString).Options;
    private readonly FakeClock _clock = new() { Value = new DateTime(2026, 8, 12, 8, 10, 0) };
    public async Task InitializeAsync() { await using var c = new AppDbContext(_options); await c.Database.EnsureDeletedAsync(); await c.Database.MigrateAsync(); }
    public async Task DisposeAsync() { await using var c = new AppDbContext(_options); await c.Database.EnsureDeletedAsync(); }

    [Fact]
    public async Task Employee_CreateUpdateAndStatus_EnforcesUniqueImmutableCode()
    {
        var employees = new EmployeeService(new Factory(_options));
        await employees.CreateEmployeeAsync(new(null, "nv0003", "Lê Thu Hà", "ha@smartpos.local", "0903", EmployeeRole.Cashier));
        var created = (await employees.GetEmployeesAsync("NV0003", null, null)).Single();
        Assert.Equal("NV0003", created.EmployeeCode);
        await Assert.ThrowsAsync<EmployeeServiceException>(() => employees.CreateEmployeeAsync(new(null, "NV0003", "Khác", "other@smartpos.local", null, EmployeeRole.Cashier)));
        await Assert.ThrowsAsync<EmployeeServiceException>(() => employees.UpdateEmployeeAsync(new(created.EmployeeId, "NV0099", "Lê Thu Hà", "ha@smartpos.local", null, EmployeeRole.Manager)));
        await employees.SetEmployeeActiveStateAsync(created.EmployeeId, false);
        Assert.False((await employees.GetEmployeesAsync("NV0003", null, null)).Single().IsActive);
    }

    [Fact]
    public async Task Attendance_CheckInNormalizesQr_AndClassifiesOnTimeLate()
    {
        var service = CreateAttendance();
        var onTime = await service.CheckInAsync(" EMPLOYEE:nv0001 ");
        Assert.Equal("NV0001", onTime.EmployeeCode);
        await using (var c = new AppDbContext(_options))
        {
            var record = await c.Attendances.SingleAsync(x => x.EmployeeId == 1);
            Assert.Equal(AttendanceStatus.OnTime, record.Status);
        }
        _clock.Value = new DateTime(2026, 8, 13, 8, 16, 0);
        await service.CheckInAsync("NV0002");
        await using var verify = new AppDbContext(_options);
        Assert.Equal(AttendanceStatus.Late, await verify.Attendances.Where(x => x.EmployeeId == 2).Select(x => x.Status).SingleAsync());
    }

    [Fact]
    public async Task Attendance_PreventsDoubleCheckIn_AndRequiresCheckInBeforeCheckout()
    {
        var service = CreateAttendance();
        await Assert.ThrowsAsync<AttendanceServiceException>(() => service.CheckOutAsync("NV0001"));
        await service.CheckInAsync("NV0001");
        await Assert.ThrowsAsync<AttendanceServiceException>(() => service.CheckInAsync("NV0001"));
        await service.CheckOutAsync("NV0001");
        await Assert.ThrowsAsync<AttendanceServiceException>(() => service.CheckOutAsync("NV0001"));
    }

    [Fact]
    public async Task Attendance_RejectsUnknownAndInactiveEmployees()
    {
        var service = CreateAttendance();
        await Assert.ThrowsAsync<AttendanceServiceException>(() => service.CheckInAsync("NV4040"));
        await using (var c = new AppDbContext(_options)) { var employee = await c.Employees.FindAsync(2); employee!.IsActive = false; await c.SaveChangesAsync(); }
        await Assert.ThrowsAsync<AttendanceServiceException>(() => service.CheckInAsync("NV0002"));
    }

    private AttendanceService CreateAttendance() => new(new Factory(_options), _clock, new AttendanceSettings());
    private sealed class FakeClock : IDateTimeProvider { public DateTime Value { get; set; } public DateTime Now => Value; }
    private sealed class Factory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext> { public AppDbContext CreateDbContext() => new(options); public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext()); }
}
