using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Core.Library.Clean.AdditionalService
{
    public class BookDirector : BaseDirector, IEntityDirector<BookDTO, BookCreateDTO>
    {
        public BookDirector(
            HttpClient _httpClient, 
            IHttpContextAccessor _httpContextAccessor,
            ILogger<BookDirector> logger) 
            : base(_httpClient, _httpContextAccessor, logger)
        {
            Console.WriteLine(httpClient.BaseAddress);
        }

        public async Task<IEnumerable<BookDTO>> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = "";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }

        public async Task<BookDTO> GetEntityByIdAsync(string entityId, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"{entityId}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<BookDTO>(response, cancellationToken);
        }

        public async Task<IEnumerable<BookDTO>> SearchEntitiesAsync(string searchValue, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"SearchByBook/{searchValue}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }
        public async Task<IEnumerable<BookDTO>> SearchEntitiesByForeignIdAsync(string foreignKeyId, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"SearchByPersonId/{foreignKeyId}";

            using var response = await httpClient.GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }

        public async Task<long> UpdateEntityByIdAsync(string entityId, BookDTO entity, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"{entityId}";

            using var content = CreateJsonContent(entity);

            using var response = await httpClient.PutAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<long> UpdateEntitiesAsync(IEnumerable<string> entityIds, IEnumerable<BookDTO> entities, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"Many/{entityIds}";

            using var content = CreateJsonContent(entities);

            using var response = await httpClient.PutAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<BookDTO> CreateEntityAsync(BookCreateDTO entity, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = "";

            using var content = CreateJsonContent(entity);

            using var response = await httpClient.PostAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<BookDTO>(response, cancellationToken);
        }

        public async Task<IEnumerable<BookDTO>> CreateEntitiesAsync(IEnumerable<BookCreateDTO> entities, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = "Many";

            using var content = CreateJsonContent(entities);

            using var response = await httpClient.PostAsync(requestUrl, content, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<BookDTO>>(response, cancellationToken);
        }

        public async Task<long> DeleteEntityByIdAsync(string entityId, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"{entityId}";

            using var response = await httpClient.DeleteAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<long> DeleteEntitiesAsync(CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"Many";

            using var response = await httpClient.DeleteAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }
    }
}
