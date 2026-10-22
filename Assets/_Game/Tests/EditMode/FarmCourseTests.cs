using DevGames.Editor;
using NUnit.Framework;
using UnityEngine;

namespace DevGames.Tests
{
    public sealed class FarmCourseTests
    {
        [Test]
        public void CourseRoute_EveryJumpIsReachableAndClear()
            => Assert.DoesNotThrow(() => FarmProjectBuilder.ValidateRoute(FarmProjectBuilder.CourseRoute()));

        [Test]
        public void CourseRoute_EveryPointIsAboveTheField()
        {
            // Only Up: a fall from anywhere on the course lands on the farm, never in the void.
            var field = FarmProjectBuilder.FieldIsland;
            foreach (var (name, points) in FarmProjectBuilder.CourseRoute())
                foreach (var point in points)
                    Assert.That(field.Contains(new Vector2(point.x, point.z)), $"{name}: {point} fora do campo");
        }

        [Test]
        public void RouteModels_AllExistInProject()
        {
            foreach (var path in FarmProjectBuilder.RouteModelPaths())
                Assert.That(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path), Is.Not.Null, path);
        }

        [Test]
        public void CourseRoute_EndsHigherThanItStarts()
        {
            var route = FarmProjectBuilder.CourseRoute();
            var last = route[route.Count - 1].points;
            Assert.That(last[last.Length - 1].y, Is.GreaterThan(route[0].points[0].y + 60f));
        }

        [TestCase(0f, 4f, TestName = "ValidateRoute_RejectsFlatJump")]
        [TestCase(2f, 4f, TestName = "ValidateRoute_RejectsTooHighJump")]
        [TestCase(1f, 7f, TestName = "ValidateRoute_RejectsTooFarJump")]
        public void ValidateRoute_RejectsImpossibleJump(float rise, float distance)
        {
            var route = new[] { ("Teste", new[] { Vector3.zero, new Vector3(0f, rise, distance) }) };
            Assert.Throws<System.InvalidOperationException>(() => FarmProjectBuilder.ValidateRoute(route));
        }

        [Test]
        public void ValidateRoute_RejectsPropOverAnotherProp()
        {
            var points = new[]
            {
                Vector3.zero, new Vector3(0f, 1f, 4f), new Vector3(0f, 2f, 8f),
                new Vector3(0f, 3f, 4.5f), new Vector3(0f, 4f, 0f)
            };
            Assert.Throws<System.InvalidOperationException>(() => FarmProjectBuilder.ValidateRoute(new[] { ("Teste", points) }));
        }
    }
}
