using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BlueHeighliner.DocumentGenerator.Tests")]

// Lets Moq's DynamicProxy create mocks of internal interfaces (this app has no public API at all):
// without this, Moq.Setup on an internal interface throws because the dynamically generated proxy
// assembly can't otherwise see internal members.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
