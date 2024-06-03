namespace DienstDuizend.AuthenticationService.Common.Dto;

public class PaginationResult<T>
{
    public List<T> Data { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
}
