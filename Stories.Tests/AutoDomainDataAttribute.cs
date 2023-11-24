using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;

namespace Stories.Tests
{
    public class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute()
          : base(() => new Fixture().Customize(new AutoMoqCustomization()))
        {
        }
    }
}
