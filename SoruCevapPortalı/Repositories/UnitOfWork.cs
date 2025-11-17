using SoruCevapPortalı.Data;
using SoruCevapPortalı.Interfaces;
using SoruCevapPortalı.Models;
using SoruCevapPortalı.Repositories;

namespace SoruCevapPortalı.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Categories = new Repository<Category>(_context);
            Questions = new Repository<Question>(_context);
            Answers = new Repository<Answer>(_context);
            QuestionVotes = new Repository<QuestionVote>(_context);
            AnswerVotes = new Repository<AnswerVote>(_context);
        }

        public IRepository<Category> Categories { get; private set; }
        public IRepository<Question> Questions { get; private set; }
        public IRepository<Answer> Answers { get; private set; }
        public IRepository<QuestionVote> QuestionVotes { get; private set; }
        public IRepository<AnswerVote> AnswerVotes { get; private set; }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}