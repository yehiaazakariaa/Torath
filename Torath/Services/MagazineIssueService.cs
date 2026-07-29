using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Torath.Entities;
using Torath.DTOs;

namespace Torath.Services
{
    public class MagazineIssueService : IMagazineIssueService
    {
        private readonly TorathDbContext _context;

        // Inject the database context via Dependency Injection
        public MagazineIssueService(TorathDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetAllAsync(int page, int pageSize)
        {
            // Calculate total records for pagination metadata
            var totalRecords = await _context.MagazineIssues.CountAsync();

            // Fetch the paginated issues using Skip and Take
            var data = await _context.MagazineIssues
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return the exact JSON structure required for pagination
            return new
            {
                data = data,
                totalRecords = totalRecords,
                pageNumber = page,
                pageSize = pageSize
            };
        }

        public async Task<MagazineIssue> GetByIdAsync(int id)
        {
            // Finds the specific issue by its primary key
            return await _context.MagazineIssues.FindAsync(id);
        }

        public async Task<IEnumerable<Article>> GetArticlesByIssueIdAsync(int issueId)
        {
            // Fulfills the nested endpoint requirement to get all articles for one issue
            return await _context.Articles
                .Where(a => a.MagazineIssueId == issueId)
                .ToListAsync();
        }

        public async Task<MagazineIssue> CreateAsync(MagazineIssueWriteDto request)
        {
            // Map the DTO properties to a new database entity
            var issue = new MagazineIssue
            {
                IssueNumber = request.IssueNumber,
                VolumeNumber = request.VolumeNumber,
                PublicationDate = request.PublicationDate,
                MagazineId = request.MagazineId // Links to the parent Magazine
            };

            // Add and save to the database
            _context.MagazineIssues.Add(issue);
            await _context.SaveChangesAsync();

            return issue;
        }

        public async Task UpdateAsync(int id, MagazineIssueWriteDto request)
        {
            // Locate the existing issue
            var issue = await _context.MagazineIssues.FindAsync(id);
            if (issue == null)
            {
                throw new Exception($"Magazine Issue with ID {id} not found.");
            }

            // Apply the updated values
            issue.IssueNumber = request.IssueNumber;
            issue.VolumeNumber = request.VolumeNumber;
            issue.PublicationDate = request.PublicationDate;
            issue.MagazineId = request.MagazineId;

            _context.MagazineIssues.Update(issue);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // Locate and remove the issue
            var issue = await _context.MagazineIssues.FindAsync(id);
            if (issue != null)
            {
                _context.MagazineIssues.Remove(issue);
                await _context.SaveChangesAsync();
            }
        }
    }
}