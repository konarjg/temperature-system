using ExternalServiceAdapters.EmailService;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.ExternalServiceAdapters {
  public class MockEmailServiceTests {
    private readonly Mock<ILogger<MockEmailService>> _loggerMock;
    private readonly MockEmailService _mockEmailService;

    public MockEmailServiceTests() {
      _loggerMock = new Mock<ILogger<MockEmailService>>();
      _mockEmailService = new MockEmailService(_loggerMock.Object);
    }

    [Fact]
    public async Task SendEmail_ShouldLogEmailDetails() {
      // Arrange
      string subject = "Test Subject";
      string body = "Test Body";
      string to = "recipient@test.com";
      string expectedLogMessage = $"Subject: {subject}\nBody: {body}";

      // Act
      bool result = await _mockEmailService.SendEmail(subject, body, to);

      // Assert
      Assert.True(result);

      // Verify that the logger was called with the correct information
      _loggerMock.Verify(
          x => x.Log(
              LogLevel.Information,
              It.IsAny<EventId>(),
              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedLogMessage)),
              null,
              It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }
  }
}