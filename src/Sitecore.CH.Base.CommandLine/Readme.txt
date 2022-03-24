Base commandline project - This project should used to group commands which are not specific for the implementation, and could be shared with other implementations
This project makes use of Autofac, so any created services can be called via dependency injection without having an explicit registration
This project is executable, which means it makes use of the following config files:
	appsettings.json
	local.settings.json
	user secrets

This project can access services from the following projects:
	Sitecore.CH.Base