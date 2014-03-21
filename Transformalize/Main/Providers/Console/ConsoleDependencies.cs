namespace Transformalize.Main.Providers.Console {
    public class ConsoleDependencies : AbstractConnectionDependencies {
        public ConsoleDependencies()
            : base(
                new ConsoleProvider(),
                new FalseTableQueryWriter(),
                new FalseConnectionChecker(),
                new FalseEntityRecordsExist(),
                new FalseEntityDropper(),
                new FalseEntityCreator(),
                new FalseViewWriter(),
                new FalseTflWriter(),
                new FalseScriptRunner(),
                new FalseProviderSupportsModifier()) { }
    }
}