namespace Domain.Entities.Util;

public record PagedResult<T>(List<T> Items,DateTime? NextCursor,bool HasNextPage);
