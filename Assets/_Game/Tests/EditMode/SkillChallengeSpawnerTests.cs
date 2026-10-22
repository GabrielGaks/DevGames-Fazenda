using System.Linq;
using DevGames.Level;
using NUnit.Framework;
using UnityEngine;

namespace DevGames.Tests
{
    public sealed class SkillChallengeSpawnerTests
    {
        [Test]
        public void Spawn_PicksPrefabAndPointFromTheArrays()
        {
            var prefabs = new[] { Prop("Caixa"), Prop("Barril") };
            var points = new[] { new Vector3(1f, 5f, 0f), new Vector3(-4f, 6f, 2f) }
                .Select(p => new GameObject("Ponto") { transform = { position = p } }.transform).ToArray();
            var spawner = new GameObject("Spawner").AddComponent<SkillChallengeSpawner>();
            spawner.Configure(prefabs, points);

            for (var i = 0; i < 20; i++)
            {
                var spawned = spawner.Spawn();
                Assert.That(prefabs.Any(p => spawned.name.StartsWith(p.name)), spawned.name);
                Assert.That(points.Any(p => p.position == spawned.transform.position), spawned.transform.position.ToString());
                Object.DestroyImmediate(spawned);
            }

            Object.DestroyImmediate(spawner.gameObject);
            foreach (var go in prefabs.Concat(points.Select(p => p.gameObject))) Object.DestroyImmediate(go);
        }

        [Test]
        public void Spawn_WithEmptyArrays_ReturnsNull()
        {
            var spawner = new GameObject("Spawner").AddComponent<SkillChallengeSpawner>();
            spawner.Configure(new GameObject[0], new Transform[0]);
            Assert.That(spawner.Spawn(), Is.Null);
            Object.DestroyImmediate(spawner.gameObject);
        }

        static GameObject Prop(string name) => new(name, typeof(Rigidbody));
    }
}
