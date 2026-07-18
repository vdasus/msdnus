using AutoFixture;
using FluentAssertions;
using msdnus.Core.Model;
using Xunit;

namespace msdnus.Tests.Model
{
    public class DataTargetTests : IDisposable
    {
        private readonly Fixture _fixture = new();
        private readonly string _dbPath;
        private readonly DataTarget _target;

        public DataTargetTests()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"msdnus-tests-{Guid.NewGuid():N}.litedb");
            _target = new DataTarget(_dbPath, "test-password");
        }

        public void Dispose()
        {
            _target.Dispose();
            if (File.Exists(_dbPath)) File.Delete(_dbPath);
        }

        private YourKeyDTO NewDto(string name) => new()
        {
            Name = name,
            Items = { new YourKeyKeyDTO
            {
                ID = _fixture.Create<string>(),
                Type = _fixture.Create<string>(),
                ClaimedDate = "2015-01-01",
                Value = _fixture.Create<string>()
            } }
        };

        [Fact]
        [Trait("Category", "SmokeTest")]
        [Trait("Priority", "High")]
        public async Task SaveData_ThenLoadAll_ReturnsSavedItems()
        {
            var sample = new List<YourKeyDTO> { NewDto("TEST1"), NewDto("TEST2") };

            var saved = _target.SaveData(sample, iscleanbefore: true, isdistinct: true);
            var loaded = await _target.GetAllDataAsync();

            saved.Should().BeTrue();
            loaded.Should().HaveCount(2);
            loaded.Select(x => x.Name).Should().BeEquivalentTo("TEST1", "TEST2");
            loaded.Should().OnlyContain(x => x.Items.Count == 1);
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "High")]
        public async Task SaveData_WithCleanBefore_ReplacesExistingData()
        {
            _target.SaveData(new List<YourKeyDTO> { NewDto("OLD") }, true, true);

            _target.SaveData(new List<YourKeyDTO> { NewDto("NEW") }, iscleanbefore: true, isdistinct: true);
            var loaded = await _target.GetAllDataAsync();

            loaded.Should().ContainSingle().Which.Name.Should().Be("NEW");
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "Medium")]
        public async Task SaveData_WithDistinct_DropsDuplicates()
        {
            var dto = NewDto("SAME");
            var duplicate = new YourKeyDTO { Name = dto.Name, Items = { dto.Items[0] } };

            _target.SaveData(new List<YourKeyDTO> { dto, duplicate }, true, isdistinct: true);
            var loaded = await _target.GetAllDataAsync();

            loaded.Should().ContainSingle();
        }

        [Theory]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "Medium")]
        [InlineData("visual", 1)]
        [InlineData("VISUAL", 1)]
        [InlineData("office", 1)]
        [InlineData("nothing-matches", 0)]
        [InlineData("", 2)]
        public async Task GetDataValueByName_FiltersCaseInsensitively(string search, int expectedCount)
        {
            _target.SaveData(new List<YourKeyDTO> { NewDto("Visual Studio"), NewDto("Office 365") }, true, true);

            var found = await _target.GetDataValueByNameAsync(search);

            found.Should().HaveCount(expectedCount);
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "Medium")]
        public async Task CleanDb_RemovesAllData()
        {
            _target.SaveData(new List<YourKeyDTO> { NewDto("TEST") }, true, true);

            var cleaned = await _target.CleanDbAsync();
            var loaded = await _target.GetAllDataAsync();

            cleaned.Should().BeTrue();
            loaded.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        [Trait("Priority", "High")]
        public async Task Data_SurvivesReopen_WithSamePassword()
        {
            _target.SaveData(new List<YourKeyDTO> { NewDto("PERSISTED") }, true, true);
            _target.Dispose();

            using var reopened = new DataTarget(_dbPath, "test-password");
            var loaded = await reopened.GetAllDataAsync();

            loaded.Should().ContainSingle().Which.Name.Should().Be("PERSISTED");
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        [Trait("Priority", "High")]
        public void OpeningDb_WithWrongPassword_Fails()
        {
            _target.SaveData(new List<YourKeyDTO> { NewDto("SECRET") }, true, true);
            _target.Dispose();

            var openWithWrongPassword = () => new DataTarget(_dbPath, "wrong-password");

            openWithWrongPassword.Should().Throw<LiteDB.LiteException>();
        }
    }
}
