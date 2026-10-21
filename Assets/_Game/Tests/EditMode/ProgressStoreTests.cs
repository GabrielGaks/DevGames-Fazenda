using DevGames.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace DevGames.Tests
{
    public sealed class ProgressStoreTests
    {
        [SetUp]
        public void SetUp() => PlayerPrefs.DeleteAll();

        [TearDown]
        public void TearDown() => PlayerPrefs.DeleteAll();

        [TestCase(-1, "--:--.--")]
        [TestCase(0, "00:00.00")]
        [TestCase(305678, "05:05.67")]
        public void FormatMilliseconds_UsesTimerFormat(int milliseconds, string expected)
            => Assert.That(ProgressStore.FormatMilliseconds(milliseconds), Is.EqualTo(expected));

        [Test]
        public void CompleteLevel_KeepsFastestTime()
        {
            Assert.That(ProgressStore.CompleteLevel(1, 2, 120000), Is.True);
            Assert.That(ProgressStore.CompleteLevel(1, 2, 130000), Is.False);
            Assert.That(ProgressStore.CompleteLevel(1, 2, 110000), Is.True);
            Assert.That(ProgressStore.BestMilliseconds(1), Is.EqualTo(110000));
        }

        [Test]
        public void CompleteLevel_NeverRegressesUnlock()
        {
            ProgressStore.CompleteLevel(3, 4, 120000);
            ProgressStore.CompleteLevel(1, 2, 90000);

            Assert.That(ProgressStore.HighestUnlockedLevel, Is.EqualTo(4));
            Assert.That(ProgressStore.IsCompleted(1), Is.True);
            Assert.That(ProgressStore.IsCompleted(3), Is.True);
        }
    }
}
