using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Torath.Entities;
using Torath.DTOs;
using Torath.Repositories;

namespace Torath.Services
{
    public class NewspaperService : INewspaperService
    {
        private readonly IRepository<Newspaper> _newspaperRepository;
        private readonly IRepository<NewspaperIssue> _issueRepository;

        public NewspaperService(IRepository<Newspaper> newspaperRepository, IRepository<NewspaperIssue> issueRepository)
        {
            _newspaperRepository = newspaperRepository;
            _issueRepository = issueRepository;
        }

        public async Task<object> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = _newspaperRepository.GetQueryable();
            var totalRecords = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<Newspaper?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _newspaperRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<NewspaperIssue>> GetIssuesByNewspaperIdAsync(int newspaperId, CancellationToken cancellationToken)
        {
            return await _issueRepository.GetQueryable()
                .Where(i => i.NewspaperId == newspaperId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Newspaper> CreateAsync(NewspaperWriteDto request, CancellationToken cancellationToken)
        {
            var newspaper = new Newspaper
            {
                Title = request.Title,
                Description = request.Description,
                Language = request.Language,
                PublicationDate = request.PublicationDate,
                Publisher = request.Publisher,
                CategoryId = request.CategoryId,
             
                Frequency = request.Frequency,
                Price = request.Price,
                PdfFilePath = request.PdfFilePath
            };

            await _newspaperRepository.AddAsync(newspaper, cancellationToken);
            await _newspaperRepository.SaveChangesAsync(cancellationToken);
            return newspaper;
        }

        public async Task UpdateAsync(int id, NewspaperWriteDto request, CancellationToken cancellationToken)
        {
            var newspaper = await _newspaperRepository.GetByIdAsync(id, cancellationToken);
            if (newspaper == null) throw new Exception($"Newspaper with ID {id} not found.");

            newspaper.Title = request.Title;
            newspaper.Description = request.Description;
            newspaper.Language = request.Language;
            newspaper.PublicationDate = request.PublicationDate;
            newspaper.Publisher = request.Publisher;
            newspaper.CategoryId = request.CategoryId;
        
            newspaper.Frequency = request.Frequency;
            newspaper.Price = request.Price;
            newspaper.PdfFilePath = request.PdfFilePath;

            _newspaperRepository.Update(newspaper);
            await _newspaperRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var newspaper = await _newspaperRepository.GetByIdAsync(id, cancellationToken);
            if (newspaper != null)
            {
                _newspaperRepository.Delete(newspaper);
                await _newspaperRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}