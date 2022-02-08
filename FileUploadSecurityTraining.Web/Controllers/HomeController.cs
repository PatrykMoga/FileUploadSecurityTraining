using FileUploadSecurityTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FileUploadSecurityTraining.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly long _fileSizeLimit;
        private string[] permittedExtensions = { ".txt", ".pdf", ".jpg" };
        private readonly bool hiddeFileName = true;


        public HomeController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;

            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");
        }

        public ActionResult Index()
        {
            var path = Path.Combine(_env.ContentRootPath, "Assets/images");
            var filePaths = Directory.GetFiles(path);

            var files = filePaths.Select(filePath => new FileModel()
            {
                FileName = Path.GetFileName(filePath)
            });

            return View(files);
        }

        public FileResult DownloadFile(string fileName)
        {
            var path = Path.Combine(_env.ContentRootPath, "Assets/images", fileName);
            var bytes = System.IO.File.ReadAllBytes(path);

            return File(bytes, "application/octet-stream", fileName);
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadFile(List<IFormFile> files)
        {
            var path = Path.Combine(_env.ContentRootPath, "Assets/images");

            foreach (IFormFile file in files)
            {
                if (file.Length > _fileSizeLimit)
                    return View("Error");

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!permittedExtensions.Contains(extension))
                    return View("Error");

                var fileName = hiddeFileName ? Guid.NewGuid().ToString() + extension : Path.GetFileNameWithoutExtension(file.FileName);
                var filePath = Path.Combine(path, fileName);

                using Stream fileStream = new FileStream(filePath, FileMode.Create);
                file.CopyTo(fileStream);
            }

            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
