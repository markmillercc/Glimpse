using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;

namespace Glimpse.Tests;
public static class AutoFx
{
    public static IFixture Fixture { get; }
    public static T Create<T>() => Fixture.Create<T>();
    public static object Create(Type type) => Fixture.Create(type, new SpecimenContext(Fixture));
    public static ICustomizationComposer<T> Build<T>() => Fixture.Build<T>();
    static AutoFx()
    {
        Fixture = new Fixture()
            .Customize(new OmitOnRecursionBehaviorCustomization());
    }
}

