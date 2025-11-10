using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MiniLibraryMgmtSys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext db;
        public BooksController()
        {
            db = new AppDbContext();
        }
        
        private IQueryable<TblBook> BookQuery =>
            db.TblBooks.Where(book => book.DeleteFlag == false);



        [HttpGet]
        public IActionResult GetBooks()
        {
            var result = BookQuery.ToList();

            var lst = result.Select(book => new
            {
                book.Id,
                book.Author,
                book.Title,
                book.Genre,
                book.CreatedAt,
                book.UpdatedAt,
                book.CreatedBy,
                book.UpdatedBy
            }).ToList();

            return Ok(lst);
        }
    }
}
