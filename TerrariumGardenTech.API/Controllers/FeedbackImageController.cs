using FeedbackGardenTech.Service.IService;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.RequestModel.FeedbackImage;
using TerrariumGardenTech.Service.Base;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackImageController : ControllerBase
    {
        private readonly IFeedbackImageService _feedbackImageService;   
        // GET: api/<FeedbackImageController>
        [HttpGet("get-all")]
        public async Task<IBusinessResult> Get()
        {
            return await _feedbackImageService.GetAllFeedbackImageAsync();
        }

        // GET api/<FeedbackImageController>/5
        [HttpGet("get/{id}")]
        public async Task<IBusinessResult> Get(int id)
        {
            return await _feedbackImageService.GetFeedbackImageByIdAsync(id);
        }

        // GET api/<FeedbackImageController>/feedbackId/5
        [HttpGet("get-by-feedbackId/{feedbackId}")]
        public async Task<IBusinessResult> GetByFeedbackId(int feedbackId)
        {
            return await _feedbackImageService.GetByFeedbackId(feedbackId);
        }

        // POST api/<FeedbackImageController>
        [HttpPost("add-image")]
        public Task<IBusinessResult> Post([FromBody] FeedbackImageUploadRequest feedbackImageUploadRequest)
        {
            return _feedbackImageService.CreateFeedbackImageAsync(feedbackImageUploadRequest.ImageFile, feedbackImageUploadRequest.FeedbackId);
        }

        // PUT api/<FeedbackImageController>/5
        [HttpPut("update-image/{id}")]
        public Task<IBusinessResult> Put(int id, [FromBody] FeedbackImageUploadUpdateRequest feedbackImageUploadUpdateRequest)
        {
            feedbackImageUploadUpdateRequest.FeedbackImageId = id; // Set the ID for the update request
            return _feedbackImageService.UpdateFeedbackImageAsync(feedbackImageUploadUpdateRequest);
        }

        // DELETE api/<FeedbackImageController>/5
        [HttpDelete("delete-image/{id}")]
        public async Task<IBusinessResult> Delete(int id)
        {
            return await _feedbackImageService.DeleteFeedbackImageAsync(id);
        }
    }
}
