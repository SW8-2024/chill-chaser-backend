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

public class DataAnalysisControllerTests_StressMetrics
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
        LatestHeartRate testLatestHeartRate = new() { DateTime = new DateTime(2024, 5, 15), Value = 50 };

        Mock<CCDbContext> mockDbContextRepo = new(new DbContextOptions<CCDbContext>());
        Mock<IAnalysisService> mockanalysisService = new();
        mockanalysisService.Setup(repo => repo.GetTodayRange(new DateTime(2024, 5, 15)))
            .Returns(testDateRange);
        mockanalysisService.Setup(repo => repo.GetStressMetrics(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testStressMetrics);
        mockanalysisService.Setup(repo => repo.GetLatestHeartRate(mockDbContextRepo.Object, testUserId))
            .ReturnsAsync(testLatestHeartRate);
        var controller = new DataAnalysisController(mockanalysisService.Object, mockDbContextRepo.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            }
        };

        // Act
        var result = await controller.StressMetrics(new DateTime(2024,5,15));

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnStressMetrics = Assert.IsType<GetStressMetricsResponse>(okResult.Value);
        Assert.Equal(returnStressMetrics.Min, testStressMetrics.Min);
        Assert.Equal(returnStressMetrics.Max, testStressMetrics.Max);
        Assert.Equal(returnStressMetrics.Average, testStressMetrics.Average);
        Assert.Equal(returnStressMetrics.Latest, testLatestHeartRate.Value);
        Assert.Equal(returnStressMetrics.LatestDateTime, testLatestHeartRate.DateTime);
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
        LatestHeartRate testLatestHeartRate = new() { DateTime = new DateTime(2019, 12, 1), Value = 50 };

        Mock<CCDbContext> mockDbContextRepo = new(new DbContextOptions<CCDbContext>());
        Mock<IAnalysisService> mockanalysisService = new();
        mockanalysisService.Setup(repo => repo.GetTodayRange(new DateTime(2019, 12, 1)))
            .Returns(testDateRange);
        mockanalysisService.Setup(repo => repo.GetStressMetrics(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testStressMetrics);
        mockanalysisService.Setup(repo => repo.GetLatestHeartRate(mockDbContextRepo.Object, testUserId))
            .ReturnsAsync(testLatestHeartRate);
        var controller = new DataAnalysisController(mockanalysisService.Object, mockDbContextRepo.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            }
        };

        // Act
        var result = await controller.StressMetrics(new DateTime(2019, 12, 1));

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
        LatestHeartRate testLatestHeartRate = new() { DateTime = new DateTime(2024, 5, 15), Value = 50 };

        Mock<CCDbContext> mockDbContextRepo = new(new DbContextOptions<CCDbContext>());
        Mock<IAnalysisService> mockanalysisService = new();
        mockanalysisService.Setup(repo => repo.GetTodayRange(new DateTime(2024, 5, 15)))
            .Returns(testDateRange);
        mockanalysisService.Setup(repo => repo.GetStressMetrics(mockDbContextRepo.Object, testUserId, testDateRange))
            .ReturnsAsync(testStressMetrics);
        mockanalysisService.Setup(repo => repo.GetLatestHeartRate(mockDbContextRepo.Object, testUserId))
            .ReturnsAsync(testLatestHeartRate);
        var controller = new DataAnalysisController(mockanalysisService.Object, mockDbContextRepo.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            }
        };

        // Act
        var result = () => controller.StressMetrics(new DateTime(2024, 5, 15));

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(result);
        Assert.Equal("No user id", exception.Message);
    }

    private readonly StressMetrics testStressMetrics = new() { 
        Average = 75,
        Min = 50,
        Max = 100    
    };
}