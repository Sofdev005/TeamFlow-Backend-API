using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TeamFlow.Application;
using TeamFlow.Application.Features.Boards.Commands.CreateBoardColumn;
using TeamFlow.Application.Features.Boards.Common;

namespace TeamFlow.Application.Tests;

public class ApplicationServiceRegistrationTests
{
    [Fact]
    public void CreateBoardColumnHandler_IsRegisteredWithMediatR()
    {
        var services = new ServiceCollection();
        services.AddApplicationServices();

        var handlerRegistration = services.SingleOrDefault(descriptor =>
            descriptor.ServiceType == typeof(IRequestHandler<CreateBoardColumnCommand, BoardColumnResponse>));

        Assert.NotNull(handlerRegistration);
        Assert.Equal(typeof(CreateBoardColumnCommandHandler), handlerRegistration.ImplementationType);
    }
}