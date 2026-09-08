using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services;
using FootballFull.Services.Interfaces;
using Moq;

namespace FootballFull.Tests
{
    public class TestCompetitionRulesService
    {
        private CompetitionRulesService CreateService(
    Mock<IRepository<CompetitionRules>> repository)
        {
            return new CompetitionRulesService(
                Mock.Of<ICompetitionService>(),
                Mock.Of<IClubPerCompetitionService>(),
                Mock.Of<IClubLeagueCompetitionService>(),
                Mock.Of<IClubService>(),
                repository.Object);
        }

        [Fact]
        public void GetCompetitionRules_WhenRulesDoNotExist_ReturnsEmptyRules()
        {
            // Arrange
            var competitionId = Guid.NewGuid();

            var competitionService = new Mock<ICompetitionService>();
            var clubPerCompetitionService = new Mock<IClubPerCompetitionService>();
            var clubLeagueCompetitionService = new Mock<IClubLeagueCompetitionService>();
            var clubService = new Mock<IClubService>();
            var repository = new Mock<IRepository<CompetitionRules>>();

            repository
                .Setup(x => x.Load())
                .Returns(new List<CompetitionRules>());

            var service = new CompetitionRulesService(
                competitionService.Object,
                clubPerCompetitionService.Object,
                clubLeagueCompetitionService.Object,
                clubService.Object,
                repository.Object);

            // Act
            var result = service.GetCompetitionRules(competitionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(competitionId, result.CompetitionId);
        }

        [Fact]
        public void Save_WhenRulesHaveId_UpdatesRules()
        {
            // Arrange
            var repository = new Mock<IRepository<CompetitionRules>>();

            var service = CreateService(repository);

            var rules = new CompetitionRules
            {
                Id = Guid.NewGuid(),
                CompetitionId = Guid.NewGuid()
            };

            // Act
            var result = service.Save(rules);

            // Assert
            Assert.True(result);

            repository.Verify(
                x => x.Update(rules),
                Times.Once);

            repository.Verify(
                x => x.Add(It.IsAny<CompetitionRules>()),
                Times.Never);
        }

        [Fact]
        public void Save_WhenRulesHaveNoId_AssignsNewIdAndAddsRules()
        {
            // Arrange
            var repository = new Mock<IRepository<CompetitionRules>>();
            var service = CreateService(repository);

            var rules = new CompetitionRules
            {
                Id = Guid.Empty
            };

            // Act
            service.Save(rules);

            // Assert
            Assert.NotEqual(Guid.Empty, rules.Id);

            repository.Verify(
                x => x.Add(rules),
                Times.Once);

            repository.Verify(
                x => x.Update(It.IsAny<CompetitionRules>()),
                Times.Never);
        }
    }
}
