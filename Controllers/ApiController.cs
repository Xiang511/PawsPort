using Microsoft.AspNetCore.Mvc;
using PawsPort.Responses;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PawsPort.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
         // (200 OK)
        protected IActionResult Success<T>(T data, string message = "Success", int statusCode = 200)
        {
            return StatusCode(statusCode, ApiResponse<T>.Ok(data, message));
        }

        // (400 401 403 404 405)
        protected IActionResult Failure(string code, string message, int statusCode = 400)
        {
            var result = ApiResponse<Array>.Fail(code, message);
            return StatusCode(statusCode, result);
        }

        // 204 使用原本的 NoContent()
    }
}
