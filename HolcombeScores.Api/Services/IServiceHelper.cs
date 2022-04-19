using HolcombeScores.Api.Models;

namespace HolcombeScores.Api.Services
{
    public interface IServiceHelper
    {
        ActionResultDto<T> Success<T>(string message, T outcome = default);
        ActionResultDto<T> NotSuccess<T>(string message);
        ActionResultDto<T> NotFound<T>(string message);
        ActionResultDto<T> NotAnAdmin<T>();
        ActionResultDto<T> NotPermitted<T>(string message);
        ActionResultDto<T> NotLoggedIn<T>();
        ActionResultDto<T> Error<T>(string error);
    }
}