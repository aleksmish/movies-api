using Microsoft.AspNetCore.Mvc;

namespace MoviesAPI.ApiBehavior
{
    public class BadRequestBehavior
    {
        public static void Parse(ApiBehaviorOptions options)
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var response = new List<String>();

                foreach (var key in context.ModelState.Keys)
                {
                    foreach (var error in context.ModelState[key].Errors)
                    {
                        response.Add($"{key}: {error.ErrorMessage}");
                    }
                }

                return new BadRequestObjectResult(response);
            };
        }
    }
}
