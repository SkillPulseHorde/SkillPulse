using AssessmentService.Application;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using AssessmentService.Domain.ValueObjects;
using Moq;

namespace AssessmentService.Tests;

public class EvaluationAnalyzerTests
{
    private readonly Mock<IAssessmentResultRepository> _assessmentResultRepositoryMock;
    private readonly Mock<IEvaluationRepository> _evaluationRepositoryMock;
    private readonly Mock<ICompetenceRepository> _competenceRepositoryMock;
    private readonly EvaluationAnalyzer _analyzer;

    public EvaluationAnalyzerTests()
    {
        _assessmentResultRepositoryMock = new Mock<IAssessmentResultRepository>();
        _evaluationRepositoryMock = new Mock<IEvaluationRepository>();
        _competenceRepositoryMock = new Mock<ICompetenceRepository>();
        _analyzer = new EvaluationAnalyzer(
            _assessmentResultRepositoryMock.Object,
            _evaluationRepositoryMock.Object,
            _competenceRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAssessmentResultByAssessmentIdAsync_ShouldReturnExistingResult_WhenResultExists()
    {
        // Arrange
        var assessmentId = Guid.NewGuid();
        var existingResult = new AssessmentResult
        {
            AssessmentId = assessmentId,
            Data = new AssessmentResultData(new Dictionary<Guid, CompetenceSummary?>())
        };

        _assessmentResultRepositoryMock
            .Setup(x => x.GetByAssessmentIdAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync(existingResult);

        // Act
        var result = await _analyzer.GetAssessmentResultByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(assessmentId, result.AssessmentId);
        _assessmentResultRepositoryMock.Verify(x => x.GetByAssessmentIdAsync(
            assessmentId, CancellationToken.None), Times.Once);
        _evaluationRepositoryMock.Verify(x => x.GetEvaluationsByAssessmentIdReadonlyAsync(
            It.IsAny<Guid>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task GetAssessmentResultByAssessmentIdAsync_ShouldReturnNull_WhenNoEvaluations()
    {
        // Arrange
        var assessmentId = Guid.NewGuid();

        _assessmentResultRepositoryMock
            .Setup(x => x.GetByAssessmentIdAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync((AssessmentResult?)null);

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdReadonlyAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync([]);

        // Act
        var result = await _analyzer.GetAssessmentResultByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.Null(result);
        _assessmentResultRepositoryMock.Verify(x => x.CreateAsync(
            It.IsAny<AssessmentResult>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task GetAssessmentResultByAssessmentIdAsync_ShouldCalculateWeightedAverage_WithSingleEvaluator()
    {
        // Arrange
        var assessmentId = Guid.NewGuid();
        var competenceId = Guid.NewGuid();
        var criterion1Id = Guid.NewGuid();
        var criterion2Id = Guid.NewGuid();
        var evaluationId = Guid.NewGuid();

        var evaluation = new Evaluation
        {
            Id = evaluationId,
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = evaluationId,
                    Comment = "Good work",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 8,
                            Comment = "Excellent"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion2Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 6,
                            Comment = "Good"
                        }
                    }
                }
            }
        };

        _assessmentResultRepositoryMock
            .Setup(x => x.GetByAssessmentIdAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync((AssessmentResult?)null);

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdReadonlyAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync([evaluation]);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competenceId, [criterion1Id, criterion2Id] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AssessmentResult>(), CancellationToken.None))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _analyzer.GetAssessmentResultByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data.CompetenceSummaries);
        
        var competenceSummary = result.Data.CompetenceSummaries[competenceId];
        Assert.NotNull(competenceSummary);
        const double expectedScore = (double)(8 + 6) / 2;
        Assert.Equal(expectedScore, competenceSummary.AverageScore); 
        Assert.Equal(2, competenceSummary.CriterionSummaries.Count);
        Assert.Single(competenceSummary.Comments);
        Assert.Equal("Good work", competenceSummary.Comments[0]);
    }

    [Fact]
    public async Task GetAssessmentResultByAssessmentIdAsync_ShouldCalculateWeightedAverage_WithMultipleEvaluatorsAndDifferentRatios()
    {
        // Arrange
        var assessmentId = Guid.NewGuid();
        var competence1Id = Guid.NewGuid();
        var competence2Id = Guid.NewGuid();
        var criterion1Id = Guid.NewGuid();
        var criterion2Id = Guid.NewGuid();

        var managerEvaluationId = Guid.NewGuid();
        var colleagueEvaluationId = Guid.NewGuid();

        var managerEvaluation = new Evaluation
        {
            Id = managerEvaluationId,
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 2, // Менеджер с коэффициентом 2
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competence1Id,
                    EvaluationId = managerEvaluationId,
                    Comment = "Manager feedback on competence 1",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 9,
                            Comment = "Excellent technical skills"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion2Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 7,
                            Comment = "Good problem solving"
                        }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competence2Id,
                    EvaluationId = managerEvaluationId,
                    Comment = "Manager feedback on competence 2",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 8,
                            Comment = "Strong leadership"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion2Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 6,
                            Comment = "Decent communication"
                        }
                    }
                }
            }
        };

        var colleagueEvaluation = new Evaluation
        {
            Id = colleagueEvaluationId,
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1, // Коллега с коэффициентом 1
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competence1Id,
                    EvaluationId = colleagueEvaluationId,
                    Comment = "Colleague feedback on competence 1",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 6,
                            Comment = "Solid technical knowledge"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion2Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 5,
                            Comment = "Needs improvement in problem solving"
                        }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competence2Id,
                    EvaluationId = colleagueEvaluationId,
                    Comment = "Colleague feedback on competence 2",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 7,
                            Comment = "Good team player"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion2Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 8,
                            Comment = "Great communication skills"
                        }
                    }
                }
            }
        };

        _assessmentResultRepositoryMock
            .Setup(x => x.GetByAssessmentIdAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync((AssessmentResult?)null);

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdReadonlyAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync([managerEvaluation, colleagueEvaluation]);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competence1Id, [criterion1Id, criterion2Id] },
                { competence2Id, [criterion1Id, criterion2Id] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AssessmentResult>(), CancellationToken.None))
            .ReturnsAsync(Guid.NewGuid());

        // Expected values for Competence 1
        var competence1Criterion1Expected = Math.Round((9.0 * 2 + 6.0 * 1) / (2.0 + 1.0), 2);
        var competence1Criterion2Expected = Math.Round((7.0 * 2 + 5.0 * 1) / (2.0 + 1.0), 2);
        var competence1Expected = Math.Round((competence1Criterion1Expected + competence1Criterion2Expected) / 2.0, 2); // среднее по критериям

        // Expected values for Competence 2
        var competence2Criterion1Expected = Math.Round((8.0 * 2 + 7.0 * 1) / (2.0 + 1.0), 2);
        var competence2Criterion2Expected = Math.Round((6.0 * 2 + 8.0 * 1) / (2.0 + 1.0), 2);
        var competence2Expected = Math.Round((competence2Criterion1Expected + competence2Criterion2Expected) / 2.0, 2); // среднее по критериям

        // Act
        var result = await _analyzer.GetAssessmentResultByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Data.CompetenceSummaries.Count);

        // Проверяем Competence 1
        var competence1Summary = result.Data.CompetenceSummaries[competence1Id];
        Assert.NotNull(competence1Summary);
        Assert.Equal(competence1Expected, competence1Summary.AverageScore, precision: 10);
        Assert.Equal(2, competence1Summary.Comments.Count);
        Assert.Contains("Manager feedback on competence 1", competence1Summary.Comments);
        Assert.Contains("Colleague feedback on competence 1", competence1Summary.Comments);

        var competence1Criterion1Summary = competence1Summary.CriterionSummaries[criterion1Id];
        Assert.Equal(competence1Criterion1Expected, competence1Criterion1Summary.Score, precision: 10);
        Assert.Equal(2, competence1Criterion1Summary.Comments.Count);

        var competence1Criterion2Summary = competence1Summary.CriterionSummaries[criterion2Id];
        Assert.Equal(competence1Criterion2Expected, competence1Criterion2Summary.Score, precision: 10);
        Assert.Equal(2, competence1Criterion2Summary.Comments.Count);

        // Проверяем Competence 2
        var competence2Summary = result.Data.CompetenceSummaries[competence2Id];
        Assert.NotNull(competence2Summary);
        Assert.Equal(competence2Expected, competence2Summary.AverageScore, precision: 10);
        Assert.Equal(2, competence2Summary.Comments.Count);
        Assert.Contains("Manager feedback on competence 2", competence2Summary.Comments);
        Assert.Contains("Colleague feedback on competence 2", competence2Summary.Comments);

        var competence2Criterion1Summary = competence2Summary.CriterionSummaries[criterion1Id];
        Assert.Equal(competence2Criterion1Expected, competence2Criterion1Summary.Score, precision: 10);
        Assert.Equal(2, competence2Criterion1Summary.Comments.Count);

        var competence2Criterion2Summary = competence2Summary.CriterionSummaries[criterion2Id];
        Assert.Equal(competence2Criterion2Expected, competence2Criterion2Summary.Score, precision: 10);
        Assert.Equal(2, competence2Criterion2Summary.Comments.Count);
    }

    [Fact]
    public async Task GetAssessmentResultByAssessmentIdAsync_ShouldSkipCriteriaWithoutScores()
    {
        // Arrange
        var assessmentId = Guid.NewGuid();
        var competenceId = Guid.NewGuid();
        var criterion1Id = Guid.NewGuid();
        var criterion2Id = Guid.NewGuid();
        var evaluationId = Guid.NewGuid();

        var evaluation = new Evaluation
        {
            Id = evaluationId,
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = evaluationId,
                    Comment = "Test",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 8,
                            Comment = "Has score"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion2Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = null, // Нет оценки
                            Comment = null
                        }
                    }
                }
            }
        };

        _assessmentResultRepositoryMock
            .Setup(x => x.GetByAssessmentIdAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync((AssessmentResult?)null);

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdReadonlyAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync([evaluation]);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competenceId, [criterion1Id, criterion2Id] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AssessmentResult>(), CancellationToken.None))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _analyzer.GetAssessmentResultByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.NotNull(result);
        var competenceSummary = result.Data.CompetenceSummaries[competenceId];
        Assert.NotNull(competenceSummary);
        
        // Только criterion1 должен быть в результате, так как у criterion2 нет оценки
        Assert.Single(competenceSummary.CriterionSummaries);
        Assert.True(competenceSummary.CriterionSummaries.ContainsKey(criterion1Id));
        Assert.False(competenceSummary.CriterionSummaries.ContainsKey(criterion2Id));
    }

    [Fact]
    public async Task GetAssessmentResultByAssessmentIdAsync_ShouldReturnNull_WhenCompetenceHasNoEvaluations()
    {
        // Arrange
        var assessmentId = Guid.NewGuid();
        var competenceId = Guid.NewGuid();
        var criterionId = Guid.NewGuid();
        var evaluationId = Guid.NewGuid();

        // Оценка существует, но для другой компетенции
        var evaluation = new Evaluation
        {
            Id = evaluationId,
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = Guid.NewGuid(), // Другая компетенция
                    EvaluationId = evaluationId,
                    Comment = "Test",
                    CriterionEvaluations = new List<CriterionEvaluation>()
                }
            }
        };

        _assessmentResultRepositoryMock
            .Setup(x => x.GetByAssessmentIdAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync((AssessmentResult?)null);

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdReadonlyAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync([evaluation]);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competenceId, [criterionId] }
            });

        // Act
        var result = await _analyzer.GetAssessmentResultByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.Null(result); // Нет оценок по компетенциям из справочника
    }

    [Fact]
    public async Task GetAssessmentResultByAssessmentIdAsync_ShouldHandleMultipleCompetences()
    {
        // Arrange
        var assessmentId = Guid.NewGuid();
        var competence1Id = Guid.NewGuid();
        var competence2Id = Guid.NewGuid();
        var criterion1Id = Guid.NewGuid();
        var criterion2Id = Guid.NewGuid();
        var evaluationId = Guid.NewGuid();

        var evaluation = new Evaluation
        {
            Id = evaluationId,
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competence1Id,
                    EvaluationId = evaluationId,
                    Comment = "Competence 1",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 7
                        }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competence2Id,
                    EvaluationId = evaluationId,
                    Comment = "Competence 2",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion2Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 9
                        }
                    }
                }
            }
        };

        _assessmentResultRepositoryMock
            .Setup(x => x.GetByAssessmentIdAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync((AssessmentResult?)null);

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdReadonlyAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync([evaluation]);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competence1Id, [criterion1Id] },
                { competence2Id, [criterion2Id] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AssessmentResult>(), CancellationToken.None))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _analyzer.GetAssessmentResultByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Data.CompetenceSummaries.Count);

        Assert.True(result.Data.CompetenceSummaries.TryGetValue(competence1Id, out var competence1Summary));
        Assert.Equal(7.0, competence1Summary!.AverageScore);

        Assert.True(result.Data.CompetenceSummaries.TryGetValue(competence2Id, out var competence2Summary));
        Assert.Equal(9.0, competence2Summary!.AverageScore);
    }

    [Fact]
    public async Task GetAssessmentResultByAssessmentIdAsync_ShouldCollectAllComments()
    {
        // Arrange
        var assessmentId = Guid.NewGuid();
        var competenceId = Guid.NewGuid();
        var criterionId = Guid.NewGuid();
        var evaluation1Id = Guid.NewGuid();
        var evaluation2Id = Guid.NewGuid();

        var evaluation1 = new Evaluation
        {
            Id = evaluation1Id,
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = evaluation1Id,
                    Comment = "First competence comment",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterionId,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 8,
                            Comment = "First criterion comment"
                        }
                    }
                }
            }
        };

        var evaluation2 = new Evaluation
        {
            Id = evaluation2Id,
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = evaluation2Id,
                    Comment = "Second competence comment",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterionId,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 6,
                            Comment = "Second criterion comment"
                        }
                    }
                }
            }
        };

        _assessmentResultRepositoryMock
            .Setup(x => x.GetByAssessmentIdAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync((AssessmentResult?)null);

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdReadonlyAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync([evaluation1, evaluation2]);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competenceId, [criterionId] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AssessmentResult>(), CancellationToken.None))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _analyzer.GetAssessmentResultByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.NotNull(result);
        var competenceSummary = result.Data.CompetenceSummaries[competenceId];
        Assert.NotNull(competenceSummary);
        
        // Проверяем комментарии к компетенции
        Assert.Equal(2, competenceSummary.Comments.Count);
        Assert.Contains("First competence comment", competenceSummary.Comments);
        Assert.Contains("Second competence comment", competenceSummary.Comments);
        
        // Проверяем комментарии к критерию
        var criterionSummary = competenceSummary.CriterionSummaries[criterionId];
        Assert.Equal(2, criterionSummary.Comments.Count);
        Assert.Contains("First criterion comment", criterionSummary.Comments);
        Assert.Contains("Second criterion comment", criterionSummary.Comments);
    }

    [Fact]
    public async Task GetAssessmentResultByAssessmentIdAsync_ShouldIgnoreEmptyComments()
    {
        // Arrange
        var assessmentId = Guid.NewGuid();
        var competenceId = Guid.NewGuid();
        var criterionId = Guid.NewGuid();
        var evaluationId = Guid.NewGuid();

        var evaluation = new Evaluation
        {
            Id = evaluationId,
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = evaluationId,
                    Comment = "", // Пустой комментарий
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterionId,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 8,
                            Comment = null // Null комментарий
                        }
                    }
                }
            }
        };

        _assessmentResultRepositoryMock
            .Setup(x => x.GetByAssessmentIdAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync((AssessmentResult?)null);

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdReadonlyAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync([evaluation]);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competenceId, [criterionId] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AssessmentResult>(), CancellationToken.None))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _analyzer.GetAssessmentResultByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.NotNull(result);
        var competenceSummary = result.Data.CompetenceSummaries[competenceId];
        Assert.NotNull(competenceSummary);
        
        Assert.Empty(competenceSummary.Comments);
        
        var criterionSummary = competenceSummary.CriterionSummaries[criterionId];
        Assert.Empty(criterionSummary.Comments);
    }

    [Fact]
    public async Task GetAssessmentResultByAssessmentIdAsync_ShouldSaveResultToRepository()
    {
        // Arrange
        var assessmentId = Guid.NewGuid();
        var competenceId = Guid.NewGuid();
        var criterionId = Guid.NewGuid();
        var evaluationId = Guid.NewGuid();

        var evaluation = new Evaluation
        {
            Id = evaluationId,
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = evaluationId,
                    Comment = "Test",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterionId,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 10
                        }
                    }
                }
            }
        };

        _assessmentResultRepositoryMock
            .Setup(x => x.GetByAssessmentIdAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync((AssessmentResult?)null);

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdReadonlyAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync([evaluation]);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competenceId, [criterionId] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AssessmentResult>(), CancellationToken.None))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await _analyzer.GetAssessmentResultByAssessmentIdAsync(assessmentId);

        // Assert
        _assessmentResultRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<AssessmentResult>(r => r.AssessmentId == assessmentId),
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task GetAssessmentResultByAssessmentIdAsync_ShouldSkipCompetenceWithNullCriterionEvaluations()
    {
        // Arrange
        var assessmentId = Guid.NewGuid();
        var competence1Id = Guid.NewGuid();
        var competence2Id = Guid.NewGuid();
        var criterion1Id = Guid.NewGuid();

        var evaluator1Id = Guid.NewGuid();
        var evaluator2Id = Guid.NewGuid();

        // Первый оценщик - оценивает обе компетенции
        var evaluation1 = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = evaluator1Id,
            RoleRatio = 1,
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competence1Id,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Evaluator 1 feedback on competence 1",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 8,
                            Comment = "Good skills"
                        }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competence2Id,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Evaluator 1 feedback on competence 2",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 7,
                            Comment = "Solid performance"
                        }
                    }
                }
            }
        };

        // Второй оценщик - оценивает только первую компетенцию
        var evaluation2 = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = evaluator2Id,
            RoleRatio = 1,
            AssessmentId = assessmentId,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competence1Id,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Evaluator 2 feedback on competence 1",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 6,
                            Comment = "Needs improvement"
                        }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competence2Id,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Evaluator 2 did not evaluate competence 2",
                    CriterionEvaluations = null // Не оценивал эту компетенцию
                }
            }
        };

        _assessmentResultRepositoryMock
            .Setup(x => x.GetByAssessmentIdAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync((AssessmentResult?)null);

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdReadonlyAsync(assessmentId, CancellationToken.None))
            .ReturnsAsync([evaluation1, evaluation2]);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competence1Id, [criterion1Id] },
                { competence2Id, [criterion1Id] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<AssessmentResult>(), CancellationToken.None))
            .ReturnsAsync(Guid.NewGuid());
        
        // Competence 1: оба оценщика оценили -> (8 + 6) / 2 = 7.0
        const double competence1Expected = 7.0;
        // Competence 2: только первый оценщик оценил -> 7.0
        const double competence2Expected = 7.0;

        // Act
        var result = await _analyzer.GetAssessmentResultByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Data.CompetenceSummaries.Count);

        // Проверяем Competence 1 - учитываются оба оценщика
        var competence1Summary = result.Data.CompetenceSummaries[competence1Id];
        Assert.NotNull(competence1Summary);
        Assert.Equal(competence1Expected, competence1Summary.AverageScore, precision: 10);
        Assert.Equal(2, competence1Summary.Comments.Count);
        Assert.Contains("Evaluator 1 feedback on competence 1", competence1Summary.Comments);
        Assert.Contains("Evaluator 2 feedback on competence 1", competence1Summary.Comments);

        var competence1CriterionSummary = competence1Summary.CriterionSummaries[criterion1Id];
        Assert.Equal(competence1Expected, competence1CriterionSummary.Score, precision: 10);
        Assert.Equal(2, competence1CriterionSummary.Comments.Count);

        // Проверяем Competence 2 - учитывается только первый оценщик 
        var competence2Summary = result.Data.CompetenceSummaries[competence2Id];
        Assert.NotNull(competence2Summary);
        Assert.Equal(competence2Expected, competence2Summary.AverageScore, precision: 10);
        // Только комментарий от первого оценщика, второй не учитывается 
        Assert.Single(competence2Summary.Comments);
        Assert.Contains("Evaluator 1 feedback on competence 2", competence2Summary.Comments);
        Assert.DoesNotContain("Evaluator 2 did not evaluate competence 2", competence2Summary.Comments);

        var competence2CriterionSummary = competence2Summary.CriterionSummaries[criterion1Id];
        Assert.Equal(competence2Expected, competence2CriterionSummary.Score, precision: 10);
        Assert.Single(competence2CriterionSummary.Comments);
        Assert.Contains("Solid performance", competence2CriterionSummary.Comments);
    }

    #region CreateAssessmentResultsAsync

    [Fact]
    public async Task CreateAssessmentResultsAsync_ShouldReturnEmptyList_WhenAssessmentIdsListIsEmpty()
    {
        // Arrange
        var emptyList = new List<Guid>();

        // Act
        var result = await _analyzer.CreateAssessmentResultsAsync(emptyList);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _evaluationRepositoryMock.Verify(
            x => x.GetEvaluationsByAssessmentIdsReadonlyAsync(It.IsAny<List<Guid>>(), CancellationToken.None),
            Times.Never);
        _assessmentResultRepositoryMock.Verify(
            x => x.CreateRangeAsync(It.IsAny<List<AssessmentResult>>(), CancellationToken.None),
            Times.Never);
    }

    [Fact]
    public async Task CreateAssessmentResultsAsync_ShouldReturnEmptyList_WhenNoEvaluationsExistForAnyAssessment()
    {
        // Arrange
        var assessmentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdsReadonlyAsync(assessmentIds, CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, Evaluation[]>());

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>());

        // Act
        var result = await _analyzer.CreateAssessmentResultsAsync(assessmentIds);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _assessmentResultRepositoryMock.Verify(
            x => x.CreateRangeAsync(It.IsAny<List<AssessmentResult>>(), CancellationToken.None),
            Times.Never);
    }

    [Fact]
    public async Task CreateAssessmentResultsAsync_ShouldSkipAssessmentsWithNoEvaluations()
    {
        // Arrange
        var assessment1Id = Guid.NewGuid();
        var assessment2Id = Guid.NewGuid();
        var assessment3Id = Guid.NewGuid();
        var assessmentIds = new List<Guid> { assessment1Id, assessment2Id, assessment3Id };

        var competenceId = Guid.NewGuid();
        var criterionId = Guid.NewGuid();

        // Только для assessment1Id есть оценки
        var evaluation = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessment1Id,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Good work",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterionId,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 8,
                            Comment = "Excellent"
                        }
                    }
                }
            }
        };

        var evaluationsByAssessmentId = new Dictionary<Guid, Evaluation[]>
        {
            { assessment1Id, [evaluation] },
            { assessment2Id, [] }, // Нет оценок
            { assessment3Id, [] } // Нет оценок
        };

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdsReadonlyAsync(assessmentIds, CancellationToken.None))
            .ReturnsAsync(evaluationsByAssessmentId);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competenceId, [criterionId] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateRangeAsync(It.IsAny<List<AssessmentResult>>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _analyzer.CreateAssessmentResultsAsync(assessmentIds);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(assessment1Id, result[0].AssessmentId);
        _assessmentResultRepositoryMock.Verify(
            x => x.CreateRangeAsync(
                It.Is<List<AssessmentResult>>(list => list.Count == 1 && list[0].AssessmentId == assessment1Id),
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task CreateAssessmentResultsAsync_ShouldCreateMultipleResults_WhenAllAssessmentsHaveEvaluations()
    {
        // Arrange
        var assessment1Id = Guid.NewGuid();
        var assessment2Id = Guid.NewGuid();
        var assessmentIds = new List<Guid> { assessment1Id, assessment2Id };

        var competenceId = Guid.NewGuid();
        var criterionId = Guid.NewGuid();

        var evaluation1 = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessment1Id,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Assessment 1 feedback",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterionId,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 7,
                            Comment = "Good"
                        }
                    }
                }
            }
        };

        var evaluation2 = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessment2Id,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Assessment 2 feedback",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterionId,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 9,
                            Comment = "Excellent"
                        }
                    }
                }
            }
        };

        var evaluationsByAssessmentId = new Dictionary<Guid, Evaluation[]>
        {
            { assessment1Id, [evaluation1] },
            { assessment2Id, [evaluation2] }
        };

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdsReadonlyAsync(assessmentIds, CancellationToken.None))
            .ReturnsAsync(evaluationsByAssessmentId);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competenceId, [criterionId] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateRangeAsync(It.IsAny<List<AssessmentResult>>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _analyzer.CreateAssessmentResultsAsync(assessmentIds);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var result1 = result.FirstOrDefault(r => r.AssessmentId == assessment1Id);
        Assert.NotNull(result1);
        Assert.Single(result1.Data.CompetenceSummaries);
        Assert.Equal(7.0, result1.Data.CompetenceSummaries[competenceId]!.AverageScore);

        var result2 = result.FirstOrDefault(r => r.AssessmentId == assessment2Id);
        Assert.NotNull(result2);
        Assert.Single(result2.Data.CompetenceSummaries);
        Assert.Equal(9.0, result2.Data.CompetenceSummaries[competenceId]!.AverageScore);

        _assessmentResultRepositoryMock.Verify(
            x => x.CreateRangeAsync(
                It.Is<List<AssessmentResult>>(list => list.Count == 2),
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task CreateAssessmentResultsAsync_ShouldCalculateCorrectWeightedAverages_ForMultipleAssessments()
    {
        // Arrange
        var assessment1Id = Guid.NewGuid();
        var assessment2Id = Guid.NewGuid();
        var assessmentIds = new List<Guid> { assessment1Id, assessment2Id };

        var competenceId = Guid.NewGuid();
        var criterion1Id = Guid.NewGuid();
        var criterion2Id = Guid.NewGuid();

        // Assessment 1: два оценщика с разными коэффициентами
        var manager1Evaluation = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 2,
            AssessmentId = assessment1Id,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Manager feedback",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 9,
                            Comment = "Excellent"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion2Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 7,
                            Comment = "Good"
                        }
                    }
                }
            }
        };

        var colleague1Evaluation = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessment1Id,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Colleague feedback",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 6,
                            Comment = "Solid"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion2Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 5,
                            Comment = "Needs improvement"
                        }
                    }
                }
            }
        };

        // Assessment 2: один оценщик
        var manager2Evaluation = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessment2Id,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Single evaluator",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion1Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 8,
                            Comment = "Very good"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterion2Id,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 6,
                            Comment = "Acceptable"
                        }
                    }
                }
            }
        };

        var evaluationsByAssessmentId = new Dictionary<Guid, Evaluation[]>
        {
            { assessment1Id, [manager1Evaluation, colleague1Evaluation] },
            { assessment2Id, [manager2Evaluation] }
        };

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdsReadonlyAsync(assessmentIds, CancellationToken.None))
            .ReturnsAsync(evaluationsByAssessmentId);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competenceId, [criterion1Id, criterion2Id] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateRangeAsync(It.IsAny<List<AssessmentResult>>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Expected values for Assessment 1
        var assessment1Criterion1Expected = Math.Round((9.0 * 2 + 6.0 * 1) / (2.0 + 1.0), 2); // 8.0
        var assessment1Criterion2Expected = Math.Round((7.0 * 2 + 5.0 * 1) / (2.0 + 1.0), 2); // 6.33
        var assessment1Expected = Math.Round((assessment1Criterion1Expected + assessment1Criterion2Expected) / 2.0, 2);

        // Expected values for Assessment 2
        var assessment2Criterion1Expected = 8.0;
        var assessment2Criterion2Expected = 6.0;
        var assessment2Expected = Math.Round((assessment2Criterion1Expected + assessment2Criterion2Expected) / 2.0, 2);

        // Act
        var result = await _analyzer.CreateAssessmentResultsAsync(assessmentIds);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        // Проверяем Assessment 1
        var result1 = result.FirstOrDefault(r => r.AssessmentId == assessment1Id);
        Assert.NotNull(result1);
        var competence1Summary = result1.Data.CompetenceSummaries[competenceId];
        Assert.NotNull(competence1Summary);
        Assert.Equal(assessment1Expected, competence1Summary.AverageScore, precision: 10);
        Assert.Equal(2, competence1Summary.Comments.Count);

        // Проверяем Assessment 2
        var result2 = result.FirstOrDefault(r => r.AssessmentId == assessment2Id);
        Assert.NotNull(result2);
        var competence2Summary = result2.Data.CompetenceSummaries[competenceId];
        Assert.NotNull(competence2Summary);
        Assert.Equal(assessment2Expected, competence2Summary.AverageScore, precision: 10);
        Assert.Single(competence2Summary.Comments);
    }

    [Fact]
    public async Task CreateAssessmentResultsAsync_ShouldNotCallCreateRange_WhenAllResultsAreNull()
    {
        // Arrange
        var assessment1Id = Guid.NewGuid();
        var assessment2Id = Guid.NewGuid();
        var assessmentIds = new List<Guid> { assessment1Id, assessment2Id };

        // Оценки есть, но нет оценок по критериям (все Score = null)
        var evaluation1 = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessment1Id,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = Guid.NewGuid(),
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Comment",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = Guid.NewGuid(),
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = null, // Нет оценки
                            Comment = null
                        }
                    }
                }
            }
        };

        var evaluation2 = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessment2Id,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = Guid.NewGuid(),
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Comment",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = Guid.NewGuid(),
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = null, // Нет оценки
                            Comment = null
                        }
                    }
                }
            }
        };

        var evaluationsByAssessmentId = new Dictionary<Guid, Evaluation[]>
        {
            { assessment1Id, [evaluation1] },
            { assessment2Id, [evaluation2] }
        };

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdsReadonlyAsync(assessmentIds, CancellationToken.None))
            .ReturnsAsync(evaluationsByAssessmentId);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>());

        // Act
        var result = await _analyzer.CreateAssessmentResultsAsync(assessmentIds);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _assessmentResultRepositoryMock.Verify(
            x => x.CreateRangeAsync(It.IsAny<List<AssessmentResult>>(), CancellationToken.None),
            Times.Never);
    }

    [Fact]
    public async Task CreateAssessmentResultsAsync_ShouldHandleMixedScenario_WithPartialResults()
    {
        // Arrange
        var assessment1Id = Guid.NewGuid(); // Есть валидная оценка
        var assessment2Id = Guid.NewGuid(); // Нет оценок
        var assessment3Id = Guid.NewGuid(); // Оценка есть, но все Score = null
        var assessment4Id = Guid.NewGuid(); // Есть валидная оценка
        var assessmentIds = new List<Guid> { assessment1Id, assessment2Id, assessment3Id, assessment4Id };

        var competenceId = Guid.NewGuid();
        var criterionId = Guid.NewGuid();

        var evaluation1 = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessment1Id,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Valid assessment 1",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterionId,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 7,
                            Comment = "Good"
                        }
                    }
                }
            }
        };

        var evaluation3 = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessment3Id,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Invalid assessment 3",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterionId,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = null, // Нет оценки
                            Comment = null
                        }
                    }
                }
            }
        };

        var evaluation4 = new Evaluation
        {
            Id = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            RoleRatio = 1,
            AssessmentId = assessment4Id,
            CompetenceEvaluations = new List<CompetenceEvaluation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CompetenceId = competenceId,
                    EvaluationId = Guid.NewGuid(),
                    Comment = "Valid assessment 4",
                    CriterionEvaluations = new List<CriterionEvaluation>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CriterionId = criterionId,
                            CompetenceEvaluationId = Guid.NewGuid(),
                            Score = 9,
                            Comment = "Excellent"
                        }
                    }
                }
            }
        };

        var evaluationsByAssessmentId = new Dictionary<Guid, Evaluation[]>
        {
            { assessment1Id, [evaluation1] },
            // assessment2Id отсутствует - нет оценок
            { assessment3Id, [evaluation3] },
            { assessment4Id, [evaluation4] }
        };

        _evaluationRepositoryMock
            .Setup(x => x.GetEvaluationsByAssessmentIdsReadonlyAsync(assessmentIds, CancellationToken.None))
            .ReturnsAsync(evaluationsByAssessmentId);

        _competenceRepositoryMock
            .Setup(x => x.GetCompetenceCriteriaMapAsync(CancellationToken.None))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { competenceId, [criterionId] }
            });

        _assessmentResultRepositoryMock
            .Setup(x => x.CreateRangeAsync(It.IsAny<List<AssessmentResult>>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _analyzer.CreateAssessmentResultsAsync(assessmentIds);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Только assessment1Id и assessment4Id должны быть в результате

        var result1 = result.FirstOrDefault(r => r.AssessmentId == assessment1Id);
        Assert.NotNull(result1);
        Assert.Equal(7.0, result1.Data.CompetenceSummaries[competenceId]!.AverageScore);

        var result4 = result.FirstOrDefault(r => r.AssessmentId == assessment4Id);
        Assert.NotNull(result4);
        Assert.Equal(9.0, result4.Data.CompetenceSummaries[competenceId]!.AverageScore);

        // assessment2Id и assessment3Id не должны быть в результате
        Assert.DoesNotContain(result, r => r.AssessmentId == assessment2Id);
        Assert.DoesNotContain(result, r => r.AssessmentId == assessment3Id);

        _assessmentResultRepositoryMock.Verify(
            x => x.CreateRangeAsync(
                It.Is<List<AssessmentResult>>(list => list.Count == 2),
                CancellationToken.None),
            Times.Once);
    }

    #endregion
}