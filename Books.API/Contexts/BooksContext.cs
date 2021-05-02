using Books.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Books.API.Contexts
{
    public class BooksContext : DbContext
    {
        public BooksContext(DbContextOptions<BooksContext> options)
            : base(options)
        {

        }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Data seed for Authors
            modelBuilder.Entity<Author>().HasData(
                new Author()
                {
                    Id = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                    FirstName = "George",
                    LastName = "RR Martin"
                },
                new Author()
                {
                    Id = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96"),
                    FirstName = "Stephen",
                    LastName = "Fry"
                },
                new Author()
                {
                    Id = Guid.Parse("24810dfc-2d94-4cc7-aab5-cdf98b83f0c9"),
                    FirstName = "James",
                    LastName = "Elroy"
                },
                new Author()
                {
                    Id = Guid.Parse("2902b665-1190-4c70-9915-b9c2d7680450"),
                    FirstName = "Douglas",
                    LastName = "Adams"
                });

            //Intial data seed for Books
            modelBuilder.Entity<Book>().HasData(
                new Book()
                {
                    Id = Guid.Parse("5b1c2b4d-48c7-402a-80c3-cc796ad49c6b"),
                    AuthorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                    Title = "The Winds of Winter",
                    Description = "The book that seems impossible to write"
                },
                new Book()
                {
                    Id = Guid.Parse("d8663e5e-7494-4f81-8739-6e0de1bea7ee"),
                    AuthorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                    Title = "A Game of Thrones",
                    Description = "A Game of Thrones is the first novel in A Song of Ice"
                },
                new Book()
                {
                    Id = Guid.Parse("d173e20d-159e-4127-9ce9-b0ac2564ad97"),
                    AuthorId = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96"),
                    Title = "Mythos",
                    Description = "The Greek myths are amongst the best stories ever told"
                },
                new Book()
                {
                    Id = Guid.Parse("493c3228-3444-4a49-9cc0-e8532edc59b2"),
                    AuthorId = Guid.Parse("24810dfc-2d94-4cc7-aab5-cdf98b83f0c9"),
                    Title = "American Tabloid",
                    Description = "American Tabloid is a 1995 novel by James Ellroy that chr"
                },
                new Book()
                {
                    Id = Guid.Parse("40ff5488-fdab-45b5-bc3a-14302d59869a"),
                    AuthorId = Guid.Parse("2902b665-1190-4c70-9915-b9c2d7680450"),
                    Title = "The Hitchhiker's Guide to the Galaxy",
                    Description = "The Hitchhiker's Guide to the Galaxy, the characters"
                });
        }
    }
}
