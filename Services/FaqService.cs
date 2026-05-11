using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;


namespace PawsPort.Services
{
    public class FaqService
    {

        private readonly PetDbContext _db;

        public FaqService(PetDbContext db)
        {
            _db = db;
        }

        //List
        public async Task<List<FaqDTO>> GetAllFaqsAsync()
        {
            return await _db.Faqs
                .Where(f => f.IsExist == true)
                .Select(f => new FaqDTO
                {
                    Faqid = f.Faqid,
                    Question = f.Question,
                    Answer = f.Answer
                })
                .ToListAsync();
        }


        public async Task<FaqDTO> CreateFaqAsync(FaqCreateDTO dto)
        {
            var newFaq = new Faq
            {
                QuestionType = dto.QuestionType,
                Question = dto.Question,
                Answer = dto.Answer,
                CreateAt = DateTime.Now,
                IsExist = true,
                Status = "待發佈",

            };

            if (string.IsNullOrEmpty(dto.Note))
            {
                newFaq.Note = "無";
            }
            else
            {
                newFaq.Note = dto.Note;
            }

            _db.Faqs.Add(newFaq);
            await _db.SaveChangesAsync();

            return new FaqDTO
            {
                Faqid = newFaq.Faqid,
                Question = newFaq.Question,
                Answer = newFaq.Answer
            };
        }


        public async Task<bool> UpdateFaqAsync(int id, FaqUpdateDTO dto)
        {
            var faqData = await _db.Faqs.FindAsync(id);

            if (faqData == null || faqData.IsExist == false)
            {
                return false;
            }

            faqData.QuestionType = dto.QuestionType;
            faqData.Question = dto.Question;
            faqData.Answer = dto.Answer;
            faqData.Status = dto.Status;

            if (string.IsNullOrEmpty(dto.Note))
            {
                faqData.Note = "無";
            }
            else
            {
                faqData.Note = dto.Note;
            }

            _db.Update(faqData);

            await _db.SaveChangesAsync();

            return true;
        }


        public async Task<bool> SoftDeleteFaqAsync(int id)
        {
            var item = await _db.Faqs.FindAsync(id);

            if (item == null || item.IsExist == false)
            {
                return false;
            }

            item.IsExist = false;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
