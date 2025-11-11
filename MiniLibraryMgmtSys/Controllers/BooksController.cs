using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTO;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static MiniLibraryMgmtSys.DTO.BookDTO;

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
        
        //Book Table Query
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
                book.Genre
            }).ToList();

            return Ok(lst);
        }

        [HttpGet("{Id}")]
        public IActionResult GetBooksById(string Id)
        {
            var book = BookQuery.FirstOrDefault(book => book.Id == Id);
            if (book is null)
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }
            var result = new BookDto
            {
                Id = book.Id,
                Author = book.Author,
                Title = book.Title,
                Genre = book.Genre,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt,
                CreatedBy = book.CreatedBy,
                UpdatedBy = book.UpdatedBy
            };

            return Ok(new BookResponseDto
            {
                IsSuccess = true,
                Message = "Book retrieved successfully.",
                Data = result
            });
        }

        [HttpPost]
        public IActionResult CreateBook([FromBody] BookDto request)
        {
            db.TblBooks.Add(new TblBook
            {
                Id = Guid.NewGuid().ToString(),
                Author = request.Author,
                Title = request.Title,
                Genre = request.Genre,
                DeleteFlag = false
            });

            var result = db.SaveChanges();

            string message = result > 0 ? "Book created successfully." : "Failed to create book.";

            BookResponseDto response = new BookResponseDto
            {
                IsSuccess = result > 0,
                Message = message,
            };

            return Ok(response);
        }

        [HttpPatch("id/{Id}")]
        public IActionResult UpdateBook(string Id, [FromBody] BookDto request)
        {
            var book = BookQuery.FirstOrDefault(book => book.Id == Id);

            if (book is null)
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }


            if (!string.IsNullOrEmpty(request.Author))
            {
                book.Author = request.Author;
            }

            if (!string.IsNullOrEmpty(request.Title))
            {
                book.Title = request.Title;
            }

            if (!string.IsNullOrEmpty(request.Genre))
            {
                book.Genre = request.Genre;
            }

            return Ok(new BookResponseDto
            {
                IsSuccess = db.SaveChanges() > 0,
                Message = "Book updated successfully!"
            });

        }

        [HttpDelete("id/{Id}")]
        public IActionResult DeleteBook(string Id) {
            var book = BookQuery.FirstOrDefault(book => book.Id == Id);
            
            if (book is null)
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            book.DeleteFlag = true;

            return Ok(new BookResponseDto
            {
                IsSuccess = db.SaveChanges() > 0,
                Message = "Book deleted successfully!"
            });
        }







    }
}
