using BlazorWeb.Data;
using BlazorWeb.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace BlazorWeb.Controllers
{
    public class FileController : Controller
    {
        private readonly WebFileRepository fileRepo;
        private readonly WebFileService fileService;
        public FileController(WebFileService fileService, WebFileRepository fileRepo)
        {
            this.fileService = fileService;
            this.fileRepo = fileRepo;
        }

        [HttpGet("/file/{oid}/download")]
        public async Task<IActionResult> Download(string oid)
        {
            var file = await fileRepo.GetAsync(oid);

            if (file == null || file == default || file?.GridfsId == null || file.GridfsId == default)
                return NotFound();

            var stream = await fileService.OpenDownloadStreamAsync(file);
            var fileStream = new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = file.Filename
            };

            return fileStream;
        }
    }
}
