using CareerForge.Api.Models;

namespace CareerForge.Api.Tests;

public sealed class DomainPolicyTests
{
    [Theory]
    [InlineData(1, ProficiencyLevel.Beginner)]
    [InlineData(25, ProficiencyLevel.Basic)]
    [InlineData(45, ProficiencyLevel.Intermediate)]
    [InlineData(65, ProficiencyLevel.Advanced)]
    [InlineData(85, ProficiencyLevel.Expert)]
    public void Proficiency_levels_have_stable_order(int score, ProficiencyLevel expected)
    {
        var actual = score switch
        {
            < 25 => ProficiencyLevel.Beginner,
            < 45 => ProficiencyLevel.Basic,
            < 65 => ProficiencyLevel.Intermediate,
            < 85 => ProficiencyLevel.Advanced,
            _ => ProficiencyLevel.Expert
        };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Self_assessment_and_measured_level_are_independent()
    {
        var skill = new UserSkill
        {
            SelfAssessedLevel = ProficiencyLevel.Advanced,
            MeasuredLevel = ProficiencyLevel.Basic,
            TargetLevel = ProficiencyLevel.Advanced,
            ConfidenceScore = 20
        };

        Assert.NotEqual(skill.SelfAssessedLevel, skill.MeasuredLevel);
        Assert.True(skill.ConfidenceScore < 50);
    }

    [Fact]
    public void Content_maturity_is_explicit()
    {
        var technology = new Technology { Name = "Go", Maturity = ContentMaturity.Preview };

        Assert.Equal(ContentMaturity.Preview, technology.Maturity);
    }
}
