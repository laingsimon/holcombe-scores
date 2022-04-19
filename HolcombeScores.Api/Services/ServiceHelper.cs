using HolcombeScores.Api.Models;

namespace HolcombeScores.Api.Services
{
    public class ServiceHelper : IServiceHelper
    {
        public ActionResultDto<T> Success<T>(string message, T outcome = default)
        {
           return new ActionResultDto<T>
           {
               Messages =
               {
                   message,
               },
               Outcome = outcome,
               Success = true,
           };
        }

        public ActionResultDto<T> NotSuccess<T>(string message)
        {
           return new ActionResultDto<T>
           {
               Messages =
               {
                   message,
               }
           };
        }

        public ActionResultDto<T> NotFound<T>(string message)
        {
           return new ActionResultDto<T>
           {
               Warnings =
               {
                   message,
               },
           };
        }

        public ActionResultDto<T> NotAnAdmin<T>()
        {
           return new ActionResultDto<T>
           {
               Errors =
               {
                   "Not an admin",
               },
           };
        }

        public ActionResultDto<T> NotPermitted<T>(string message)
        {
           return new ActionResultDto<T>
           {
               Errors =
               {
                   message,
               },
           };
        }

        public ActionResultDto<T> NotLoggedIn<T>()
        {
           return new ActionResultDto<T>
           {
               Warnings =
               {
                   "Not logged in",
               },
           };
        }

        public ActionResultDto<T> Error<T>(string error)
        {
            return new ActionResultDto<T>
            {
                Errors =
                {
                    error,
                }
            };
        }
    }
}