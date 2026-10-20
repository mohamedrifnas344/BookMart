using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Application.IServices
{
    public interface ICloudinaryImage
    {
        Task<UploadResult> UploadImageAsync(IFormFile file);
        Task DeleteImageAsync(string publicId);
    }
}
