using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunGroupWebApp.Data;
using RunGroupWebApp.Interfaces;
using RunGroupWebApp.Models;
using RunGroupWebApp.ViewModels;

namespace RunGroupWebApp.Controllers
{
    public class ClubController : Controller
    {
        //inject
        private readonly ApplicationDbContext _context;
        private readonly IClubRepository _clubRepository;
		private readonly IPhotoService _photoService;

		//public ClubController(ApplicationDbContext context)
		public ClubController(ApplicationDbContext context, IClubRepository clubRepository, IPhotoService photoService)
        {
            _context = context;
            _clubRepository = clubRepository;
			_photoService = photoService;
		}
        public async Task<IActionResult> Index()
        {
            IEnumerable<Club> clubs = await _clubRepository.GetAll(); 
            //to excute the SQL query
            return View(clubs);
        }
        public async Task<IActionResult> Detail(int id)
        {
            Club club = await _clubRepository.GetByIdAsync(id);
            return View(club);
        }

        public IActionResult Create()
        {
            return View();
        }

        //[HttpPost]  //before adding images & cloudinary
        //public async Task<IActionResult> Create(Club club)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(club);
        //    }
        //    _clubRepository.Add(club);
        //    return RedirectToAction("Index");
        //}
        [HttpPost]
        public async Task<IActionResult> Create(CreateClubViewModel clubVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _photoService.AddPhotoAsync(clubVM.Image);
                var club = new Club
                {
                    Title = clubVM.Title,
                    Description = clubVM.Description,
                    Image = result.Url.ToString(),
                    Address = new Address
                    {
                        Street = clubVM.Address.Street,
                        City = clubVM.Address.City,
                        PostalCode = clubVM.Address.PostalCode,
                        State = clubVM.Address.State,
                        Country = clubVM.Address.Country,
                    }
                };
				_clubRepository.Add(club);
				return RedirectToAction("Index");
			}
            else
            {
                ModelState.AddModelError("", "Photo Upload Failed");
            }
            return View(clubVM);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            if (club == null) return View("Error");

            var clubVM = new EditClubViewModel
            {
                Title = club.Title,
                Description = club.Description,
                AddressId = club.AddressId,
                Address = club.Address,
                URL = club.Image,
                ClubCategory = club.ClubCategory,
            };
            return View(clubVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditClubViewModel clubVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit club");
                return View("Edit", clubVM);
            }

            var userClub = await _clubRepository.GetByIdAsyncNoTracking(id);

            if (userClub != null)
            {
                try
                {
                    await _photoService.DeletePhotoAsync(userClub.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Failed to delete Image");
                    return View(clubVM);
                }
                var photoResult = await _photoService.AddPhotoAsync(clubVM.Image);
                var club = new Club
                {
                    Id = id,
                    Title = clubVM.Title,
                    Description = clubVM.Description,
                    Image = photoResult.Url.ToString(),
                    AddressId = clubVM.AddressId,
                    Address = clubVM.Address,
                    ClubCategory = clubVM.ClubCategory,
                };

            _clubRepository.Update(club);

            return RedirectToAction("Index");
            }
            else
            {
                return View(clubVM);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var clubDetails = await _clubRepository.GetByIdAsync(id); 
            if (clubDetails == null) return View("Error");
            return View(clubDetails);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteClub(int id)
        {
            var clubDetails = await _clubRepository.GetByIdAsync(id);
            if (clubDetails == null) return View("Error");

            _clubRepository.Delete(clubDetails);
            return RedirectToAction("Index");

        }
    }
}




























//============================================================
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using RunGroupWebApp.Data;
//using RunGroupWebApp.Interfaces;
//using RunGroupWebApp.Models;

//namespace RunGroupWebApp.Controllers
//{
//    public class ClubController : Controller
//    {
//        //inject
//        private readonly ApplicationDbContext _context;
//        private readonly IClubRepository _clubRepository;

//        //public ClubController(ApplicationDbContext context)
//        public ClubController(ApplicationDbContext context, IClubRepository clubRepository)
//        {
//            _context = context;
//            _clubRepository = clubRepository;
//        }
//        public IActionResult Index()
//        {
//            List<Club> clubs = _context.Clubs.ToList(); //to excute the SQL query
//            return View(clubs);
//        }
//        public IActionResult Detail(int id)
//        {
//            Club club = _context.Clubs.Include(a => a.Address).FirstOrDefault(c => c.Id == id);
//            return View(club);
//        }
//    }
//}

