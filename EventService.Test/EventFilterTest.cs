using EventManagement.Common;
using EventManagement.Interfaces;
using EventManagement.Models.FilterModels;
using EventManagement.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace EventServiceTest;

public class EventFilterTest
{
    private readonly IEventValidator _validator;
    private readonly IEventRepository _repository;
    private readonly IEventService _service;
 
    public EventFilterTest()
    {
        _validator = new EventValidator();
        _repository = new EventRepository();
        _service = new EventService(_validator, _repository);
    }

    [Fact]
    public void Test_EmptyFilter_ReturnAllEvents()
    {
        var count = TestData.GetTestEvents().Count();
        var result = _service.GetEvents(new EventFilterRequestDTO());

        result.Value.Should().NotBeNull();
        result.Value.Events.Count.Should().Be(count);
    }

    /// <summary>
    /// Тест для фильтрации данных (Тестируется три варианта вхождение строки по краям и в середине)
    /// </summary>
    /// <param name="filter">Фильтр для поиска событий</param>
    /// <param name="count">Количество найденных событий</param>
    /// <param name="id">id найденного события</param>
    [Theory]
    [MemberData(nameof(GetEventFilterByTitle))]    
    public void TestFilter_ByTitle_ReturnRelevantEvents(EventFilterRequestDTO filter, int count, int id)
    {
        var result = _service.GetEvents(filter);

        result.Value.Should().NotBeNull();
        result.Value.Events.Count.Should().Be(count);
        result.Value.Events.First().Id.Should().Be(id);
    }

    [Theory]
    [MemberData(nameof(GetEventFilterByDates))]
    public void TestFilter_ByDate_ReturnRelevantEvents(EventFilterRequestDTO filter, int count, int[] ids)
    {
        var result = _service.GetEvents(filter);
        var resultIds = result.Value?.Events.Select(o => o.Id).ToArray() ?? new int[] { };
        
        result.Value.Should().NotBeNull();
        result.Value.Events.Count.Should().Be(count);
        if (resultIds.Length > 0)
            resultIds.Should().Contain(ids);
    }

    [Theory]
    [MemberData(nameof(GetPaginationFilterData))]
    public void TestPagination(EventFilterRequestDTO filter, int currentPage, int eventCount)
    {
        var result = _service.GetEvents(filter);
        result.Value.Should().NotBeNull();
        result.Value.Page.Should().Be(currentPage);
        result.Value.EventsCountOnCurrentPage.Should().Be(eventCount);
    }


    public static IEnumerable<object[]> GetEventFilterByTitle()
    {
        var titles = new string[]{ "Другое", "событие для", "теста 2"};
        var filters = new object[titles.Length][];

        for(int i = 0; i < titles.Length; ++i)
        {
            var f = new EventFilterRequestDTO() { Title = titles[i] };
            filters[i] = new object[3] { f, 1, 2 };
        }

        return filters;
    }

    public static IEnumerable<object[]> GetEventFilterByDates()
    {
        var dates = new[]
        {
            new Tuple<DateTime?, DateTime?, int, int[]>(new DateTime(2026,03,23), new DateTime(2023,03,21), 0, new int []{ }),
            new Tuple<DateTime?, DateTime?, int, int[]>(new DateTime(2026,03,22), null, 2, new int []{1, 2}),            
            new Tuple<DateTime?, DateTime?, int, int[]>(new DateTime(2026,03,24), null, 1, new int []{2 }),
            new Tuple<DateTime?, DateTime?, int, int[]>(new DateTime(2026,03,25), null, 0 ,new int []{}),
                                   
            new Tuple<DateTime?, DateTime?, int, int[]>(null, new DateTime(2026,03,27), 2, new int []{ 1, 2}),            
            new Tuple<DateTime?, DateTime?, int, int[]>(null, new DateTime(2026,03,22), 1, new int []{1}),
            new Tuple<DateTime?, DateTime?, int, int[]>(null, new DateTime(2026,03,21), 0, new int []{}),

            
            new Tuple<DateTime?, DateTime?, int, int[]>(new DateTime(2026,03,22), new DateTime(2026,03,27), 2, new int []{ 1, 2}),
            new Tuple<DateTime?, DateTime?, int, int[]>(new DateTime(2026,03,23), new DateTime(2026,03,27), 1, new int []{2}),            
            new Tuple<DateTime?, DateTime?, int, int[]>(new DateTime(2026,03,22), new DateTime(2026,03,26), 1, new int []{ 1}),
        };
        var filters = new object[dates.Length][];

        for (int i =0; i< dates.Length; ++i)
        {
            var f = new EventFilterRequestDTO() 
            { 
                From = dates[i].Item1,
                To = dates[i].Item2,
            };

            filters[i] = new object[3] { f, dates[i].Item3, dates[i].Item4 };
        }

        return filters;
    }

    public static IEnumerable<object[]> GetPaginationFilterData()
    {
        var paginationData = new [] 
        { 
            new {Page = 1, PageSize = 10, CurrentPage = 1, EventCount = 2 },
            new {Page = 1, PageSize = 1, CurrentPage = 1, EventCount = 1},
            new {Page = 2, PageSize = 1, CurrentPage = 2, EventCount = 1},
            new {Page = 3, PageSize = 1, CurrentPage = 3, EventCount = 0},
        };
        var filters = new object[paginationData.Length][];

        for (int i = 0; i < paginationData.Length; ++i)
        {
            var f = new EventFilterRequestDTO() 
            {
                Page = paginationData[i].Page,
                PageSize = paginationData[i].PageSize,                
            };
            filters[i] = new object[] { f, paginationData[i].CurrentPage, paginationData[i].EventCount };
        }

        return filters;
    }
}
