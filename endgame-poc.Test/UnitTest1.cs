using System;
using Xunit;
using endgame_poc;

namespace endgame_poc.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            int result = endgame_poc.complexCalc.calculate(1, 2);

            if (result == 0)
            {
                Assert.True(true);
            }
            else
            {
                Assert.False(true);
            }
        }
    }
}
