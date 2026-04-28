using Microsoft.AspNetCore.Mvc;
using PawsPort.ViewModels;
using PawsPort.Dtos;
using PawsPort.Services;

namespace PawsPort.Controllers
{
    public class MissingReportsController : Controller
    {
        private readonly MissingReportsService _service;
        public MissingReportsController(MissingReportsService service)
        {
            _service = service;
        }

        public IActionResult List(KeywordViewModel vm)
        {
            var dtoList = _service.GetReports(vm.txtKeyword);
            return View(dtoList);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(MissingReportCreateDto dto)
        {
            _service.CreateReport(dto);
            return RedirectToAction("List");
        }

        public IActionResult Delete(int? id)
        {
            if (id.HasValue) _service.DeleteReport(id.Value);
            return RedirectToAction("List");
        }

        public IActionResult Edit(int? id)
        {
            if (!id.HasValue) return RedirectToAction("List");

            var dto = _service.GetReportForEdit(id.Value);
            if (dto == null) return RedirectToAction("List");

            return View(dto);
        }

        [HttpPost]
        public IActionResult Edit(MissingReportEditDto dto)
        {
            _service.UpdateReport(dto);
            return RedirectToAction("List");
        }
    }
}