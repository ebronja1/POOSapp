using Microsoft.AspNetCore.Mvc;
using ImageProcessingAPI.Services;

namespace ImageProcessingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ImageClassificationService _classificationService;

        public ImageController()
        {
            // Path to the model file
            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "TrainedModels", "model.h5");
            _classificationService = new ImageClassificationService(modelPath);
        }

        [HttpPost("upload")]
        public IActionResult UploadImage(IFormFile file)
        {
            Console.WriteLine("poziva");
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                using var memoryStream = new MemoryStream();
                file.CopyTo(memoryStream);
                var imageBytes = memoryStream.ToArray();

                // Classify the image
                var result = _classificationService.ClassifyImage(imageBytes);

                return Ok(new { Prediction = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
