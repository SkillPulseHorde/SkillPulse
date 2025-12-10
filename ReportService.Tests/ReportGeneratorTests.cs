using System.Diagnostics.CodeAnalysis;
using ReportService.Application.Models;
using ReportService.Infrastructure;

namespace ReportService.Tests;

public class ReportGeneratorTests
{
    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task GenerateReport_CreatesDocxFile_WithSpecificCompetenceAndCriteria()
    {
        // arrange
        var generator = new ReportGenerator();

        // Самостоятельность и ответственность (6 критериев)
        var competence1Id = Guid.NewGuid();
        var criterion1_1Id = Guid.NewGuid(); // атомарные задачи
        var criterion1_2Id = Guid.NewGuid(); // самостоятельность без наставника
        var criterion1_3Id = Guid.NewGuid(); // соблюдение сроков
        var criterion1_4Id = Guid.NewGuid(); // нетипичные задачи
        var criterion1_5Id = Guid.NewGuid(); // ситуации уровня проекта
        var criterion1_6Id = Guid.NewGuid(); // блокирующие проблемы

        // Коммуникативность (8 критериев)
        var competence2Id = Guid.NewGuid();
        var criterion2_1Id = Guid.NewGuid(); // задаёт вопросы
        var criterion2_2Id = Guid.NewGuid(); // понимает задачу с первого раза
        var criterion2_3Id = Guid.NewGuid(); // активное слушание
        var criterion2_4Id = Guid.NewGuid(); // кратко и чётко излагает
        var criterion2_5Id = Guid.NewGuid(); // адекватно на критику
        var criterion2_6Id = Guid.NewGuid(); // качественно обрабатывает входящую информацию
        var criterion2_7Id = Guid.NewGuid(); // настойчивость/умение отстаивать позицию
        var criterion2_8Id = Guid.NewGuid(); // лидерские качества/авторитет

        // Ориентация на бизнес (7 критериев)
        var competence3Id = Guid.NewGuid();
        var criterion3_1Id = Guid.NewGuid(); // выясняет цель/ценность
        var criterion3_2Id = Guid.NewGuid(); // следует целям
        var criterion3_3Id = Guid.NewGuid(); // саморефлексия
        var criterion3_4Id = Guid.NewGuid(); // понимает и учитывает риски
        var criterion3_5Id = Guid.NewGuid(); // сам формулирует цели
        var criterion3_6Id = Guid.NewGuid(); // формирует новые подходы/бизнес-решения
        var criterion3_7Id = Guid.NewGuid(); // оптимизирует процессы

        // Аналитическое и критическое мышление (6 критериев)
        var competence4Id = Guid.NewGuid();
        var criterion4_1Id = Guid.NewGuid(); // декомпозирует
        var criterion4_2Id = Guid.NewGuid(); // собирает информацию
        var criterion4_3Id = Guid.NewGuid(); // устраняет корневую причину
        var criterion4_4Id = Guid.NewGuid(); // видит связи между задачами
        var criterion4_5Id = Guid.NewGuid(); // стратегически-выгодные решения
        var criterion4_6Id = Guid.NewGuid(); // выходит за рамки команды/отдела

        // Обучаемость (6 критериев)
        var competence5Id = Guid.NewGuid();
        var criterion5_1Id = Guid.NewGuid(); // надёжно выполняет поручения без возвратов
        var criterion5_2Id = Guid.NewGuid(); // развивается на основе ОС
        var criterion5_3Id = Guid.NewGuid(); // ежемесячно виден прогресс
        var criterion5_4Id = Guid.NewGuid(); // привлекает внешние ресурсы
        var criterion5_5Id = Guid.NewGuid(); // проактивно менторит
        var criterion5_6Id = Guid.NewGuid(); // публично выступает

        var assessmentResult = new AssessmentResultModel
        {
            CompetenceSummaries = new Dictionary<Guid, CompetenceSummaryModel?>
            {
                {
                    competence1Id,
                    new CompetenceSummaryModel
                    {
                        CriterionSummaries = new Dictionary<Guid, CriterionSummaryModel>
                        {
                            {
                                criterion1_1Id,
                                new CriterionSummaryModel
                                {
                                    Score = 5.0,
                                    Comments =
                                    [
                                        "Отлично справился с атомарными задачами.",
                                        "Всегда выполняет задачи качественно и в срок.",
                                        "Показывает стабильный результат."
                                    ]
                                }
                            },
                            {
                                criterion1_2Id,
                                new CriterionSummaryModel
                                {
                                    Score = null, // не оценивался
                                    Comments = ["Хорошо проявил самостоятельность без наставника."]
                                }
                            },
                            {
                                criterion1_3Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.5,
                                    Comments = [] // без комментариев
                                }
                            },
                            {
                                criterion1_4Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.2,
                                    Comments =
                                    [
                                        "Успешно решает нетипичные задачи.",
                                        "Проявляет креативность в подходах."
                                    ]
                                }
                            },
                            {
                                criterion1_5Id,
                                new CriterionSummaryModel
                                {
                                    Score = null,
                                    Comments = []
                                }
                            },
                            {
                                criterion1_6Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.4,
                                    Comments = ["Способен решать блокирующие проблемы отдела."]
                                }
                            }
                        },
                        Comments =
                        [
                            "Демонстрирует высокий уровень самостоятельности и ответственности."
                        ]
                    }
                },
                {
                    competence2Id,
                    new CompetenceSummaryModel
                    {
                        CriterionSummaries = new Dictionary<Guid, CriterionSummaryModel>
                        {
                            {
                                criterion2_1Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.5,
                                    Comments = []
                                }
                            },
                            {
                                criterion2_2Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.7,
                                    Comments =
                                    [
                                        "Быстро понимает суть задачи.",
                                        "Редко требуются уточнения."
                                    ]
                                }
                            },
                            {
                                criterion2_3Id,
                                new CriterionSummaryModel
                                {
                                    Score = null,
                                    Comments =
                                    [
                                        "Практикует активное слушание.",
                                        "Задаёт уточняющие вопросы."
                                    ]
                                }
                            },
                            {
                                criterion2_4Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.0,
                                    Comments = ["Излагает мысли кратко и чётко."]
                                }
                            },
                            {
                                criterion2_5Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.3,
                                    Comments = []
                                }
                            },
                            {
                                criterion2_6Id,
                                new CriterionSummaryModel
                                {
                                    Score = null,
                                    Comments = []
                                }
                            },
                            {
                                criterion2_7Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.1,
                                    Comments =
                                    [
                                        "Умеет отстаивать свою позицию аргументированно.",
                                        "Не боится высказывать своё мнение.",
                                        "Конструктивно участвует в дискуссиях."
                                    ]
                                }
                            },
                            {
                                criterion2_8Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.2,
                                    Comments = ["Проявляет лидерские качества в команде."]
                                }
                            }
                        },
                        Comments =
                        [
                            "Коммуникативные навыки на хорошем уровне.",
                            "Есть потенциал для развития лидерских качеств.",
                            "Рекомендуется больше участвовать в межкомандных инициативах."
                        ]
                    }
                },
                {
                    competence3Id,
                    new CompetenceSummaryModel
                    {
                        CriterionSummaries = new Dictionary<Guid, CriterionSummaryModel>
                        {
                            {
                                criterion3_1Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.6,
                                    Comments =
                                    [
                                        "Всегда выясняет цель и ценность задачи.",
                                        "Понимает бизнес-контекст."
                                    ]
                                }
                            },
                            {
                                criterion3_2Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.5,
                                    Comments = []
                                }
                            },
                            {
                                criterion3_3Id,
                                new CriterionSummaryModel
                                {
                                    Score = null,
                                    Comments = ["Регулярно проводит саморефлексию."]
                                }
                            },
                            {
                                criterion3_4Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.4,
                                    Comments =
                                    [
                                        "Хорошо понимает и учитывает риски.",
                                        "Предупреждает о потенциальных проблемах заранее."
                                    ]
                                }
                            },
                            {
                                criterion3_5Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.2,
                                    Comments = []
                                }
                            },
                            {
                                criterion3_6Id,
                                new CriterionSummaryModel
                                {
                                    Score = null,
                                    Comments =
                                    [
                                        "Предлагает новые подходы и бизнес-решения.",
                                        "Проявляет инициативу в улучшении процессов."
                                    ]
                                }
                            },
                            {
                                criterion3_7Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.3,
                                    Comments = ["Активно оптимизирует рабочие процессы."]
                                }
                            }
                        },
                        Comments = ["Отличная ориентация на бизнес-результаты."]
                    }
                },
                {
                    competence4Id,
                    new CompetenceSummaryModel
                    {
                        CriterionSummaries = new Dictionary<Guid, CriterionSummaryModel>
                        {
                            {
                                criterion4_1Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.8,
                                    Comments =
                                    [
                                        "Отлично декомпозирует сложные задачи.",
                                        "Разбивает задачи на логические части.",
                                        "Помогает команде с планированием."
                                    ]
                                }
                            },
                            {
                                criterion4_2Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.5,
                                    Comments = []
                                }
                            },
                            {
                                criterion4_3Id,
                                new CriterionSummaryModel
                                {
                                    Score = null,
                                    Comments = ["Всегда устраняет корневую причину проблем."]
                                }
                            },
                            {
                                criterion4_4Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.6,
                                    Comments =
                                    [
                                        "Видит взаимосвязи между разными задачами.",
                                        "Учитывает системные зависимости."
                                    ]
                                }
                            },
                            {
                                criterion4_5Id,
                                new CriterionSummaryModel
                                {
                                    Score = 4.4,
                                    Comments = []
                                }
                            },
                            {
                                criterion4_6Id,
                                new CriterionSummaryModel
                                {
                                    Score = null,
                                    Comments = []
                                }
                            }
                        },
                        Comments =
                        [
                            "Сильное аналитическое и критическое мышление.",
                            "Один из лучших в команде по декомпозиции задач."
                        ]
                    }
                },
                {
                    competence5Id,
                    null
                }
            }
        };

        var names = new CompetencesAndCriteriaNamesModel
        {
            CompetenceNames = new Dictionary<Guid, string>
            {
                { competence1Id, "Самостоятельность и ответственность" },
                { competence2Id, "Коммуникативность" },
                { competence3Id, "Ориентация на бизнес" },
                { competence4Id, "Аналитическое и критическое мышление" },
                { competence5Id, "Обучаемость" }
            },
            CriterionNames = new Dictionary<Guid, string>
            {
                // Самостоятельность и ответственность
                { criterion1_1Id, "Атомарные задачи" },
                { criterion1_2Id, "Самостоятельность без наставника" },
                { criterion1_3Id, "Соблюдение сроков" },
                { criterion1_4Id, "Нетипичные задачи" },
                { criterion1_5Id, "Ситуации уровня проекта" },
                { criterion1_6Id, "Блокирующие проблемы отдела/компании" },
                
                // Коммуникативность
                { criterion2_1Id, "Задаёт вопросы" },
                { criterion2_2Id, "Понимает задачу с первого раза" },
                { criterion2_3Id, "Активное слушание" },
                { criterion2_4Id, "Кратко и чётко излагает" },
                { criterion2_5Id, "Адекватно на критику" },
                { criterion2_6Id, "Качественно обрабатывает входящую информацию" },
                { criterion2_7Id, "Настойчивость/умение отстаивать позицию" },
                { criterion2_8Id, "Лидерские качества/авторитет" },
                
                // Ориентация на бизнес
                { criterion3_1Id, "Выясняет цель/ценность" },
                { criterion3_2Id, "Следует целям" },
                { criterion3_3Id, "Саморефлексия" },
                { criterion3_4Id, "Понимает и учитывает риски" },
                { criterion3_5Id, "Сам формулирует цели" },
                { criterion3_6Id, "Формирует новые подходы/бизнес-решения" },
                { criterion3_7Id, "Оптимизирует процессы" },
                
                // Аналитическое и критическое мышление
                { criterion4_1Id, "Декомпозирует" },
                { criterion4_2Id, "Собирает информацию" },
                { criterion4_3Id, "Устраняет корневую причину" },
                { criterion4_4Id, "Видит связи между задачами" },
                { criterion4_5Id, "Стратегически-выгодные решения (долгосрок)" },
                { criterion4_6Id, "Выходит за рамки команды/отдела" },
                
                // Обучаемость
                { criterion5_1Id, "Надёжно выполняет поручения без возвратов" },
                { criterion5_2Id, "Развивается на основе ОС" },
                { criterion5_3Id, "Ежемесячно виден прогресс" },
                { criterion5_4Id, "Привлекает внешние ресурсы" },
                { criterion5_5Id, "Проактивно менторит" },
                { criterion5_6Id, "Публично выступает" }
            }
        };
        
        var fakeChart = File.Exists("test_chart.png")
            ? await File.ReadAllBytesAsync("test_chart.png")
            : [];
        
        const string employeeName = " Петрова Мария Анатольевна ";

        var recommendations = new RecommendationsModel
        {
            CompetenceRecommendations =
            [
                new CompetenceRecommendationModel
                {
                    CompetenceName = "Самостоятельность и ответственность",
                    CompetenceReason = "Ключевая компетенция для текущей роли и дальнейшего роста.",
                    WayToImproveCompetence = "Брать больше нетипичных задач из бэклога. §§ Регулярно запрашивать обратную связь у тимлида.",
                    LearningMaterials =
                    [
                        new LearningMaterialModel
                        {
                            LearningMaterialName = "Самостоятельность в работе разработчика",
                            LearningMaterialType = "Book",
                            LearningMaterialUrl = "https://example.com/article-self-responsibility"
                        },
                        new LearningMaterialModel
                        {
                            LearningMaterialName = "Как брать ответственность на себя",
                            LearningMaterialType = "Video",
                            LearningMaterialUrl = "https://example.com/video-responsibility"
                        }
                    ],
                    IsEvaluated = true
                },
                new CompetenceRecommendationModel // Не оценённая 
                {
                    CompetenceName = "Коммуникативность",
                    CompetenceReason = "Важно для развития лидерских качеств.",
                    WayToImproveCompetence = string.Empty,
                    LearningMaterials = [],
                    IsEvaluated = false
                },
                new CompetenceRecommendationModel // Всё в порядке (выше порога)
                {
                    CompetenceName = "Ориентация на бизнес",
                    CompetenceReason = "Важна для понимания целей компании и принятия эффективных решений.",
                    WayToImproveCompetence = "Вы отлично справляетесь с этой компетенцией!",
                    LearningMaterials = [],
                    IsEvaluated = true
                }
            ]
        };

        // act
        var bytes = await generator.GenerateReportAsync(assessmentResult, names, recommendations, fakeChart, employeeName, CancellationToken.None);

        // assert
        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);

        // доп. сохранение для ручной проверки
        var outDir = Path.Combine(Directory.GetCurrentDirectory(), "TestReports");
        Directory.CreateDirectory(outDir);
        var path = Path.Combine(outDir, "test_report_1.docx");
        await File.WriteAllBytesAsync(path, bytes);
    }
}