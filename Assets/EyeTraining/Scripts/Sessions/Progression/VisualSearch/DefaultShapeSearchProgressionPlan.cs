using System;

namespace EyeTraining.Sessions.Progression.VisualSearch
{
    public static class DefaultShapeSearchProgressionPlan
    {
        public const int RequiredCompletedExecutionsPerLevel = 3;

        public static ShapeSearchProgressionPlan Create()
        {
            return new ShapeSearchProgressionPlan(
                new[]
                {
                    new ShapeSearchLevelSettings(0, 20, 4, 0.066f, TimeSpan.FromSeconds(30)),
                    new ShapeSearchLevelSettings(1, 24, 4, 0.064f, TimeSpan.FromSeconds(35)),
                    new ShapeSearchLevelSettings(2, 24, 5, 0.062f, TimeSpan.FromSeconds(40)),
                    new ShapeSearchLevelSettings(3, 28, 5, 0.059f, TimeSpan.FromSeconds(45)),
                    new ShapeSearchLevelSettings(4, 30, 6, 0.056f, TimeSpan.FromSeconds(50)),
                    new ShapeSearchLevelSettings(5, 32, 6, 0.053f, TimeSpan.FromSeconds(55))
                },
                RequiredCompletedExecutionsPerLevel);
        }
    }
}
