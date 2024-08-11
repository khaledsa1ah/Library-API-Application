namespace Library.DTOs;

using AutoMapper;
using Library.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AuthorDto, Author>();
        CreateMap<CategoryDto, Category>();
        CreateBooksMapping();

    }

    private void CreateBooksMapping()
    {
        CreateMap<BookDto, Book>();
        CreateMap<Book, GetBooksDto>();
    }
}