using Microsoft.AspNetCore.Mvc;
using PawsPort.ViewModels; // 給 KeywordViewModel 用
using PawsPort.Dtos;
using PawsPort.Services;

namespace PawsPort.Controllers
{
    public class PassPortController : Controller
    {
        // 1. 定義私有唯讀欄位
        private readonly PassPortService _service;

        // 2. 透過建構子注入：跟系統說「我需要 PassPortService」
        
        public PassPortController(PassPortService service)
        {
            _service = service;
        }

        // --- List ---
        public IActionResult List(KeywordViewModel vm)
        {
            var dtoList = _service.GetPassports(vm.txtKeyword);
            return View(dtoList);
        }

        // --- Create ---
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(HealthPassportCreateDto dto)
        {
            _service.CreatePassport(dto);
            TempData["SuccessMessage"] = "新增資料成功！";
            return RedirectToAction("List");
        }

        // --- Delete ---
        public IActionResult Delete(int? id)
        {
            if (id.HasValue)
            {
                _service.DeletePassport(id.Value);
            }
            return RedirectToAction("List");
        }

        // --- Edit ---
        public IActionResult Edit(int? id)
        {
            if (id == null) return RedirectToAction("List");

            var dto = _service.GetPassportForEdit(id.Value);
            if (dto == null) return RedirectToAction("List");

            return View(dto);
        }

        [HttpPost]
        public IActionResult Edit(HealthPassportEditDto dto)
        {
            _service.UpdatePassport(dto);
            return RedirectToAction("List");
        }

        // --- Details ---
        public IActionResult Details(int? id)
        {
            if (id == null) return RedirectToAction("List");

            var dto = _service.GetPassportDetails(id.Value);

            // 防呆機制：Service 回傳 null 代表真的都沒資料
            if (dto == null)
            {
                TempData["ErrorMessage"] = "該名寵物目前沒有任何病歷與疫苗資料喔！";
                return RedirectToAction("List");
            }

            return View(dto);
        }
    }
}