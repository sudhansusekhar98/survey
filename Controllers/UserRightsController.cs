using AnalyticaDocs.Models;
using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using System.Diagnostics;

namespace AnalyticaDocs.Controllers
{
    public class UserRightsController : Controller
    {
        private readonly IAdmin _repository;
        private readonly ICommonUtil _util;

        public UserRightsController(IAdmin repository, ICommonUtil util)
        {
            _repository = repository;
            _util = util;
        }
        public IActionResult Index()
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "View");
            if (result != null) return result;

            ViewBag.UserOptions =  _util.GetUserOptions();
            
            return View("UserRightsView", new UsersRightsFormModel());
        }

        //public IActionResult BindRecord(int? recordId)
        //{
        //    var result = _util.CheckAuthorization(this, "101");
        //    if (result != null) return result;

        //    if (!recordId.HasValue)
        //        return RedirectToAction("Index");
        //    UsersRightsModel urights = new UsersRightsModel();
        //    urights.UserID = recordId.Value;
        //    ViewBag.UserOptions = _util.GetUserOptions();
        //    ViewBag.UserRights = _repository.GetUserRights(recordId.Value);
        //    return View("UserRightsView", urights);
        //}

        public IActionResult BindRecord(int? recordId)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "View");
            if (result != null) return result;

            if (!recordId.HasValue || recordId.Value==0)
                return RedirectToAction("Index");

            var user = _repository.GetUserById(recordId.Value);
            if (user == null)
            {
                return RedirectToAction("Index");
            }
            // Prepare the form model
            var formModel = new UsersRightsFormModel
            {
                UserID = user.UserId,
                RoleID = user.RoleId,
                UserName = user.LoginName,

                RightsList = _repository.GetUserRights(recordId.Value) ?? new List<UsersRightsModel>()
            };
            
            ViewBag.UserOptions = _util.GetUserOptions();
            return View("UserRightsView", formModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateRights(UsersRightsFormModel model)
        {
            
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Update");
            if (result != null) return Json("unauthorized");

            if (!ModelState.IsValid)
            {
                return Json("invalid");
            }

            model.CreateBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            bool isSaved = _repository.UpdateRights(model);

            TempData["ResultMessage"] = "User rights updated successfully.";
            TempData["ResultType"] = "success";
            return RedirectToAction("Index");
        }


        //public IActionResult BindRecord(int? recordId)
        //{
        //    var result = _util.CheckAuthorization(this, "101");
        //    if (result != null) return result;

        //    if (!recordId.HasValue)
        //        return RedirectToAction("Index");


        //    ViewBag.UserRights = _repository.GetUserRights(recordId.Value);
        //    return PartialView("_UserRightsPartialView");

        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
