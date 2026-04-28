using Microsoft.AspNetCore.Mvc;
using PawsPort.ViewModels;
using PawsPort.Dtos;
using PawsPort.Services;

namespace PawsPort.Controllers
{
    public class PetController : Controller
    {
        private readonly PetService _service;
        public PetController(PetService service)
        {
            _service = service;
        }

        public IActionResult List(KeywordViewModel vm)
        {
            var dtoList = _service.GetPets(vm.txtKeyword);
            return View(dtoList);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(PetCreateDto dto)
        {
            _service.CreatePet(dto);
            return RedirectToAction("List");
        }

        public IActionResult Delete(int? id)
        {
            if (id.HasValue) _service.SoftDeletePet(id.Value);
            return RedirectToAction("List");
        }

        public IActionResult Edit(int? id)
        {
            if (!id.HasValue) return RedirectToAction("List");

            var dto = _service.GetPetForEdit(id.Value);
            if (dto == null) return RedirectToAction("List");

            return View(dto);
        }

        [HttpPost]
        public IActionResult Edit(PetEditDto dto)
        {
            _service.UpdatePet(dto);
            return RedirectToAction("List");
        }
    }
}