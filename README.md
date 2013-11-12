﻿Nancy.Sessions
==============

Session providers for Nancy web framework.
Currently, the MemorySession supported.

Usage:

```csharp
public class Bootstrapper : DefaultNancyBootstrapper
{
	protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
	{
		base.ApplicationStartup(container, pipelines);
		
		var sessioonManager = new SessionManager(new MemorySessionStore());
		sessioonManager.SessionStart += SessionStart;
		sessioonManager.Run(pipelines);
	}

	private static void SessionStart(ISession session)
	{
		//session start code here
	}
}
```
