using SoruCevapPortalı.Models;

namespace SoruCevapPortalı.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Category> Categories { get; }
        IRepository<Question> Questions { get; }
        IRepository<Answer> Answers { get; }
        IRepository<QuestionVote> QuestionVotes { get; }
        IRepository<AnswerVote> AnswerVotes { get; }

        Task<int> CompleteAsync();
    }
}