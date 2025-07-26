using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace POS_Server_PL.Controllers
{
    [Route("")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetInfo()
        {
            var info = new
            {
                message = "Welcome to the Point of Sale API.",
                status = "Running",
                description = "This is a backend Web API for a Point of Sale system. If you're seeing this, you're likely visiting the API root directly.",
                resources = new
                {
                    postman_collection = "https://www.postman.com/ammar-0/public-workspace-1/collection/y8uul8m/pointofsale",
                    github_repository = "https://github.com/Ammar-000/PointOfSale",
                    docker_hub_repository = "https://hub.docker.com/repository/docker/ammarot/point_of_sale/general",
                },
                note = "This API is designed to be consumed programmatically. Please refer to the GitHub README or Postman collection to explore endpoints and usage instructions."
            };

            return Ok(info);
        }
    }
}
