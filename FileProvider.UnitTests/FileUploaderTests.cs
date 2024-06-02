using FileProvider.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace FileProvider.UnitTests
{
    public class FileUploaderTests
    {
        private readonly Mock<ILogger<FileUploader>> _loggerMock;
        private readonly FileUploader _fileUploader;

        public FileUploaderTests()
        {
            _loggerMock = new Mock<ILogger<FileUploader>>();
            _fileUploader = new FileUploader(_loggerMock.Object);
        }

        [Fact]
        public async Task Run_Should_Return_OkObjectResult_When_File_Is_Uploaded_Successfully()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.txt";
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Position = 0;

            fileMock.Setup(_ => _.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(memoryStream.Length);
            fileMock.Setup(_ => _.ContentType).Returns("text/plain");

            var formFileCollection = new FormFileCollection { fileMock.Object };

            var formCollectionMock = new Mock<IFormCollection>();
            formCollectionMock.Setup(_ => _.Files).Returns(formFileCollection);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(_ => _.Form).Returns(formCollectionMock.Object);

            Environment.SetEnvironmentVariable("ConnectionString", "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable("FileContainerName", "test-container");

            // Act
            var result = await _fileUploader.Run(httpRequestMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var blobUri = Assert.IsType<Uri>(okResult.Value);
            Assert.EndsWith(fileName, blobUri.ToString());
        }
    }
}
