using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUParkingApi.Controllers
{
    [ApiController]
    [Route("api/role")]
    [Authorize(AuthenticationSchemes = "Defaut")]
    public class RoleController : Controller
    {
        //private readonly IRoleService _roleService;

        //public RoleController(IRoleService roleService)
        //{
        //    _roleService = roleService;
        //}
    }
}
