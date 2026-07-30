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
    public class MagazineService : IMagazineService
    {
        private readonly IRepository<Magazine> _magazineRepository;
        private readonly IRepository<MagazineIssue> _issueRepository;

        public MagazineService(IRepository<Magazine> magazineRepository, IRepository<MagazineIssue> issueRepository)
        {
            _magazineRepository = magazineRepository;
            _issueRepository = issueRepository;
        }

        public async Task<object> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = _magazineRepository.GetQueryable();
            var totalRecords = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<Magazine?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _magazineRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<MagazineIssue>> GetIssuesByMagazineIdAsync(int magazineId, CancellationToken cancellationToken)
        {
            return await _issueRepository.GetQueryable()
                .Where(i => i.MagazineId == magazineId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Magazine> CreateAsync(MagazineWriteDto request, CancellationToken cancellationToken)
        {
            var magazine = new Magazine
            {
                Title = request.Title,
                Description = request.Description,
                Language = request.Language,
                PublicationDate = request.PublicationDate,
                Publisher = request.Publisher,
                CategoryId = request.CategoryId,
                ISSN = request.ISSN,
              
            };

            await _magazineRepository.AddAsync(magazine, cancellationToken);
            await _magazineRepository.SaveChangesAsync(cancellationToken);
            return magazine;
        }

        public async Task UpdateAsync(int id, MagazineWriteDto request, CancellationToken cancellationToken)
        {
            var magazine = await _magazineRepository.GetByIdAsync(id, cancellationToken);
            if (magazine == null) throw new Exception($"Magazine with ID {id} not found.");

            magazine.Title = request.Title;
            magazine.Description = request.Description;
            magazine.Language = request.Language;
            magazine.PublicationDate = request.PublicationDate;
            magazine.Publisher = request.Publisher;
            magazine.CategoryId = request.CategoryId;
            magazine.ISSN = request.ISSN;
          

            _magazineRepository.Update(magazine);
            await _magazineRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var magazine = await _magazineRepository.GetByIdAsync(id, cancellationToken);
            if (magazine != null)
            {
                _magazineRepository.Delete(magazine);
                await _magazineRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}