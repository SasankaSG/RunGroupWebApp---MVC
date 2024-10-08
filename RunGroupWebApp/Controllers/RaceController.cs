using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunGroupWebApp.Data;
using RunGroupWebApp.Interfaces;
using RunGroupWebApp.Models;
using RunGroupWebApp.Repository;
using RunGroupWebApp.Services;
using RunGroupWebApp.ViewModels;

namespace RunGroupWebApp.Controllers
{
    public class RaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRaceRepository _raceRepository;
		private readonly IPhotoService _photoService;

		public RaceController(ApplicationDbContext context, IRaceRepository raceRepository, IPhotoService photoService)
        {
            _context = context;
            _raceRepository = raceRepository;
			_photoService = photoService;
		}
        public async Task<IActionResult> Index()
        {
            IEnumerable<Race> Races = await _raceRepository.GetAll();
            return View(Races);
        }
        public async Task<IActionResult> Detail(int id)
        {
            Race race = await _raceRepository.GetByIdAsync(id);
            return View(race);
        }


        public IActionResult Create()
        {
            return View();
        }

		//[HttpPost] //before photoos ervce & viewmodel
		//public async Task<IActionResult> Create(Race race)
		//{
		//	if (!ModelState.IsValid)
		//	{
		//		return View(race);
		//	}
		//	_raceRepository.Add(race);
		//	return RedirectToAction("Index");
		//}
		[HttpPost]
		public async Task<IActionResult> Create(CreateRaceViewModel raceVM)
		{
			if (ModelState.IsValid)
			{
				var result = await _photoService.AddPhotoAsync(raceVM.Image);
				var race = new Race
				{
					Title = raceVM.Title,
					Description = raceVM.Description,
					Image = result.Url.ToString(),
					Address = new Address
					{
						Street = raceVM.Address.Street,
						City = raceVM.Address.City,
						PostalCode = raceVM.Address.PostalCode,
						State = raceVM.Address.State,
						Country = raceVM.Address.Country,
					}
				};
				_raceRepository.Add(race);
				return RedirectToAction("Index");
			}
			else
			{
				ModelState.AddModelError("", "Photo Upload Failed");
			}
			return View(raceVM);
		}


        public async Task<IActionResult> Edit(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            if (race == null) return View("Error");

            var raceVM = new EditRaceViewModel
            {
                Title = race.Title,
                Description = race.Description,
                AddressId = race.AddressId,
                Address = race.Address,
                URL = race.Image,
                RaceCategory = race.RaceCategory,
            };
            return View(raceVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditRaceViewModel raceVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit club");
                return View("Edit", raceVM);
            }

            var userClub = await _raceRepository.GetByIdAsyncNoTracking(id);

            if (userClub != null)
            {
                try
                {
                    await _photoService.DeletePhotoAsync(userClub.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Failed to delete Image");
                    return View(raceVM);
                }
                var photoResult = await _photoService.AddPhotoAsync(raceVM.Image);
                var race = new Race
                {
                    Id = id,
                    Title = raceVM.Title,
                    Description = raceVM.Description,
                    Image = photoResult.Url.ToString(),
                    AddressId = raceVM.AddressId,
                    Address = raceVM.Address,
                    RaceCategory = raceVM.RaceCategory,
                };

                _raceRepository.Update(race);

                return RedirectToAction("Index");
            }
            else
            {
                return View(raceVM);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var raceDetails = await _raceRepository.GetByIdAsync(id);
            if (raceDetails == null) return View("Error");
            return View(raceDetails);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteClub(int id)
        {
            var raceDetails = await _raceRepository.GetByIdAsync(id);
            if (raceDetails == null) return View("Error");

            _raceRepository.Delete(raceDetails);
            return RedirectToAction("Index");

        }

    }
}


//public class RaceController : Controller
//{
//    private readonly ApplicationDbContext _context;

//    public RaceController(ApplicationDbContext context)
//    {
//        _context = context;
//    }
//    public IActionResult Index()
//    {
//        List<Race> Races = _context.Races.ToList();
//        return View(Races);
//    }
//    public IActionResult Detail(int id)
//    {
//        Race race = _context.Races.Include(a => a.Address).FirstOrDefault(c => c.Id == id);
//        return View(race);
//    }
