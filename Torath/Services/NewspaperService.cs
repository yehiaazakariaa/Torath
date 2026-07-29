using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Torath.DTOs;
using Torath.Entities;


namespace Torath.Services
{
    public class NewspaperService : INewspaperService
    {
        private readonly TorathDbContext _context;

        public NewspaperService(TorathDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetAllAsync(int page, int pageSize)
        {
            // 1. Get the total count for pagination metadata
            var totalRecords = await _context.Newspapers.CountAsync();

            // 2. Fetch the paginated data
            var data = await _context.Newspapers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 3. Return the exact JSON structure your API expects
            return new
            {
                data = data,
                totalRecords = totalRecords,
                pageNumber = page,
                pageSize = pageSize
            };
        }

        public async Task<Newspaper> GetByIdAsync(int id)
        {
            return await _context.Newspapers.FindAsync(id);
        }

        public async Task<IEnumerable<NewspaperIssue>> GetIssuesByNewspaperIdAsync(int newspaperId)
        {
            return await _context.NewspaperIssues
                .Where(i => i.NewspaperId == newspaperId)
                .ToListAsync();
        }

        public async Task<Newspaper> CreateAsync(NewspaperWriteDto request)
        {
            var newspaper = new Newspaper
            {
                Title = request.Title,
                Publisher = request.Publisher,
                Frequency = request.Frequency,
                Price = request.Price,
                Language = request.Language,
                CategoryId = request.CategoryId, // Required to pass the EF Foreign Key constraint
                PdfFilePath = request.PdfFilePath
            };

            _context.Newspapers.Add(newspaper);
            await _context.SaveChangesAsync();

            return newspaper;
        }

        public async Task UpdateAsync(int id, NewspaperWriteDto request)
        {
            var newspaper = await _context.Newspapers.FindAsync(id);
            if (newspaper == null)
            {
                throw new Exception($"Newspaper with ID {id} not found.");
            }

            // Map the new values to the existing entity
            newspaper.Title = request.Title;
            newspaper.Publisher = request.Publisher;
            newspaper.Frequency = request.Frequency;
            newspaper.Price = request.Price;
            newspaper.Language = request.Language;
            newspaper.CategoryId = request.CategoryId;
            newspaper.PdfFilePath = request.PdfFilePath;

            _context.Newspapers.Update(newspaper);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var newspaper = await _context.Newspapers.FindAsync(id);
            if (newspaper != null)
            {
                _context.Newspapers.Remove(newspaper);
                await _context.SaveChangesAsync();
            }
        }
    }
}