using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Torath.Entities;
using Torath.DTOs;

namespace Torath.Services
{
    public class NewspaperIssueService : INewspaperIssueService
    {
        private readonly TorathDbContext _context;

        // Constructor: Injects the database context so we can query SQL Server
        public NewspaperIssueService(TorathDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetAllAsync(int page, int pageSize)
        {
            // 1. Count total issues in the database (used for the pagination metadata)
            var totalRecords = await _context.NewspaperIssues.CountAsync();

            // 2. Fetch only the requested page of data
            var data = await _context.NewspaperIssues
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize) // Skips the records from previous pages
                .Take(pageSize)              // Takes only the amount specified by pageSize
                .ToListAsync();              // Executes the SQL query

            // 3. Construct and return the pagination JSON object
            return new
            {
                data = data,
                totalRecords = totalRecords,
                pageNumber = page,
                pageSize = pageSize
            };
        }

        public async Task<NewspaperIssue> GetByIdAsync(int id)
        {
            // Searches the NewspaperIssues table for a primary key matching 'id'
            return await _context.NewspaperIssues.FindAsync(id);
        }

        public async Task<IEnumerable<Article>> GetArticlesByIssueIdAsync(int issueId)
        {
            // Fulfills the nested endpoint requirement: Finds all articles where NewspaperIssueId matches
            return await _context.Articles
                .Where(a => a.NewspaperIssueId == issueId)
                .ToListAsync();
        }

        public async Task<NewspaperIssue> CreateAsync(NewspaperIssueWriteDto request)
        {
            // Create a new entity and map the values from the user's JSON DTO
            var issue = new NewspaperIssue
            {
                IssueNumber = request.IssueNumber,
                PublicationDate = request.PublicationDate,
                NewspaperId = request.NewspaperId // The Foreign Key link
            };

            // Stage the insert command and commit it to the SQL database
            _context.NewspaperIssues.Add(issue);
            await _context.SaveChangesAsync();

            return issue;
        }

        public async Task UpdateAsync(int id, NewspaperIssueWriteDto request)
        {
            // Find the issue first to ensure it exists
            var issue = await _context.NewspaperIssues.FindAsync(id);
            if (issue == null)
            {
                throw new Exception($"Newspaper Issue with ID {id} not found."); // Triggers your global exception middleware
            }

            // Update the entity with the new values
            issue.IssueNumber = request.IssueNumber;
            issue.PublicationDate = request.PublicationDate;
            issue.NewspaperId = request.NewspaperId;

            // Stage the update command and commit it to SQL Server
            _context.NewspaperIssues.Update(issue);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // Find the issue to delete
            var issue = await _context.NewspaperIssues.FindAsync(id);
            if (issue != null)
            {
                // Stage the delete command and commit it
                _context.NewspaperIssues.Remove(issue);
                await _context.SaveChangesAsync();
            }
        }
    }
}