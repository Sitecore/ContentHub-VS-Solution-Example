Implementation specific commandline project
This project makes use of Autofac, so any created services can be called via dependency injection without having an explicit registration
This project is executable, which means it makes use of the following config files:
	appsettings.json
	local.settings.json
	user secrets

This project can access services from the following projects:
	Sitecore.CH.Implementation
	Sitecore.CH.Base
	Sitecore.CH.Base.CommandLine


