using System.Collections.Generic;

namespace Runtime.Utilities
{
    public static class RemoteConfigDummy
    {
        public static List<int> levels = new List<int> { 1, };
        public static List<int> timers = new List<int> { 300, };

        public const int LevelLoopStart = 2;
        public const int DefaultTimer = 60;

        public static bool hasTimer = true;
    }
}