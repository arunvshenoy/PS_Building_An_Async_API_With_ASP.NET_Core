using AutoMapper;
using System.Collections.Generic;

namespace Books.API.Profiles
{
    public class BooksProfile : Profile
    {
        public BooksProfile()
        {
            CreateMap<Entities.Book, Models.Book>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src =>
                    $"{src.Author.FirstName} {src.Author.LastName}"));

            CreateMap<Models.BookForCreation, Entities.Book>();

            //Map Book Entity to BookWithCover
            CreateMap<Entities.Book, Models.BookWithCovers>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src =>
                $"{src.Author.FirstName} {src.Author.LastName}"));

            //Then Map BookCovers IEnumerable with the same BookWithCovers class
            CreateMap<IEnumerable<ExternalModels.BookCover>, Models.BookWithCovers>()
                .ForMember(dest => dest.BookCovers, opt => opt.MapFrom(src =>
                        src));

            CreateMap<ExternalModels.BookCover, Models.BookCover>();
        }
    }
}