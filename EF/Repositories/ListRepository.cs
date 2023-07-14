using Domain.DTO;
using Infrastructure.Exceptions;
using Infrastructure.IRepositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Task = System.Threading.Tasks.Task;

namespace EF.Repositories
{
    public sealed class ListRepository : IListRepository
    {
        public readonly ProjectContext _context;
        public readonly DbSet<List> _dbSet;
        private readonly Guid _userId;

        public ListRepository(ProjectContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<List>();
            var email = httpContextAccessor.HttpContext.User.Claims.First(x => x.Type == ClaimTypes.Email).Value;
            _userId = context.Users.First(x => x.Email == email).Id;
        }

        public async Task<Guid> CreateAsync(List entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<IQueryable<List>> GetAllAsync()
        {
            var models = await Task.FromResult(_dbSet.AsNoTracking().Include(x=>x.Tasks).Where(x => x.UserId == _userId));

            return models;
        }

        public async Task<IQueryable<List>> GetAsync(Expression<Func<List, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<List> GetByIdAsync(Guid id)
        {
            var existEntity = await _dbSet.FirstOrDefaultAsync(x => x.UserId == _userId && x.Id == id);

            if (existEntity == null)
            {
                throw new NotFoundException($"'{typeof(List)}' with id '{id}' not found.");
            }

            return existEntity;
        }

        public async Task RemoveAsync(Guid id)
        {
            var existEntity = await _dbSet.FirstOrDefaultAsync(x => x.UserId == _userId && x.Id == id);

            if (existEntity == null)
            {
                return;
            }

            _dbSet.Remove(existEntity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(List entity)
        {
            var existEntity = await _dbSet.FirstOrDefaultAsync(x => x.UserId == _userId && x.Id == entity.Id);

            if (existEntity == null)
            {
                throw new NotFoundException($"'{typeof(Domain.DTO.Task)}' with id '{entity.Id}' not found.");
            }
            _context.Entry(existEntity).State = EntityState.Detached;

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
