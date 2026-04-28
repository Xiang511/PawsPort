using Microsoft.AspNetCore.Mvc;
using PawsPort.ViewModels; 
using PawsPort.DTOs;
using PawsPort.Services;

namespace PawsPort.Controllers
{
    public class AdoptionRecordController : Controller
    {
        private readonly AdoptionRecordService _service;
        public AdoptionRecordController(AdoptionRecordService service)
        {
            _service = service;
        }


        public IActionResult List(KeywordViewModel vm)
        {
            // 直接呼叫 Service 幫我們做所有的資料庫查詢
            var dtoList = _service.GetAdoptionRecords(vm.txtKeyword);

            // 把 DTO 傳給畫面 (在還沒轉成 API 之前，我們可以暫時把 DTO 當 ViewModel 傳給 View)
            return View(dtoList);
        }

        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(AdoptionRecordCreateDto dto) // 直接接收 DTO
        {
            // 交給 Service 處理新增
            _service.CreateRecord(dto);
            return RedirectToAction("List");
        }

        
        public IActionResult Delete(int? id)
        {
            if (id.HasValue)
            {
                _service.DeleteRecord(id.Value);
            }
            return RedirectToAction("List");
        }

        
        public IActionResult Edit(int? id)
        {
            if (id == null) return RedirectToAction("List");

            var dto = _service.GetRecordForEdit(id.Value);

            if (dto == null) return RedirectToAction("List");

            return View(dto);
        }

        [HttpPost]
        public IActionResult Edit(AdoptionRecordEditDto dto) // 直接接收 DTO
        {
            _service.UpdateRecord(dto);
            return RedirectToAction("List");
        }
    }
}