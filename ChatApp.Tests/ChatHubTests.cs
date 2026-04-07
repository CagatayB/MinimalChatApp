using ChatApp.API.Hubs;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Services;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Tests
{
    public class ChatHubTests
    {
        [Fact]
        public async Task SendPrivateMessage_ValidMessage_SavesToDbAndCallsClient()
        {
            var options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name per test
                .Options;

            using var context = new ChatDbContext(options);

            var mockClients = new Mock<IHubCallerClients<IChatClient>>();
            var mockClientProxy = new Mock<IChatClient>();
            var mockContext = new Mock<HubCallerContext>();

            mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
            mockClients.Setup(c => c.Caller).Returns(mockClientProxy.Object);

            var senderId = "2";
            mockContext.Setup(c => c.UserIdentifier).Returns(senderId);

            var validator = new ChatApp.Application.Services.ChatValidationService();

            var hub = new ChatHub(context, validator)
            {
                Clients = mockClients.Object,
                Context = mockContext.Object
            };

            var receiverId = "ReceiverUser";
            var messageContent = "Hello World";
            await hub.SendPrivateMessage(receiverId, messageContent);


            var savedMessage = await context.Messages.FirstOrDefaultAsync();
            Assert.NotNull(savedMessage);
            Assert.Equal(messageContent, savedMessage.Content);
            Assert.Equal(senderId, savedMessage.SenderId);
            Assert.Equal(receiverId, savedMessage.ReceiverId);


            mockClientProxy.Verify(
                c => c.ReceiveMessage(
                    It.Is<string>(s => s == senderId),
                    It.Is<string>(m => m == messageContent),
                    It.IsAny<DateTime>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task SendPrivateMessage_InvalidMessage_ReturnsErrorAndDoesNotSave()
        {
            var options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: "ValidationFailDb")
                .Options;
            using var context = new ChatDbContext(options);

            var mockClients = new Mock<IHubCallerClients<IChatClient>>();
            var mockCallerProxy = new Mock<IChatClient>();
            var mockValidator = new Mock<IChatValidationService>();

            mockValidator.Setup(v => v.IsValidMessage(It.IsAny<string>())).Returns(false);

            mockClients.Setup(c => c.Caller).Returns(mockCallerProxy.Object);

            var hub = new ChatHub(context, mockValidator.Object)
            {
                Clients = mockClients.Object,
                Context = new Mock<HubCallerContext>().Object
            };

            await hub.SendPrivateMessage("ReceiverUser", "");

            var count = await context.Messages.CountAsync();
            Assert.Equal(0, count);

            mockCallerProxy.Verify(
                c => c.ReceiveError(
                    It.Is<string>(code => code == "Message cannot be empty and must be under 500 characters.")),
                Times.Once);

            mockClients.Verify(c => c.User(It.IsAny<string>()), Times.Never);
        }
    }
}
