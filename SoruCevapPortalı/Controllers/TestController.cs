using Microsoft.AspNetCore.Mvc;

namespace SoruCevapPortali.Controllers
{
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Ask()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Ask(int CategoryId, string Title, string QuestionContent)
        {
            return Content(
                "POST ÇALIŞTI ✅\n\n" +
                $"CategoryId = {CategoryId}\n" +
                $"Title = {Title}\n" +
                $"QuestionContent = {QuestionContent}"
            );
        }
    }
}
