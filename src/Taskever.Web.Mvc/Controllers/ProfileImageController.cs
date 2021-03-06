using System;
using System.IO;
using System.Web.Mvc;
using Abp.IO;
using Abp.UI;
using Abp.Users;
using Abp.Users.Dto;
using Abp.Web.Models;
using Abp.Web.Mvc.Authorization;
using Taskever.Users;

namespace Taskever.Web.Mvc.Controllers
{
    [AbpAuthorize]
    public class ProfileImageController : TaskeverController
    {
        private readonly ITaskeverUserAppService _userAppService;

        public ProfileImageController(ITaskeverUserAppService userAppService)
        {
            _userAppService = userAppService;
        }

        [HttpPost]
        public JsonResult UploadProfileImage()
        {
            if (Request.Files.Count > 0)
            {
                var uploadfile = Request.Files[0];
                if (uploadfile != null)
                {
                    var extension = Path.GetExtension(uploadfile.FileName).ToLower();
                    
                    if (!(extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp"))
                    {
                        throw new UserFriendlyException("Unsupported profile image type");
                    }

                    if (uploadfile.ContentLength > 101400)
                    {
                        throw new UserFriendlyException("Unsupported file size");
                    }

                    //Save uploaded file
                    var tempPath = GenerateProfileImagePath(Path.GetExtension(uploadfile.FileName));
                    FileHelper.DeleteIfExists(tempPath);
                    uploadfile.SaveAs(tempPath);

                    //Change profile picture
                    var fileName = Path.GetFileName(tempPath);
                    var result = _userAppService.ChangeProfileImage(new ChangeProfileImageInput { FileName = fileName });
                    
                    //Delete old file
                    if(!string.IsNullOrWhiteSpace(result.OldFileName))
                    {
                        var oldFilePath = Path.Combine(Server.MapPath("~/ProfileImages"), result.OldFileName);
                        FileHelper.DeleteIfExists(oldFilePath);
                    }

                    //Return response
                    return Json(new AjaxResponse(new
                    {
                        imageUrl = "/ProfileImages/" + fileName
                    }));
                }
            }

            //No file
            return Json(new AjaxResponse(false)); //TODO: Error message?
        }

        private string GenerateProfileImagePath(string fileExtension)
        {
            var userId = Abp.Security.Users.AbpUser.CurrentUserId;
            return Path.Combine(Server.MapPath("~/ProfileImages"), userId + "_" + DateTime.Now.Ticks + fileExtension);
        }
    }
}