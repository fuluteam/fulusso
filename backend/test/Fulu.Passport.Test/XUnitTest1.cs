using System;
using Fulu.Passport.Domain.Component;
using Xunit;

namespace Fulu.Passport.Test
{
    public class Tests
    {
        [SkippableFact]
        public void Setup()
        {
        }

        [SkippableFact]
        public void Test1()
        {
        }

        [Fact]
        public void EnumTest()
        {
            if (Enum.TryParse("Reg", true, out ValidationType type))
            {

            }
        }
    }
}