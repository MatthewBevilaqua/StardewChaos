using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace StardewChaos
{
    public class TwitchVoteManager
    {
        private readonly Dictionary<string, int> _userVotes = new();
        private readonly object _lock = new();
        private readonly TwitchIrcClient _irc;

        public bool IsConnected => _irc?.IsConnected ?? false;

        public TwitchVoteManager(TwitchIrcClient irc)
        {
            _irc = irc;
        }

        public void DrainQueue()
        {
            if (_irc == null) return;
            while (_irc.VoteQueue.TryDequeue(out var vote))
            {
                lock (_lock)
                {
                    _userVotes[vote.user] = vote.vote;
                }
            }
        }

        public int[] GetVoteCounts(int slotCount, int baseNumber = 1)
        {
            var counts = new int[slotCount];
            lock (_lock)
            {
                foreach (var kvp in _userVotes)
                {
                    int slot = kvp.Value - baseNumber;
                    if (slot >= 0 && slot < slotCount)
                        counts[slot]++;
                }
            }
            return counts;
        }

        public int GetWinner(int slotCount, Random rng, int baseNumber = 1)
        {
            var counts = GetVoteCounts(slotCount, baseNumber);
            int maxVotes = counts.Max();
            if (maxVotes == 0) return -1;

            var tied = new List<int>();
            for (int i = 0; i < slotCount; i++)
            {
                if (counts[i] == maxVotes)
                    tied.Add(i);
            }

            return tied[rng.Next(tied.Count)];
        }

        public int GetTotalVotes()
        {
            lock (_lock)
            {
                return _userVotes.Count;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _userVotes.Clear();
            }
        }
    }
}