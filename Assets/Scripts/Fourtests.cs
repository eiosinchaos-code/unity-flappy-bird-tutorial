using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Fourtests
{
    private GameObject sessionGo;
    private GameSessionData session;

    [SetUp]
    public void SetUp()
    {
        sessionGo = new GameObject("TestSession");
        session = sessionGo.AddComponent<GameSessionData>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(sessionGo);
    }

    [Test]
    public void DefaultPreferences_MatchRequiredSpawnRateAndGap()
    {
        Assert.AreEqual(1f, session.spawnRate, 0.001f);
        Assert.AreEqual(3f, session.verticalGap, 0.001f);
    }

    [Test]
    public void PlayerNameAndPreferences_UpdateAndPersist()
    {
        session.playerName = "Ernst";
        session.spawnRate = 0.75f;
        session.verticalGap = 2.5f;

        Assert.AreEqual("Ernst", GameSessionData.Instance.playerName);
        Assert.AreEqual(0.75f, GameSessionData.Instance.spawnRate, 0.001f);
        Assert.AreEqual(2.5f, GameSessionData.Instance.verticalGap, 0.001f);
    }

    [Test]
    public void LastGameStats_StoreScoreLevelAndTimePlayed()
    {
        session.lastScore = 7;
        session.lastLevel = 4;
        session.lastTimePlayed = 23.75f;

        Assert.AreEqual(7, session.lastScore);
        Assert.AreEqual(4, session.lastLevel);
        Assert.AreEqual(23.75f, session.lastTimePlayed, 0.001f);
    }

    [Test]
    public void TopTenSorting_OrdersHighestScoreFirst()
    {
        List<ScoreEntry> list = new List<ScoreEntry>
        {
            new ScoreEntry { playerName = "Low", level = 1, score = 2, timePlayed = 10f },
            new ScoreEntry { playerName = "High", level = 5, score = 15, timePlayed = 40f },
            new ScoreEntry { playerName = "Mid", level = 3, score = 8, timePlayed = 22f }
        };

        list.Sort((a, b) => b.score.CompareTo(a.score));

        Assert.AreEqual("High", list[0].playerName);
        Assert.AreEqual("Mid", list[1].playerName);
        Assert.AreEqual("Low", list[2].playerName);
    }
}