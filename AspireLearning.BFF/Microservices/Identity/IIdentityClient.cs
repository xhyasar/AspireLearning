using Microsoft.Extensions.Hosting.GlobalModel.Identity;
using Refit;

namespace AspireLearning.BFF.Microservices.Identity;

public interface IIdentityClient: IUserClient, IAuthClient;

public interface IUserClient
{
    [Post("user/create")]
    Task Register([Body]UserCreateModel model);
    
    [Get("user/{id}")]
    Task<UserViewModel> GetUser(Guid id);
}

public interface IAuthClient
{
    [Post("auth/login")]
    Task<UserTokenModel> Login([Body]LoginModel model);
}