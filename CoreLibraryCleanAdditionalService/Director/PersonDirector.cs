using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Core.Library.Clean.AdditionalService
{
    public class PersonDirector : BaseDirector, IEntityDirector<PersonDTO, PersonCreateDTO>
    {
        public PersonDirector(
            HttpClient _httpClient, 
            IHttpContextAccessor _httpContextAccessor,
            ILogger<PersonDirector> logger) 
            : base(_httpClient, _httpContextAccessor, logger)
        {
            Console.WriteLine(httpClient.BaseAddress);
        }

        public async Task<IEnumerable<PersonDTO>> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = "";

            // Don't pass cancellationToken to HttpClient - Polly manages timeout/cancellation internally
            using var response = await httpClient.GetAsync(requestUrl, CancellationToken.None)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<PersonDTO>>(response, cancellationToken);
        }

        public async Task<PersonDTO> GetEntityByIdAsync(string entityId, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"{entityId}";

            // Don't pass cancellationToken to HttpClient - Polly manages timeout/cancellation internally
            using var response = await httpClient.GetAsync(requestUrl, CancellationToken.None)
                .ConfigureAwait(false);

            return await HandleResponseAsync<PersonDTO>(response, cancellationToken);
        }

        public async Task<IEnumerable<PersonDTO>> SearchEntitiesAsync(string searchValue, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"SearchByPerson/{searchValue}";

            // Don't pass cancellationToken to HttpClient - Polly manages timeout/cancellation internally
            using var response = await httpClient.GetAsync(requestUrl, CancellationToken.None)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<PersonDTO>>(response, cancellationToken);
        }

        public async Task<IEnumerable<PersonDTO>> SearchEntitiesByForeignIdAsync(string foreignKeyId, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"SearchByBookId/{foreignKeyId}";

            // Don't pass cancellationToken to HttpClient - Polly manages timeout/cancellation internally
            using var response = await httpClient.GetAsync(requestUrl, CancellationToken.None)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<PersonDTO>>(response, cancellationToken);
        }

        public async Task<long> UpdateEntityByIdAsync(string entityId, PersonDTO entity, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"{entityId}";

            using var content = CreateJsonContent(entity);

            // Don't pass cancellationToken to HttpClient - Polly manages timeout/cancellation internally
            using var response = await httpClient.PutAsync(requestUrl, content, CancellationToken.None)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<long> UpdateEntitiesAsync(IEnumerable<string> entityIds, IEnumerable<PersonDTO> entities, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"Many/{entityIds}";

            using var content = CreateJsonContent(entities);

            // Don't pass cancellationToken to HttpClient - Polly manages timeout/cancellation internally
            using var response = await httpClient.PutAsync(requestUrl, content, CancellationToken.None)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<PersonDTO> CreateEntityAsync(PersonCreateDTO entity, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = "";

            using var content = CreateJsonContent(entity);

            // Don't pass cancellationToken to HttpClient - Polly manages timeout/cancellation internally
            using var response = await httpClient.PostAsync(requestUrl, content, CancellationToken.None)
                .ConfigureAwait(false);

            return await HandleResponseAsync<PersonDTO>(response, cancellationToken);
        }

        public async Task<IEnumerable<PersonDTO>> CreateEntitiesAsync(IEnumerable<PersonCreateDTO> entities, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = "Many";

            using var content = CreateJsonContent(entities);

            // Don't pass cancellationToken to HttpClient - Polly manages timeout/cancellation internally
            using var response = await httpClient.PostAsync(requestUrl, content, CancellationToken.None)
                .ConfigureAwait(false);

            return await HandleResponseAsync<IEnumerable<PersonDTO>>(response, cancellationToken);
        }

        public async Task<long> DeleteEntityByIdAsync(string entityId, CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"{entityId}";

            // Don't pass cancellationToken to HttpClient - Polly manages timeout/cancellation internally
            using var response = await httpClient.DeleteAsync(requestUrl, CancellationToken.None)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }

        public async Task<long> DeleteEntitiesAsync(CancellationToken cancellationToken)
        {
            AddCorrelationIdHeader();
            var requestUrl = $"Many";

            // Don't pass cancellationToken to HttpClient - Polly manages timeout/cancellation internally
            using var response = await httpClient.DeleteAsync(requestUrl, CancellationToken.None)
                .ConfigureAwait(false);

            return await HandleResponseAsync<long>(response, cancellationToken);
        }
    }
}
