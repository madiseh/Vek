using Vek.Docs.Services;

namespace Vek.UnitTests.Mocks;

public class MockDocsJsApiService : IDocsJsApiService
{
    public ValueTask<string> GetInnerTextByIdAsync(string id) => ValueTask.FromResult("inner text");
}
