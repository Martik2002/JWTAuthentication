using JWTAuthentication.Entities;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthentication.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}