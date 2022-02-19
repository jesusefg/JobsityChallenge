using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WebApplication.Data.Interfaces;

namespace WebApplication.Data
{
    public class SQLRepository<T> : ISQLRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;

        public SQLRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<T> GetAll() 
        {
            return _context.Set<T>();
        }

        public void Insert(T newEntity)
        {
            _context.Set<T>().Add(newEntity);

            _context.SaveChanges();
        }
    }
}
