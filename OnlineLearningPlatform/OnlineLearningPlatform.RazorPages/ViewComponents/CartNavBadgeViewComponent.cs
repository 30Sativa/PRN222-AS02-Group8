using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatform.RazorPages.Services;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.ViewComponents
{
    public class CartNavBadgeViewComponent : ViewComponent
    {
        private readonly StudentCartService _cart;

        public CartNavBadgeViewComponent(StudentCartService cart)
        {
            _cart = cart;
        }

        public IViewComponentResult Invoke()
        {
            if (UserClaimsPrincipal.Identity?.IsAuthenticated != true)
                return Content(string.Empty);

            if (!UserClaimsPrincipal.IsInRole("Student"))
                return Content(string.Empty);

            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Content(string.Empty);

            return View(_cart.Count);
        }
    }
}
