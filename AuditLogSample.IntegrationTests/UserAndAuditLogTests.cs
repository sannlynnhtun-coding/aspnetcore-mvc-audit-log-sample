namespace AuditLogSample.IntegrationTests;

public sealed class UserAndAuditLogTests
{
    [Fact]
    public async Task UserLifecycleRecordsAuditEntriesAndReturnsExpectedResults()
    {
        var databaseFile = Path.GetTempFileName();

        try
        {
            await using var factory = new ApiFactory(databaseFile);
            using var client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost"),
                    AllowAutoRedirect = false
                });

            #region Create the user and check duplicate handling

            await CheckResultAsync<List<UserResponse>>(
                await client.GetAsync("/api/users"), HttpStatusCode.OK);

            var request = new CreateUserRequest
            {
                Name = "Runtime Test",
                Email = "runtime-test@example.com",
                Limit = 100
            };

            var createResponse = await client.PostAsJsonAsync("/api/users", request);
            var createdLocation = createResponse.Headers.Location;
            var created = await CheckResultAsync<UserResponse>(createResponse, HttpStatusCode.Created);
            var user = Assert.IsType<UserResponse>(created.Data);
            var userPath = $"/api/users/{user.Id}";

            Assert.Equal("Pending", user.Status);
            Assert.Equal(userPath, createdLocation?.AbsolutePath);

            await CheckResultAsync<UserResponse>(
                await client.PostAsJsonAsync("/api/users", request), HttpStatusCode.BadRequest);

            #endregion

            #region Read, update, approve, reject, and delete the user

            var found = await CheckResultAsync<UserResponse>(
                await client.GetAsync(userPath), HttpStatusCode.OK);
            Assert.Equal(user.Id, found.Data?.Id);

            var update = new UpdateUserRequest
            {
                Name = request.Name,
                Email = request.Email,
                Limit = 200
            };

            var updated = await CheckResultAsync<UserResponse>(
                await client.PutAsJsonAsync(userPath, update), HttpStatusCode.OK);
            Assert.Equal(200m, updated.Data?.Limit);

            var approved = await CheckResultAsync<UserResponse>(
                await client.PostAsJsonAsync(
                    $"{userPath}/approve",
                    new UserActionRequest
                    {
                        Reason = "Verified"
                    }),
                HttpStatusCode.OK);
            Assert.Equal("Approved", approved.Data?.Status);

            await CheckResultAsync<UserResponse>(
                await client.PostAsync($"{userPath}/approve", null), HttpStatusCode.BadRequest);

            var rejected = await CheckResultAsync<UserResponse>(
                await client.PostAsJsonAsync(
                    $"{userPath}/reject",
                    new UserActionRequest
                    {
                        Reason = "Needs review"
                    }),
                HttpStatusCode.OK);
            Assert.Equal("Rejected", rejected.Data?.Status);

            using var deleted = await client.DeleteAsync(userPath);
            Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);

            var deleteResult = await deleted.Content.ReadFromJsonAsync<Result>();
            Assert.True(Assert.IsType<Result>(deleteResult).IsSuccess);

            await CheckResultAsync<UserResponse>(
                await client.GetAsync(userPath), HttpStatusCode.BadRequest);

            #endregion

            #region Check audit entries and an unknown target

            string[] expectedActions = ["DELETE", "REJECT", "APPROVE", "UPDATE", "CREATE"];

            var allLogs = await CheckResultAsync<List<AuditLogResponse>>(
                await client.GetAsync("/api/audit-logs"), HttpStatusCode.OK);
            var userLogs = Assert.IsType<List<AuditLogResponse>>(allLogs.Data)
                .Where(log => log.TargetId == user.Id);
            Assert.Equal(expectedActions, userLogs.Select(log => log.Action));

            var targetLogs = await CheckResultAsync<List<AuditLogResponse>>(
                await client.GetAsync($"/api/audit-logs/target/{user.Id}"), HttpStatusCode.OK);
            Assert.Equal(
                expectedActions,
                Assert.IsType<List<AuditLogResponse>>(targetLogs.Data).Select(log => log.Action));

            var unknownTarget = await CheckResultAsync<List<AuditLogResponse>>(
                await client.GetAsync("/api/audit-logs/target/no-such-user"), HttpStatusCode.OK);
            Assert.Empty(Assert.IsType<List<AuditLogResponse>>(unknownTarget.Data));

            #endregion
        }
        finally
        {
            File.Delete(databaseFile);
        }
    }

    private static async Task<Result<T>> CheckResultAsync<T>(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus)
    {
        using (response)
        {
            Assert.Equal(expectedStatus, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<Result<T>>();
            var result = Assert.IsType<Result<T>>(body);
            Assert.Equal(expectedStatus != HttpStatusCode.BadRequest, result.IsSuccess);

            return result;
        }
    }

    private sealed class ApiFactory(string databaseFile) : WebApplicationFactory<CurrentUser>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

                var connectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = databaseFile,
                    Pooling = false
                };

                services.AddDbContext<AppDbContext>(
                    options => options.UseSqlite(connectionString.ToString()));
            });
        }
    }
}
