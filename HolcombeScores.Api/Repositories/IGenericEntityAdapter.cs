using System.Collections.Generic;
using Azure;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Repositories
{
    public interface IGenericEntityAdapter
    {
        GenericTableEntity<T> Adapt<T>(T content, object partitionKey, object rowKey);
        T Adapt<T>(GenericTableEntity<T> tableRecord);
        IAsyncEnumerable<T> AdaptAll<T>(IAsyncEnumerable<GenericTableEntity<T>> results);
    }
}