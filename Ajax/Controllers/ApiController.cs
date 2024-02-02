using Ajax.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Scaffolding;
using MSIT155Site.Models.DTO;
using System.Text;

namespace Ajax.Controllers
{
    public class ApiController : Controller
    {
        MyDBContext _context = new MyDBContext();
        private readonly IWebHostEnvironment _environment;
        public ApiController(MyDBContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return Content("<h1>Hello,World!</h1>","text/html",Encoding.UTF8);
        }
        public IActionResult Cities()
        {
            var cities = _context.Addresses.Select(p => p.City).Distinct();
            return Json(cities);
        }
        public IActionResult Sites(string city)
        {
            var sites = _context.Addresses.Where(p=>p.City==city).Select(p => p.SiteId).Distinct();
            return Json(sites);
        }
        public IActionResult Roads(string site)
        {
            var roads = _context.Addresses.Where(p => p.SiteId == site).Select(p => p.Road).Distinct();
            return Json(roads);
        }
        public IActionResult Avatar(int id = 1)
        {
            Member? member = _context.Members.Find(id);
            if (member != null)
            {
                byte[] img = member.FileData;
                if (img != null)
                {
                    return File(img, "image/jpeg");
                }
            }
            return NotFound();
        }
        public IActionResult Fetch() 
        {
            return View();
        }
        public IActionResult Register(UserDTO _user)
        {
            if (string.IsNullOrEmpty(_user.Name))
            {
                _user.Name = "guest";
            }
            string fileName = "empty.jpg";
            if (_user.Avatar != null)
            {
                fileName = _user.Avatar.FileName;
            }
            string uploadPath = Path.Combine(_environment.WebRootPath, "uploads", fileName);

            //using (var fileStream = new FileStream(uploadPath, FileMode.Create))
            //{
            //    _user.Avatar?.CopyTo(fileStream);
            //}
            //return Content($"Hello {name}. {age}歲. email:{email}","text/explain",Encoding.UTF8);
            return Content(uploadPath);
        }
        [HttpPost]
        [HttpPost]
        public IActionResult Spots([FromBody] SearchDTO _search)
        {
            //return Json(_search);
            //根據分類編號搜尋
            var spots = _search.CategoryId == 0 ? _context.SpotImagesSpots : _context.SpotImagesSpots.Where(s => s.CategoryId == _search.CategoryId);

            //根據關鍵字搜尋
            if (!string.IsNullOrEmpty(_search.Keyword))
            {
                spots = spots.Where(s => s.SpotTitle.Contains(_search.Keyword) || s.SpotDescription.Contains(_search.Keyword));
            }

            //排序
            switch (_search.SortBy)
            {
                case "spotTitle":
                    spots = _search.SortType == "asc" ? spots.OrderBy(s => s.SpotTitle) : spots.OrderByDescending(s => s.SpotTitle);
                    break;
                case "categoryId":
                    spots = _search.SortType == "asc" ? spots.OrderBy(s => s.CategoryId) : spots.OrderByDescending(s => s.CategoryId);
                    break;
                default: //spotId
                    spots = _search.SortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending(s => s.SpotId);
                    break;
            }

            //總共有幾筆
            int totalCount = spots.Count();
            //一頁幾筆資料
            int pageSize = _search.PageSize ?? 9;
            //計算總共有幾頁
            int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);
            //目前第幾頁
            int page = _search.Page ?? 1;


            //分頁
            spots = spots.Skip((page - 1) * pageSize).Take(pageSize);

            SpotsPagingDTO spotsPaging = new SpotsPagingDTO();
            spotsPaging.TotalPages = totalPages;
            spotsPaging.SpotsResult = spots.ToList();


            return Json(spotsPaging);
            //return Json(spots);
        }

        public IActionResult AutoComplete(string keyword)
        {
            var titles = _context.Spots.Where(s=>s.SpotTitle.Contains(keyword))
                .Select(s => s.SpotTitle).Take(8);
            return Json(titles);
        }

        public IActionResult CheckAccount(string name)
        {
            if (_context.Members.Count(n=>n.Name ==name) !=0)
            {
                return Content("名稱已存在");
            }
            else return Content("OK");
        }
        public IActionResult imgId()
        {
            var memID = _context.Members.Select(p => p.MemberId);
            return Json(memID);
        }
    }
}
