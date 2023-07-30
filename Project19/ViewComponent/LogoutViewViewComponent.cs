using Microsoft.AspNetCore.Mvc;

namespace Project19.Component
{
    public class LogoutViewViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
