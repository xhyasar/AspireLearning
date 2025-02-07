using Microsoft.Extensions.Hosting.GlobalModel.Identity;
using Refit;

namespace AspireLearning.BFF.Microservices.Identity;

public interface IIdentityClient: IUserClient, IAuthClient;

public interface IUserClient;

public interface IAuthClient
{
    [Post("auth/login")]
    Task<UserTokenModel> Login([Body]LoginModel model);
}