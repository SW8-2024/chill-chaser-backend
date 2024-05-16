using ChillChaser.Controllers;
using ChillChaser.Models.Internal;
using ChillChaser.Models.Response;
using ChillChaser.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace ChillChaser.UnitTests.Controllers;

public class DataAnalysisControllerTests_BreakDown
{
    [Fact]
    public async Task Returns_OkResult()
    {
        // Arrange
        string testUserId = "testUser";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
            new (ClaimTypes.NameIdentifier, testUserId),
        }, "mock"));
        DateOnlyRange testDateRange = new(){ To = DateOnly.MinValue, From = DateOnly.MaxValue };
        
        Mock<CCDbContext> mockDbContextRepo = new(new DbContextOptions<CCDbContext>());
        Mock<IAnalysisService> mockanalysisService = new();
        mockanalysisService.Setup(repo => repo.GetLastMonthRange(new DateTime(2024, 5, 15)))
            .Returns(testDateRange);
        mockanalysisService.Setup(repo => repo.GetStressByApp(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testStressByAppDataPoints);
        mockanalysisService.Setup(repo => repo.GetDailyStress(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testDailyStressDataPoint);
        mockanalysisService.Setup(repo => repo.GetStressMetrics(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testStressMetrics);
        var controller = new DataAnalysisController(mockanalysisService.Object, mockDbContextRepo.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            }
        };

        // Act
        var result = await controller.BreakDown(new DateTime(2024,5,15));

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnBreakdown = Assert.IsType<GetBreakdownDataResponse>(okResult.Value);
        Assert.Equal(returnBreakdown.StressByApp.Count(), testStressByAppDataPoints.Count());
        Assert.Equal(returnBreakdown.DailyStressDataPoints.Count(), testDailyStressDataPoint.Count());
        Assert.Equal(returnBreakdown.AverageStress, testStressMetrics.Average);
    }

    [Fact]
    public async Task BadRequest_Before_2020()
    {
        // Arrange
        string testUserId = "testUser";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
            new (ClaimTypes.NameIdentifier, testUserId),
        }, "mock"));
        DateOnlyRange testDateRange = new() { To = DateOnly.MinValue, From = DateOnly.MaxValue };

        Mock<CCDbContext> mockDbContextRepo = new(new DbContextOptions<CCDbContext>());
        Mock<IAnalysisService> mockanalysisService = new();
        mockanalysisService.Setup(repo => repo.GetLastMonthRange(new DateTime(2019, 12, 1)))
            .Returns(testDateRange);
        mockanalysisService.Setup(repo => repo.GetStressByApp(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testStressByAppDataPoints);
        mockanalysisService.Setup(repo => repo.GetDailyStress(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testDailyStressDataPoint);
        mockanalysisService.Setup(repo => repo.GetStressMetrics(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testStressMetrics);
        var controller = new DataAnalysisController(mockanalysisService.Object, mockDbContextRepo.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            }
        };

        // Act
        var result = await controller.BreakDown(new DateTime(2019, 12, 1));

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Exception_When_No_User_Found()
    {
        // Arrange
        string testUserId = "testUser";
        var user = new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>(), "mock"));
        DateOnlyRange testDateRange = new() { To = DateOnly.MinValue, From = DateOnly.MaxValue };

        Mock<CCDbContext> mockDbContextRepo = new(new DbContextOptions<CCDbContext>());
        Mock<IAnalysisService> mockanalysisService = new();
        mockanalysisService.Setup(repo => repo.GetLastMonthRange(new DateTime(2024, 5, 15)))
            .Returns(testDateRange);
        mockanalysisService.Setup(repo => repo.GetStressByApp(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testStressByAppDataPoints);
        mockanalysisService.Setup(repo => repo.GetDailyStress(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testDailyStressDataPoint);
        mockanalysisService.Setup(repo => repo.GetStressMetrics(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testStressMetrics);
        var controller = new DataAnalysisController(mockanalysisService.Object, mockDbContextRepo.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            }
        };

        // Act
        var result = () => controller.BreakDown(new DateTime(2024, 5, 15));

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(result);
        Assert.Equal("No user id", exception.Message);
    }

    private readonly IEnumerable<StressByAppDataPoint> testStressByAppDataPoints = [
        new StressByAppDataPoint()
        {
            Name = "Tinder",
            Value = 10
        },
        new StressByAppDataPoint()
        {
            Name = "Hinge",
            Value = 50
        },
        new StressByAppDataPoint()
        {
            Name = "Grindr",
            Value = 100
        },
    ];

    private readonly IEnumerable<DailyStressDataPoint> testDailyStressDataPoint = [
        new DailyStressDataPoint()
        {
            Date = new DateTime(2024, 5, 15),
            Value = 10
        },
        new DailyStressDataPoint()
        {
            Date = new DateTime(2024, 5, 15),
            Value = 50
        },
        new DailyStressDataPoint()
        {
            Date = new DateTime(2024, 5, 15),
            Value = 100
        },
    ];

    private readonly StressMetrics testStressMetrics = new() { 
        Average = 75,
        Min = 50,
        Max = 100    
    };
}